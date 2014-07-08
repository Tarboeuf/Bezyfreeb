﻿using System;
using System.Collections.Generic;
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
using BezyFB.EzTv;
using BezyFB.Properties;

namespace BezyFB.Configuration
{
    /// <summary>
    /// Logique d'interaction pour WindowShow.xaml
    /// </summary>
    public partial class WindowShow : Window
    {
        public WindowShow()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(WindowShow_DataContextChanged);
        }

        private void WindowShow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(Show.IdEztv))
            {
                Eztv ez = new Eztv();
                var liste = ez.GetListShow().ToList();
                comboSeries.ItemsSource = liste;

                Show.IdEztv = liste.Where(c => Show.ShowName.ToLower() == c.Name.ToLower()).Select(c => c.Name).FirstOrDefault();
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void LoadSerieEZTV(object sender, RoutedEventArgs e)
        {
            Eztv ez = new Eztv();
            comboSeries.ItemsSource = ez.GetListShow();
        }

        private void ComboSeries_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var addedItem in e.AddedItems.OfType<Eztv.Show>())
            {
                Show.IdEztv = addedItem.Id;
            }
        }

        private ShowConfiguration Show
        {
            get { return DataContext as ShowConfiguration; }
        }
    }
}