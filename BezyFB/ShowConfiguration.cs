// Créer par : pepinat
// Le : 23-06-2014

namespace BezyFB
{
    public class ShowConfiguration
    {
        public string IdBetaSerie { get; set; }

        public string PathFreeBox { get; set; }

        public string IdEztv { get; set; }

        public string NomSerie { get; set; }

        public ShowConfiguration(string collection)
        {
            var strs = collection.Split('¤');
            IdBetaSerie = strs[0];
            if (strs.Length > 0)
            {
                PathFreeBox = strs[1];
                if (strs.Length > 1)
                {
                    IdEztv = strs[2];
                    if (strs.Length > 2)
                    {
                        NomSerie = strs[3];
                    }
                }
            }
        }

        public string GetString()
        {
            return string.Concat(IdBetaSerie, "¤", PathFreeBox, "¤", IdEztv, "¤", NomSerie);
        }
    }
}