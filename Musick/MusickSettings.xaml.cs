﻿using System;
using System.Collections.Generic;
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
using MahApps.Metro;

namespace Musick
{
    /// <summary>
    /// Interaction logic for MusickSettings.xaml
    /// </summary>
    public partial class MusickSettings
    {
        public MusickSettings()
        {
            InitializeComponent();
            this.DataContext = this;
            cboAccentList.ItemsSource = accentList;
            cboThemeList.ItemsSource = themeList;
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            cboAccentList.SelectedItem = theme.Item2.Name;
            cboThemeList.SelectedItem = theme.Item1.Name;
        }

        private void cboAccentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get the theme from the window
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            // Set the accent and save it to user settings.
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(cboAccentList.SelectedItem.ToString()), theme.Item1);
            
        }



        private void cboThemeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get the theme from the window
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            // Set the accent and save it to user settings.
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2 , ThemeManager.GetAppTheme(cboThemeList.SelectedItem.ToString()));
        }

        #region Lists
        public List<string> accentList = new List<string>()
            {
            "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan",
            "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber",
            "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna"
            };
        public List<string> themeList = new List<string>()
        {
            "BaseDark","BaseLight"
        };
        #endregion
    }
}
