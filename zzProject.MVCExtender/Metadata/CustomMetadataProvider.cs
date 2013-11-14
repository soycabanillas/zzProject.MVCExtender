using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace zzProject.MVCExtender.Metadata
{
    public class CustomMetadataProvider<TModelMetadata> : DataAnnotationsModelMetadataProvider where TModelMetadata : CustomModelMetadata
    {
        // This dictionary is a mapping from incoming metadata request to a list of
        // functions which will modify the metadata. The incoming metadata request
        // might be for just a type (so, getting metadata on a class), in which case
        // the key tuple will contain the type and a null value for the string; if
        // the incoming request is for a property, then the key tuple will contain
        // the container type and the name of the property.
        ConcurrentDictionary<Tuple<Type, string>, List<Action<TModelMetadata>>> modifiers
            = new ConcurrentDictionary<Tuple<Type, string>, List<Action<TModelMetadata>>>();

        Func<CustomMetadataProvider<TModelMetadata>, Type, Func<object>, Type, string, DisplayColumnAttribute, TModelMetadata> modelMetadataGenerator;

        private CustomMetadataProvider()
        {
        }

        public CustomMetadataProvider(Func<CustomMetadataProvider<TModelMetadata>, Type, Func<object>, Type, string, DisplayColumnAttribute, TModelMetadata> ModelMetadataGenerator)
        {
            this.modelMetadataGenerator = ModelMetadataGenerator;
        }

        protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes,
                                                        Type containerType,
                                                        Func<object> modelAccessor,
                                                        Type modelType,
                                                        string propertyName)
        {
            

            //COPY OF MVC - BEGIN
            List<Attribute> attributeList = new List<Attribute>(attributes);
            DisplayColumnAttribute displayColumnAttribute = attributeList.OfType<DisplayColumnAttribute>().FirstOrDefault();
            //CHANGE OF MVC - BEGIN
            //DataAnnotationsModelMetadata result = new CustomModelMetadata(this, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute);
            TModelMetadata result = this.modelMetadataGenerator(this, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute);
            //CHANGE OF MVC - END
            // Do [HiddenInput] before [UIHint], so you can override the template hint
            HiddenInputAttribute hiddenInputAttribute = attributeList.OfType<HiddenInputAttribute>().FirstOrDefault();
            if (hiddenInputAttribute != null)
            {
                result.TemplateHint = "HiddenInput";
                result.HideSurroundingHtml = !hiddenInputAttribute.DisplayValue;
            }

            // We prefer [UIHint("...", PresentationLayer = "MVC")] but will fall back to [UIHint("...")]
            IEnumerable<UIHintAttribute> uiHintAttributes = attributeList.OfType<UIHintAttribute>();
            UIHintAttribute uiHintAttribute = uiHintAttributes.FirstOrDefault(a => String.Equals(a.PresentationLayer, "MVC", StringComparison.OrdinalIgnoreCase))
                                           ?? uiHintAttributes.FirstOrDefault(a => String.IsNullOrEmpty(a.PresentationLayer));
            if (uiHintAttribute != null)
            {
                result.TemplateHint = uiHintAttribute.UIHint;
            }

            DataTypeAttribute dataTypeAttribute = attributeList.OfType<DataTypeAttribute>().FirstOrDefault();
            if (dataTypeAttribute != null)
            {
                //CHANGE OF MVC
                //result.DataTypeName = dataTypeAttribute.ToDataTypeName();
                result.DataTypeName = dataTypeAttribute.GetDataTypeName();
            }

            EditableAttribute editable = attributes.OfType<EditableAttribute>().FirstOrDefault();
            if (editable != null)
            {
                result.IsReadOnly = !editable.AllowEdit;
            }
            else
            {
                ReadOnlyAttribute readOnlyAttribute = attributeList.OfType<ReadOnlyAttribute>().FirstOrDefault();
                if (readOnlyAttribute != null)
                {
                    result.IsReadOnly = readOnlyAttribute.IsReadOnly;
                }
            }

            DisplayFormatAttribute displayFormatAttribute = attributeList.OfType<DisplayFormatAttribute>().FirstOrDefault();
            if (displayFormatAttribute == null && dataTypeAttribute != null)
            {
                displayFormatAttribute = dataTypeAttribute.DisplayFormat;
            }
            if (displayFormatAttribute != null)
            {
                result.NullDisplayText = displayFormatAttribute.NullDisplayText;
                result.DisplayFormatString = displayFormatAttribute.DataFormatString;
                result.ConvertEmptyStringToNull = displayFormatAttribute.ConvertEmptyStringToNull;

                if (displayFormatAttribute.ApplyFormatInEditMode)
                {
                    result.EditFormatString = displayFormatAttribute.DataFormatString;
                }

                if (!displayFormatAttribute.HtmlEncode && String.IsNullOrWhiteSpace(result.DataTypeName))
                {
                    //CHANGE OF MVC
                    //result.DataTypeName = DataTypeUtil.HtmlTypeName;
                    result.DataTypeName = DataType.Html.ToString();
                }
            }

            ScaffoldColumnAttribute scaffoldColumnAttribute = attributeList.OfType<ScaffoldColumnAttribute>().FirstOrDefault();
            if (scaffoldColumnAttribute != null)
            {
                result.ShowForDisplay = result.ShowForEdit = scaffoldColumnAttribute.Scaffold;
            }

            DisplayAttribute display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            string name = null;
            if (display != null)
            {
                result.Description = display.GetDescription();
                result.ShortDisplayName = display.GetShortName();
                result.Watermark = display.GetPrompt();
                result.Order = display.GetOrder() ?? ModelMetadata.DefaultOrder;

                name = display.GetName();
            }

            if (name != null)
            {
                result.DisplayName = name;
            }
            else
            {
                DisplayNameAttribute displayNameAttribute = attributeList.OfType<DisplayNameAttribute>().FirstOrDefault();
                if (displayNameAttribute != null)
                {
                    result.DisplayName = displayNameAttribute.DisplayName;
                }
            }

            RequiredAttribute requiredAttribute = attributeList.OfType<RequiredAttribute>().FirstOrDefault();
            if (requiredAttribute != null)
            {
                result.IsRequired = true;
            }

            //CHANGE OF MVC
            //return result;
            //COPY OF MVC - END

            //CUSTOMIZATION Reading attributes - BEGIN

            KeyAttribute keyAttribute = attributeList.OfType<KeyAttribute>().FirstOrDefault();
            result.getMM_CommonExtensionModelMetadata().IsKey = (keyAttribute != null);

            //CUSTOMIZATION Reading attributes - END

            // We determine the request type. If the property name is null, then
            // we're being asked for model-level metadata; otherwise, we're being asked
            // for property-level metadata. We make a tuple which matches the type of
            // request we're being asked to perform.
            Tuple<Type, string> key = propertyName == null ? new Tuple<Type, string>(modelType, null)
                                                            : new Tuple<Type, string>(containerType, propertyName);

            // If we have any modifier functions that match the requested key tuple, then
            // we'll loop over them and run them all.
            List<Action<TModelMetadata>> modifierList;
            if (modifiers.TryGetValue(key, out modifierList))
                foreach (Action<TModelMetadata> modifier in modifierList)
                    modifier(result);

            return result;
        }

        // This method is called by the registrar class so that it can add modification
        // actions to the given registration.
        public void Add(Type type, string propertyName, Action<TModelMetadata> modifier)
        {
            modifiers.GetOrAdd(
                new Tuple<Type, string>(type, propertyName),
                _ => new List<Action<TModelMetadata>>()
            ).Add(modifier);
        }

        // This method is called by the developer to get an instance of the registrar which
        // can be used to record model-level metadata changes.
        public MetadataRegistrar<TModel> ForModel<TModel>()
        {
            return new MetadataRegistrar<TModel>(this);
        }

        // The MetadataRegistrar class is the core type that records registrations. It
        // contains methods which add actions to the provider list which modify the
        // metadata object appropriately. Because this is a fluent API, we always return
        // an instance of ourselves so that we can chain calls together.
        public class MetadataRegistrar<TModel>
        {
            CustomMetadataProvider<TModelMetadata> provider;
            string propertyName;

            public MetadataRegistrar(CustomMetadataProvider<TModelMetadata> provider, string propertyName = null)
            {
                this.provider = provider;
                this.propertyName = propertyName;
            }

            public MetadataRegistrar<TModel> ModelMetadata(Action<TModelMetadata> delegateModelMetadata)
            {
                provider.Add(typeof(TModel), propertyName, delegateModelMetadata);
                return this;
            }

            // Support implicit conversion back to the provider, so that the user can create,
            // configure, and register the provider with a single line of code.
            public static implicit operator CustomMetadataProvider<TModelMetadata>(MetadataRegistrar<TModel> registrar)
            {
                return registrar.provider;
            }

            // This method is like ForModel above, except that it lets the developer transition
            // to a new model without needing to go back to the registrar itself.
            public MetadataRegistrar<TNewModel> ForModel<TNewModel>()
            {
                return new MetadataRegistrar<TNewModel>(provider);
            }

            // This method is called by the developer to get an instance of the registrar which
            // can be used to record property-level metadata changes. We use Expression<T> here
            // so that the user can get Intellisense for the property expression, and also so
            // that their registration code can support refactoring operations like renaming
            // properties on models.
            public MetadataRegistrar<TModel> ForProperty(Expression<Func<TModel, object>> expression)
            {
                return new MetadataRegistrar<TModel>(provider, ExpressionToPropertyName(expression));
            }

            // This helper method extracts the property name from the Expression<T>
            private static string ExpressionToPropertyName(Expression<Func<TModel, object>> expression)
            {
                Expression body = expression.Body;

                UnaryExpression unaryExpression = body as UnaryExpression;
                if (unaryExpression != null && unaryExpression.NodeType == ExpressionType.Convert)  // Boxing value type to object
                    body = unaryExpression.Operand;

                MemberExpression memberExpression = (MemberExpression)body;
                return memberExpression.Member.Name;
            }
        }
    }


}