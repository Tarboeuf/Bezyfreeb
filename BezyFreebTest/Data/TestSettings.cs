using System;
using FreeboxPortableLib;

namespace BezyFreebTest.Data
{
    public class TestSettings : ISettingsFreebox
    {
        public string AppId { get; } = "fr.freebox.bezyfreeb";

        public string AppName { get; } = "BezyFreeb";

        public string AppVersion { get; } = "Beta";

        public string FreeboxIp { get; set; } = "à définir";
        public string PathVideo { get; set; } = "/Disque dur/Vidéos/";
        public string Hostname { get; } = Environment.MachineName;
        public string TokenFreebox { get; set; } = "à définir";
    }
}