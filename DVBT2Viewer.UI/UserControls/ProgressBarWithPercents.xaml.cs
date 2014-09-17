using System.Windows;
using System.Windows.Controls;

namespace DVBT2Viewer.UI.UserControls
{
    /// <summary>
    /// Interaction logic for ProgressBarWithPercents.xaml
    /// </summary>
    public partial class ProgressBarWithPercents : UserControl
    {
        #region Value dependency property (ProgressBar value)
        public static readonly DependencyProperty ValueProperty =
             DependencyProperty.Register("Value", 
             typeof(double),
             typeof(ProgressBarWithPercents),
             new FrameworkPropertyMetadata((double)0));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        #endregion

        public ProgressBarWithPercents()
        {
            InitializeComponent();
        }
    }
}
