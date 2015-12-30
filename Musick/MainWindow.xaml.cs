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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;
using TagLib;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Drawing;
using MColor = System.Windows.Media.Color;
using DColor = System.Drawing.Color;
using System.ComponentModel;
using Musick.Musick_Classes;
using System.Windows.Interop;
using Newtonsoft.Json;

namespace Musick
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Variables

        public static MediaPlayer mediaPlayer = new MediaPlayer();
        private bool mediaPlayerIsPlaying = false;
        public BitmapSource noAlbumArt;
        public static UserSettings currentSettings = new UserSettings();
        private LowLevelKeyboardListener _listener;

        #endregion


        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            DoUseSettings();
            
            // Converts the "no album art" placeholder into a useable format
            var image = Properties.Resources.NoAlbumArt;
            var bitmap = new System.Drawing.Bitmap(image);
            noAlbumArt = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmap.Dispose();
            
            // Sets the default image to the "no album art" placeholder
            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = noAlbumArt;
            imgBrush.Opacity = 0.4;
            MainWindowGrid.Background = imgBrush;

            // Control visibility @ runtime.
            volumeBar.Visibility = Visibility.Hidden;
            AudioControlGrid.Opacity = 0; AudioControlGrid.Visibility = Visibility.Hidden;
            playerMenu.Opacity = 0; playerMenu.Visibility = Visibility.Hidden;

            // AutoHide controls timer (Maybe user setting?)
            controlHideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.0) };
            controlHideTimer.Tick += timer_Tick;

            // Timer for the timing of media playing.
            mediaTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
            mediaTimer.Tick += mediaTimer_Tick;
            mediaTimer.Start();

            // Subscribing to the doubleclick event in the library and firing the doubleclick event here.
            MusickLibrary objectToSubscribeTo = Library;
            objectToSubscribeTo.songSelected += songDoubleClicked;
        }


        #region Load up settings
        // Load up those settings boys.
        private void DoUseSettings()
        {        
            // Load up player window settings
            this.Left = currentSettings.playerLeft; this.Top = currentSettings.playerTop; // Window position
            volumeBar.Value = currentSettings.volumeValue; mediaPlayer.Volume = currentSettings.volumeValue; // Volume values
            shuffleButtonVisual.IsEnabled = currentSettings.shuffleEnabled; shuffleIsEnabled = currentSettings.shuffleEnabled; // Shuffle toggle
            

            // Load up library window settings
            Library.Left = currentSettings.libraryLeft; Library.Top = currentSettings.libraryTop; Library.Width = currentSettings.libraryWidth; Library.Height = currentSettings.libraryHeight;         
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;

            _listener.HookKeyboard();
        }

        #endregion


        #region Actions

        // Event fires when a song in the library is doubleclicked.
        public void songDoubleClicked(object sender, EventArgs e)
        {
            Song tempSong = Library.getSong();
            DoLoadSongFromLibrary(tempSong);
        }

        // Grabs a song from the library (currently selected) and loads it up into the mediaPlayer - then it sets album artwork and window title before playing it.
        public void DoLoadSongFromLibrary(Song song)
        {
            mediaPlayer.Open(new Uri(song.FileLocation));
            mediaPlayerIsPlaying = false;
            DoSetAlbumArt(song);
            DoSetWindowTitle(song);
            DoPlaySong();
        }

        // Sets the window title to the current song name & artist.
        public void DoSetWindowTitle(Song song)
        {
            this.Title = song.SongTitle+ " - " +song.SongArtist;
        }
        
        // Plays previous track - Add method for playing from previous played list when shuffle is used.
        public void DoPreviousTrack()
        {
            Library.PreviousSong();
            Song tempPrevSong = Library.getSong();
            DoLoadSongFromLibrary(tempPrevSong);
        }

        // Plays the next track, if shuffle is enabled then it'll play a random track.
        public void DoNextTrack()
        {
            if (shuffleIsEnabled == true)
            {
                Library.RandomSong();
                Song tempRandSong = Library.getSong();
                DoLoadSongFromLibrary(tempRandSong);
            }
            else if (shuffleIsEnabled == false)
            {
                Library.NextSong();
                Song tempNextSong = Library.getSong();
                DoLoadSongFromLibrary(tempNextSong);
            }
        }

        // Plays the track if there is a track loaded and 
        public void DoPlayTrack()
        {
            if (mediaPlayer.Source != null && mediaPlayerIsPlaying == false)
            {
                DoPlaySong();
            }

            else if (mediaPlayerIsPlaying == true)
            {
                mediaPlayer.Pause();
                mediaPlayerIsPlaying = false;
                playButtonVisual.IsEnabled = true;
            }
        }

        // Plays the song.
        public void DoPlaySong()
        {
            if (mediaPlayer.Source != null && mediaPlayerIsPlaying == false)
            {
                mediaPlayer.Play();
                mediaPlayerIsPlaying = true;
                playButtonVisual.IsEnabled = false;
                sliProgress.IsEnabled = true;
            }
        }
         
        #endregion


        #region UI

            #region control Animations
        
        private bool isUsingControls; // Toggle for checking if the user is currently using the controls
        private DispatcherTimer controlHideTimer; // timer for the hiding of controls

        // When the timer hits the limit, fires this event and fades the controls out
        void timer_Tick(object sender, EventArgs e)
        {
            controlHideTimer.Stop();
            DoAudioControlFade("out", AudioControlGrid, playerMenu);
        }

        // When the mouse leaves the window it automatically sets the control in use boolean to false, and starts the timer.
        private void MetroWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            isUsingControls = false;
            controlHideTimer.Start();
        }

        private void MetroWindow_MouseMove(object sender, MouseEventArgs e)
        {
            // Timer controls when you move your mouse on the window.
            controlHideTimer.Stop();
            if(isUsingControls == false)
            {
                controlHideTimer.Start();
            }
            
            // If the opacity is 0 when you move your mouse, it'll go ahead and do this.
            if (AudioControlGrid.Opacity == 0)
            {
                DoAudioControlFade("in", AudioControlGrid, playerMenu);
            }
        }

        // Timer controls for wherever the mouse happens to be - pretty self explanatory
        private void AudioControlGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            controlHideTimer.Stop();
            isUsingControls = true;
        }

        private void AudioControlGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            controlHideTimer.Start();
            isUsingControls = false;
        }

        private void playerMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            controlHideTimer.Stop();
            isUsingControls = true;
        }

        private void playerMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            controlHideTimer.Stop();
            isUsingControls = false;
        }

        private void DoAudioControlFade(string inOrOut, params FrameworkElement[] control)
        {
            // Fade the controls in or out, based on the set property.
            foreach(FrameworkElement thiscontrol in control)
            if (inOrOut == "in")
            {
                thiscontrol.Visibility = Visibility.Visible;
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0;
                da.To = 0.8;
                da.Duration = new Duration(TimeSpan.FromSeconds(0.4));
                thiscontrol.BeginAnimation(OpacityProperty, da);
            }
            else if (inOrOut == "out")
            {
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0.8;
                da.To = 0;
                da.Duration = new Duration(TimeSpan.FromSeconds(0.4));
                da.Completed += (sender, eargs) =>
                {
                    thiscontrol.Visibility = Visibility.Hidden;
                };
                thiscontrol.BeginAnimation(OpacityProperty, da);
            }
        }
        #endregion

            #region AlbumArt
        // First grabs the album art from the current file, converts it over to a useable bitmap and finally slaps it on the player - if no album art is available then the placeholder will be used.
        private BitmapImage currentAlbumArtBMI;
        public void DoSetAlbumArt(Song song)
        {
            string songAlbumArt = song.FileLocation;

            TagLib.File f = TagLib.File.Create(songAlbumArt);
            if (f.Tag.Pictures.Length > 0)
            {
                TagLib.IPicture pic = f.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic.Data.Data);
                if (ms != null && ms.Length > 4096)
                    ms.Seek(0, SeekOrigin.Begin);
                {
                    currentAlbumArtBMI = new BitmapImage();
                    currentAlbumArtBMI.BeginInit();
                    currentAlbumArtBMI.StreamSource = ms;
                    currentAlbumArtBMI.EndInit();
                }
                ImageBrush imgBrush = new ImageBrush();
                imgBrush.ImageSource = currentAlbumArtBMI;
                MainWindowGrid.Background = imgBrush;
            }
            else
            {
                
                ImageBrush imgBrush = new ImageBrush();
                imgBrush.ImageSource = noAlbumArt;
                imgBrush.Opacity = 0.4;
                MainWindowGrid.Background = imgBrush;
            }
        }
        #endregion

        #endregion

        
        #region Controls

        // Does what it says on the tin- plays the song.
        private void btnPlayTrack_Click(object sender, RoutedEventArgs e)
        {
            DoPlayTrack();
        }

        // Shuffle enable toggle.
        private bool shuffleIsEnabled;
        private void btnShuffle_Click(object sender, RoutedEventArgs e)
        {
            shuffleIsEnabled = !shuffleIsEnabled;
            shuffleButtonVisual.IsEnabled = !shuffleButtonVisual.IsEnabled;            
        }

        // Unhides the volume bar when you click on the audio button - might replace this later with something a bit better.
        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            if (volumeBar.Visibility == Visibility.Hidden)
            {
                volumeBar.Visibility = Visibility.Visible;
            }
            else
            {
                volumeBar.Visibility = Visibility.Hidden;
            }
        }

        //Right click setter for the volume bar
        private void btnVolume_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(mediaPlayer.Volume == 0)
            {
                mediaPlayer.Volume = volumeBar.Value;
                audioButtonVisual.IsEnabled = true;
            }
            else if (mediaPlayer.Volume != 0)
            {
                mediaPlayer.Volume = 0;
                audioButtonVisual.IsEnabled = false;
            }
        }

        // Self explanatory - button does the thing.
        private void btnPreviousTrack_Click(object sender, RoutedEventArgs e)
        {
            DoPreviousTrack();
        }

        // Self explanatory - button does the thing.
        private void btnNextTrack_Click(object sender, RoutedEventArgs e)
        {
            DoNextTrack();
        }

        // Global listener event - Fires on keypress, if any of those keypresses happens to be a media key, then good news.
        void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed == Key.MediaPlayPause)
            {
                DoPlayTrack();
            }
            if (e.KeyPressed == Key.MediaNextTrack)
            {
                DoNextTrack();
            }
            if (e.KeyPressed == Key.MediaPreviousTrack)
            {
                DoPreviousTrack();
            }
        }

        #endregion


        #region Progress Slider

        private DispatcherTimer mediaTimer; // Timer for the media.

        // Very long and boring bit of code which simply put sets up the progress bars values based on the currently playing media.
        void mediaTimer_Tick(object sender, EventArgs e)
        {
            if ((mediaPlayer.Source != null) && (mediaPlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mediaPlayer.Position.TotalSeconds;
                if (mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds == mediaPlayer.Position.TotalSeconds)
                {
                    // This happens when you reach the end of the track.
                    sliProgress.Value = 0;
                    if (shuffleIsEnabled == true)
                    {
                        Library.RandomSong();
                        Song tempRandSong = Library.getSong();
                        DoLoadSongFromLibrary(tempRandSong);
                    }
                    else if (shuffleIsEnabled == false)
                    {
                        Library.NextSong();
                        Song tempNextSong = Library.getSong();
                        DoLoadSongFromLibrary(tempNextSong);
                    }
                }
            }
        }

        
        private bool userIsDraggingSlider = false; // Checks if the user is dragging the slider
        // toggles the slider drag
        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        // When the drag ends, set the position of the currently playing media to the value given by the slider.
        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        // Whenever the slider changes, update the time readout and the media player.
        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
            mediaPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }
        #endregion


        #region Volume Controls and Values
        // Sets the media players volume to the value of the slider when it's changed.
        private void volumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = volumeBar.Value;
        }

        #endregion


        #region Window Hotkeys
        // Hotkeys for the window - might abstract this out somehow so it doesn't become a horrible mess - might make these customisable in future.
        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.H && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                this.ShowTitleBar = !this.ShowTitleBar;
                if(playerMenu.Visibility == Visibility.Visible)
                {
                    playerMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    playerMenu.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion


        #region Window Controls
        public MusickLibrary Library = new MusickLibrary(); // sets a public instance of the Library window for everything to use.
           
        // controls the showing and hiding of the player window.    
        private void LibraryItem_Click(object sender, RoutedEventArgs e)
        {
            if (Library.IsVisible)
            {
                Library.Hide();
            }
            else
            {
                Library.Show();
                Library.Activate();
            }
        }


        public MusickSettings Settings = new MusickSettings(); // public instance of the Settings window to be used.

        // Controls showing and hiding of the settings window.
        private void SettingsItem_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.IsVisible)
            {
                Settings.Hide();
            }
            else
            {
                Settings.Show();
                Settings.Activate();
            }
        }

        #endregion


        #region Misc code
        // Overrides the "OnClosed" event to force the application to shut down when the player is closed. - Runs the Save method to serialise the current settings to local storage.
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            DoSaveSettings();
            Application.Current.Shutdown();
        }

        #endregion


        #region Save Stuff

        // Sets currentSettings values to reflect the current state of the application, then serialises them to file.
        private void DoSaveSettings()
        {
            // Library window values
            currentSettings.libraryLeft = Library.Left;
            currentSettings.libraryTop = Library.Top;
            currentSettings.libraryWidth = Library.Width;
            currentSettings.libraryHeight = Library.Height;

            // Player window values
            currentSettings.playerTop = this.Top;
            currentSettings.playerLeft = this.Left;
            currentSettings.volumeValue = volumeBar.Value;
            currentSettings.shuffleEnabled = shuffleIsEnabled;
            
            // Serialise save data to file with the current settings.
            string settingsFile = System.IO.Path.Combine(ConfigClass.appSettingsFolder, "Settings.txt");
            //System.IO.File.Delete(settingsFile);
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamWriter sw = new StreamWriter(settingsFile))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, currentSettings);
            }
        }

        #endregion


        #region Legacy UI Continuity crap - Massive needless performance sink        
        // Disgusting legacy code for UI continuity that I decided not to use, huge performance sink and the result wasn't all that good. - Will save this elsewhere for future reference at some point.

        /*
        // Convert the currently displayed album art to a bitmap so I can use it for UI stuff.
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        // An object for passing the colours up to the UI.
        public struct OppositeColours
        {
            public MColor foregroundColour;
            public MColor backgroundColour;
        }

        // Gets the average colour of the displayed album art and sends values to the UI.
        public static OppositeColours ContrastColourfromDominantColour(Bitmap bmp)
        {
            OppositeColours textOpposite;

            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    DColor clr = bmp.GetPixel(x, y);
                    r += clr.R;
                    g += clr.G;
                    b += clr.B;
                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;

            DColor color = DColor.FromArgb(r, g, b);

            byte fore = 0;
            byte back = 0;

            // Counting the perceptive luminance - human eye favors green color... 
            double a = 1 - (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

            if (a < 0.5)
            {
                fore = 255; // bright colors - light font
                back = 0;   //                 black background
            }
            else
            {
                fore = 0;   // dark colors - black font
                back = 255; //               light background
            }
                           

            textOpposite.foregroundColour = MColor.FromRgb(fore, fore, fore);
            textOpposite.backgroundColour = MColor.FromRgb(back, back, back);

            return textOpposite;
        }
        */
        #endregion


    }
}