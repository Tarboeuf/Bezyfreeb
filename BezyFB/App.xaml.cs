using System;
using BezyFB.Freebox;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;

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
                try
                {
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
                }
                catch (Exception)
                {
                    // /uri:magnet:?xt=urn:btih:26ef9ec049ac9806fb5b13dd675a0b56e288bf92&dn=Marvels+Daredevil+S01E01+WEBRip+x264-2HD+%5Beztv%5D&tr=udp%3A%2F%2Ftracker.openbittorrent.com%3A80&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969
                    if (nomFichier.Contains("/uri:magnet"))
                    {
                        FreeboxExplorer fb = new FreeboxExplorer();
                        if (fb.ShowDialog() ?? false)
                        {
                            fb.Freebox.Download(nomFichier.Substring(5), fb.FilePath + "/");
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