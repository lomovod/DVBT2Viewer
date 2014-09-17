using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DVBT2Viewer.BDA.Helpers;
using DVBT2Viewer.BDA.Interfaces;
using DVBT2Viewer.BDA.Models;
using DVBT2Viewer.UI.Helpers;
using DVBT2Viewer.UI.Interfaces;
using DVBT2Viewer.UI.Tools;
using Size = System.Drawing.Size;

namespace DVBT2Viewer.UI.ViewModels
{
    #region MainWindow ViewModel
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Private fields
        private readonly IDVBT2Renderer renderer = BDAHelper.GetRenderer();

        private string windowTitle = "DVBT2 Viewer";

        private double signalLevel;
        private double signalQuality;

        private IntPtr renderHandle;

        private double videoFrameWidth;
        private double videoFrameHeight;

        private readonly ObservableCollection<DigitalChannel> channels = new ObservableCollection<DigitalChannel>();
        private DigitalChannel selectedChannel;

        private DelegateCommand loadedCommand;
        private DelegateCommand closingCommand;
        private DelegateCommand searchChannelsCommand;
        private DelegateCommand makeScreenshotCommand;
        private DelegateCommand closeViewModelCommand;
        private DelegateCommand resizeVideoFrameCommand;
        private DelegateCommand repaintVideoFrameCommand;

        private readonly DispatcherTimer levelAndQualityTimer = new DispatcherTimer();

        private const string channelsXmlFileName = "channels.xml";
        #endregion

        #region Commands
        public ICommand LoadedCommand { get { return loadedCommand ?? (loadedCommand = new DelegateCommand(ExecuteLoaded)); } }

        public ICommand ClosingCommand { get { return closingCommand ?? (closingCommand = new DelegateCommand(x => ExecuteClosing())); } }

        public ICommand SearchChannelsCommand 
        {
            get { return searchChannelsCommand ?? (searchChannelsCommand = new DelegateCommand(ExecuteSearchChannels)); }
        }

        public ICommand MakeScreenshotCommand
        {
            get { return makeScreenshotCommand ?? (makeScreenshotCommand = new DelegateCommand(x => ExecuteMakeScreenshot())); }
        }

        public ICommand CloseViewModelCommand
        {
            get { return closeViewModelCommand ?? (closeViewModelCommand = new DelegateCommand(ExecuteCloseViewModel)); }
        }

        public ICommand ResizeVideoFrameCommand
        {
            get { return resizeVideoFrameCommand ?? (resizeVideoFrameCommand = new DelegateCommand(ExecuteResizeVideoFrame)); }
        }

        public ICommand RepaintVideoFrameCommand
        {
            get { return repaintVideoFrameCommand ?? (repaintVideoFrameCommand = new DelegateCommand(x => ExecuteRepaintVideoFrame())); }
        }
        #endregion

        #region Public properties
        public string WindowTitle
        {
            get { return windowTitle; }
            set { windowTitle = value; OnPropertyChanged("WindowTitle"); }
        }

        public double SignalLevel
        {
            get { return signalLevel; }
            set { signalLevel = value; OnPropertyChanged("SignalLevel"); } 
        }

        public double SignalQuality
        {
            get { return signalQuality; }
            set { signalQuality = value; OnPropertyChanged("SignalQuality"); }
        }

        public double VideoFrameWidth { set { videoFrameWidth = value; } }

        public double VideoFrameHeight { set { videoFrameHeight = value; } }

        public IntPtr RenderHandle { set { renderHandle = value; } }

        public ObservableCollection<DigitalChannel> Channels { get { return channels; } }

