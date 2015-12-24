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
using Musick.Musick_Classes;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace Musick
{
    /// <summary>
    /// Interaction logic for MusickWelcome.xaml
    /// </summary>
    public partial class MusickWelcome
    {
        public MusickWelcome()
        {
            InitializeComponent();
        }

        string musicLibraryFile = System.IO.Path.Combine(ConfigClass.appDataFolder, "MusicLibrary.txt");
        ObservableCollection<Song> tempSongList = new ObservableCollection<Song>();

        private async void MusickWelcome_Loaded(object sender, RoutedEventArgs e)
        {
            lblStatus.Content = "Checking for library...";
            await Task.Delay(2000);
            await DoLibraryCheck();
            lblStatus.Content = "Loading Musick...";
            await Task.Delay(1000);
            MainWindow newWin = new MainWindow();
            newWin.Show();
            this.Close();             
        }

        private async Task<string> DoLibraryCheck()
        {
            if (!System.IO.File.Exists(musicLibraryFile))
            {
                lblStatus.Content = "Library not found - Please select a root folder for your music.";
                Directory.CreateDirectory(ConfigClass.appDataFolder);
                FolderBrowserDialog selectFolder = new FolderBrowserDialog();
                selectFolder.Description = "Library not found - Please select a root folder for your music.";
                DialogResult result = selectFolder.ShowDialog();

                if (result.ToString() == "OK")
                {
                    lblStatus.Content = "Generating library from selected folder...";
                    await Task.Delay(1500);                 
                    await DoGenerateLibrary(selectFolder.SelectedPath);
                    return "Library not found - Generating from path...";
                }
                else
                {
                    System.Windows.MessageBox.Show("You must select a library location!");
                    this.Close();
                    return "No Folder selected.";                   
                }
            }
            else
            {
                lblStatus.Content = "Library found - Loading Library...";
                await Task.Delay(1500);
                await DoLoadLibrary();
                return "Library not found - Generating from path...";
            }
        }

        // Generate a new library file using the selected directory.
        private async Task<string> DoGenerateLibrary(string selectedFolder)
        {
            await Task.Run(() =>
            {
                foreach (var file in Directory.GetFiles(selectedFolder, "*", SearchOption.AllDirectories))
                {
                    if (file.Contains(".mp3") || file.Contains(".wma") || file.Contains(".wav") || file.Contains(".ogg"))
                    {
                        string tempYear;
                        var tempFile = TagLib.File.Create(file);
                        if (tempFile.Tag.Year == 0)
                        {
                            tempYear = "";
                        }
                        else
                        {
                            tempYear = tempFile.Tag.Year.ToString();
                        }
                        tempSongList.Add(new Song(file.ToString(), tempFile.Tag.Title, tempFile.Tag.FirstPerformer, tempFile.Tag.Album, tempFile.Tag.FirstGenre, tempYear));
                    }
                }

                MusickLibrary.SongList = tempSongList;

                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                using (StreamWriter sw = new StreamWriter(musicLibraryFile))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, tempSongList);
                }
            }
            );
            lblStatus.Content = "Library Generated";
            return "Library Generated!";
        }

        // Loads an existing library file (if one exists)
        private async Task<string> DoLoadLibrary()
        {
            await Task.Run(() =>
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader sr = System.IO.File.OpenText(musicLibraryFile))
                using (JsonTextReader jsonTR = new JsonTextReader(sr))
                {
                    tempSongList = serializer.Deserialize<ObservableCollection<Song>>(jsonTR);
                }
                MusickLibrary.SongList = tempSongList;
            }
            );
            lblStatus.Content = "Library loaded.";
            await Task.Delay(1000);
            return "Library Loaded!";
        }

    }
}
