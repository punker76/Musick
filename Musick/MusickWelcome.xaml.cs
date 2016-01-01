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
            // Set the theme to the value stored in currentSettings
            ThemeManager.ChangeAppStyle(System.Windows.Application.Current, ThemeManager.GetAccent(MainWindow.currentSettings.accent), ThemeManager.GetAppTheme(MainWindow.currentSettings.theme));
        }


        static ObservableCollection<Song> tempSongList = new ObservableCollection<Song>();
        string selectedFolder;
        string libraryName;

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
            bool isEmpty = !File.Exists(ConfigClass.settingsFile); // Check if file exists

            // If file doesn't exist, generate one from the DefaultSettings class.
            if (isEmpty)
            {
                UserSettings tempSettings = new UserSettings();
                tempSettings = DefaultSettings.set();
                MainWindow.currentSettings = tempSettings;
                JSON.SerializeSettings(tempSettings);
            }

            // If file does exist, deserialise the file and load the object up into the "currentSettings" object - If the object is unusable, prompts the user and creates settings from default.
            else
            {
                try
                {
                    UserSettings tempSettings = JSON.DeserializeSettings();
                    MainWindow.currentSettings = tempSettings;
                }
                catch
                {
                    MusickError errorWin = new MusickError();
                    errorWin.Owner = this;
                    errorWin.lblError.Content = "Settings file corrupted - Using default settings";
                    if (errorWin.ShowDialog() == true)
                    {
                        UserSettings tempSettings = new UserSettings();
                        tempSettings = DefaultSettings.set();
                        MainWindow.currentSettings = tempSettings;
                        JSON.SerializeSettings(tempSettings);
                    }
                }
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
                folderSelectDialog.Owner = this;
                if (folderSelectDialog.ShowDialog() == true)
                {
                    selectedFolder = folderSelectDialog.lblSelectedFolder.Content.ToString();
                    folderSelectDialog.Close();
                    MusickInputLibraryName libraryNameDialog = new MusickInputLibraryName();
                    libraryNameDialog.Owner = this;
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

                JSON.SerializeLibrary(tempMusicLibraryFile, tempLibrary);

                MusickSettings.libList.Add(GenerateLibrary.CreateLibraryEntry(tempLibrary,tempMusicLibraryFile)); // Creates an entry for the local library file to be displayed and interracted with.
            }
            );
            lblStatus.Content = "Library Generated...";
            return "";
        }
        #endregion


        #region Load library
        // If a local library (or libraries) exists, this will deserialise the file(s) into the songList object for the Library window to use. 
        private async Task<string> DoLoadLibrary()
        {
            await Task.Run(() => 
            {               
                foreach (var file in Directory.GetFiles(ConfigClass.appLibraryFolder))
                {
                    ObservableCollection<Song> tempLibrary = JSON.DeserializeLibrary(file);
                    LibraryFile tempLibFile = GenerateLibrary.CreateLibraryEntry(tempLibrary, file);
                    try
                    {
                        foreach (var song in tempLibrary.ToList())
                        {
                            if (!File.Exists(song.FileLocation))
                            {
                                tempLibrary.Remove(song);
                            } 
                        }

                        foreach (var musicFile in Directory.GetFiles(tempLibFile.LibrarySource, "*", SearchOption.AllDirectories))
                        {
                            if (!tempLibrary.Any(p => p.FileLocation == musicFile))
                            {
                                tempLibrary.Add(GenerateLibrary.GenerateSong(musicFile));
                            }
                        }

                        JSON.SerializeLibrary(file, tempLibrary);

                        foreach (var tempSong in tempLibrary)
                        {
                            tempSongList.Add(tempSong);
                        }

                        MusickSettings.libList.Add(tempLibFile); // Creates an entry for the local library file to be displayed and interracted with.
                    }


                    catch // If the object returned is unusable, it'll throw this error.
                    {                                              
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
            return "";
        }
        #endregion
    }
}
