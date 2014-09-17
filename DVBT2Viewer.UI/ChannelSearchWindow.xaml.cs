using System.Windows;
using DVBT2Viewer.UI.Interfaces;

namespace DVBT2Viewer.UI
{
    /// <summary>
    /// Interaction logic for ChannelSearchWindow.xaml
    /// </summary>
    public partial class ChannelSearchWindow : Window, IClosableWindowActions
    {
        public ChannelSearchWindow()
        {
            InitializeComponent();
        }

        public void Close(bool closeValue)
        {
            DialogResult = closeValue;
            Close();
        }
    }
}
