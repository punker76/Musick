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

namespace Musick
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

                
        public string currentTrack;
        public MediaPlayer mediaPlayer = new MediaPlayer();
        private bool userIsDraggingSlider = false;
        private bool mediaPlayerIsPlaying = false;
        


        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            // Set media voluma (user default goes here) and set volumeBar to the mediaPlayer volume.
            mediaPlayer.Volume = 0.5;
            volumeBar.Value = mediaPlayer.Volume;

            // Control visibility @ runtime.
            volumeBar.Visibility = Visibility.Hidden;
            AudioControlGrid.Opacity = 0; AudioControlGrid.Visibility = Visibility.Hidden;

            // AutoHide controls timer.
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.0) };
            timer.Tick += timer_Tick;

            // Timer for the playing of media.
            mediaTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
            mediaTimer.Tick += mediaTimer_Tick;
            mediaTimer.Start();
        }



        #region Animation Shit

        private DispatcherTimer timer;
        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            DoAudioControlFade("out", AudioControlGrid);

        }

        private void MetroWindow_MouseMove(object sender, MouseEventArgs e)
        {
            // Timer controls when you move your mouse on the window.
            timer.Stop();
            timer.Start();

            // If the opacity is 0 when you move your mouse, it'll go ahead and do this.
            if (AudioControlGrid.Opacity == 0)
            {
                DoAudioControlFade("in", AudioControlGrid);

            }
        }

        private void DoAudioControlFade(string inOrOut, Grid grid)
        {
            // Fade the controls in or out, based on the set property.
            if (inOrOut == "in")
            {
                grid.Visibility = Visibility.Visible;
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0;
                da.To = 0.8;
                da.Duration = new Duration(TimeSpan.FromSeconds(0.4));
                grid.BeginAnimation(OpacityProperty, da);
            }
            else if (inOrOut == "out")
            {
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0.8;
                da.To = 0;
                da.Duration = new Duration(TimeSpan.FromSeconds(0.4));
                da.Completed += (sender, eargs) =>
                {
                    grid.Visibility = Visibility.Hidden;
                };
                grid.BeginAnimation(OpacityProperty, da);
            }
        }
        #endregion
        

        #region Audio Controls.

        // -- Audio Controls -- //
        private void btnPlayTrack_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source != null && mediaPlayerIsPlaying == false)
            {
                mediaPlayer.Play();
                mediaPlayerIsPlaying = true;
                playButtonVisual.IsEnabled = false;
                sliProgress.IsEnabled = true;
            }

            else if (mediaPlayerIsPlaying == true)
            {
                mediaPlayer.Pause();
                mediaPlayerIsPlaying = false;
                playButtonVisual.IsEnabled = true;
            }

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
            // Play previous track - only useful when i get to making a library
        }

        private void btnNextTrack_Click(object sender, RoutedEventArgs e)
        {
            // Play next track - only useful when I actually get to making a library
        }

        private void btnBrowseTrack_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openMusicFileDialog = new OpenFileDialog();
            openMusicFileDialog.Filter = "Music Files(*.MP3;*.WAV;*.OGG)|*.MP3;*.WAV;*.OGG";
            openMusicFileDialog.FilterIndex = 1;
            openMusicFileDialog.Multiselect = false;
            if (openMusicFileDialog.ShowDialog() == true)
            {
                currentTrack = openMusicFileDialog.FileName.ToString();
                mediaPlayer.Open(new Uri(currentTrack));

                TagLib.File f = new TagLib.Mpeg.AudioFile(currentTrack);
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
                    //Bitmap currentAlbumArt = BitmapImage2Bitmap(currentAlbumArtBMI);
                    //lblProgressStatus.Foreground = new SolidColorBrush(ContrastColourfromDominantColour(currentAlbumArt).foregroundColour);
                    //lblProgressStatus.Background = new SolidColorBrush(ContrastColourfromDominantColour(currentAlbumArt).backgroundColour);
                    ImageBrush imgBrush = new ImageBrush();
                    imgBrush.ImageSource = currentAlbumArtBMI;
                    MainWindowGrid.Background = imgBrush;                    
                }
            }
        }
        #endregion


        #region The nightmare that is albumArt/UI continuity
        
        private BitmapImage currentAlbumArtBMI;
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
                    mediaPlayer.Stop();
                    sliProgress.IsEnabled = false;
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
        // -- Volume Controls -- //
        private void volumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = volumeBar.Value;
        }

        #endregion

        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.H && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                this.ShowTitleBar = !this.ShowTitleBar;
            }
        }
    }
}