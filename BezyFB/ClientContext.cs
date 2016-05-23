using BezyFB.Configuration;
using BezyFB.EzTv;
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

namespace BezyFB
{
    public class ClientContext
    {
        private static ClientContext _current;
        public static ClientContext Current { get; } = new ClientContext();

        public static UnityContainer Container { get; set; }

        public IMessageDialogService MessageDialogService => Container.Resolve<IMessageDialogService>();
        public IApiConnectorService ApiConnector => Container.Resolve<IApiConnectorService>();

        public Freebox Freebox => Container.Resolve<Freebox>();

        public T411Client T411 => Container.Resolve<T411Client>();

        public BetaSerie.BetaSerie BetaSerie => Container.Resolve<BetaSerie.BetaSerie>();

        public Eztv Eztv => Container.Resolve<Eztv>();

        public static void Init()
        {
            Container.RegisterType<Freebox>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(MySettings.Current));
            Container.RegisterType<IMessageDialogService>(new ContainerControlledLifetimeManager()
                , new InjectionFactory(c => new MessageDialogService()));
            Container.RegisterType<IApiConnectorService>(new ContainerControlledLifetimeManager()
                , new InjectionFactory(c => new Api()));
            Container.RegisterType<BetaSerie.BetaSerie>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(MySettings.Current.LoginBetaSerie, MySettings.Current.PwdBetaSerie));
            Container.RegisterType<T411Client>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(MySettings.Current.LoginT411, MySettings.Current.PassT411));
            Container.RegisterType<Eztv>(new ContainerControlledLifetimeManager());


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
