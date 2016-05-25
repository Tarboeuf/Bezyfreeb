using BezyFB_UWP.Lib.EzTv;
using BezyFB_UWP.Lib.T411;
using CommonPortableLib;
using FreeboxPortableLib;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BezyFB_UWP.Lib.BetaSerie;
using CommonLib;

namespace BezyFB_UWP.Lib
{
    public class ClientContext
    {
        private static ClientContext _current;
        public static ClientContext Current { get; } = new ClientContext();

        public static UnityContainer Container { get; set; }

        public IMessageDialogService MessageDialogService => Container.Resolve<IMessageDialogService>();

        public Freebox Freebox => Container.Resolve<Freebox>();

        public T411Client T411 => Container.Resolve<T411Client>();

        public BetaSerie.BetaSerie BetaSerie => Container.Resolve<BetaSerie.BetaSerie>();

        public Eztv Eztv => Container.Resolve<Eztv>();

        public static void Init()
        {
            Container.RegisterType<Freebox>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(Settings.Current));
            Container.RegisterType<IMessageDialogService>(new ContainerControlledLifetimeManager()
                , new InjectionFactory(c => new MessageDialogService()));
            Container.RegisterType<BetaSerie.BetaSerie>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(Settings.Current.LoginBetaSerie, Settings.Current.PwdBetaSerie));
            Container.RegisterType<T411Client>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(Settings.Current.LoginT411, Settings.Current.PassT411));
            Container.RegisterType<Eztv>(new ContainerControlledLifetimeManager());


            Container = new UnityContainer();
            Container.AddNewExtension<PropertiesInjectionExtension>(); // active l'injection par propriétés pour toutes les instance résolue dans le container

            Container.RegisterType<Freebox>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(Settings.Current));

            Container.RegisterType<IMessageDialogService, MessageDialogService>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IApiConnectorService, ApiConnector>(new ContainerControlledLifetimeManager());

            Container.RegisterType<BetaSerie.BetaSerie>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(Settings.Current.LoginBetaSerie, Settings.Current.PwdBetaSerie));

            Container.RegisterType<T411Client>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(Settings.Current.LoginT411, Settings.Current.PassT411));

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
