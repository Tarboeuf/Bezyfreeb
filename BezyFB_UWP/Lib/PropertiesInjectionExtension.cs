using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace BezyFB_UWP.Lib
{
    public class PropertiesInjectionExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            // pour AmpiEntities
            //Context.Strategies.Add(new AmpiEntitiesInjectorStrategy(Container), UnityBuildStage.PostInitialization);

            // pour IloggerService<>
            //Context.Strategies.Add(new LoggerInjectorStrategy(Container), UnityBuildStage.PostInitialization);

            // intercepteur de resolution qui permettent de setter les propriétés qui peuvent etre résolue dans le container
            Context.Strategies.Add(new PropertiesInjectorBuilderStrategy(Container), UnityBuildStage.PostInitialization);
        }
    }
}