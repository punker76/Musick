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
            dtgLibrary.ItemsSource = null;
            lstAlbum.ItemsSource = null;
            lstArtist.ItemsSource = null;
        }

        public static ObservableCollection<Song> SongList;

        public Song currentSong;

        #region Window data binding logic
        private void LibraryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            lstArtist.ItemsSource = SongList.Select(x => x.SongArtist).Distinct().ToList();          
        }

        private void lstArtist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dtgLibrary.ItemsSource = null;
            
            if(lstArtist.SelectedIndex != -1)
            {
                if(tglAllAlbums.IsChecked == true)
                {
                    lstAlbum.ItemsSource = SongList.Where(x => x.SongArtist == lstArtist.SelectedItem.ToString()).Select(x => x.SongAlbum).Distinct().ToList();
                    dtgLibrary.ItemsSource = SongList.Where(song => lstAlbum.Items.Contains(song.SongAlbum)).Select(song => song).Distinct().ToList();
                }
                else
                {
                    lstAlbum.ItemsSource = SongList.Where(x => x.SongArtist == lstArtist.SelectedItem.ToString()).Select(x => x.SongAlbum).Distinct().ToList();
                    lstAlbum.SelectedIndex = 0;
                }                     
            }        
        }

        private void lstAlbum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {         
            if (lstAlbum.SelectedIndex != -1)
            {               
                dtgLibrary.ItemsSource = SongList.Where(x => x.SongAlbum == lstAlbum.SelectedItem.ToString()).Select(x => x).ToList();            
            }
        }

        private void tglAllArtists_Checked(object sender, RoutedEventArgs e)
        {
            lstArtist.SelectedIndex = -1;
            lstArtist.IsEnabled = false;
            lstAlbum.ItemsSource = SongList.Select(x => x.SongAlbum).Distinct().ToList();
            if (tglAllAlbums.IsChecked == true)
            {
                dtgLibrary.ItemsSource = SongList.Where(song => lstAlbum.Items.Contains(song.SongAlbum)).Select(song => song).Distinct().ToList();
            }
        }

        private void tglAllArtists_Unchecked(object sender, RoutedEventArgs e)
        {
            lstArtist.IsEnabled = true;
            lstArtist.SelectedIndex = 0;
        }

        private void tglAllAlbums_Checked(object sender, RoutedEventArgs e)
        {
            lstAlbum.SelectedIndex = -1;
            lstAlbum.IsEnabled = false;
            dtgLibrary.ItemsSource = SongList.Where(song => lstAlbum.Items.Contains(song.SongAlbum)).Select(song => song).Distinct().ToList();
        }

        private void tglAllAlbums_Unchecked(object sender, RoutedEventArgs e)
        {
            lstAlbum.IsEnabled = true;
            lstAlbum.SelectedIndex = 0;
        }
        #endregion


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
