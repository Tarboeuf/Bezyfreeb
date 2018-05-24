using System;
using Unity;
using Unity.Builder;
using Unity.Builder.Strategy;

namespace BezyFB_UWP.Lib
{
    /// <summary>
    /// Permet d'activation de l'injection par propri�t� lors des r�solutions du container
    /// </summary>
    public class PropertiesInjectorBuilderStrategy : BuilderStrategy
    {
        private readonly IUnityContainer _container;

        public PropertiesInjectorBuilderStrategy(IUnityContainer container)
        {
            _container = container;
        }

        public override void PostBuildUp(IBuilderContext context)
        {
            var resolvedObject = context.Existing;

            var properties = resolvedObject.GetType().GetProperties();

            foreach (var propertyInfo in properties)
            {
                Type typeToResolveInContainer = propertyInfo.PropertyType;
                
                if (_container.IsRegistered(typeToResolveInContainer)) // si le type de la propri�t� existe dans le container...
                {
                    propertyInfo.SetValue(resolvedObject, _container.Resolve(typeToResolveInContainer), null); // ...on la set apres r�solution dans celui ci
                }
            }
        }
    }
}