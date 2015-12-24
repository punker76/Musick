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
using System.Windows.Forms;
using System.IO;
using TagLib;
using Musick.Musick_Classes;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel;

namespace Musick
{
    /// <summary>
    /// Interaction logic for MusickLibrary.xaml
    /// </summary>
    public partial class MusickLibrary
    {
        public MusickLibrary()
        {           
            InitializeComponent();

            this.DataContext = this;
            dtgLibrary.ItemsSource = SongList;
        }

        
        public static ObservableCollection<Song> SongList;


        #region song selection.
        public void NextSong()
        {
            dtgLibrary.SelectedIndex++;
            Song currentObject = (Song)dtgLibrary.SelectedItem;
            var mainWindow = this.Owner as MainWindow;
            mainWindow.DoLoadSongFromLibrary(currentObject);
        }

        public void PreviousSong()
        {
            dtgLibrary.SelectedIndex--;
            Song currentObject = (Song)dtgLibrary.SelectedItem;
            var mainWindow = this.Owner as MainWindow;
            mainWindow.DoLoadSongFromLibrary(currentObject);
        }

        public void RandomSong()
        {
            Random rnd = new Random();
            int randomTrack = rnd.Next(0, dtgLibrary.Items.Count);
            dtgLibrary.SelectedIndex = randomTrack;
            Song currentObject = (Song)dtgLibrary.SelectedItem;
            var mainWindow = this.Owner as MainWindow;
            mainWindow.DoLoadSongFromLibrary(currentObject);

        }

        private void dtgLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Song currentObject = (Song)dtgLibrary.SelectedItem;
            var mainWindow = this.Owner as MainWindow;           
            mainWindow.DoLoadSongFromLibrary(currentObject);
        }
        #endregion

    }
}
