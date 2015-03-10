using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BezyFB.Helpers;
using Newtonsoft.Json.Linq;

namespace LolStats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            const string region = "euw";
            const string server = "https://" + region + ".api.pvp.net";
            const string summonerId = "/api/lol/" + region + "/v1.4/summoner/by-name/";
            const string appKey = "?api_key=81dc1e31-5ca6-429e-a6b7-a0c5ba68a914";
            var stringSummoner = ApiConnector.Call(server + summonerId + tbName.Text + appKey, WebMethod.Get);
            try
            {
                var jsonSummoner = JObject.Parse(stringSummoner);
                try
                {
                    tbSuID.Text = jsonSummoner[tbName.Text.ToLower()]["id"].ToString();
                }
                catch (Exception es)
                {
                    MessageBox.Show("stringSummoner  : " + stringSummoner + "\nexception : " + es.Message);
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("stringSummoner  : " + stringSummoner);
                return;
            }

            //récupération de la liste des champions
            const string champions = "/api/lol/static-data/" + region + "/v1.2/champion";
            Dictionary<int, string> dicoChampions = new Dictionary<int, string>();
            var sC = ApiConnector.Call(server + champions + appKey, WebMethod.Get);
            var jC = JObject.Parse(sC);
            foreach (var item in jC["data"])
            {
                var first = item.First();
                dicoChampions.Add((int)first["id"], (string)first["name"]);
            }


            const string matchHistory = "/api/lol/" + region + "/v2.2/matchhistory/";

            var sMH = ApiConnector.Call(server + matchHistory + tbSuID.Text + appKey, WebMethod.Get);
            var matches = JObject.Parse(sMH)["matches"] as JArray;

            foreach (var item in matches)
            {
                var matchCreation = (long)item["matchCreation"];
                var dt = new DateTime(1970, 1, 1, 0, 0, 0);
                dt = dt.AddSeconds(matchCreation/1000);
                Console.WriteLine(dt.ToString());


                var participantIds = item["participantIdentities"] as JArray;
                int participantId = -1;
                foreach (var participant in participantIds)
                {
                    if ((string)participant["player"]["summonerName"] == tbName.Text)
                    {
                        participantId = (int)participant["participantId"];
                    }
                }
                if (-1 != participantId)
                {
                    var participants = item["participants"] as JArray;
                    foreach (var participant in participants)
                    {
                        if (participantId == (int)participant["participantId"])
                        {

                            Console.WriteLine(dicoChampions[(int)participant["championId"]]);
                            break;
                        }
                    }
                }
                Console.WriteLine("___________________________________");
            }
        }
    }
}
