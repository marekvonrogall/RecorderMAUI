using Android.Views;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.IO;

namespace Recorder
{
    public partial class Recorder : TabbedPage
    {
        readonly IAudioManager _audioManager;
        readonly IAudioRecorder _audioRecorder;

        public ObservableCollection<AudioFile> RecordedFiles { get; set; } = new ObservableCollection<AudioFile>();

        public Recorder(IAudioManager audioManager)
        {
            InitializeComponent();
            BindingContext = this;
            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();
            LoadRecordedFiles();
        }


        private async void OnRecordClicked(object sender, EventArgs e)
        {
            if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Microphone permission is required to record audio.", "OK");
                return;
            }

            if (_audioRecorder.IsRecording)
            {
                ImageMicrophone.Source = "mic_off.svg";
                ButtonRecord.Text = "Start recording";

                var recordedAudio = await _audioRecorder.StopAsync();

                // Get the Music directory
                var musicPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Music");

                if (!Directory.Exists(musicPath))
                {
                    Directory.CreateDirectory(musicPath);
                }

                var fileName = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
                var filePath = Path.Combine(musicPath, fileName);

                using (var fileStream = File.Create(filePath))
                {
                    var audioStream = recordedAudio.GetAudioStream();
                    await audioStream.CopyToAsync(fileStream);
                }

                await DisplayAlert("Success", "Recording saved to Music folder.", "OK");
                recordedAudio.GetAudioStream().Dispose();

                LoadRecordedFiles();
            }
            else
            {
                ImageMicrophone.Source = "mic.svg";
                ButtonRecord.Text = "Stop recording";
                await _audioRecorder.StartAsync();
            }
        }

        private async void OnPlayClicked(object sender, EventArgs e)
        {
            if (await Permissions.RequestAsync<Permissions.StorageRead>() != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Storage permission is required to play audio files.", "OK");
                return;
            }

            var button = sender as Button;
            var filePath = button?.CommandParameter as string;

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var player = AudioManager.Current.CreatePlayer(File.OpenRead(filePath));
                player.Play();
            }
            else
            {
                await DisplayAlert("Error", "File not found", "OK");
            }
        }


        private void LoadRecordedFiles()
        {
            var musicPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Music");

            if (Directory.Exists(musicPath))
            {
                var files = Directory.GetFiles(musicPath, "*.wav");
                RecordedFiles.Clear();

                foreach (var file in files)
                {
                    RecordedFiles.Add(new AudioFile
                    {
                        Filename = Path.GetFileName(file),
                        FilePath = file
                    });
                }
            }
        }

        public class AudioFile
        {
            public string Filename { get; set; }
            public string FilePath { get; set; }
        }
    }
}
