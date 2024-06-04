using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;

namespace AudioPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> _playlist = new List<string>();
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            mediaElement.MediaEnded += MediaElement_MediaEnded;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.Source != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                progressSlider.Minimum = 0;
                progressSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                progressSlider.Value = mediaElement.Position.TotalSeconds;
                lblTime.Content = $"{mediaElement.Position.ToString(@"mm\:ss")} / {mediaElement.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}";
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                lblLength.Content = $"Length: {mediaElement.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss")}";
                _timer.Start();
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.Source != null)
            {
                mediaElement.Play();
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }

        private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                mediaElement.Position = TimeSpan.FromSeconds(progressSlider.Value);
            }
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = volumeSlider.Value;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio Files|*.mp3;*.wav;*.wma";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

                string artist;
                string title;

                if (fileName.Contains("-"))
                {
                    string[] parts = fileName.Split('-');
                    artist = parts[0].Trim();
                    title = parts[1].Trim();
                }
                else
                {
                    artist = "Unable to get information";
                    title = "Unable to get information";
                }

                _playlist.Add(filePath);
                playlistListBox.Items.Add($"{artist} - {title}");

                lblArtist.Content = $"Artist: {artist}";
                lblTitle.Content = $"Title: {title}";
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (playlistListBox.SelectedIndex >= 0)
            {
                _playlist.RemoveAt(playlistListBox.SelectedIndex);
                playlistListBox.Items.RemoveAt(playlistListBox.SelectedIndex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Playlist Files|*.playlist";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllLines(saveFileDialog.FileName, _playlist);
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Playlist Files|*.playlist";
            if (openFileDialog.ShowDialog() == true)
            {
                _playlist = File.ReadAllLines(openFileDialog.FileName).ToList();
                playlistListBox.Items.Clear();
                foreach (var song in _playlist)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(song);
                    string artist;
                    string title;

                    if (fileName.Contains("-"))
                    {
                        string[] parts = fileName.Split('-');
                        artist = parts[0].Trim();
                        title = parts[1].Trim();
                    }
                    else
                    {
                        artist = "Unable to get information";
                        title = "Unable to get information";
                    }
                    playlistListBox.Items.Add($"{artist} - {title}");
                }
            }
        }

        private void playlistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlaySelectedSong();
        }

        private void PlaySelectedSong()
        {
            if (playlistListBox.SelectedIndex >= 0)
            {
                string filePath = _playlist[playlistListBox.SelectedIndex];
                mediaElement.Source = new Uri(filePath);
                mediaElement.Play();

                string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                string artist;
                string title;

                if (fileName.Contains("-"))
                {
                    string[] parts = fileName.Split('-');
                    artist = parts[0].Trim();
                    title = parts[1].Trim();
                }
                else
                {
                    artist = "Unable to get information";
                    title = "Unable to get information";
                }

                lblArtist.Content = $"Artist: {artist}";
                lblTitle.Content = $"Title: {title}";
            }
        }

        private void PlayNextSong()
        {
            if (playlistListBox.SelectedIndex < _playlist.Count - 1)
            {
                playlistListBox.SelectedIndex++;
                PlaySelectedSong();
            }
            else
            {
                mediaElement.Stop();
                _timer.Stop();
            }
        }
    }
}