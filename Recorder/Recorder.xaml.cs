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

        public Recorder(IAudioManager audioManager)
        {
            InitializeComponent();
            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();
        }

        private async void OnRecordClicked(object sender, EventArgs e)
        {
            if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
            {
                return;
            }

            if (_audioRecorder.IsRecording)
            {
                ImageMicrophone.Source = "mic_off.svg";
                ButtonRecord.Text = "Start recording";
                var recordedAudio = await _audioRecorder.StopAsync();
                var player = AudioManager.Current.CreatePlayer(recordedAudio.GetAudioStream());
                player.Play();
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

        }
    }
}
