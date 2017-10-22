using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System.Windows.Documents;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System;
using System.Management;
using Bonjour;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using ThingMagic;
using ThingMagic.URA2;
using ThingMagic.URA2.Models;
using System.Linq;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Net.NetworkInformation;

namespace ThingMagic.URA2.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ConnectionWizardVM : ViewModelBase
    {
        #region Constants

        private const string READERTYPENOTSELECTED = "Please Select a Reader Type.";
        private const string NOSERIALREADERDETECTED = "No Serial Reader Detected. Please make sure that the device is connected to this Computer.";
        private const string NONETWORKREADERDETECTED = "No Network Reader Detected. Please make sure that the device is connected to the network.";
        private const string ADDSERIALREADERMANUALINFO = "Please type in the Serial Reader COM port.\nEx. COM1.";
        private const string ADDNETWORKREADERMANUALINFO = "Please type in  the Network Reader name.\nOR\nPlease enter the IP address where the Network Reader is connected. \nIP Format : xxx.xxx.xxx.xxx";
        private const string ADDCUSTOMREADERMANUALINFO = "Please type in custom reader in given format.";
        private const string NOREADERSELECTED = "No Reader has been Selected. Please select a reader from the list or type in reader port/IP.";
        private const int ANTENNACOUNT = 4;
        private const int PROTOCOLCOUNT = 5;

        #endregion

        #region Properties

        private static int UserControlIndex = 0;
        public static bool IsConfigurationAvaiable = false;
        // public static string ConfigFilesdirPath = System.IO.Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ConfigFiles");
        public static string ConfigFilesdirPath = System.IO.Path.Combine(@"C:\URALogs", "ConfigFiles");
        private bool isBonjourServicesInstalled = true;
        private bool isReadConnect = true;
        private Dictionary<string, string> ReaderListAutoConnect = null;
        private Dictionary<string, string> ReaderListAutoConnectModified = null;

        // Bonjour initialization fields

        private DNSSDEventManager eventManager = null;
        private DNSSDService service = null;
        private DNSSDService browser = null;
        private DNSSDService resolver = null;
        private Dictionary<string, string> HostNameIpAddress = new Dictionary<string, string>();

        private int _backgroundNotifierCallbackCount = 0;
        private Object _backgroundNotifierLock = new Object();
        List<String> servicesList = new List<String>();

        BackgroundWorker bgw = null;
        BackgroundWorker bgwConnect = null;
        bool IsFirmwareUpdateSuccess = false;

        private string firmwareUpdatePath;
        public string FirmwareUpdatePath
        {
            get { return firmwareUpdatePath; }
            set { firmwareUpdatePath = value; RaisePropertyChanged("FirmwareUpdatePath"); }
        }

        private Visibility firmwareUpdateVisibility;
        public Visibility FirmwareUpdateVisibility
        {
            get { return firmwareUpdateVisibility; }
            set
            {
                firmwareUpdateVisibility = value;
                RaisePropertyChanged("FirmwareUpdateVisibility");
                if (value == Visibility.Visible)
                    ConnectionWizardButtonVisibility = Visibility.Collapsed;
                else
                    ConnectionWizardButtonVisibility = Visibility.Visible;
            }
        }

        private string _firmwareUpdateReaderName;
        public string FirmwareUpdateReaderName
        {
            get { return _firmwareUpdateReaderName; }
            set { _firmwareUpdateReaderName = value; RaisePropertyChanged("FirmwareUpdateVisibility"); }
        }


        private Visibility _lastConnectedVisibility;
        public Visibility LastConnectedVisibility
        {
            get { return _lastConnectedVisibility; }
            set
            {
                _lastConnectedVisibility = value;
                RaisePropertyChanged("LastConnectedVisibility");
                if (value == Visibility.Visible)
                    ReaderDetailContent = "Reader Details (Auto Detected)";
                else
                    ReaderDetailContent = "Reader Details";
            }
        }

        private Visibility connectionWizardButtonVisibility;
        public Visibility ConnectionWizardButtonVisibility
        {
            get { return connectionWizardButtonVisibility; }
            set { connectionWizardButtonVisibility = value; RaisePropertyChanged("ConnectionWizardButtonVisibility"); }
        }

        private bool _gen2ProtocolIsChecked;
        public bool Gen2ProtocolIsChecked
        {
            get { return _gen2ProtocolIsChecked; }
            set { _gen2ProtocolIsChecked = value; RaisePropertyChanged("Gen2ProtocolIsChecked"); NextButtonVisibilitySelectReaderPage(); }
        }

        private Visibility _gen2ProtocolVisbility;
        public Visibility Gen2ProtocolVisbility
        {
            get { return _gen2ProtocolVisbility; }
            set { _gen2ProtocolVisbility = value; RaisePropertyChanged("Gen2ProtocolVisbility"); }
        }

        private bool _iso18000_6bIsChecked;
        public bool ISO18000_6BIsChecked
        {
            get { return _iso18000_6bIsChecked; }
            set { _iso18000_6bIsChecked = value; RaisePropertyChanged("ISO18000_6BIsChecked"); NextButtonVisibilitySelectReaderPage(); }
        }

        private Visibility _iso18000_6bVisbility;
        public Visibility ISO18000_6BVisbility
        {
            get { return _iso18000_6bVisbility; }
            set { _iso18000_6bVisbility = value; RaisePropertyChanged("ISO18000_6BVisbility"); }
        }

        private bool _ipx64IsChecked;
        public bool IPX64IsChecked
        {
            get { return _ipx64IsChecked; }
            set { _ipx64IsChecked = value; RaisePropertyChanged("IPX64IsChecked"); NextButtonVisibilitySelectReaderPage(); }
        }

        private Visibility _ipx64Visbility;
        public Visibility IPX64Visbility
        {
            get { return _ipx64Visbility; }
            set { _ipx64Visbility = value; RaisePropertyChanged("IPX64Visbility"); }
        }

        private bool _ipx256IsChecked;
        public bool IPX256IsChecked
        {
            get { return _ipx256IsChecked; }
            set { _ipx256IsChecked = value; RaisePropertyChanged("IPX256IsChecked"); NextButtonVisibilitySelectReaderPage(); }
        }

        private Visibility _ipx256Visbility;
        public Visibility IPX256Visbility
        {
            get { return _ipx256Visbility; }
            set { _ipx256Visbility = value; RaisePropertyChanged("IPX256Visbility"); }
        }

        private bool _ataIsChecked;
        public bool ATAIsChecked
        {
            get { return _ataIsChecked; }
            set { _ataIsChecked = value; RaisePropertyChanged("ATAIsChecked"); NextButtonVisibilitySelectReaderPage(); }
        }

        private Visibility _ataVisbility;
        public Visibility ATAVisbility
        {
            get { return _ataVisbility; }
            set { _ataVisbility = value; RaisePropertyChanged("ATAVisbility"); }
        }

        private bool _antennaIsChecked1;
        public bool AntennaIsChecked1
        {
            get { return _antennaIsChecked1; }
            set
            {
                _antennaIsChecked1 = value; RaisePropertyChanged("AntennaIsChecked1");
                if (RegionListSelectedItem != "Select" && IsProtocolSelected() && IsAntennaSelected())
                    IsNextButtonEnabled = true;
                else
                    IsNextButtonEnabled = false;
            }
        }

        private Visibility _antennaVisibility1;
        public Visibility AntennaVisibility1
        {
            get { return _antennaVisibility1; }
            set { _antennaVisibility1 = value; RaisePropertyChanged("AntennaVisibility1"); }
        }


        private bool _antennaIsChecked2;
        public bool AntennaIsChecked2
        {
            get { return _antennaIsChecked2; }
            set
            {
                _antennaIsChecked2 = value; RaisePropertyChanged("AntennaIsChecked2"); if (RegionListSelectedItem != "Select" && IsProtocolSelected() && IsAntennaSelected())
                    IsNextButtonEnabled = true;
                else
                    IsNextButtonEnabled = false;
            }
        }

        private Visibility _antennaVisibility2;
        public Visibility AntennaVisibility2
        {
            get { return _antennaVisibility2; }
            set { _antennaVisibility2 = value; RaisePropertyChanged("AntennaVisibility2"); }
        }


        private bool _antennaIsChecked3;
        public bool AntennaIsChecked3
        {
            get { return _antennaIsChecked3; }
            set
            {
                _antennaIsChecked3 = value; RaisePropertyChanged("AntennaIsChecked3"); if (RegionListSelectedItem != "Select" && IsProtocolSelected() && IsAntennaSelected())
                    IsNextButtonEnabled = true;
                else
                    IsNextButtonEnabled = false;
            }
        }

        private Visibility _antennaVisibility3;
        public Visibility AntennaVisibility3
        {
            get { return _antennaVisibility3; }
            set { _antennaVisibility3 = value; RaisePropertyChanged("AntennaVisibility3"); }
        }

        private bool _antennaIsChecked4;
        public bool AntennaIsChecked4
        {
            get { return _antennaIsChecked4; }
            set
            {
                _antennaIsChecked4 = value; RaisePropertyChanged("AntennaIsChecked4");

                if (RegionListSelectedItem != "Select" && IsProtocolSelected() && IsAntennaSelected())
                    IsNextButtonEnabled = true;
                else
                    IsNextButtonEnabled = false;
            }
        }

        private Visibility _antennaVisibility4;
        public Visibility AntennaVisibility4
        {
            get { return _antennaVisibility4; }
            set { _antennaVisibility4 = value; RaisePropertyChanged("AntennaVisibility4"); }
        }

        private bool _AntennaDetectionIsEnabled;
        public bool AntennaDetectionIsEnabled
        {
            get { return _AntennaDetectionIsEnabled; }
            set { _AntennaDetectionIsEnabled = value; RaisePropertyChanged("AntennaDetectionIsEnabled"); }
        }

        private bool _AntennaDetectionIsChecked;
        public bool AntennaDetectionIsChecked
        {
            get { return _AntennaDetectionIsChecked; }
            set { _AntennaDetectionIsChecked = value; RaisePropertyChanged("AntennaDetectionIsChecked"); }
        }

        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; RaisePropertyChanged("IsBusy"); }
        }

        private string busyContent;
        public string BusyContent
        {
            get { return busyContent; }
            set { busyContent = value; RaisePropertyChanged("BusyContent"); }
        }

        private string detectedreadername;
        public string DetectedReaderName
        {
            get { return detectedreadername; }
            set { detectedreadername = value; RaisePropertyChanged("DetectedReaderName"); }
        }

        private string detectedReaderLastConnected;
        public string DetectedReaderLastConnected
        {
            get { return detectedReaderLastConnected; }
            set { detectedReaderLastConnected = value; RaisePropertyChanged("DetectedReaderLastConnected"); }
        }

        private string detectedReaderType;
        public string DetectedReaderType
        {
            get { return detectedReaderType; }
            set { detectedReaderType = value; RaisePropertyChanged("DetectedReaderType"); }
        }

        private string detectedReaderModel;
        public string DetectedReaderModel
        {
            get { return detectedReaderModel; }
            set { detectedReaderModel = value; RaisePropertyChanged("DetectedReaderModel"); }
        }

        private string detectedReaderRegion;
        public string DetectedReaderRegion
        {
            get { return detectedReaderRegion; }
            set { detectedReaderRegion = value; RaisePropertyChanged("DetectedReaderRegion"); }
        }

        private string detectedSelectedAntenna;
        public string DetectedSelectedAntenna
        {
            get { return detectedSelectedAntenna; }
            set { detectedSelectedAntenna = value; RaisePropertyChanged("DetectedSelectedAntenna"); }
        }

        private string detectedReaderProtocol;
        public string DetectedReaderProtocol
        {
            get { return detectedReaderProtocol; }
            set { detectedReaderProtocol = value; RaisePropertyChanged("DetectedReaderProtocol"); }
        }

        private FrameworkElement _contentControlView;
        public FrameworkElement ContentControlView
        {
            get { return _contentControlView; }
            set { _contentControlView = value; RaisePropertyChanged("ContentControlView"); }
        }

        private Visibility _backButtonVisibility;
        public Visibility BackButtonVisibility
        {
            get { return _backButtonVisibility; }
            set { _backButtonVisibility = value; RaisePropertyChanged("BackButtonVisibility"); }
        }

        private Visibility _nextButtonVisibility;
        public Visibility NextButtonVisibility
        {
            get { return _nextButtonVisibility; }
            set { _nextButtonVisibility = value; RaisePropertyChanged("NextButtonVisibility"); }
        }

        private Visibility _connectReadButtonVisibility;
        public Visibility ConnectReadButtonVisibility
        {
            get { return _connectReadButtonVisibility; }
            set { _connectReadButtonVisibility = value; RaisePropertyChanged("ConnectReadButtonVisibility"); }
        }

        private bool _isNextButtonEnabled;
        public bool IsNextButtonEnabled
        {
            get { return _isNextButtonEnabled; }
            set { _isNextButtonEnabled = value; RaisePropertyChanged("IsNextButtonEnabled"); }
        }

        private bool _isConnectionSettingButtonEnabled;
        public bool IsConnectionSettingButtonEnabled
        {
            get { return _isConnectionSettingButtonEnabled; }
            set { _isConnectionSettingButtonEnabled = value; RaisePropertyChanged("IsConnectionSettingButtonEnabled"); }
        }

        private bool _isAdvancedSettingButtonEnabled;
        public bool IsAdvancedSettingButtonEnabled
        {
            get { return _isAdvancedSettingButtonEnabled; }
            set { _isAdvancedSettingButtonEnabled = value; RaisePropertyChanged("IsAdvancedSettingButtonEnabled"); }
        }

        private bool _isSelectReaderButtonEnabled;
        public bool IsSelectReaderButtonEnabled
        {
            get { return _isSelectReaderButtonEnabled; }
            set { _isSelectReaderButtonEnabled = value; RaisePropertyChanged("IsSelectReaderButtonEnabled"); }
        }

        private string _nextConnectButtonContent;
        public string NextConnectButtonContent
        {
            get { return _nextConnectButtonContent; }
            set { _nextConnectButtonContent = value; RaisePropertyChanged("NextConnectButtonContent"); }
        }

        private string _backChangeReaderButtonContnet;
        public string BackChangeReaderButtonContnet
        {
            get { return _backChangeReaderButtonContnet; }
            set { _backChangeReaderButtonContnet = value; RaisePropertyChanged("BackChangeReaderButtonContnet"); }
        }

        private string _hostAddress;
        public string HostAddress
        {
            get { return _hostAddress; }
            set 
            { 
                _hostAddress = value; RaisePropertyChanged("NextConnectButtonContent"); 
                if (IsAddCustomReader)
                    IsNextButtonEnabled = (string.IsNullOrWhiteSpace(HostAddress)) ? false : true; 
            }
        }

        private bool _isAddCustomReader;
        public bool IsAddCustomReader
        {
            get { return _isAddCustomReader; }
            set { _isAddCustomReader = value; RaisePropertyChanged("IsAddCustomReader"); IsNextButtonEnabled = (string.IsNullOrWhiteSpace(HostAddress)) ? false : true; }
        }

        private bool _isSerialReader;
        public bool IsSerialReader
        {
            get { return _isSerialReader; }
            set { _isSerialReader = value; RaisePropertyChanged("IsSerialReader"); }
        }

        private bool _isNetworkReader;
        public bool IsNetworkReader
        {
            get { return _isNetworkReader; }
            set { _isNetworkReader = value; RaisePropertyChanged("IsNetworkReader"); }
        }

        private bool _isAddManualChecked;
        public bool IsAddManualChecked
        {
            get { return _isAddManualChecked; }
            set { _isAddManualChecked = value; RaisePropertyChanged("IsAddManualChecked"); }
        }

        private string _statusWarningText;
        public string StatusWarningText
        {
            get { return _statusWarningText; }
            set { _statusWarningText = value; RaisePropertyChanged("StatusWarningText"); }
        }

        private Brush _statusWarningColor;
        public Brush StatusWarningColor
        {
            get { return _statusWarningColor; }
            set { _statusWarningColor = value; RaisePropertyChanged("StatusWarningColor"); }
        }

        private ObservableCollection<string> _readerList;
        public ObservableCollection<string> ReaderList
        {
            get { return _readerList; }
            set { _readerList = value; RaisePropertyChanged("ReaderList"); }
        }

        private string _readerListSelectedItem;
        public string ReaderListSelectedItem
        {
            get { return _readerListSelectedItem; }
            set
            {
                _readerListSelectedItem = value;
                RaisePropertyChanged("ReaderListSelectedItem");
                if ((IsSerialReader || IsNetworkReader) && !IsAddManualChecked)
                {
                    StatusWarningText = "";
                    IsNextButtonEnabled = (string.IsNullOrWhiteSpace(ReaderURI())) ? false : true;
                }
                else
                    IsNextButtonEnabled = (string.IsNullOrWhiteSpace(HostAddress)) ? false : true;
            }
        }

        private string _readerListText;
        public string ReaderListText
        {
            get { return _readerListText; }
            set
            {
                _readerListText = value;
                RaisePropertyChanged("ReaderListText");
                IsNextButtonEnabled = string.IsNullOrWhiteSpace(ReaderListText) ? false : true;
            }
        }


        private string _regionListSelectedItem;
        public string RegionListSelectedItem
        {
            get { return _regionListSelectedItem; }
            set
            {
                _regionListSelectedItem = value;
                if (objReader != null && IsSerialReader && value.ToLower() != "select")
                    objReader.ParamSet("/reader/region/id", Enum.Parse(typeof(Reader.Region), value));
                //else if (RegionListSelectedItem.ToLower() == "select")
                //objReader.ParamSet("/reader/region/id", Reader.Region.UNSPEC);
                if (value != "Select" && IsProtocolSelected() && IsAntennaSelected())
                    IsNextButtonEnabled = true;
                else
                    IsNextButtonEnabled = false;
                RaisePropertyChanged("RegionListSelectedItem");
            }
        }

        private ObservableCollection<string> _regionList;
        public ObservableCollection<string> RegionList
        {
            get { return _regionList; }
            set { _regionList = value; RaisePropertyChanged("RegionList"); }
        }

        private ObservableCollection<string> _BaudRateComboBoxSource;
        public ObservableCollection<string> BaudRateComboBoxSource
        {
            get { return _BaudRateComboBoxSource; }
            set { _BaudRateComboBoxSource = value; RaisePropertyChanged("BaudRateComboBoxSource"); }
        }

        private string _baudRateSelectedItem;
        public string BaudRateSelectedItem
        {
            get { return _baudRateSelectedItem; }
            set
            {
                _baudRateSelectedItem = value;
                if (objReader != null && IsSerialReader && BaudRateSelectedItem != null)
                    objReader.ParamSet("/reader/baudRate", Int32.Parse(BaudRateSelectedItem));
                RaisePropertyChanged("BaudRateSelectedItem");
            }
        }

        private Visibility _baudRateVisibility;
        public Visibility BaudRateVisibility
        {
            get { return _baudRateVisibility; }
            set { _baudRateVisibility = value; RaisePropertyChanged("BaudRateVisibility"); }
        }

        private string _selectedReaderName;
        public string SelectedReaderName
        {
            get { return _selectedReaderName; }
            set { _selectedReaderName = value; RaisePropertyChanged("SelectedReaderName"); }
        }

        private string _readerDetailContent;
        public string ReaderDetailContent
        {
            get { return _readerDetailContent; }
            set { _readerDetailContent = value; RaisePropertyChanged("ReaderDetailContent"); }
        }

        private List<FrameworkElement> _userControlList = null;


        //Reader Properties

        string uri = string.Empty;
        /// <summary>
        /// Define a reader variable
        /// </summary>
        Reader objReader = null;
        string model = null;


        #endregion

        #region CommandProperties

        public ICommand CancelCommand { get; private set; }
        public ICommand NextConnectCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand WizardButtonCommand { get; private set; }
        public ICommand ReaderTypeCheckedCommand { get; private set; }
        public ICommand ConnectReadCommand { get; private set; }
        public ICommand OpenDialogCommand { get; private set; }
        public ICommand UpdateFirmwareCommand { get; private set; }
        public ICommand TestCommand { get; private set; }
        public ICommand ClosingCommand { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ConnectionWizardVM()
        {
            try
            {
                Thread th = new Thread(new ThreadStart(A));
                th.Start();
                // HostAddress = "10.2.0.104:5000";
                bgw = new BackgroundWorker();
                bgw.WorkerSupportsCancellation = true;
                bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
                bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);

                bgwConnect = new BackgroundWorker();
                bgwConnect.DoWork += new DoWorkEventHandler(bgwConnect_DoWork);
                bgwConnect.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwConnect_RunWorkerCompleted);

                try
                {
                    // Bonjour Related
                    eventManager = new DNSSDEventManager();
                    eventManager.ServiceFound += new _IDNSSDEvents_ServiceFoundEventHandler(this.ServiceFound);
                    eventManager.ServiceResolved += new _IDNSSDEvents_ServiceResolvedEventHandler(this.ServiceResolved);
                    eventManager.ServiceLost += new _IDNSSDEvents_ServiceLostEventHandler(this.ServiceLost);
                    service = new DNSSDService();
                }
                catch (Exception)
                {
                    isBonjourServicesInstalled = false;
                }

                PopulateNetworkReader();
                IsBusy = false;

                _userControlList = new List<FrameworkElement>();
                _userControlList.Add(new ucWizardSelectReader());
                _userControlList.Add(new ucWizardReaderSetting());
                _userControlList.Add(new ucWizardConnectRead());

                // Creating BaudRate ItemSource
                BaudRateComboBoxSource = new ObservableCollection<string>();
                string[] baudrate = new string[] { "9600", "19200", "38400", "115200", "230400", "460800", "921600" };
                foreach (string temp in baudrate)
                    BaudRateComboBoxSource.Add(temp);

                ReaderList = new ObservableCollection<string>();
                ContentControlView = _userControlList[UserControlIndex];
                ButtonVisibility();
                LastConnectedVisibility = Visibility.Collapsed;

                // Button Command Handler Region
                CancelCommand = new RelayCommand<object>(OnCancel);
                NextConnectCommand = new RelayCommand<object>(OnNextConnect);
                BackCommand = new RelayCommand(OnBack);
                WizardButtonCommand = new RelayCommand<string>(OnWizardButton);
                ReaderTypeCheckedCommand = new RelayCommand(OnReaderTypeChecked);
                ConnectReadCommand = new RelayCommand<object>(OnConnectRead);
                OpenDialogCommand = new RelayCommand(OnOpenDialog);
                UpdateFirmwareCommand = new RelayCommand(OnUpdateFirmware);
                TestCommand = new RelayCommand(OnTest);

                ReaderListAutoConnect = new Dictionary<string, string>();
                ReaderListAutoConnectModified = new Dictionary<string, string>();
                foreach (string temp in GetComPortNames())
                {
                    MatchCollection mc = Regex.Matches(temp, @"(?<=\().+?(?=\))");
                    foreach (Match m in mc)
                    {
                        ReaderListAutoConnectModified.Add(m.ToString().ToUpper(), "serial");
                        ReaderListAutoConnect.Add(m.ToString().ToUpper(), temp);
                    }
                }

                //foreach (string temp in HostNameIpAddress.Values)
                //    ReaderListAutoConnect.Add(temp, "network");

                if (Directory.Exists(ConfigFilesdirPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(ConfigFilesdirPath);
                    FileInfo[] files = dir.GetFiles().Where(p => ((DateTime.Now.ToLocalTime() - p.LastWriteTime.ToLocalTime()) <= TimeSpan.FromDays(3))).OrderByDescending(p => p.LastWriteTime).ToArray();
                    string[] configurationfiles = files.Select(p => Path.GetFileNameWithoutExtension(p.FullName)).ToArray();
                    foreach (string temp in configurationfiles)
                    {
                        string comport = temp.Remove(temp.LastIndexOf('_'));
                        if (comport.ToLower().Contains("com"))
                        {
                            if (ReaderListAutoConnectModified.Keys.Contains(comport.ToUpper()))
                            {
                                if (ReaderListAutoConnectModified[comport.ToUpper()] == "serial")
                                {
                                    IsSerialReader = true; IsNetworkReader = false;
                                }
                                else if (ReaderListAutoConnectModified[comport.ToUpper()] == "network")
                                {
                                    IsNetworkReader = true; IsSerialReader = false;
                                }
                                IsSerialReader = true; IsNetworkReader = false;
                                ReaderConnectionDetail.ReaderName = ReaderListAutoConnect[comport.ToUpper()];
                                if (IsSerialReader)
                                {
                                    if (!IsReaderConnected())
                                    {
                                        GetReaderDetailsFromFile(comport.ToUpper());
                                        if (DetectedReaderModel == model && !(string.IsNullOrWhiteSpace(DetectedReaderRegion) || DetectedReaderRegion.Contains("Select")))
                                        {
                                            IsConfigurationAvaiable = true;
                                            AutoConnectToURA();
                                            LastConnectedVisibility = Visibility.Visible;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            IsNetworkReader = true; IsSerialReader = false;
                            //Ping ping = new Ping();
                            //try
                            //{
                            //    PingReply reply = ping.Send(filename.ToUpper());
                            //    if (reply.Status == IPStatus.Success)
                            //    {
                            //        ReaderConnectionDetail.ReaderName = filename.ToUpper();
                            //        if (!IsReaderConnected())
                            //        {
                            //            GetReaderDetailsFromFile(filename.ToUpper());
                            //            if (DetectedReaderModel == model && !(string.IsNullOrWhiteSpace(DetectedReaderRegion) || DetectedReaderRegion.Contains("Select")))
                            //            {
                            //                IsConfigurationAvaiable = true;
                            //                AutoConnectToURA();
                            //                LastConnectedVisibility = Visibility.Visible;
                            //            }
                            //            break;
                            //        }
                            //    }
                            //}
                            //catch (Exception)
                            //{ }
                        }
                    }
                }

                if (!IsConfigurationAvaiable)
                {
                    SelectReaderType();
                }

                StatusWarningText = "";
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        public void A()
        {
            IsNetworkReader = true; IsSerialReader = false;
            Dispatcher dispatchObject = Application.Current.Dispatcher;
            dispatchObject.BeginInvoke(new ThreadStart(delegate()
            {
                if (isBonjourServicesInstalled)
                {
                    _backgroundNotifierCallbackCount = 0;
                    if (browser != null)
                    {
                        browser.Stop();
                        servicesList.Clear();
                    }

                    HostNameIpAddress.Clear();
                    //cmbFixedReaderAddr.Items.Clear();
                    string[] serviceTypes = { "_llrp._tcp", "_m4api._udp." };//,
                    foreach (string serviceType in serviceTypes)
                    {
                        browser = service.Browse(0, 0, serviceType, null, eventManager);
                    }
                    Thread.Sleep(500);
                    while (0 < _backgroundNotifierCallbackCount)
                    {
                        Thread.Sleep(100);
                    }
                }
            }));
        }

        #endregion

        #region WPF Control Command Handler

        /// <summary>
        /// To be deleted before release
        /// </summary>
        private void OnTest()
        {
            //try
            //{
            //    NextButtonVisibility = Visibility.Collapsed;
            //    ContentControlView = new ucWizardFirmwareUpadate();
            //    FirmwareUpdateVisibility = Visibility.Visible;
            //}
            //catch (Exception ex)
            //{
            //    ShowErrorMessage(ex.Message);
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OnConnectRead(object obj)
        {
            try
            {
                ValidateReadConnectPage(obj);
            }
            catch (Exception ex)
            {
                if (ex is FAULT_BL_INVALID_IMAGE_CRC_Exception || ex is FAULT_BL_INVALID_APP_END_ADDR_Exception)
                {
                    ShowErrorMessage(ex);
                }
                else
                {
                    ShowErrorMessage(ex);
                    if (App.Current.MainWindow == null)
                    {
                        Window win = new ConnectionWizard();
                        win.Show();
                    }
                    else
                    {
                        Window win = (Window)App.Current.MainWindow;
                        win.Show();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnBack()
        {
            try
            {
                StatusWarningText = "";
                LastConnectedVisibility = Visibility.Collapsed;
                if (BackChangeReaderButtonContnet == "Change Reader")
                {
                    UserControlIndex = 0;
                    SelectReaderType();
                    IsConfigurationAvaiable = false;
                }
                else
                {
                    if (ContentControlView is ThingMagic.URA2.ucWizardFirmwareUpadate)
                        UserControlIndex = 0;
                    else
                        UserControlIndex--;
                }
                ContentControlView = _userControlList[UserControlIndex];
                ButtonVisibility();
                if (UserControlIndex == 0)
                {
                    if (objReader != null)
                    {
                        objReader.Destroy();
                        objReader = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
                if (NextConnectButtonContent.ToLower() == "next")
                {
                    ContentControlView = _userControlList[--UserControlIndex];
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void OnNextConnect(object obj)
        {
            try
            {
                StatusWarningText = "";
                if (NextConnectButtonContent.ToLower() == "next")
                {
                    if (UserControlIndex == 0)
                    {
                        if (ValidateSelectReaderPage())
                        {
                            ReaderSettingsPageIntialize();
                        }
                    }
                    else if (UserControlIndex == 1)
                    {
                        if (ValidateConnectReaderPage())
                        {
                            ContentControlView = _userControlList[++UserControlIndex];
                            DetectedReaderName = ReaderURI();
                            DetectedReaderRegion = RegionListSelectedItem;
                            if (IsSerialReader)
                                DetectedReaderType = "Serial Reader";
                            else if (IsNetworkReader)
                                DetectedReaderType = "Network Reader";
                            else if (IsAddCustomReader)
                                DetectedReaderType = "Custom Transport Reader";
                            else
                                DetectedReaderType = "";

                            DetectedSelectedAntenna = "";
                            bool[] AntennaCheckedBool = { AntennaIsChecked1, AntennaIsChecked2, AntennaIsChecked3, AntennaIsChecked4 };
                            int tempint = 0;
                            foreach (bool temp in AntennaCheckedBool)
                            {
                                if (temp)
                                    DetectedSelectedAntenna = DetectedSelectedAntenna + (tempint + 1).ToString() + ",";
                                tempint++;
                            }
                            DetectedSelectedAntenna = DetectedSelectedAntenna.TrimEnd(',');

                            DetectedReaderProtocol = "";
                            bool[] ProtocolList = { Gen2ProtocolIsChecked, ISO18000_6BIsChecked, IPX64IsChecked, IPX256IsChecked, ATAIsChecked };
                            tempint = 0;
                            foreach (bool temp in ProtocolList)
                            {
                                if (temp)
                                {
                                    switch (tempint)
                                    {
                                        case 0:
                                            DetectedReaderProtocol = DetectedReaderProtocol + "Gen2" + ",";
                                            break;
                                        case 1:
                                            DetectedReaderProtocol = DetectedReaderProtocol + "ISO18000-6B" + ",";
                                            break;
                                        case 2:
                                            DetectedReaderProtocol = DetectedReaderProtocol + "IPX64" + ",";
                                            break;
                                        case 3:
                                            DetectedReaderProtocol = DetectedReaderProtocol + "IPX256" + ",";
                                            break;
                                        case 4:
                                            DetectedReaderProtocol = DetectedReaderProtocol + "ATA" + ",";
                                            break;
                                        default:
                                            DetectedReaderProtocol = DetectedReaderProtocol + "";
                                            break;
                                    }
                                }
                                tempint++;
                            }
                            DetectedReaderProtocol = DetectedReaderProtocol.TrimEnd(',');
                            DetectedReaderLastConnected = "";
                        }
                    }
                    ButtonVisibility();
                }
                else if (NextConnectButtonContent.ToLower() == "connect")
                {
                    isReadConnect = false;
                    OnConnectRead(obj);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OnCancel(object obj)
        {
            try
            {
                if (objReader != null)
                {
                    objReader.Destroy();
                    objReader = null;
                }
                Window win = (Window)obj;
                Window win1 = new Main();
                win1.Show();
                win.Close();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radioButtonContent"></param>
        private void OnReaderTypeChecked()
        {
            try
            {
                StatusWarningText = "";
                SelectReaderPageIntialize();
                if (IsSerialReader)
                {
                    if (IsAddManualChecked)
                    {
                        SetStatusWarningMessage(ADDSERIALREADERMANUALINFO, Brushes.Blue);
                    }
                    else
                    {
                        if (IsReaderListNull())
                        {
                            SetStatusWarningMessage(NOSERIALREADERDETECTED, Brushes.Red);
                        }
                        else
                        {
                            ReaderListText = ReaderList[0];
                            ReaderListSelectedItem = ReaderList[0];
                        }
                    }
                }
                else if (IsNetworkReader)
                {
                    if (IsAddManualChecked)
                    {
                        SetStatusWarningMessage(ADDNETWORKREADERMANUALINFO, Brushes.Blue);
                    }
                    else
                    {
                        ReaderList = new ObservableCollection<string>();
                        if (IsReaderListNull())
                        {
                            SetStatusWarningMessage(NONETWORKREADERDETECTED, Brushes.Red);
                        }
                        else
                        {
                            ReaderListText = ReaderList[0];
                            ReaderListSelectedItem = ReaderList[0];
                        }
                    }
                }
                else if (IsAddCustomReader)
                {
                    SetStatusWarningMessage(ADDCUSTOMREADERMANUALINFO, Brushes.Blue);
                }
                else
                {
                    ShowErrorMessage(READERTYPENOTSELECTED);
                }

                // Not to be commented out
                //IsNextButtonEnabled = (ReaderListSelectedItem == null) ? false : true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void SelectReaderType()
        {
            try
            {
                IsNetworkReader = true; IsSerialReader = false;
                OnReaderTypeChecked();
                if (ReaderList != null)
                {
                    if (ReaderList.Count == 0)
                    {
                        IsSerialReader = true; IsNetworkReader = false;
                        OnReaderTypeChecked();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnOpenDialog()
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Firmware File (.*sim; *.deb)|*sim; *.deb|" + "ThingMagic Firmware (*.tmfw)|*.tmfw";
                openDialog.Title = "Select Firmware File";
                openDialog.ShowDialog();
                FirmwareUpdatePath = openDialog.FileName.ToString();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnUpdateFirmware()
        {
            try
            {
                if (IsSerialReader || IsAddCustomReader)
                {
                    if (!FirmwareUpdatePath.Contains(".sim"))
                    {
                        ShowErrorMessage("Invalid File Extension. Please select a .sim extension file");
                        return;
                    }
                }
                else if (IsNetworkReader)
                {
                    if (!(FirmwareUpdatePath.Contains(".tmfw") || FirmwareUpdatePath.Contains(".deb")))
                    {
                        ShowErrorMessage("Invalid File Extension. Please select a .tmfw (for M6,Astra etc.) or .deb (for Sargas Reader) extension file");
                        return;
                    }
                }
                else
                {
                    ContentControlView = _userControlList[UserControlIndex = 0];
                }

                IsBusy = true;
                BusyContent = "\n" + FirmwareUpdateReaderName + " : Firmware Update In Progress... \n";
                IsFirmwareUpdateSuccess = false;
                if (!bgw.IsBusy)
                    bgw.RunWorkerAsync();
                else
                {
                    RestartApplication();
                }

            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                System.IO.FileStream firmware = new System.IO.FileStream(FirmwareUpdatePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                if (objReader != null)
                {
                    objReader.FirmwareLoad(firmware);
                    IsFirmwareUpdateSuccess = true;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("successful"))
                {
                    IsFirmwareUpdateSuccess = true;
                    MessageBox.Show(ex.Message, "URA: Firmware Update Status", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    IsFirmwareUpdateSuccess = false;
                    ShowErrorMessage(ex.Message);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            if (IsFirmwareUpdateSuccess)
            {
                ContentControlView = _userControlList[UserControlIndex = 0];
                NextButtonVisibility = Visibility.Visible;
                StatusWarningText = "";
                FirmwareUpdateVisibility = Visibility.Collapsed;
                ButtonVisibility();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttonContent"></param>
        private void OnWizardButton(string buttonContent)
        {

        }

        #endregion

        #region Command Handler

        /// <summary>
        /// 
        /// </summary>
        private void AutoConnectToURA()
        {
            try
            {
                UserControlIndex = 2;
                ContentControlView = _userControlList[UserControlIndex];
                ButtonVisibility();
                //IsConfigurationAvaiable = false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CheckForLastSaveSettings()
        {
            if (!(Directory.Exists(ConfigFilesdirPath)))
            {
                return false;
            }
            else
            {
                string fileName = ReaderURI() + ".urac";
                if (Directory.Exists(ConfigFilesdirPath))
                {
                    string[] configurationFiles = null;
                    configurationFiles = Directory.GetFiles(ConfigFilesdirPath, "*.urac").Select(p => Path.GetFileName(p)).ToArray();

                    foreach (string t in configurationFiles)
                    {
                        if (t.ToLower().Contains(fileName.ToLower()))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetReaderDetailsFromFile(string fname)
        {
            try
            {
                string fileFullPath = System.IO.Path.Combine(ConnectionWizardVM.ConfigFilesdirPath, fname + "_config" + ".txt");
                FileInfo fileinfo = new FileInfo(fileFullPath);
                //DetectedReaderLastConnected = fileinfo.LastWriteTimeUtc.ToLocalTime().ToString();
                DetectedReaderName = ReaderConnectionDetail.ReaderName;
                if (File.Exists(fileFullPath))
                {
                    string[] lines = System.IO.File.ReadAllLines(fileFullPath);
                    foreach (string line in lines)
                    {
                        if (line.Contains("/reader/region/id"))
                        {
                            DetectedReaderRegion = (line.Split('='))[1];
                        }
                        else if (line.Contains("/application/readwriteOption/Antennas"))
                        {
                            DetectedSelectedAntenna = (line.Split('='))[1];
                        }
                        else if (line.Contains("/application/readwriteOption/Protocols"))
                        {
                            DetectedReaderProtocol = (line.Split('='))[1];
                        }
                        else if (line.Contains("/application/connect/readerType"))
                        {
                            DetectedReaderType = (line.Split('='))[1];
                        }
                        else if (line.Contains("readermodel"))
                        {
                            DetectedReaderModel = (line.Split('='))[1];
                        }
                        else if (line.Contains("lastconnected"))
                        {
                            DetectedReaderLastConnected = Convert.ToDateTime((line.Split('='))[1]).ToLocalTime().ToString();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void PopulateNetworkReader()
        {
            try
            {
                if (isBonjourServicesInstalled)
                {
                    _backgroundNotifierCallbackCount = 0;
                    if (browser != null)
                    {
                        browser.Stop();
                        servicesList.Clear();
                    }

                    HostNameIpAddress.Clear();
                    //cmbFixedReaderAddr.Items.Clear();
                    string[] serviceTypes = { "_llrp._tcp", "_m4api._udp." };//, 

                    foreach (string serviceType in serviceTypes)
                    {
                        browser = service.Browse(0, 0, serviceType, null, eventManager);
                    }
                    Thread.Sleep(500);
                    while (0 < _backgroundNotifierCallbackCount)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsReaderConnected()
        {
            try
            {
                if (objReader != null)
                {
                    objReader.Destroy();
                    objReader = null;
                }
                objReader = CreateReaderObject(ReaderConnectionDetail.ReaderName);
                if (objReader != null)
                {
                    objReader.Connect();
                    model = objReader.ParamGet("/reader/version/model").ToString();
                }

                return false;
            }
            catch (Exception)
            {
                if (objReader != null)
                {
                    objReader.Destroy();
                    objReader = null;
                }
                return true;
            }
        }

        /// <summary>
        /// Configure antennas
        /// </summary>
        public void ConfigureAntennaBoxes()
        {
            // Cast int[] return values to IList<int> instead of int[] to get Contains method
            IList<int> existingAntennas = null;
            IList<int> detectedAntennas = null;
            IList<int> validAntennas = null;

            if (null == objReader)
            {
                int[] empty = new int[0];
                existingAntennas = detectedAntennas = validAntennas = empty;
            }
            else
            {
                bool checkPort;
                switch (objReader.ParamGet("/reader/version/model").ToString())
                {
                    case "Astra":
                        checkPort = true;
                        break;
                    default:
                        checkPort = (bool)objReader.ParamGet("/reader/antenna/checkPort");
                        break;
                }
                existingAntennas = (IList<int>)objReader.ParamGet("/reader/antenna/PortList");
                detectedAntennas = (IList<int>)objReader.ParamGet("/reader/antenna/connectedPortList");
                validAntennas = checkPort ? detectedAntennas : existingAntennas;
                Visibility[] AntennaVisibility = { AntennaVisibility1, AntennaVisibility2, AntennaVisibility3, AntennaVisibility4 };
                bool[] AntennaCheckedBool = { AntennaIsChecked1, AntennaIsChecked2, AntennaIsChecked3, AntennaIsChecked4 };

                for (int i = 0; i < ANTENNACOUNT; i++)
                {
                    switch (i)
                    {
                        case 0:
                            AntennaVisibility1 = (existingAntennas.Contains(i + 1)) ? Visibility.Visible : Visibility.Collapsed;
                            AntennaIsChecked1 = (detectedAntennas.Contains(i + 1)) ? true : false;
                            break;
                        case 1:
                            AntennaVisibility2 = (existingAntennas.Contains(i + 1)) ? Visibility.Visible : Visibility.Collapsed;
                            AntennaIsChecked2 = (detectedAntennas.Contains(i + 1)) ? true : false;
                            break;
                        case 2:
                            AntennaVisibility3 = (existingAntennas.Contains(i + 1)) ? Visibility.Visible : Visibility.Collapsed;
                            AntennaIsChecked3 = (detectedAntennas.Contains(i + 1)) ? true : false;
                            break;
                        case 3:
                            AntennaVisibility4 = (existingAntennas.Contains(i + 1)) ? Visibility.Visible : Visibility.Collapsed;
                            AntennaIsChecked4 = (detectedAntennas.Contains(i + 1)) ? true : false;
                            break;
                    }
                }
            }
        }

        private void ConfigureProtocols()
        {
            TagProtocol[] supportedProtocols = null;
            supportedProtocols = (TagProtocol[])objReader.ParamGet("/reader/version/supportedProtocols");
            Gen2ProtocolVisbility = ISO18000_6BVisbility = IPX256Visbility = IPX64Visbility = ATAVisbility = Visibility.Collapsed;

            if (null != supportedProtocols)
            {
                foreach (TagProtocol proto in supportedProtocols)
                {
                    switch (proto)
                    {
                        case TagProtocol.GEN2:
                            Gen2ProtocolVisbility = Visibility.Visible;
                            Gen2ProtocolIsChecked = true;
                            break;
                        case TagProtocol.ISO180006B:
                            ISO18000_6BVisbility = Visibility.Visible;
                            break;
                        case TagProtocol.IPX64:
                            IPX64Visbility = Visibility.Visible;
                            break;
                        case TagProtocol.IPX256:
                            IPX256Visbility = Visibility.Visible;
                            break;
                        case TagProtocol.ATA:
                            ATAVisbility = Visibility.Visible;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ButtonVisibility()
        {
            BackChangeReaderButtonContnet = IsConfigurationAvaiable ? "Change Reader" : "Back";
            FirmwareUpdateVisibility = Visibility.Collapsed;
            if (UserControlIndex == 0)
            {
                BackButtonVisibility = Visibility.Hidden;
                NextButtonVisibility = Visibility.Visible;
                ConnectReadButtonVisibility = Visibility.Collapsed;
                NextConnectButtonContent = "Next";
                IsAdvancedSettingButtonEnabled = true;
                IsConnectionSettingButtonEnabled = true;
                IsSelectReaderButtonEnabled = false;
                if (IsAddCustomReader)
                {
                    IsNextButtonEnabled = (string.IsNullOrWhiteSpace(HostAddress) ? false : true);
                }
                else
                {
                    IsNextButtonEnabled = string.IsNullOrWhiteSpace(ReaderURI()) ? false : true;
                }
            }
            else if (UserControlIndex == 2)
            {
                BackButtonVisibility = Visibility.Visible;
                NextButtonVisibility = Visibility.Visible;
                if (!(string.IsNullOrWhiteSpace(DetectedSelectedAntenna) || (string.IsNullOrWhiteSpace(DetectedReaderProtocol))))
                    ConnectReadButtonVisibility = Visibility.Visible;
                else
                    ConnectReadButtonVisibility = Visibility.Collapsed;
                NextConnectButtonContent = "Connect";
                IsAdvancedSettingButtonEnabled = false;
                IsConnectionSettingButtonEnabled = true;
                IsSelectReaderButtonEnabled = true;
                IsNextButtonEnabled = true;
            }
            else if (UserControlIndex == 1)
            {
                BackButtonVisibility = Visibility.Visible;
                NextButtonVisibility = Visibility.Visible;
                ConnectReadButtonVisibility = Visibility.Collapsed;
                NextConnectButtonContent = "Next";
                IsAdvancedSettingButtonEnabled = true;
                IsConnectionSettingButtonEnabled = false;
                IsSelectReaderButtonEnabled = true;
                if (RegionListSelectedItem != "Select" && IsProtocolSelected() && IsAntennaSelected())
                    IsNextButtonEnabled = true;
                else
                    IsNextButtonEnabled = false;
            }
        }

        private bool IsProtocolSelected()
        {
            bool[] ProtocolList = { Gen2ProtocolIsChecked, ISO18000_6BIsChecked, IPX64IsChecked, IPX256IsChecked, ATAIsChecked };
            foreach (bool temp in ProtocolList)
            {
                if (temp)
                    return true;
            }
            return false;
        }

        private bool IsAntennaSelected()
        {
            bool[] AntennaCheckedBool = { AntennaIsChecked1, AntennaIsChecked2, AntennaIsChecked3, AntennaIsChecked4 };
            foreach (bool temp in AntennaCheckedBool)
            {
                if (temp)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SelectReaderPageIntialize()
        {
            try
            {
                ReaderListText = "";
                ReaderList = null;
                ReaderList = new ObservableCollection<string>();
                if (IsSerialReader)
                {
                    foreach (string temp in GetComPortNames())
                        ReaderList.Add(temp);
                }
                else if (IsNetworkReader)
                {
                    if (isBonjourServicesInstalled)
                    {
                        _backgroundNotifierCallbackCount = 0;
                        if (browser != null)
                        {
                            browser.Stop();
                            servicesList.Clear();
                        }

                        HostNameIpAddress.Clear();

                        //cmbFixedReaderAddr.Items.Clear();
                        string[] serviceTypes = { "_llrp._tcp", "_m4api._udp." };//,
                        foreach (string serviceType in serviceTypes)
                        {
                            browser = service.Browse(0, 0, serviceType, null, eventManager);
                        }
                        Thread.Sleep(500);
                        while (0 < _backgroundNotifierCallbackCount)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                else
                {
                    SetStatusWarningMessage(READERTYPENOTSELECTED, Brushes.Red);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReaderSettingsPageIntialize()
        {
            if (objReader != null)
            {
                objReader.Destroy();
                objReader = null;
            }
            try
            {
                BaudRateVisibility = Visibility.Collapsed;
                IsBusy = true;
                if (!string.IsNullOrWhiteSpace(ReaderListSelectedItem))
                {
                    BusyContent = "Checking connection to " + (IsAddCustomReader ? HostAddress : ReaderListSelectedItem);
                }
                else
                {
                    BusyContent = "Checking connection to " + (IsAddCustomReader ? HostAddress : ReaderListText);
                }
                if (!bgwConnect.IsBusy)
                    bgwConnect.RunWorkerAsync();
                else
                {
                    RestartApplication();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void bgwConnect_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (IsSerialReader || IsNetworkReader)
                {
                    objReader = CreateReaderObject(ReaderURI());
                }
                else if (IsAddCustomReader)
                {
                    objReader = CreateReaderObject(HostAddress);
                }
                if (objReader != null)
                {
                    objReader.Connect();
                }
            }
            catch (Exception)
            {
                IsBusy = false;
            }
        }

        void bgwConnect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsBusy)
            {
                IsBusy = false;
                ContentControlView = _userControlList[++UserControlIndex];
                ButtonVisibility();
                SelectedReaderName = ReaderURI();
                RegionList = GetSupportedRegion();
                if (((Reader.Region)objReader.ParamGet("/reader/region/id")) != Reader.Region.UNSPEC)
                {
                    //set the region on module
                    RegionListSelectedItem = ((Reader.Region)objReader.ParamGet("/reader/region/id")).ToString();
                }
                else
                {
                    RegionListSelectedItem = "Select";
                }

                ConfigureAntennaBoxes();

                ConfigureProtocols();

                // Set BaudRate
                if (IsSerialReader)
                {
                    BaudRateVisibility = Visibility.Visible;
                    BaudRateSelectedItem = this.objReader.ParamGet("/reader/baudRate").ToString();
                }

                DetectedReaderModel = this.objReader.ParamGet("/reader/version/model").ToString();

            }
            else
            {
                //if (StatusWarningText.Contains("Firmware is broken"))
                //{
                //    ContentControlView = new ucWizardFirmwareUpadate();
                //    FirmwareUpdateVisibility = Visibility.Visible;
                //    NextButtonVisibility = Visibility.Collapsed;
                //}
                //else
                //{
                ContentControlView = _userControlList[UserControlIndex = 0];
                ButtonVisibility();
                //}
                ShowErrorMessage("Unable to connect to " + ReaderURI() +".\nPlease check if the device is properly connected or Device might be in use.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReaderName"></param>
        /// <returns></returns>
        private Reader CreateReaderObject(string ReaderName)
        {
            if (IsSerialReader)
            {
                if (!ValidatePortNumber(ReaderName) || ReaderName == "")
                {
                    throw new IOException();
                }

                // Creates a Reader Object for operations on the Reader.
                string readerUri = ReaderName;
                //Regular Expression to get the com port number from comport name .
                //for Ex: If The Comport name is "USB Serial Port (COM19)" by using this 
                // regular expression will get com port number as "COM19".
                MatchCollection mc = Regex.Matches(readerUri, @"(?<=\().+?(?=\))");
                foreach (Match m in mc)
                {
                    if (!string.IsNullOrWhiteSpace(m.ToString()))
                        readerUri = m.ToString();
                }
                return Reader.Create(string.Concat("tmr:///", readerUri));
            }
            else if (IsNetworkReader)
            {
                string key = HostNameIpAddress.Keys.Where(x => x.Contains(ReaderName)).FirstOrDefault();
                string readerUri;
                if (string.IsNullOrWhiteSpace(key) || key == null)
                    readerUri = ReaderName;
                else
                    readerUri = HostNameIpAddress[key];
                MatchCollection mc = Regex.Matches(readerUri, @"(?<=\().+?(?=\))");
                foreach (Match m in mc)
                {
                    if (!string.IsNullOrWhiteSpace(m.ToString()))
                        readerUri = m.ToString();
                }
                //Creates a Reader Object for operations on the Reader.
                return Reader.Create(string.Concat("tmr://", readerUri));
            }
            else if (IsAddCustomReader)
            {
                Reader.SetSerialTransport("tcp", SerialTransportTCP.CreateSerialReader);
                string readerUri = HostAddress;
                //Creates a Reader Object for operations on the Reader.
                return Reader.Create(string.Concat("tcp://", readerUri));
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<string> GetSupportedRegion()
        {
            Reader.Region[] regions;

            if (objReader is LlrpReader || objReader is RqlReader)
            {
                regions = new Reader.Region[] { (Reader.Region)objReader.ParamGet("/reader/region/id") };
            }
            else
            {
                regions = (Reader.Region[])objReader.ParamGet("/reader/region/supportedRegions");
            }

            ObservableCollection<string> tempRegionList = new ObservableCollection<string>();
            foreach (var region in regions)
            {
                tempRegionList.Add(region.ToString());
            }

            tempRegionList.Add("Select");
            return tempRegionList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<string> GetComPortNames()
        {
            List<string> portNames = new List<string>();
            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0"))
            {
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if ((queryObj != null) && (queryObj["Name"] != null))
                    {
                        if (queryObj["Name"].ToString().Contains("(COM"))
                            portNames.Add(queryObj["Name"].ToString());
                        //portNames.Add((string)queryObj.GetPropertyValue("Description"));
                    }
                }
            }

            return portNames;
        }

        /// <summary>
        /// Check for valid port numbers
        /// </summary>
        /// <param name="portNumber"></param>
        /// <returns></returns>
        private bool ValidatePortNumber(string readerName)
        {
            List<string> portNames = new List<string>();
            List<string> portValues = new List<string>();
            //converting comport number from small letter to capital letter.Eg:com18 to COM18.
            string portNumber = Regex.Replace(readerName, @"[^a-zA-Z0-9_\\]", "").ToUpperInvariant();
            // getting the list of comports value and name which device manager shows
            portNames = GetComPortNames();
            for (int i = 0; i < portNames.Count; i++)
            {
                MatchCollection mc = Regex.Matches(portNames[i], @"(?<=\().+?(?=\))");
                foreach (Match m in mc)
                {
                    portValues.Add(m.ToString());
                }
            }
            if ((portNames.Contains(readerName)) || (portValues.Contains(portNumber)))
            {
                //Specified port number exist
                return true;
            }
            else
            {
                //Specified port number doesn't exist
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void ShowErrorMessage(string msg)
        {
            MessageBox.Show(msg, "Connection Wizard : Error", MessageBoxButton.OK, MessageBoxImage.Error);
            SetStatusWarningMessage(msg, Brushes.Red);
        }

        private void ShowErrorMessage(Exception ex)
        {

            if (ex is UnauthorizedAccessException)
            {
                MessageBox.Show(ex.Message + "\n" + "Please check if another program is accessing the Reader.", "Connection Wizard : Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SetStatusWarningMessage(ex.Message + "\n" + "Please check if another program is accessing the Reader.", Brushes.Red);
            }
            else if (ex is FAULT_BL_INVALID_IMAGE_CRC_Exception || ex is FAULT_BL_INVALID_APP_END_ADDR_Exception)
            {
                MessageBox.Show("Firmware is broken. " + ex.Message + ".\nPlease Upgrade the firmware.\nClick on Cancel Button to go to URA and update the firmware.", "Firmware Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SetStatusWarningMessage("Firmware is broken. " + ex.Message + ".\nPlease Upgrade the firmware.\nClick on Cancel Button to go to URA and update the firmware.", Brushes.Red);
                //FirmwareUpdateReaderName = ReaderURI();
                //FirmwareUpdateVisibility = Visibility.Visible;
                //NextButtonVisibility = Visibility.Collapsed;
                //BackButtonVisibility = Visibility.Visible;
                //ContentControlView = new ucWizardFirmwareUpadate();
            }
            else
            {
                MessageBox.Show(ex.Message, "Connection Wizard : Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SetStatusWarningMessage(ex.Message, Brushes.Red);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        private void SetStatusWarningMessage(string message, Brush color)
        {
            StatusWarningText = message;
            StatusWarningColor = color;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsReaderListNull()
        {
            if (ReaderList.Count > 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsReaderListSelectedItemNull()
        {
            if (ReaderListSelectedItem != null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsReaderTypeSelected()
        {
            if (IsSerialReader || IsNetworkReader || IsAddCustomReader)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Validates tje Select Reader Page. Returns True : If all validations are successfull.
        /// </summary>
        /// <returns></returns>
        private bool ValidateSelectReaderPage()
        {
            if (IsReaderTypeSelected())
            {
                if (IsSerialReader || IsNetworkReader)
                {
                    if (IsReaderListNull() && string.IsNullOrWhiteSpace(ReaderListText))
                    {
                        ShowErrorMessage(IsSerialReader ? NOSERIALREADERDETECTED : (IsNetworkReader ? NONETWORKREADERDETECTED : "Error"));
                        return false;
                    }
                    else
                    {
                        if (IsAddManualChecked)
                        {
                            if (string.IsNullOrWhiteSpace(HostAddress))
                                return false;
                            else
                                return true;
                        }
                        if (IsReaderListSelectedItemNull() && string.IsNullOrWhiteSpace(ReaderListText))
                        {
                            ShowErrorMessage(NOREADERSELECTED);
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (!(String.IsNullOrWhiteSpace(HostAddress)))
                    {
                        Regex ip = new Regex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):\d{1,4}\b");
                        if (ip.IsMatch(HostAddress))
                            return true;
                        else
                        {
                            ShowErrorMessage("Incorrect URI Format.\n" + ADDCUSTOMREADERMANUALINFO);
                            return false;
                        }
                    }
                    else
                    {
                        ShowErrorMessage("Host Address Cannout be left blank");
                        return false;
                    }
                }
            }
            else
            {
                ShowErrorMessage(READERTYPENOTSELECTED);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool ValidateConnectReaderPage()
        {
            if (RegionListSelectedItem == null || (IsSerialReader && BaudRateSelectedItem == null))
                return false;

            else
                return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void ValidateReadConnectPage(object obj)
        {
            ReaderConnectionDetail.ReaderName = DetectedReaderName;
            if (IsSerialReader)
            {
                ReaderConnectionDetail.BaudRate = "115200";
            }
            else if (IsNetworkReader)
            {
                //ReaderConnectionDetail.ReaderName = HostNameIpAddress.ContainsKey(ReaderConnectionDetail.ReaderName) ? HostNameIpAddress[ReaderConnectionDetail.ReaderName] : ReaderConnectionDetail.ReaderName;
            }
            ReaderConnectionDetail.ReaderType = DetectedReaderType;
            ReaderConnectionDetail.Region = DetectedReaderRegion;
            ReaderConnectionDetail.Antenna = DetectedSelectedAntenna;
            ReaderConnectionDetail.Protocol = DetectedReaderProtocol;
            ReaderConnectionDetail.ReaderModel = DetectedReaderModel;

            if (objReader != null)
            {
                objReader.Destroy();
                objReader = null;
            }
            IsBusy = true;
            BusyContent = "Opening Universal Assistant Reader - " + ReaderConnectionDetail.ReaderName;

            SetStatusWarningMessage(BusyContent, Brushes.DarkGreen);

            Main window = new Main();
            window.Show();
            window.LoadURAnFromWizardFlow(isReadConnect);

            if (objReader != null)
            {
                objReader.Destroy();
                objReader = null;
            }

            Window win = (Window)obj;
            win.Close();
        }

        private void NextButtonVisibilitySelectReaderPage()
        {
            try
            {
                if (RegionListSelectedItem != "Select" && IsProtocolSelected() && IsAntennaSelected())
                    IsNextButtonEnabled = true;
                else
                    IsNextButtonEnabled = false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private string ReaderURI()
        {
            if (!IsAddCustomReader)
            {
                if (IsNetworkReader)
                {
                    string key = HostNameIpAddress.Keys.Where(x => x.Contains(string.IsNullOrWhiteSpace(ReaderListSelectedItem) ? ReaderListText : ReaderListSelectedItem)).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(string.IsNullOrWhiteSpace(ReaderListSelectedItem) ? ReaderListText : ReaderListSelectedItem) || key == null)
                        return string.IsNullOrWhiteSpace(ReaderListSelectedItem) ? ReaderListText : ReaderListSelectedItem;
                    else
                        return key;
                }
                else
                {
                    return (IsAddManualChecked ? HostAddress : (string.IsNullOrWhiteSpace(ReaderListSelectedItem) ? ReaderListText : ReaderListSelectedItem));
                }
            }
            else
                return HostAddress;
        }

        private void RestartApplication()
        {
            Window mw = Application.Current.MainWindow;
            Window cw = new ConnectionWizard();
            Application.Current.MainWindow = cw;
            MessageBox.Show("Application Encountered a fatal exception. Restarting the application", "Universal Reader Assitant : Restarting...");
            cw.Show();
            mw.Close();
        }

        #endregion

        #region Bonjour

        /// <summary>
        /// ServiceLost
        /// </summary>
        public void ServiceLost(DNSSDService browser, DNSSDFlags flags, uint ifIndex, string serviceName, string regtype, string domain)
        {
            if (IsNetworkReader)
            {
                ReaderListSelectedItem = null;
                ReaderList = null;
            }
            servicesList.Clear();
            HostNameIpAddress.Clear();
        }

        // ServiceFound
        /// <summary>
        /// This call is invoked by the DNSService core.  We create
        /// a BrowseData object and invoked the appropriate method
        /// in the GUI thread so we can update the UI
        /// </summary>
        /// <param name="sref"></param>
        /// <param name="flags"></param>
        /// <param name="ifIndex"></param>
        /// <param name="serviceName"></param>
        /// <param name="regType"></param>
        /// <param name="domain"></param>
        public void ServiceFound(DNSSDService sref, DNSSDFlags flags, uint ifIndex, String serviceName, String regType, String domain)
        {
            int index = servicesList.IndexOf(serviceName);

            //
            // Check to see if we've seen this service before. If the machine has multiple
            // interfaces, we could potentially get called back multiple times for the
            // same service. Implementing a simple reference counting scheme will address
            // the problem of the same service showing up more than once in the browse list.
            //
            if (index == -1)
            {
                lock (_backgroundNotifierLock)
                    _backgroundNotifierCallbackCount++;
                BrowseData data = new BrowseData();

                data.InterfaceIndex = ifIndex;
                data.Name = serviceName;
                data.Type = regType;
                data.Domain = domain;
                data.Refs = 1;
                servicesList.Add(serviceName);
                resolver = service.Resolve(0, data.InterfaceIndex, data.Name, data.Type, data.Domain, eventManager);
            }
            else
            {
                BrowseData data = new BrowseData();
                data.InterfaceIndex = ifIndex;
                data.Name = servicesList[index];
                data.Name = serviceName;
                data.Type = regType;
                data.Domain = domain;
                resolver = service.Resolve(0, data.InterfaceIndex, data.Name, data.Type, data.Domain, eventManager);
                data.Refs++;
            }
        }

        // BrowseData
        /// <summary>
        /// This class is used to store data associated
        /// with a DNSService.Browse() operation 
        /// </summary>
        public class BrowseData
        {
            public uint InterfaceIndex;
            public String Name;
            public String Type;
            public String Domain;
            public int Refs;

            public override String
            ToString()
            {
                return Name;
            }

            public override bool
            Equals(object other)
            {
                bool result = false;

                if (other != null)
                {
                    result = (this.Name == other.ToString());
                }

                return result;
            }

            public override int
            GetHashCode()
            {
                return Name.GetHashCode();
            }
        };

        // ResolveData                       
        /// <summary>
        /// This class is used to store data associated
        /// with a DNSService.Resolve() operation
        /// </summary>
        public class ResolveData
        {
            public uint InterfaceIndex;
            public String FullName;
            public String HostName;
            public int Port;
            public TXTRecord TxtRecord;

            public override String
                ToString()
            {
                return FullName;
            }
        };

        /// <summary>
        /// Populate the comports or ip addresses in the combo-box when resolved
        /// </summary>
        /// <param name="sref"></param>
        /// <param name="flags"></param>
        /// <param name="ifIndex"></param>
        /// <param name="fullName"></param>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <param name="txtRecord"></param>
        public void ServiceResolved(DNSSDService sref, DNSSDFlags flags, uint ifIndex, String fullName, String hostName, ushort port, TXTRecord txtRecord)
        {
            //cmbReaderAddr.Items.Add(hostName);
            ResolveData data = new ResolveData();

            data.InterfaceIndex = ifIndex;
            data.FullName = fullName;
            data.HostName = hostName;
            data.Port = port;
            data.TxtRecord = txtRecord;
            string address = string.Empty;
            uint bits;

            if (txtRecord.ContainsKey("LanIP"))
            {
                object ip = txtRecord.GetValueForKey("LanIP");
                bits = BitConverter.ToUInt32((Byte[])ip, 0);
                address = new System.Net.IPAddress(bits).ToString();
            }
            if ((address == "0.0.0.0") && txtRecord.ContainsKey("WLanIP"))
            {
                object ip = txtRecord.GetValueForKey("WLanIP");
                bits = BitConverter.ToUInt32((Byte[])ip, 0);
                address = new System.Net.IPAddress(bits).ToString();
            }

            //Adding host name
            string[] hostnameArray = hostName.Split('.');
            if (ReaderList == null)
                ReaderList = new ObservableCollection<string>();

            if (hostnameArray.Length > 0)
            {
                if (!(HostNameIpAddress.ContainsKey(hostnameArray[0] + " (" + address + ")")))
                {
                    if (IsNetworkReader)
                    {
                        ReaderList.Add(hostnameArray[0] + " (" + address + ")");
                        HostNameIpAddress.Add(hostnameArray[0] + " (" + address + ")", address);
                        ReaderListText = ReaderList[0];
                        ReaderListSelectedItem = ReaderList[0];
                    }
                }
            }

            //for (uint idx = 0; idx < txtRecord.GetCount(); idx++)
            //{
            //    String key;

            //    key = txtRecord.GetKeyAtIndex(idx);
            //    object value = txtRecord.GetValueAtIndex(idx);

            //    if (key.Length > 0)
            //    {
            //        String val = "";

            //       if (key == "LanIp")
            //        {
            //            foreach (Byte b in (Byte[])value)
            //            {
            //                val += b.ToString() + ".";
            //            }
            //            System.Diagnostics.Debug.WriteLine("Reader uri:" + val.TrimEnd('.'));
            //            cmbFixedReaderAddr.Items.Add(val.TrimEnd('.'));
            //            break;
            //        }

            //    }
            //}

            //
            // Don't forget to stop the resolver. This eases the burden on the network
            //
            if (null != resolver)
            {
                resolver.Stop();
                resolver = null;
            }

            lock (_backgroundNotifierLock)
                _backgroundNotifierCallbackCount--;
        }

        #endregion Bonjour

    }
}