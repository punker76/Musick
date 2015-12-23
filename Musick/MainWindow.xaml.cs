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
using NAudio;
using NAudio.Wave;

namespace Musick
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private DispatcherTimer timer;
        public string currentTrack;
        public MediaPlayer mediaPlayer = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();

            // Start that timer up fucker.
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
            timer.Tick += timer_Tick;

        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            DoAudioControlFade("out");            
        }

        private void MetroWindow_MouseMove(object sender, MouseEventArgs e)
        {
            // Timer controls when you move your mouse on the window.
            timer.Stop();
            timer.Start();

            // If the opacity is 0 when you move your mouse, it'll go ahead and do this.
            if (AudioControlGrid.Opacity == 0)
            {
                DoAudioControlFade("in");
            }
        }

        private void DoAudioControlFade(string inOrOut)
        {
            // Fade the controls in or out, based on the set property.
            if(inOrOut=="in")
            {
                AudioControlGrid.Visibility = Visibility.Visible;
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0;
                da.To = 1;
                da.Duration = new Duration(TimeSpan.FromSeconds(0.4));
                AudioControlGrid.BeginAnimation(OpacityProperty, da);
            }
            else if (inOrOut=="out")
            {
                DoubleAnimation da = new DoubleAnimation();
                da.From = 1;
                da.To = 0;
                da.Duration = new Duration(TimeSpan.FromSeconds(0.4));
                da.Completed += (sender, eargs) =>
                {
                    AudioControlGrid.Visibility = Visibility.Hidden;
                };
                AudioControlGrid.BeginAnimation(OpacityProperty, da);
            }
        }

        private void btnPlayTrack_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void btnPreviousTrack_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNextTrack_Click(object sender, RoutedEventArgs e)
        {

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
            }
        }
    }
}
