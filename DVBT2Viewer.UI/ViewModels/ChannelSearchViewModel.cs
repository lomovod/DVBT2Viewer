using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using DVBT2Viewer.BDA.Helpers;
using DVBT2Viewer.BDA.Interfaces;
using DVBT2Viewer.BDA.Models;
using DVBT2Viewer.UI.Interfaces;
using DVBT2Viewer.UI.Tools;

namespace DVBT2Viewer.UI.ViewModels
{
    #region ChannelSearch ViewModel
    public class ChannelSearchViewModel : INotifyPropertyChanged
    {
        #region Private fields
        private readonly IDVBT2Finder finder = BDAHelper.GetFinder();

        private int frequency;
        private int bandwidth;

        private int signalLevel;
        private int signalQuality;

        private readonly ObservableCollection<DigitalChannel> channels = new ObservableCollection<DigitalChannel>();

        private readonly DispatcherTimer levelAndQualityTimer = new DispatcherTimer();

        private DelegateCommand loadedCommand;
        private DelegateCommand closingCommand;
        private DelegateCommand lockCommand;
        private DelegateCommand searchCommand;
        private DelegateCommand saveAndExitCommand;
        private DelegateCommand closeViewModelCommand;

        private bool searchCommandCanExecute = true;
        #endregion

        #region Commands
        public ICommand LoadedCommand { get { return loadedCommand ?? (loadedCommand = new DelegateCommand(x => ExecuteLoaded())); } }

        public ICommand ClosingCommand { get { return closingCommand ?? (closingCommand = new DelegateCommand(x => ExecuteClosing())); } }

        public ICommand LockCommand { get { return lockCommand ?? (lockCommand = new DelegateCommand(x => ExecuteLock())); } }

        public ICommand SearchCommand { get { return searchCommand ?? (searchCommand = new DelegateCommand(x => ExecuteSearch(), x => searchCommandCanExecute)); } }

        public ICommand SaveAndExitCommand { get { return saveAndExitCommand ?? (saveAndExitCommand = new DelegateCommand(ExecuteSaveAndExit)); } }

        public ICommand CloseViewModelCommand { get { return closeViewModelCommand ?? (closeViewModelCommand = new DelegateCommand(ExecuteCloseViewModel)); } }
        #endregion

        #region Public properties
        public int Frequency
        {
            get { return frequency; }
            set { frequency = value; OnPropertyChanged("Frequency"); }
        }

        public int Bandwidth
        {
            get { return bandwidth; }
            set { bandwidth = value; OnPropertyChanged("Bandwidth"); }
        }

        public int SignalLevel
        {
            get { return signalLevel; }
            set { signalLevel = value; OnPropertyChanged("SignalLevel"); }
        }

        public int SignalQuality
        {
            get { return signalQuality; }
            set { signalQuality = value; OnPropertyChanged("SignalQuality"); }
        }

        public ObservableCollection<DigitalChannel> Channels { get { return channels; } }

        public DigitalMultiplex Multiplex { get; private set; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public ChannelSearchViewModel()
        {
            Multiplex = null;
            levelAndQualityTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            levelAndQualityTimer.Tick += (sender, args) =>
            {
                var status = finder.GetLockStatus();
                SignalLevel = status.SignalStrength;
                SignalQuality = status.SignalQuality;
            };
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ExecuteLoaded()
        {
            finder.Build();
            levelAndQualityTimer.Start();
            finder.Run();
        }

        private void ExecuteClosing()
        {
            Decompose();
        }

        private void ExecuteLock()
        {
            finder.Stop();
            finder.Lock(Frequency, Bandwidth);
            finder.Run();
        }

        private void ExecuteSearch()
        {
            if (Frequency <= 0 || Bandwidth <= 0)
            {
                MessageBox.Show(@"Frequency and bandwidth must be positive values");
                return;
            }

            if (levelAndQualityTimer.IsEnabled)
                levelAndQualityTimer.Stop();
            searchCommandCanExecute = false;
            searchCommand.RaiseCanExecuteChanged();

            try
            {
                Multiplex = finder.GetMultiplex(Frequency, Bandwidth);
                if (Multiplex == null) return;

                channels.Clear();

                foreach (var channel in Multiplex.Channels)
                    channels.Add(channel);
            }
            finally
            {
                if (!levelAndQualityTimer.IsEnabled)
                    levelAndQualityTimer.Stop();
                searchCommandCanExecute = true;
                searchCommand.RaiseCanExecuteChanged();
            }
        }

        private static void ExecuteSaveAndExit(object parameter)
        {
            var actions = parameter as IClosableWindowActions;
            if (actions == null) return;
            actions.Close(true);
        }

        private static void ExecuteCloseViewModel(object parameter)
        {
            var actions = parameter as IClosableWindowActions;
            if (actions == null) return;
            actions.Close(false);
        }

        private void Decompose()
        {
            if (levelAndQualityTimer.IsEnabled)
                levelAndQualityTimer.Stop();
            finder.Stop();
            finder.Dispose();
        }
    }
    #endregion
}
