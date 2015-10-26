using BezyFB.Freebox;
using System.IO;
using System.Windows;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        public App()
        {
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                string nomFichier = e.Args[0];
                FileInfo fi = new FileInfo(nomFichier);
                if (fi.Exists)
                {
                    //File.WriteAllText(@"E:\azeqsd.txt", File.ReadAllText(fi.FullName, Encoding.Default), Encoding.Default);
                    if (fi.Extension == ".torrent")
                    {
                        FreeboxExplorer fb = new FreeboxExplorer();
                        if (fb.ShowDialog() ?? false)
                        {
                            fb.Freebox.DownloadFile(fi, fb.FilePath + "/", false);
                        }
                    }
                }

                Shutdown();
            }
            else
            {
                var mw = new BezyFB.MainWindow();
                mw.ShowDialog();
            }
        }
    }
}