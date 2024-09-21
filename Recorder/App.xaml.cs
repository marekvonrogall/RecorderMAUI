using Plugin.Maui.Audio;

namespace Recorder
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            IAudioManager audioManager = new AudioManager();
            RecorderViewModel viewModel = new RecorderViewModel(audioManager);
            MainPage = new Recorder(viewModel);
        }
    }
}
