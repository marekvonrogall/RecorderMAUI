using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Recorder
{
    public class RecorderViewModel : BindableObject
    {
        readonly IAudioManager _audioManager;
        readonly IAudioRecorder _audioRecorder;

        public ObservableCollection<AudioFileModel> RecordedFiles { get; set; } = new ObservableCollection<AudioFileModel>();

        public ICommand RecordCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }

        private string _microphoneImageSource = "mic_off.svg";
        public string MicrophoneImageSource
        {
            get => _microphoneImageSource;
            set
            {
                _microphoneImageSource = value;
                OnPropertyChanged(nameof(MicrophoneImageSource));
            }
        }

        private string _recordButtonText = "Start recording";
        public string RecordButtonText
        {
            get => _recordButtonText;
            set
            {
                _recordButtonText = value;
                OnPropertyChanged(nameof(RecordButtonText));
            }
        }

        public RecorderViewModel(IAudioManager audioManager)
        {
            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();

            RecordCommand = new Command(async () => await OnRecordClicked());
            PlayCommand = new Command<string>(async (filePath) => await OnPlayClicked(filePath));

            LoadRecordedFiles();
        }

        private async Task OnRecordClicked()
        {
            await Permissions.RequestAsync<Permissions.Microphone>();

            if (_audioRecorder.IsRecording)
            {
                MicrophoneImageSource = "mic_off.svg";
                RecordButtonText = "Start recording";

                var recordedAudio = await _audioRecorder.StopAsync();

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

                recordedAudio.GetAudioStream().Dispose();

                LoadRecordedFiles();
            }
            else
            {
                MicrophoneImageSource = "mic.svg";
                RecordButtonText = "Stop recording";

                await _audioRecorder.StartAsync();
            }
        }

        private async Task OnPlayClicked(string filePath)
        {
            await Permissions.RequestAsync<Permissions.StorageRead>();

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var player = _audioManager.CreatePlayer(File.OpenRead(filePath));
                player.Play();
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
                    RecordedFiles.Add(new AudioFileModel
                    {
                        Filename = Path.GetFileName(file),
                        FilePath = file
                    });
                }
            }
        }
    }
}
