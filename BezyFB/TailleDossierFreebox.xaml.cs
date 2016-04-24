using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using BezyFB.Freebox;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour TailleDossierFreebox.xaml
    /// </summary>
    public partial class TailleDossierFreebox : Window
    {
        private readonly Freebox.Freebox _freebox;

        public TailleDossierFreebox(Freebox.Freebox freebox)
        {
            _freebox = freebox;
            InitializeComponent();

            Fichiers = new ObservableCollection<Fichier>();

            Charger("/", Fichiers, null);
            DataContext = this;
        }

        private void Charger(string directory, ObservableCollection<Fichier> fichiers, Fichier parent)
        {
            try
            {
                var files = _freebox.LsFileInfo(directory);

                if (files == null) return;
                foreach (var file in files)
                {
                    var fichier = new Fichier(parent)
                    {
                        Nom = file.Name,
                        Taille = long.Parse(file.Size),
                        IsDossier = file.Type == "dir"
                    };
                    fichiers.Add(fichier);
                    if (file.Type == "dir")
                    {
                        Charger(directory + "/" + file.Name, fichier.Fichiers, fichier);
                    }
                    var view = CollectionViewSource.GetDefaultView(fichiers);
                    view.SortDescriptions.Add(new SortDescription("TailleTotal", ListSortDirection.Ascending));
                }
                
            }
            catch (Exception)
            {
            }
        }
        public ObservableCollection<Fichier> Fichiers { get; set; }
    }

    public class Fichier
    {
        private readonly Fichier _parent;

        public Fichier(Fichier parent)
        {
            _parent = parent;
            Fichiers = new ObservableCollection<Fichier>();
        }

        public string Nom { get; set; }
        public long Taille { get; set; }

        public long TailleTotal
        {
            get
            {
                if (IsDossier)
                    return Fichiers.Select(f => f.TailleTotal).Sum();
                return Taille / (1024 * 1024);
            }
        }

        public double Pourcentage
        {
            get
            {
                if (_parent == null)
                    return 1d;
                return TailleTotal / (double)_parent.TailleTotal;
            }
        }

        public bool IsDossier { get; set; }

        public ObservableCollection<Fichier> Fichiers { get; set; }

    }
}
