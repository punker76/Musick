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

            if(!System.IO.File.Exists(musicLibraryFile))
            {
                FolderBrowserDialog selectFolder = new FolderBrowserDialog();
                selectFolder.Description = "Please select a root folder for your music library.";
                DialogResult result = selectFolder.ShowDialog();

                if (result.ToString() == "OK")
                {
                    DoGenerateLibrary(selectFolder.SelectedPath);
                }
            }
            else
            {
                DoLoadLibrary();
            }
        }

        
        public ObservableCollection<Song> SongList = new ObservableCollection<Song>();

        private string GetFileTags(string path)
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



        public string musicLibraryFile = System.IO.Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData), "MusicLibrary.txt");

        private void DoLoadLibrary()
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader sr = System.IO.File.OpenText(musicLibraryFile))
            using (JsonTextReader jsonTR = new JsonTextReader(sr))
            {
                SongList = serializer.Deserialize<ObservableCollection<Song>>(jsonTR);
            }
            dtgLibrary.ItemsSource = SongList;
        }

        private void DoGenerateLibrary(string selectedFolder)
        {
            foreach (var element in Directory.GetFiles(selectedFolder))
            {
                if (element.Contains(".mp3") || element.Contains(".wma"))
                {
                    var task = GetFileTags(element);
                }

            }

            foreach (var folder in Directory.GetDirectories(selectedFolder))
            {
                foreach (var file in Directory.GetFiles(folder))
                {
                    if (file.Contains(".mp3") || file.Contains(".wma"))
                    {
                        var task = GetFileTags(file);
                    }
                }
            }
            dtgLibrary.ItemsSource = SongList;

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamWriter sw = new StreamWriter(musicLibraryFile))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, SongList);
            }
        }


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
