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

        public ObservableCollection<Song> SongList = new ObservableCollection<Song>();

        private async Task<string> GetFileTags(string path)
        {            
            string tempYear;
            var tempFile = TagLib.File.Create(path);
            if (tempFile.Tag.Year == 0)
            {
                tempYear = "";
            }
            else
            {
                tempYear = tempFile.Tag.Year.ToString();
            }
            SongList.Add(new Song(path.ToString(), tempFile.Tag.Title, tempFile.Tag.FirstPerformer, tempFile.Tag.Album, tempFile.Tag.FirstGenre, tempYear));
            return "Tag Loaded";
        }

        private async void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog selectFolder = new FolderBrowserDialog();
            DialogResult result = selectFolder.ShowDialog();

            if(result.ToString() == "OK")
            {   
                foreach(var element in Directory.GetFiles(selectFolder.SelectedPath))
                {
                    if (element.Contains(".mp3"))
                    {
                        var task = await GetFileTags(element);
                    }
                    
                }
                foreach (var folder in Directory.GetDirectories(selectFolder.SelectedPath))
                {
                    foreach (var file in Directory.GetFiles(folder))
                    {
                        if (file.Contains(".mp3"))
                        {
                            var task = await GetFileTags(file);
                        }
                    }
                }
            }
        }

        public void NextSong()
        {
            dtgLibrary.SelectedIndex++;
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
    }
}
