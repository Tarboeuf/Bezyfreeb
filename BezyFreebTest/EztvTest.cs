using System.Linq;
using System.Threading.Tasks;
using CommonStandardLib;
using EztvStandardLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BezyFreebTest
{
    /// <summary>
    /// Description résumée pour EztvTest
    /// </summary>
    [TestClass]
    public class EztvTest
    {
        public EztvTest()
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
        // Utilisez ClassInitialize pour exécuter du code avant d'exécuter le premier test de la classe
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
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
        public async Task GetListShowTest()
        {
            Eztv eztv = new Eztv();
            eztv.ApiConnector = new ApiConnector();
            var e = await eztv.GetListShow();
            var list = e.ToList();
            Assert.AreNotEqual(0, list.Count);
        }


        //[TestMethod]
        //public async Task GetTorrentSerieEpisodeTest()
        //{
        //    Eztv eztv = new Eztv();
        //    eztv.ApiConnector = new ApiConnector();
        //    var e = await eztv.GetTorrentSerieEpisode("42", "S01E01");

        //    Assert.AreNotEqual("", e);
        //    Assert.AreNotEqual(null, e);
        //}
    }
}
