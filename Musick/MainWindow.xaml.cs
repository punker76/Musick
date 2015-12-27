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
        private bool userIsDraggingSlider = false;
        private bool mediaPlayerIsPlaying = false;
        public BitmapSource noAlbumArt;
        public static UserSettings currentSettings = new UserSettings();
        

        #endregion


        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            DoUseSettings();

            var image = Properties.Resources.NoAlbumArt;
            var bitmap = new System.Drawing.Bitmap(image);
            noAlbumArt = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmap.Dispose();

            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = noAlbumArt;
            imgBrush.Opacity = 0.4;
            MainWindowGrid.Background = imgBrush;

            // Set media voluma (user stored variable goes here) and set volumeBar to the mediaPlayer volume.
            mediaPlayer.Volume = 0.5;
            volumeBar.Value = mediaPlayer.Volume;

            // Control visibility @ runtime.
            volumeBar.Visibility = Visibility.Hidden;
            AudioControlGrid.Opacity = 0; AudioControlGrid.Visibility = Visibility.Hidden;
            playerMenu.Opacity = 0; playerMenu.Visibility = Visibility.Hidden;

            // AutoHide controls timer (Maybe user setting?)
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.0) };
            timer.Tick += timer_Tick;

            // Shuffle enabled default.
            shuffleButtonVisual.IsEnabled = true;
            shuffleIsEnabled = false;

            // Timer for the playing of media.
            mediaTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
            mediaTimer.Tick += mediaTimer_Tick;
            mediaTimer.Start();

            MusickLibrary objectToSubscribeTo = Library;
            objectToSubscribeTo.songSelected += songDoubleClicked;
        }


        #region Load up settings
        private void DoUseSettings()
        {
            // Set the theme to the value stored in currentSettings
            ThemeManager.ChangeAppStyle(System.Windows.Application.Current, ThemeManager.GetAccent(MainWindow.currentSettings.accent), ThemeManager.GetAppTheme(MainWindow.currentSettings.theme));

            // Load up player window settings
            this.Left = currentSettings.playerLeft; this.Top = currentSettings.playerTop;

            // Load up library window settings
            Library.Left = currentSettings.libraryLeft; Library.Top = currentSettings.libraryTop; Library.Width = currentSettings.libraryWidth; Library.Height = currentSettings.libraryHeight; 
        }
        #endregion


        #region Actions

        public void songDoubleClicked(object sender, EventArgs e)
        {
            Song tempSong = Library.getSong();
            DoLoadSongFromLibrary(tempSong);
        }

        public void DoLoadSongFromLibrary(Song song)
        {
            mediaPlayer.Open(new Uri(song.FileLocation));
            mediaPlayerIsPlaying = false;
            DoSetAlbumArt(song);
            DoSetWindowTitle(song);
            DoPlaySong();
        }

        public void DoSetWindowTitle(Song song)
        {
            this.Title = song.SongTitle+ " - " +song.SongArtist;
        }
        
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
        private bool isUsingControls;
        private DispatcherTimer timer;
        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            DoAudioControlFade("out", AudioControlGrid, playerMenu);

        }

        private void MetroWindow_MouseMove(object sender, MouseEventArgs e)
        {
            // Timer controls when you move your mouse on the window.
            timer.Stop();
            if(isUsingControls == false)
            {
                timer.Start();
            }
            
            // If the opacity is 0 when you move your mouse, it'll go ahead and do this.
            if (AudioControlGrid.Opacity == 0)
            {
                DoAudioControlFade("in", AudioControlGrid, playerMenu);
            }
        }

        private void AudioControlGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            timer.Stop();
            isUsingControls = true;
        }

        private void AudioControlGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            timer.Start();
            isUsingControls = false;
        }

        private void playerMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            timer.Stop();
            isUsingControls = true;
        }

        private void playerMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            timer.Stop();
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
        private void btnPlayTrack_Click(object sender, RoutedEventArgs e)
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

        private bool shuffleIsEnabled;
        private void btnShuffle_Click(object sender, RoutedEventArgs e)
        {
            shuffleIsEnabled = !shuffleIsEnabled;
            shuffleButtonVisual.IsEnabled = !shuffleButtonVisual.IsEnabled;            
        }

        private void MetroWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            isUsingControls = false;
            timer.Start();
        }

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

        private void btnPreviousTrack_Click(object sender, RoutedEventArgs e)
        {
            Library.PreviousSong();
            Song tempPrevSong = Library.getSong();
            DoLoadSongFromLibrary(tempPrevSong);
        }


        private void btnNextTrack_Click(object sender, RoutedEventArgs e)
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

        #endregion


        #region Progress Slider

        private DispatcherTimer mediaTimer;
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

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
            mediaPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }
        #endregion


        #region Volume Controls and Values
        
        private void volumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = volumeBar.Value;
        }

        #endregion


        #region Window Hotkeys
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
        public MusickLibrary Library = new MusickLibrary();       
        private void LibraryItem_Click(object sender, RoutedEventArgs e)
        {
            DoLibraryControl();
        }

        private void DoLibraryControl()
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


        public MusickSettings Settings = new MusickSettings();
        private void SettingsItem_Click(object sender, RoutedEventArgs e)
        {
            DoSettingsControl();
        }

        private void DoSettingsControl()
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
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            DoSaveSettings();
            Application.Current.Shutdown();
        }

        #endregion


        #region Save Stuff
        private void DoSaveSettings()
        {
            currentSettings.libraryLeft = Library.Left;
            currentSettings.libraryTop = Library.Top;
            currentSettings.libraryWidth = Library.Width;
            currentSettings.libraryHeight = Library.Height;

            currentSettings.playerTop = this.Top;
            currentSettings.playerLeft = this.Left;

            // Delete the existing settings file, and re-serialise one with the current settings.
            string settingsFile = System.IO.Path.Combine(ConfigClass.appSettingsFolder, "Settings.txt");
            System.IO.File.Delete(settingsFile);
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