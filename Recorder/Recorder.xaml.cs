namespace Recorder
{
    public partial class Recorder : TabbedPage
    {
        public Recorder(RecorderViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