        public DigitalChannel SelectedChannel
        {
            get { return selectedChannel; }
            set
            {
                if (selectedChannel == value) return;

                selectedChannel = value;
                OnPropertyChanged("SelectedChannel");
                SelectChannel(value);
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            levelAndQualityTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            levelAndQualityTimer.Tick += (sender, args) =>
            {
                var status = renderer.GetLockStatus();
                SignalLevel = status.SignalStrength;
                SignalQuality = status.SignalQuality;
            };
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Decompose()
        {
            if (levelAndQualityTimer.IsEnabled)
                levelAndQualityTimer.Stop();
            renderer.Stop();
            renderer.Dispose();
        }

        private void ExecuteLoaded(object parameter)
        {
            try
            {
                renderer.Build(renderHandle);

                WindowTitle = string.Format("{0} - {1}", windowTitle, renderer.TunerDeviceName);

                levelAndQualityTimer.Start();

                var multiplex =
                    DigitalMultiplexLoadSave.LoadMultiplexFromXml(AppDomain.CurrentDomain.BaseDirectory +
                                                                  channelsXmlFileName);
                if (multiplex != null)
                    foreach (var channel in multiplex.Channels)
                        Channels.Add(channel);

                renderer.Run();
            }
            catch
            {
                MessageBox.Show("Cannot find appropriate DVBT/T2 tuner, exiting...");
                ExecuteCloseViewModel(parameter);
            }
        }

        private void ExecuteClosing()
        {
            Decompose();
        }

        private void SelectChannel(DigitalChannel value)
        {
            renderer.SelectChannel(value);
            if (value.ChannelType == DigitalChannelTypes.TV)
                renderer.ResizeVideoFrame((int)videoFrameWidth, (int)videoFrameHeight);
            else
                renderer.ResizeVideoFrame(0, 0);
            renderer.RepaintVideoFrame();
        }

        private void ExecuteSearchChannels(object parameter)
        {
            var actions = parameter as IMainWindowActions;
            if (actions == null) return;

            if (levelAndQualityTimer.IsEnabled)
                levelAndQualityTimer.Stop();
            SignalLevel = 0;
            SignalQuality = 0;

            renderer.Stop();

            var multiplex = actions.SearchChannels();
            if (multiplex != null)
            {
                DigitalMultiplexLoadSave.SaveMultiplexToXml(multiplex, AppDomain.CurrentDomain.BaseDirectory + channelsXmlFileName);

                channels.Clear();
                foreach (var channel in multiplex.Channels)
                    Channels.Add(channel);
            }

            renderer.Run();

            if (!levelAndQualityTimer.IsEnabled)
                levelAndQualityTimer.Start();
        }

        private void ExecuteMakeScreenshot()
        {
            try
            {
                var path = string.Format(@"{0}\{1}\",
                    AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var fileName = string.Concat(path, DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss"), ".jpg");

                var screenshot = renderer.GetVideoSample();
                screenshot.Save(fileName, ImageFormat.Jpeg);

                MessageBox.Show(string.Format("Screenshot successfully saved to {0}", fileName));
            }
            catch
            {
                MessageBox.Show("Failed to get & save screenshot");
            }
        }

        private static void ExecuteCloseViewModel(object parameter)
        {
            var actions = parameter as IMainWindowActions;
            if (actions == null) return;

            actions.Close(true);
        }

        private void ExecuteResizeVideoFrame(object parameter)
        {
            if (!(parameter is Rect)) return;
            if (renderer == null) return;

            Size size;
            Size arSize;
            renderer.GetVideoFrameSize(out size, out arSize);
            var rect = (Rect)parameter;

            // Very simple frame size calculation
            if (size.Width > 0 && size.Height > 0)
            {
                var ar = (double) size.Width/size.Height;
                var appAr = rect.Width/rect.Height;
                if (ar > appAr)
                    rect.Height = rect.Width/ar;
                else
                    rect.Width = rect.Height*ar;
            }

            renderer.ResizeVideoFrame((int)rect.Width, (int)rect.Height);
        }

        private void ExecuteRepaintVideoFrame()
        {
            if (renderer != null)
                renderer.RepaintVideoFrame();
        }
    }
    #endregion
}
