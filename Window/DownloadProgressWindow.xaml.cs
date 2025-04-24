using System.Windows;

namespace IFLEGameLauncher
{
    public partial class DownloadProgressWindow : Window
    {
        public DownloadProgressWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int percentage)
        {
            DownloadProgressBar.Value = percentage;
            ProgressText.Text = $"{percentage}%";
        }
    }
}
