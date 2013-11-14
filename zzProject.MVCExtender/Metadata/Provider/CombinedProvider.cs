using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace zzProject.MVCExtender.Metadata.Provider
{
    public class CombinedProvider
    {
        public class MyMetadataProvider : CustomMetadataProvider<CustomModelMetadata>
        {
            public MyMetadataProvider()
                : base((provider, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute) =>
                {
                    return new CustomModelMetadata(
                        provider, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute
                      );
                })
            {
            }
        }

        public class MyValidatorProvider : FluentValidationProvider
        {
        }

        private MyMetadataProvider metadataProvider = new MyMetadataProvider();
        private MyValidatorProvider validatorProvider = new MyValidatorProvider();

        public static implicit operator MyMetadataProvider(CombinedProvider provider)
        {
            return provider.metadataProvider;
        }
        public static implicit operator MyValidatorProvider(CombinedProvider provider)
        {
            return provider.validatorProvider;
        }

        private void AddMetadata(Type type, string propertyName, Action<CustomModelMetadata> modifier)
        {
            this.metadataProvider.Add(type, propertyName, modifier);
            //modifiers.GetOrAdd(
            //    new Tuple<Type, string>(type, propertyName),
            //    _ => new List<Action<ModelMetadata>>()
            //).Add(modifier);
        }

        private void AddValidator(Type type, string propertyName, Func<ModelMetadata, ControllerContext, ModelValidator> factory)
        {
            this.validatorProvider.Add(type, propertyName, new FluentValidationProvider.ValidatorFactory(factory));
            //validators.GetOrAdd(
            //    new Tuple<Type, string>(type, propertyName),
            //    _ => new List<ValidatorFactory>()
            //).Add(factory);
        }

        public Registrar<TModel> ForModel<TModel>()
        {
            return new Registrar<TModel>(this);
        }

        public class Registrar<TModel>
        {
            private CombinedProvider provider;
            private string propertyName;

            public Registrar(CombinedProvider provider, string propertyName = null)
            {
                this.provider = provider;
                this.propertyName = propertyName;
            }

            public static implicit operator MyMetadataProvider(Registrar<TModel> registrar)
            {
                return registrar.provider.metadataProvider;
            }
            public static implicit operator MyValidatorProvider(Registrar<TModel> registrar)
            {
                return registrar.provider.validatorProvider;
            }

            // This method is like ForModel above, except that it lets the developer transition
            // to a new model without needing to go back to the registrar itself.
            public Registrar<TNewModel> ForModel<TNewModel>()
            {
                return new Registrar<TNewModel>(provider);
            }

            // This method is called by the developer to get an instance of the registrar which
            // can be used to record property-level validation rules. We use Expression<T> here
            // so that the user can get Intellisense for the property expression, and also so
            // that their registration code can support refactoring operations like renaming
            // properties on models.
            public Registrar<TModel> ForProperty(Expression<Func<TModel, object>> expression)
            {
                return new Registrar<TModel>(provider, ExpressionToPropertyName(expression));
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

            public Registrar<TModel> SetMetadata(Action<CustomModelMetadata> metadata)
            {
                provider.AddMetadata(typeof(TModel), propertyName, metadata);
                return this;
            }

            public Registrar<TModel> SetValidator(Func<ModelMetadata, ControllerContext, ModelValidator> validator)
            {
                provider.AddValidator(
                    typeof(TModel),
                    propertyName,
                    (metadata, context) => validator(metadata, context)
                );
                return this;
            }
        }
    }
}
