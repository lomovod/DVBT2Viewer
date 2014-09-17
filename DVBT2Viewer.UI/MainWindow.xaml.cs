using System.Windows;
using DVBT2Viewer.BDA.Models;
using DVBT2Viewer.UI.Interfaces;
using DVBT2Viewer.UI.ViewModels;

namespace DVBT2Viewer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindowActions
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public DigitalMultiplex SearchChannels()
        {
            var dialog = new ChannelSearchWindow { Owner = this };
            var value = dialog.ShowDialog();
            if (value.HasValue && value.Value && dialog.DataContext is ChannelSearchViewModel)
                return (dialog.DataContext as ChannelSearchViewModel).Multiplex;
            return null;
        }

        public void Close(bool closeValue)
        {
            Close();
        }
    }
}
