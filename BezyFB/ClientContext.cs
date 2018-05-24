using BezyFB.Configuration;
using BezyFB.Helpers;
using BezyFB.T411;
using CommonPortableLib;
using FreeboxPortableLib;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetaseriesPortableLib;
using EztvPortableLib;
using Unity;
using Unity.Builder;
using Unity.Builder.Strategy;
using Unity.Extension;
using Unity.Injection;
using Unity.Lifetime;

namespace BezyFB
{
    public class ClientContext
    {
        public static ClientContext Current { get; } = new ClientContext();

        public static UnityContainer Container { get; set; }

        public IMessageDialogService MessageDialogService => Container.Resolve<IMessageDialogService>();
        public IApiConnectorService ApiConnector => Container.Resolve<IApiConnectorService>();

        public Freebox Freebox => Container.Resolve<Freebox>();

        public T411Client T411 => Container.Resolve<T411Client>();

        public BetaSerie BetaSerie => Container.Resolve<BetaSerie>();

        public Eztv Eztv => Container.Resolve<Eztv>();

        public GuessIt GuessIt => Container.Resolve<GuessIt>();
        public ICryptographic Crypto => Container.Resolve<ICryptographic>();

        public static void Init()
        {
            InitForTest(MySettings.Current);

            Container.RegisterType<IMessageDialogService, MessageDialogService>(new ContainerControlledLifetimeManager());
        }

        public static void InitForTest(ISettingsFreebox settings)
        {
            Container = new UnityContainer();
            Container.AddNewExtension<PropertiesInjectionExtension>(); // active l'injection par propriétés pour toutes les instance résolue dans le container

            Container.RegisterType<Freebox>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(settings));

            Container.RegisterType<IApiConnectorService, ApiConnector>(new ContainerControlledLifetimeManager());

            Container.RegisterType<BetaSerie>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(MySettings.Current.LoginBetaSerie, MySettings.Current.PwdBetaSerie));

            Container.RegisterType<T411Client>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(MySettings.Current.LoginT411, MySettings.Current.PassT411));

            Container.RegisterType<Eztv>(new ContainerControlledLifetimeManager());

            Container.RegisterType<GuessIt>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IFormUploadService, FormUpload>(new ContainerControlledLifetimeManager());

            Container.RegisterType<ICryptographic, Cryptographic>(new ContainerControlledLifetimeManager());
        }

        public static void Register<T1, T2>() where T2 : T1
        {
            Container.RegisterType<T1, T2>(new ContainerControlledLifetimeManager());
        }
    }
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

    /// <summary>
    /// Permet d'activation de l'injection par propriété lors des résolutions du container
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
                Type typeToResolveInContainer;

                if (propertyInfo.PropertyType.IsGenericType)
                {
                    typeToResolveInContainer = propertyInfo.PropertyType.GetGenericTypeDefinition();
                }
                else
                {
                    typeToResolveInContainer = propertyInfo.PropertyType;
                }


                if (_container.IsRegistered(typeToResolveInContainer)) // si le type de la propriété existe dans le container...
                {
                    propertyInfo.SetValue(resolvedObject, _container.Resolve(typeToResolveInContainer), null); // ...on la set apres résolution dans celui ci
                }
            }
        }
    }
}
