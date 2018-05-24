using System;
using FreeboxStandardLib;

namespace BezyFreebTest.Data
{
    public class TestSettings : ISettingsFreebox
    {
        public string AppId { get; } = "fr.freebox.bezyfreeb";

        public string AppName { get; } = "BezyFreeb";

        public string AppVersion { get; } = "Beta";

        public string FreeboxIp { get; set; } = "192.168.2.254";
        public string PathVideo { get; set; } = "/Disque dur/Vidéos";
        public string PathFilm { get; set; } = "/Disque dur/Vidéos/__FILM";
        public string Hostname { get; } = Environment.MachineName;
        public string TokenFreebox { get; set; } = "Pq3ZVNifoGF/ePYD6ZPHQaF1O+EL8S9Ujv3HOhLuHY8XgzgIp3RzatPXvPm5mH0h";
    }
}