using System;
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
using System.IO;
using Musick.Musick_Classes;
using System.Collections.ObjectModel;

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
            lstLibraries.ItemsSource = libList;
            cboAccentList.ItemsSource = accentList;
            cboThemeList.ItemsSource = themeList;

        }

        public ObservableCollection<LibraryFile> libList = new ObservableCollection<LibraryFile>();

        private void cboAccentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get the theme from the window
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            // Set the accent and save it to user settings.
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(cboAccentList.SelectedItem.ToString()), theme.Item1);
            MainWindow.currentSettings.accent = cboAccentList.SelectedItem.ToString();           
        }

        private void cboThemeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get the theme from the window
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            // Set the theme and save to user settings.
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2 , ThemeManager.GetAppTheme(cboThemeList.SelectedItem.ToString()));
            MainWindow.currentSettings.theme = cboThemeList.SelectedItem.ToString();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            cboAccentList.SelectedItem = MainWindow.currentSettings.accent;
            cboThemeList.SelectedItem = MainWindow.currentSettings.theme;

            foreach (var libraryFile in Directory.GetFiles(ConfigClass.appLibraryFolder))
            {
                string libraryName = System.IO.Path.GetFileNameWithoutExtension(libraryFile);
                libList.Add(new LibraryFile(libraryFile, libraryName));
            }
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

        private void lstLibraries_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete)
            {
                if (lstLibraries.SelectedIndex != -1)
                {
                    var tempSelected = lstLibraries.SelectedIndex;
                    File.Delete(libList[tempSelected].libraryFile);
                    libList.RemoveAt(tempSelected);
                }
                    
            }
        }
    }
}
