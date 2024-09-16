using Plugin.Maui.Audio;

namespace Recorder
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            IAudioManager audioManager = new AudioManager();

            //MainPage = new AppShell();
            MainPage = new Recorder(audioManager);
        }
    }
}
