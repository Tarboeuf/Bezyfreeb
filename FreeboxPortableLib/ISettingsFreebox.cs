namespace FreeboxPortableLib
{
    public interface ISettingsFreebox
    {
        string AppId { get;  }
        string AppName { get;  }
        string AppVersion { get;  }
        string FreeboxIp { get; set; }
        string PathVideo { get; set; }
        string PathFilm { get; set; }
        string Hostname { get; }
        string TokenFreebox { get; set; }
    }
}
