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
using System.Threading;
using MahApps.Metro.Controls.Dialogs;

namespace Musick
{
    /// <summary>
    /// Interaction logic for MusickWelcome.xaml
    /// </summary>
    public partial class MusickWelcome
    {
        public MusickWelcome()
        {
            // Creates all the directories on load to save all the effort - if they already exist then it doesn't bother.
            Directory.CreateDirectory(ConfigClass.appDataFolder);
            Directory.CreateDirectory(ConfigClass.appLibraryFolder);
            Directory.CreateDirectory(ConfigClass.appSettingsFolder);
            InitializeComponent();
            DoGetSettings();
        }


        static ObservableCollection<Song> tempSongList = new ObservableCollection<Song>();
        string selectedFolder;
        string libraryName;
        string settingsFile = System.IO.Path.Combine(ConfigClass.appSettingsFolder, "Settings.txt");

        private async void MusickWelcome_Loaded(object sender, RoutedEventArgs e)
        {
            lblStatus.Content = "Loading User Settings...";                 
            await Task.Delay(800);          
            lblStatus.Content = "Settings loaded...";
            await Task.Delay(782);
            lblStatus.Content = "Checking for library...";
            await Task.Delay(950);
            await DoLibraryCheck();
            lblStatus.Content = "Loading Musick...";
            await Task.Delay(893);
            MainWindow newWin = new MainWindow();
            newWin.Show();           
            this.Close();             
        }

        #region Settings
        // Checks for existing settings file, if none exists generate a new one using the defaults.
        private void DoGetSettings()
        {                   
            bool isEmpty = !Directory.EnumerateFiles(ConfigClass.appSettingsFolder).Any(); // Check if file exists

            // If file doesn't exist, generate one from the DefaultSettings class.
            if (isEmpty)
            {
                UserSettings tempSettings = new UserSettings();
                tempSettings = DefaultSettings.set();
                MainWindow.currentSettings = tempSettings;

                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                using (StreamWriter sw = new StreamWriter(settingsFile))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, tempSettings);
                }

            }

            // If file does exist, deserialise the file and load the object up into the "currentSettings" object.
            else
            {
                UserSettings tempSettings = new UserSettings();
                string tempSettingsFile = System.IO.Path.Combine(ConfigClass.appSettingsFolder, "Settings.txt");
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader sr = System.IO.File.OpenText(settingsFile))
                using (JsonTextReader jsonTR = new JsonTextReader(sr))
                {
                    tempSettings = serializer.Deserialize<UserSettings>(jsonTR);
                }
                MainWindow.currentSettings = tempSettings;
            }        
        }
        #endregion


        #region Library check
        // Checks for an existing library, if none exists create folders and call library gen method, if one does exist then it loads it up.
        private async Task<string> DoLibraryCheck()
        {

            bool isEmpty = !Directory.EnumerateFiles(ConfigClass.appLibraryFolder).Any(); // Checks if there are any libraries currently stored locally

            // If no local library exists, generate a new one via user input.
            if (isEmpty)
            {
                lblStatus.Content = "Library not found - Please select a root folder for your music.";
                MusickInputLibraryLocation folderSelectDialog = new MusickInputLibraryLocation();
                if (folderSelectDialog.ShowDialog() == true)
                {
                    selectedFolder = folderSelectDialog.lblSelectedFolder.Content.ToString();
                    folderSelectDialog.Close();
                    MusickInputLibraryName libraryNameDialog = new MusickInputLibraryName();

                    // Show testDialog as a modal dialog and determine if true.
                    if (libraryNameDialog.ShowDialog() == true)
                    {                       
                        this.libraryName = libraryNameDialog.txtLibraryName.Text + ".txt"; // Read the contents of testDialog's TextBox.
                    }                  
                    lblStatus.Content = "Generating library from selected folder...";                                 
                    await DoGenerateLibrary(selectedFolder);
                    return "Library not found - Generating from path...";
                }

                // If user somehow manages to dodge the dialog, this error will show (highly unlikely, but whatever)
                else
                {
                    System.Windows.MessageBox.Show("You must select a library location!");
                    this.Close();
                    return "No Folder selected.";                   
                }
            }

            // If one exists, load the library.
            else
            {
                lblStatus.Content = "Library found - Loading Library...";
                await Task.Delay(800);
                await DoLoadLibrary();
                return "Library Found...";
            }
        }
        #endregion


        #region Generate library
        // Generate a new library file using the selected directory.
        private async Task<string> DoGenerateLibrary(string selectedFolder)
        {
            await Task.Run(() =>
            {
                ObservableCollection<Song> tempLibrary = GenerateLibrary.Create(selectedFolder);

                MusickLibrary.SongList = tempLibrary;
                string tempMusicLibraryFile = System.IO.Path.Combine(ConfigClass.appLibraryFolder, libraryName);
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                using (StreamWriter sw = new StreamWriter(tempMusicLibraryFile))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, tempLibrary);
                }
                string tempSource = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(tempSongList[0].FileLocation));
                string tempLibName = System.IO.Path.GetFileNameWithoutExtension(tempMusicLibraryFile);
                string tempFileLoc = tempMusicLibraryFile;
                MusickSettings.libList.Add(new LibraryFile(tempFileLoc, tempLibName, tempSource));
            }
            );
            lblStatus.Content = "Library Generated...";
            return "Library Generated!";
        }
        #endregion


        #region Load library
        // If a local library (or libraries) exists, this will deserialise the file(s) into the songList object for the Library window to use. 
        private async Task<string> DoLoadLibrary()
        {
            await Task.Run(() => 
            {
                CancellationTokenSource cts = new CancellationTokenSource();

                JsonSerializer serializer = new JsonSerializer();
                foreach (var file in Directory.GetFiles(ConfigClass.appLibraryFolder))
                {
                    ObservableCollection<Song> tempLibrary = new ObservableCollection<Song>();
                    using (StreamReader sr = System.IO.File.OpenText(file))
                    using (JsonTextReader jsonTR = new JsonTextReader(sr))
                    {
                        tempLibrary = serializer.Deserialize<ObservableCollection<Song>>(jsonTR);
                    }
                    try
                    {
                        foreach (var song in tempLibrary)
                        {
                            tempSongList.Add(song);
                        }
                        string tempSource = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(tempSongList[0].FileLocation));
                        string tempLibName = System.IO.Path.GetFileNameWithoutExtension(file);
                        string tempFileLoc = file;
                        MusickSettings.libList.Add(new LibraryFile(tempFileLoc, tempLibName, tempSource));
                    }
                    catch
                    {                      
                        cts.Cancel();
                        Dispatcher.Invoke(() =>
                        {
                            MusickError errorWin = new MusickError();
                            errorWin.Owner = this;
                            errorWin.lblError.Content = "One or more libraries are corrupt/missing - Restart to generate a new library";
                            if (errorWin.ShowDialog() == true)
                            {
                                foreach (var libFile in Directory.GetFiles(ConfigClass.appLibraryFolder))
                                {
                                    File.Delete(libFile);
                                }
                                this.Close();
                            }                         
                        }
                        );
                    }
                }
            }
            );
            MusickLibrary.SongList = tempSongList;
            lblStatus.Content = "Library loaded...";           
            await Task.Delay(729);
            return "Library Loaded!";
        }
        private void LibCorruptedError()
        {

        }
        #endregion
    }
}
