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

        public Song currentSong;

        #region song selection.
        public void NextSong()
        {
            dtgLibrary.SelectedIndex++;
            currentSong = (Song)dtgLibrary.SelectedItem;
        }

        public void PreviousSong()
        {
            if(dtgLibrary.SelectedIndex==0)
            {               
                dtgLibrary.SelectedIndex = dtgLibrary.Items.Count -1;
                currentSong = (Song)dtgLibrary.SelectedItem;
            }
            else
            {
                dtgLibrary.SelectedIndex--;
                currentSong = (Song)dtgLibrary.SelectedItem;
            }
            
        }

        public void RandomSong()
        {
            Random rnd = new Random();
            int randomTrack = rnd.Next(0, dtgLibrary.Items.Count);
            dtgLibrary.SelectedIndex = randomTrack;
            currentSong = (Song)dtgLibrary.SelectedItem;            
        }

        public Song getSong()
        {           
            return currentSong;
        }

        public event EventHandler songSelected;
        public void dtgLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            currentSong = (Song)dtgLibrary.SelectedItem;
            if (songSelected != null)
                songSelected(this, null);
        }
        #endregion


        #region Window Hotkeys.
        private void LibraryWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.H && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                this.ShowTitleBar = !this.ShowTitleBar;
            }
        }
        #endregion

    }
}
