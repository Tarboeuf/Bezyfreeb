using EztvPortableLib;
using BezyFB_UWP.Lib.T411;
using CommonPortableLib;
using FreeboxPortableLib;
using Microsoft.Practices.Unity;
using BetaseriesPortableLib;
using CommonLib;

namespace BezyFB_UWP.Lib
{
    public class ClientContext
    {
        public static ClientContext Current { get; } = new ClientContext();

        public static UnityContainer Container { get; set; }

        public IMessageDialogService MessageDialogService => Container.Resolve<IMessageDialogService>();

        public Freebox Freebox => Container.Resolve<Freebox>();

        public T411Client T411 => Container.Resolve<T411Client>();

        public BetaSerie BetaSerie => Container.Resolve<BetaSerie>();

        public Eztv Eztv => Container.Resolve<Eztv>();

        public static void Init()
        {
            Container = new UnityContainer();
            Container.AddNewExtension<PropertiesInjectionExtension>(); // active l'injection par propriétés pour toutes les instance résolue dans le container

            Container.RegisterType<Freebox>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(Settings.Current));


            Container.RegisterType<IMessageDialogService, MessageDialogService>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IApiConnectorService, ApiConnector>(new ContainerControlledLifetimeManager());

            if (!string.IsNullOrEmpty(Settings.Current.LoginBetaSerie) && !string.IsNullOrEmpty(Settings.Current.PwdBetaSerie))
                Container.RegisterType<BetaSerie>(new ContainerControlledLifetimeManager()
                    , new InjectionConstructor(Settings.Current.LoginBetaSerie, Settings.Current.PwdBetaSerie));

            if (!string.IsNullOrEmpty(Settings.Current.LoginT411) && !string.IsNullOrEmpty(Settings.Current.PassT411))
                Container.RegisterType<T411Client>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(Settings.Current.LoginT411, Settings.Current.PassT411));


            T411Client.BaseAddress = Settings.Current.T411Address;

            Container.RegisterType<Eztv>(new ContainerControlledLifetimeManager());

            Container.RegisterType<GuessIt>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IFormUploadService, FormUpload>(new ContainerControlledLifetimeManager());

            Container.RegisterType<ICryptographic, Cryptographic>(new ContainerControlledLifetimeManager());
        }

        public void ResetBetaserie()
        {
            Container.Teardown(BetaSerie);
        }

        public void ResetT411()
        {
            Container.Teardown(T411);
        }
    }
}
