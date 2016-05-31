using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BezyFB;
using BezyFB.Helpers;
using BezyFreebTest.Data;
using CommonPortableLib;
using FreeboxPortableLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace BezyFreebTest
{
    /// <summary>
    /// Description résumée pour UnitTest1
    /// </summary>
    [TestClass]
    public class FreeboxFixture
    {
        public FreeboxFixture()
        {
            //
            // TODO: ajoutez ici la logique du constructeur
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Obtient ou définit le contexte de test qui fournit
        ///des informations sur la série de tests active, ainsi que ses fonctionnalités.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Attributs de tests supplémentaires
        //
        // Vous pouvez utiliser les attributs supplémentaires suivants lorsque vous écrivez vos tests :
        //
        //Utilisez ClassInitialize pour exécuter du code avant d'exécuter le premier test de la classe
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            ClientContext.InitForTest(new TestSettings());
            ClientContext.Register<IMessageDialogService, TestMessageDialog>();
        }
        //
        // Utilisez ClassCleanup pour exécuter du code une fois que tous les tests d'une classe ont été exécutés
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Utilisez TestInitialize pour exécuter du code avant d'exécuter chaque test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Utilisez TestCleanup pour exécuter du code après que chaque test a été exécuté
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public async Task UploadFileTest()
        {
            var contenu = new[] { (byte)12, (byte)12, (byte)12 };
            string testFilePath = @"C:\test.test";
            using (var file = File.Create(testFilePath))
            {
                file.Write(contenu, 0, 3);
            }
            
            var task = await ClientContext.Current.Freebox.UploadFile(testFilePath, "test", "test.test");
            Console.WriteLine(task);
            File.Delete(testFilePath);
        }


        [TestMethod]
        public async Task ApiConnectorCallUploadFileTest()
        {
            ApiConnector connector = new ApiConnector();
            Cryptographic crypto = new Cryptographic();
            
            var json = await connector.Call("http://" + "78.217.179.93" + "/api/v2/upload/", WebMethod.Post, "application/json",
                                         new JObject
                                         {
                                             { "dirname", crypto.EncodeTo64("/Disque dur/Vidéos/") },
                                         }.ToString(), null,
                                         new List<Tuple<string, string>> { new Tuple<string, string>("X-Fbx-App-Auth", "Ru5xflEAX9jexUJtm/mVDVfXuu1Hm9WXrrR4jEexmptXGcpPhB0QNs5FJsjWYZXD") });
        }
    }
}
