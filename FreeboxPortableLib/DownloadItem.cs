using System.Text.RegularExpressions;

namespace FreeboxPortableLib
{
    public class DownloadItem
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public double Pourcentage { get; set; }
        public double RxPourcentage { get; set; }
        public int Id { get; set; }
    }
}