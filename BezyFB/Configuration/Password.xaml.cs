using BezyFB.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BezyFB.Annotations;
using BezyFB.Properties;

namespace BezyFB.Configuration
{
    /// <summary>
    /// Logique d'interaction pour Password.xaml
    /// </summary>
    public sealed partial class PasswordForm : Window, INotifyPropertyChanged
    {
        private string _pwd;

        public PasswordForm()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Pwd
        {
            get { return _pwd; }
            set
            {
                _pwd = value;
                OnPropertyChanged("Pwd");
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Pwd = null;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}