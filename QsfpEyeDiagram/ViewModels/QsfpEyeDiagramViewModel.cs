using Microsoft.Win32;
using NLog;
using QsfpEyeDiagram.Models;
using QsfpEyeDiagram.Models.EventArguments;
using QsfpEyeDiagram.ViewModels.Commands;
using Std.Data.Database.Contexts;
using Std.Data.Database.Domain;
using Std.Equipment.Anritsu.Bertwave;
using Std.Equipment.FiberTrade.UniversalTestBoard;
using Std.Modules.ConfigurationParameters.Qsfp;
using Std.Modules.ConfigurationParameters.Qsfp.Cdr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace QsfpEyeDiagram.ViewModels
{

    public enum ModuleState
    {
        StartState, Connected, NowTune, NotTuned, Tuned, NotConnected,
    }


    public class QsfpEyeDiagramViewModel : ViewModelBase
    {
        private ModuleState _mState = ModuleState.NotConnected;

        public ModuleState MState
        {
            get => _mState;
            set => Set(ref _mState, value);
        }

        public Logger logger = LogManager.GetCurrentClassLogger();

        private const int MaxPower = 4500;
        public readonly QsfpEyeDiagramModel _model;

        private readonly double ch0Att = Properties.Settings.Default.ch0Att;
        private readonly double ch1Att = Properties.Settings.Default.ch1Att;
        private readonly double ch2Att = Properties.Settings.Default.ch2Att;
        private readonly double ch3Att = Properties.Settings.Default.ch3Att;

        public string Title => $"{Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version} - {Worker?.FullName}";

        private WorkerRecord _worker = new WorkerRecord()/*.Unassigned*/;
        public WorkerRecord Worker
        {
            get { return _worker; }
            set
            {
                _worker = value;
                _model.Worker = Worker;
                OnPropertyChanged();
            }
        }

        #region Параметры модуля
        private int _power3;
        public int Power3v3
        {
            get => _power3;
            set
            {
                if(_power3 != value)
                {
                    _power3 = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isPowerBig = false;
        public bool IsPowerBig
        {
            get => _isPowerBig;
            set
            {
                if (_isPowerBig != value)
                {
                    _isPowerBig = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _current3v3;
        public int Current3v3
        {
            get => _current3v3;
            set
            {
                if (_current3v3 != value)
                {
                    _current3v3 = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _modAbsent;//модуль отсутствует
        public bool ModAbsent
        {
            get => _modAbsent;
            set
            {
                if(_modAbsent!=value)
                {
                    _modAbsent = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _interrupt;
        public bool Interrupt
        {
            get => _interrupt;
            set
            {
                if (_interrupt != value)
                {
                    _interrupt = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _modDeSel;
        public bool ModDeSel
        {
            get => _modDeSel;
            set
            {
                if (_modDeSel != value)
                {
                    _modDeSel = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _resetL;
        public bool ResetL
        {
            get => _resetL;
            set
            {
                if (_resetL != value)
                {
                    _resetL = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _lPMode;
        public bool LPMode
        {
            get => _lPMode;
            set
            {
                if (_lPMode != value)
                {
                    _lPMode = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion


        private EyeDiagramChannelViewModel _zeroChannel = new EyeDiagramChannelViewModel();
        public EyeDiagramChannelViewModel ZeroChannel
        {
            get => _zeroChannel;
            set
            {
                _zeroChannel = value;
                _zeroChannel.Attenuation = ch0Att;
                OnPropertyChanged();
            }
        }
        private EyeDiagramChannelViewModel _firstChannel = new EyeDiagramChannelViewModel();
        public EyeDiagramChannelViewModel FirstChannel
        {
            get => _firstChannel;
            set
            {
                _firstChannel = value;
                _firstChannel.Attenuation = ch1Att;
                OnPropertyChanged();
            }
        }

        private EyeDiagramChannelViewModel _secondChannel = new EyeDiagramChannelViewModel();
        public EyeDiagramChannelViewModel SecondChannel
        {
            get => _secondChannel;
            set
            {
                _secondChannel = value;
                _secondChannel.Attenuation = ch2Att;
                OnPropertyChanged();
            }
        }
        
        private EyeDiagramChannelViewModel _thirdChannel = new EyeDiagramChannelViewModel();
        public EyeDiagramChannelViewModel ThirdChannel
        {
            get => _thirdChannel;
            set
            {
                _thirdChannel = value;
                _thirdChannel.Attenuation = ch3Att;
                OnPropertyChanged();
            }
        }
        
        private bool _writeDataImmediately;
        public bool WriteDataImmediately
        {
            get => _writeDataImmediately;
            set => Set(ref _writeDataImmediately, value);
            //{
            //    if (_writeDataImmediately != value)
            //    {
            //        _writeDataImmediately = value;
            //        OnPropertyChanged();
            //    }
            //}
        }
        
        private bool _isOperationInProgress;
        public bool IsOperationInProgress
        {
            get { return _isOperationInProgress; }
            set
            {
                if (_isOperationInProgress != value)
                {
                    _isOperationInProgress = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _isIndefinitelyLongOperation;
        public bool IsIndefinitelyLongOperation
        {
            get { return _isIndefinitelyLongOperation; }
            set
            {
                if (_isIndefinitelyLongOperation != value)
                {
                    _isIndefinitelyLongOperation = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _operationSuccess = true;
        public bool OperationSuccess
        {
            get { return _operationSuccess; }
            set
            {
                if (_operationSuccess != value)
                {
                    _operationSuccess = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private int _operationProgressPercent;
        public int OperationProgressPercent
        {
            get { return _operationProgressPercent; }
            set
            {
                if (value != _operationProgressPercent)
                {
                    if (value < 0)
                    {
                        value = 0;
                    }
                    else if (value > 100)
                    {
                        value = 100;
                    }

                    _operationProgressPercent = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private Progress<int> _progressPercentHandler;
        public Progress<int> ProgressPercentHandler
        {
            get => _progressPercentHandler ?? (_progressPercentHandler = new Progress<int>(percent => OperationProgressPercent = percent));
        }

        private double _sumOfDeltas=0;
        public double SumOfDeltas
        {
            get => _sumOfDeltas;
            set => Set(ref _sumOfDeltas, value);
        }

        private bool _isTecSliderEnabled = false;
        public bool IsTecSliderEnabled
        {
            get => _isTecSliderEnabled;
            set => Set(ref _isTecSliderEnabled, value);
        }


        #region Взаимодействие с Universal Test Board
        // Команды для взаимодействия с Universal Test Board.
        private ICommand _connectToUtbCommand;
        public ICommand ConnectToUtbCommand
        {
            get => _connectToUtbCommand ?? (_connectToUtbCommand = new RelayCommand(
                p => !string.IsNullOrEmpty(ComPortListSelectedItem) && !IsUtbConnected,
                p =>
                {
                    if (p is string)
                    {
                        ConnectToUtb((string)p);
                    }
                }));
        }

        private ICommand _connectToUtbApiCommand;
        public ICommand ConnectToUtbApiCommand
        {
            get => _connectToUtbApiCommand ?? (_connectToUtbApiCommand = new RelayCommand(
                p => !string.IsNullOrEmpty(ComPortListSelectedItem) && !IsUtbConnected,
                async p=> await ConnectToUtbFromApi(ComPortListSelectedItem)
                ));
        }

        private ICommand _disconnectFromUtbAndRefreshComPortListCommand;
        public ICommand DisconnectFromUtbAndRefreshComPortListCommand
        {
            get => _disconnectFromUtbAndRefreshComPortListCommand ?? (_disconnectFromUtbAndRefreshComPortListCommand = new RelayCommand(
                p => !IsCdrReading/*!IsUtbConnected | ! OperationSuccess*/,
                p => DisconnectFromUtbAndRefreshComPortList()));
        }

        private ICommand _readEyeDiagramParametersCommand;
        public ICommand ReadEyeDiagramParametersCommand
        {
            get => _readEyeDiagramParametersCommand ?? (_readEyeDiagramParametersCommand = new RelayCommand(
                p => true,
                p => ReadEyeDiagramParameters()));
        }
        private ICommand _switchChannelCommand;
        public ICommand SwitchChannelCommand
        {
            get => _switchChannelCommand ?? (_switchChannelCommand = new RelayCommand(
                p => true,
                p =>
                {
                    var parameters = (object[])p;
                    var channel = (Channel)parameters[0];
                    var eyeDiagramChannel = (EyeDiagramChannelViewModel)parameters[1];
                    
                    Task.Run(()=> { SwitchChannel(eyeDiagramChannel.IsEnabled, channel);
                        UpdateChannelStatuses(); });
                    //eyeDiagramChannel.IsEnabled = !eyeDiagramChannel.IsEnabled;
                }));
        }
        private ICommand _writeBiasCommand;
        public ICommand WriteBiasCommand
        {
            get => _writeBiasCommand ?? (_writeBiasCommand = new RelayCommand(
                p => IsModuleConnected && !IsOperationInProgress,
                p =>
                {
                    var parameters = (object[])p;
                    var channel = (Channel)parameters[0];
                    var bias = ((EyeDiagramChannelViewModel)parameters[1]).Bias;

                    Task.Run(()=> WriteBias(bias, channel));
                }));
        }
        private ICommand _writeModulationCommand;
        public ICommand WriteModulationCommand
        {
            get => _writeModulationCommand ?? (_writeModulationCommand = new RelayCommand(
                p => IsModuleConnected && !IsOperationInProgress,
                p =>
                {
                    var parameters = (object[])p;
                    var channel = (Channel)parameters[0];
                    var modulation = ((EyeDiagramChannelViewModel)parameters[1]).Modulation;

                    Task.Run(()=>WriteModulation(modulation, channel));
                }));
        }
        private ICommand _switchEqualizerCommand;
        public ICommand SwitchEqualizerCommand
        {
            get => _switchEqualizerCommand ?? (_switchEqualizerCommand = new RelayCommand(
                p => true,
                p =>
                {
                    var parameters = (object[])p;
                    var channel = (Channel)parameters[0];
                    var eyeDiagramChannel = (EyeDiagramChannelViewModel)parameters[1];
                    eyeDiagramChannel.IsEqualizerEnabled = !eyeDiagramChannel.IsEqualizerEnabled;

                    SwitchEqualizer(eyeDiagramChannel.IsEqualizerEnabled, channel);
                }));
        }
        private ICommand _writeEqualizerCommand;
        public ICommand WriteEqualizerCommand
        {
            get => _writeEqualizerCommand ?? (_writeEqualizerCommand = new RelayCommand(
                p => IsModuleConnected && !IsOperationInProgress,
                p =>
                {
                    var parameters = (object[])p;
                    var channel = (Channel)parameters[0];
                    var equalizer = ((EyeDiagramChannelViewModel)parameters[1]).EqualizerPhaseWithMagnitude;

                    Task.Run(()=>WriteEqualizer(equalizer, channel));
                }));
        }
        private ICommand _switchEyeOptimizationCommand;
        public ICommand SwitchEyeOptimizationCommand
        {
            get => _switchEyeOptimizationCommand ?? (_switchEyeOptimizationCommand = new RelayCommand(
                p => true,
                p =>
                {
                    var parameters = (object[])p;
                    var channel = (Channel)parameters[0];
                    var optimizationType = (EyeOptimization)parameters[1];
                    var isEnabled = (bool)parameters[2];
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                    {
                        foreach(Channel ch in Enum.GetValues(typeof(Channel)))
                            MessageBox.Show($"Command Shift {ch}");
                    }
                    else
                        Task.Run(() => SwitchEyeOptimization(isEnabled, optimizationType, channel));
                }));
        }
        private ICommand _switchCrossingCommand;
        public ICommand SwitchCrossingCommand
        {
            get => _switchCrossingCommand ?? (_switchCrossingCommand = new RelayCommand(
                p => true,
                p =>
                {
                    var parameters = (object[])p;
                    var channel = (Channel)parameters[0];
                    var eyeDiagramChannel = (EyeDiagramChannelViewModel)parameters[1];
                    eyeDiagramChannel.IsCrossingEnabled = !eyeDiagramChannel.IsCrossingEnabled;

                    SwitchCrossing(eyeDiagramChannel.IsCrossingEnabled, channel);
                }));
        }
        private ICommand _writeCrossingCommand;
        public ICommand WriteCrossingCommand
        {
            get => _writeCrossingCommand ?? (_writeCrossingCommand = new RelayCommand(
                p => IsModuleConnected && !IsOperationInProgress,
                p =>
                {
                    var parameters = (object[])p;
                    var channel = (Channel)parameters[0];
                    var crossing = ((EyeDiagramChannelViewModel)parameters[1]).CrossingMagnitude;

                    Task.Run(()=>WriteCrossing(crossing, channel));
                }));
        }
        private ICommand _writeTecOptimalTemperatureVoltageCommand;
        public ICommand WriteTecOptimalTemperatureVoltageCommand
        {
            get => _writeTecOptimalTemperatureVoltageCommand ?? (_writeTecOptimalTemperatureVoltageCommand = new RelayCommand(
                p => IsModuleConnected && !IsOperationInProgress,
                p =>
                {
                    var parameters = (object[])p;
                    var tecOptimalTemperatureVoltage = (int)parameters[0];

                    WriteTecOptimalTemperatureVoltage(tecOptimalTemperatureVoltage);
                    //Task.Delay(600).GetAwaiter().GetResult();
                    //Task.Run(()=>RefreshOsaData());
                }));
        }

        // Поля для взаимодействия с Universal Test Board.
        private bool _isUtbConnected;
        public bool IsUtbConnected
        {
            get => _isUtbConnected;
            set
            {
                if (_isUtbConnected != value)
                {
                    _isUtbConnected = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _isModuleConnected;
        public bool IsModuleConnected
        {
            get => _isModuleConnected;
            set
            {
                if (_isModuleConnected != value)
                {
                    _isModuleConnected = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _refreshTecParameters;
        public bool RefreshTecParameters
        {
            get => _refreshTecParameters;
            set
            {
                if (_refreshTecParameters != value)
                {
                    _refreshTecParameters = value;
                    _model.RefreshTecParameters = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _comPortListSelectedIndex;
        public int ComPortListSelectedIndex
        {
            get => _comPortListSelectedIndex;
            set
            {
                _comPortListSelectedIndex = value;
                OnPropertyChanged();
            }
        }
        private string _comPortListSelectedItem;
        public string ComPortListSelectedItem
        {
            get => _comPortListSelectedItem;
            set
            {
                _comPortListSelectedItem = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ComPortList { get; private set; } = new ObservableCollection<string>();

        public ObservableCollection<TecDataGridRow> TecDataGridItems { get; private set; } = new ObservableCollection<TecDataGridRow>()
        {
            new TecDataGridRow()
        };

        private int _tecOptimalTemperatureVoltage;
   
        public int TecOptimalTemperatureVoltage
        {
            get => _tecOptimalTemperatureVoltage;
            set
            {
                if (_tecOptimalTemperatureVoltage != value)
                {
                    _tecOptimalTemperatureVoltage = value;
                    OnPropertyChanged();
                }
            }
        }

        // Функции для взаимодействия с Universal Test Board.

        //Подключение к UTB
        public void ConnectToUtb(string comPortName)
        {
            _model.ConnectToUtb(comPortName);
        }
        //Подключение к UTB из внешней программы
        public async Task ConnectToUtbFromApi(string comPortName)
        {
            logger.Trace("Подключение к плате");
            OperationResult rez = new OperationResult(OperationStatuses.PartialSuccess);
            await Task.Run(() => rez = _model.ConnectToUtb(comPortName));
            logger.Trace(rez.Message+" "+rez.Status.ToString());
            //_model.RefreshUtbStatus();
            _model.InitializeMonitoringTasks();
        }

        private void DisconnectFromUtbAndRefreshComPortList()
        {
            if (_model.DisconnectFromUtb().Status == OperationStatuses.Success)
            {
                RefreshComPortList();
            }
        }

        private async void ReadEyeDiagramParameters()
        {
            await Task.Run(() =>
            {
                OperationSuccess = _model.ReadEyeDiagramParameters(out var parameters);
                if (OperationSuccess)
                {
                    ShowEyeDiagramParameters(parameters);
                }
            });
        }

        private void RefreshComPortList()
        {
            ComPortList.Clear();

            var comPortNames = UniversalTestBoard.GetComPortNamesWithUtbConnection();
            for (var i = 0; i < comPortNames.Count; i++)
            {
                ComPortList.Add(comPortNames[i]);
            }

            if (ComPortList.Count != 0)
            {
                ComPortListSelectedIndex = 0;
            }
        }

        private void ResetOperationProgressPercent()
        {
            OperationProgressPercent = 0;
        }

        private void SwitchChannel(bool isEnabled, Channel channel)
        {
            //Mouse.OverrideCursor = Cursors.Wait;
            ResetOperationProgressPercent();
            //UtbMonitoringPause();
            //bool refTec = RefreshTecParameters;
            //RefreshTecParameters = false;
            OperationSuccess = _model.SwitchChannel(isEnabled, channel, ProgressPercentHandler);
            //UpdateChannelStatuses();
            string chStatus = isEnabled ? "включен" : "отключен";
            logger.Trace($"Канал {(int)channel} {chStatus}");
            //if (refTec) RefreshTecParameters = true;
            //UtbMonitoringRelease();
            //Mouse.OverrideCursor = Cursors.Arrow;
        }

        public async Task WriteBias(int bias, Channel channel)
        {
            await Task.Run(() =>
            {
                ResetOperationProgressPercent();
                OperationSuccess = _model.WriteBias(bias, channel, ProgressPercentHandler);
            });
            logger.Trace($"Значение Bias канала {(int)channel} изменено на {bias}");
        }

        public  async Task WriteModulation(int modulation, Channel channel)
        {
            ResetOperationProgressPercent();
            await Task.Run(() =>
            {
                OperationSuccess = _model.WriteModulation(modulation, channel, ProgressPercentHandler);
            });
            logger.Trace($"Значение Modulation  канала {(int)channel} изменено на {modulation}");
        }

        private void SwitchEqualizer(bool isEnabled, Channel channel)
        {
            ResetOperationProgressPercent();
            OperationSuccess = _model.SwitchEqualizer(isEnabled, channel, ProgressPercentHandler);
        }

        public async Task WriteEqualizer(int equalizer, Channel channel)
        {
            ResetOperationProgressPercent();
            await Task.Run(() =>
            {
                OperationSuccess = _model.WriteEqualizer(equalizer, channel, ProgressPercentHandler);
            });
            logger.Trace($"Значение Equalizer  канала {(int)channel} изменено на {equalizer}");
        }

        private async Task SwitchEyeOptimization(bool isEnabled, EyeOptimization optimizationType, Channel channel)
        {
            ResetOperationProgressPercent();
            await Task.Run(()=>OperationSuccess = _model.SwitchEyeOptimization(isEnabled, optimizationType, channel, ProgressPercentHandler));
        }

        private void SwitchCrossing(bool isEnabled, Channel channel)
        {
            ResetOperationProgressPercent();
                OperationSuccess = _model.SwitchCrossing(isEnabled, channel, ProgressPercentHandler);
        }

        private async Task WriteCrossing(int crossing, Channel channel)
        {
            ResetOperationProgressPercent();
            await Task.Run(()=>OperationSuccess = _model.WriteCrossing(crossing, channel, ProgressPercentHandler));
        }

        private void WriteTecOptimalTemperatureVoltage(int tecOptimalTemperatureVoltage)
        {
            ResetOperationProgressPercent();
            OperationSuccess = _model.WriteTecOptimalTemperatureVoltage(tecOptimalTemperatureVoltage, ProgressPercentHandler);
        }
        #endregion

        #region Взаимодействие с BERTWave
        // Команды для взаимодействия с BERTWave.
        private ICommand _connectToBertwaveCommand;
        public ICommand ConnectToBertwaveCommand
        {
            get
            {
                return _connectToBertwaveCommand ??
                    (_connectToBertwaveCommand = new RelayCommand(
                        p => !IsBertwaveConnected,
                        p => ConnectToBertwave(BertwaveIpAddress)));
            }
        }

        private ICommand _disconnectFromBertwaveCommand;
        public ICommand DisconnectFromBertwaveCommand
        {
            get
            {
                return _disconnectFromBertwaveCommand ??
                    (_disconnectFromBertwaveCommand = new RelayCommand(
                        p => IsBertwaveConnected,
                        p => DisconnectFromBertwave()));
            }
        }

        // Поля для взаимодействия с BERTWave.
        private bool _isBertwaveConnected;
        public bool IsBertwaveConnected
        {
            get { return _isBertwaveConnected; }
            set
            {
                if (_isBertwaveConnected != value)
                {
                    _isBertwaveConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _bertwaveIpAddress = Properties.Settings.Default.BertwaveIP;
        public string BertwaveIpAddress
        {
            get { return _bertwaveIpAddress; }
            set
            {
                if (_bertwaveIpAddress != value)
                {
                    _bertwaveIpAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        private BertwaveEyeDiagramParameters _bertwaveEyeDiagramParameters = BertwaveEyeDiagramParameters.Unassigned;
        public BertwaveEyeDiagramParameters BertwaveEyeDiagramParameters
        {
            get => _bertwaveEyeDiagramParameters;
            set
            {
                _bertwaveEyeDiagramParameters = value;
                OnPropertyChanged();
            }
        }

        // Функции для взаимодействия с BERTWave.

        public System.Timers.Timer bertTimer;
        public void ConnectToBertwave(string ipAddress)
        {
            if (SelectedOscyll != 0)
                _model._is4chOscyll = true;

            _model.ConnectToBertwave(ipAddress);

            if (IsBertwaveConnected)
            {
                logger.Trace("Подключились к осциллографу");

                string tempStr = _model.Bertwave.GetMaskRecall();
                MaskRecall = tempStr.Substring(1, tempStr.IndexOf('.')-1);
                PutDataToParameters(_model.BertWaveParameters);
                if (!IsUseAsApi)
                {
                    if (bertTimer == null)
                    {
                        bertTimer = new System.Timers.Timer(5000)
                        {
                            AutoReset = true
                        };
                        bertTimer.Elapsed += RefreshFromBertWave;
                    }
                    if (SelectedOscyll != 0)
                        bertTimer.Start();
                }
            }
            else
            {
                MessageBox.Show($"Сбой подключения к осциллографу {ipAddress}", "Сбой", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void RefreshFromBertWave(object sender, ElapsedEventArgs e)//Обновление данных из BertWave по таймеру
        {
            //IsBertwaveConnected = false;
            //SetBertParamsToZero();
            PutDataToParameters(_model.BertWaveParameters);
            //IsBertwaveConnected = true;
        }

        public void DisconnectFromBertwave()
        {
            bertTimer.Stop();
            _model.DisconnectFromBertwave();
            //SetBertParamsToZero();
            //logger.Trace("Отключились от осциллографа");
            IsBertwaveConnected = false;
        }

        #endregion

        #region Взаимодействие с OSA
        // Поля для взаимодействия с OSA.
        public ObservableCollection<WavelengthDataGridRow> WavelengthDataGridItems { get; private set; } = new ObservableCollection<WavelengthDataGridRow>()
        {
                       new WavelengthDataGridRow()
            { FontWeight = FontWeights.Bold,
                ZeroChannelWavelength =0, FirstChannelWavelength = 0,
                SecondChannelWavelength = 0, ThirdChannelWavelength = 0
            },

            new WavelengthDataGridRow()
            { FontWeight = FontWeights.Bold,
                ZeroChannelWavelength = 1295.56, FirstChannelWavelength = 1300.05,
                SecondChannelWavelength = 1304.58, ThirdChannelWavelength = 1309.14
            }
        };

        public void ReplaceRowWithLamblas()
        {
            WavelengthDataGridRow row = WavelengthDataGridItems[0];
            row.FontWeight = FontWeights.SemiBold;
            row.ZeroChannelWavelength = Lambda1PeakValue;
            row.FirstChannelWavelength = Lambda2PeakValue;
            row.SecondChannelWavelength = Lambda3PeakValue;
            row.ThirdChannelWavelength = Lambda4PeakValue;
        }

        private bool _isOsaConnected = false;

        public bool IsOsaConnected
        {
            get => _isOsaConnected;
            set
            {
                if (_isOsaConnected != value)
                {
                    _isOsaConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _lambda1 = 0;
        public double Lambda1PeakValue
        {
            get => _lambda1;
            set
            {
                if (_lambda1 != value)
                {
                    _lambda1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _lambda2 = 0;
        public double Lambda2PeakValue
        {
            get => _lambda2;
            set
            {
                if (_lambda2 != value)
                {
                    _lambda2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _lambda3 = 0;
        public double Lambda3PeakValue
        {
            get => _lambda3;
            set
            {
                if (_lambda3 != value)
                {
                    _lambda3 = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _lambla4 = 0;
        public double Lambda4PeakValue
        {
            get => _lambla4;
            set
            {
                if (_lambla4 != value)
                {
                    _lambla4 = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _osaIpAddress = Properties.Settings.Default.OSAIP;

        public string OsaIpAddress
        {
            get => _osaIpAddress;
            set
            {
                if (_osaIpAddress != value)
                {
                    _osaIpAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public System.Timers.Timer OsaTimer = new System.Timers.Timer(7500) { AutoReset=true};

        private ICommand _connectToOsaCommand;
        public ICommand ConnectToOsaCommand
        {
            get => _connectToOsaCommand ?? (_connectToOsaCommand = new RelayCommand(
                p => !IsOsaConnected,
                p => Task.Run(()=>ConnectToOsa(OsaIpAddress))));
        }

        private ICommand _disconnectFromOsaCommand;
        public ICommand DisconnectFromOsaCommand
        {
            get => _disconnectFromOsaCommand ?? (_disconnectFromOsaCommand = new RelayCommand(
                p => IsOsaConnected,
                p => DisconnectFromOsa()
                ));
        }
       
        private ICommand _refreshOsaDataCommand;
        public ICommand RefreshOsaDataCommand
        {
            get => _refreshOsaDataCommand ?? (_refreshOsaDataCommand = new RelayCommand(
                p => !IsAutotuneProcessing,
                p => RefreshOsaData()
                ));
        }

        public void /*async Task*/ RefreshOsaData()
        {
            //if (OsaTimer != null) OsaTimer.Stop();
            if (IsOsaConnected)
            {
                double[] peaksValues = new double[4];
                //Lambda1PeakValue = 0;
                //Lambda2PeakValue = 0;
                //Lambda3PeakValue = 0;
                //Lambda4PeakValue = 0;
                try
                {
                    Task.Run(() => { while (Math.Abs(TecOptimalTemperatureVoltage - TecDataGridItems[0].TecTemp) > 2) { } });
                    //logger.Trace("Попытка получить данные с OSA (RefreshOsaData)");
                    //Task.Delay(600).GetAwaiter().GetResult();
                    do
                    {
                        /*await Task.Run(()=>*/peaksValues = GetLambdaPeaks()/*)*/;
                        //    Task.Delay(600).GetAwaiter().GetResult();
                    } while (peaksValues.Length < 4);

                    Lambda1PeakValue = peaksValues[0] * 1000;
                    Lambda2PeakValue = peaksValues[1] * 1000;
                    Lambda3PeakValue = peaksValues[2] * 1000;
                    Lambda4PeakValue = peaksValues[3] * 1000;
                    

                    RefreshDeltasVM(GetDeltas());

                }
                catch (IndexOutOfRangeException ex)
                {
                    logger.Trace($"{ex.Message} при получении длин волн (RefreshOsaData)");
                    Trace.WriteLine($"{ex.Message} при получении длин волн (RefreshOsaData)");
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Не удалось обновить данные с OSA", "Неудача", MessageBoxButton.OK, MessageBoxImage.Error);
                    logger.Trace($"Ошибка в RefreshOsaData - {ex.Message}");
                    Trace.WriteLine($"Ошибка в RefreshOsaData - {ex.Message}");
                }
            }
        }

        private bool _osaConnectionProgress = false;
        public bool OsaConnectionProgress
        {
            get => _osaConnectionProgress;
            set
            {
                if (_osaConnectionProgress != value)
                {
                    _osaConnectionProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task ConnectToOsa(string ipAddress)
        {
            OsaConnectionProgress = true;
            //List<Task> tasks = 
             _model.ConnectToOsa(ipAddress);
            if (IsOsaConnected)
            {
                if (!IsUseAsApi)
                {
                    IsTecSliderEnabled = true;
                    await Task.Run(() =>
                    {
                        Thread.Sleep(7500);//7500
                    RefreshOsaData();
                        if (OsaTimer == null)
                        {
                            OsaTimer = new System.Timers.Timer(7500)
                            {
                                AutoReset = true
                            };
                        }
                        OsaTimer.Elapsed += OsaTimer_Elapsed;
                        OsaTimer.Start();
                    });
                }
            }
            else
            {
                MessageBox.Show($"Не удалось подключитьчся к cпектроанализатору {ipAddress}","Сбой",MessageBoxButton.OK,MessageBoxImage.Exclamation);
            }
            OsaConnectionProgress = false;
        }

        private void OsaTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsOsaConnected)
            {
                RefreshOsaData();
            }
        }

        private void DisconnectFromOsa()
        {
            try
            {
                _model.DisconnectFromOsa();
                OsaTimer.Stop();
                //IsTecSliderEnabled = true;
            }
            catch(Exception)
            {
                MessageBox.Show("Не получилось отключиться от OSA");
            }
            IsAutoTecOk = false;
            atBreak = true;
        }

        private double[] GetLambdaPeaks()
        {
            List<double> peaksDouble = new List<double>();
            if (IsOsaConnected)
            {
                
                //do
                //{
                    string[] RawData = _model.RefreshOsaData();
                    //logger.Trace("Получили строку с пиками в GetLambdaPeaks");

                    string[] peaks = PartSelect(RawData, 0);
                    string[] strLevels = PartSelect(RawData, 1);

                    List<double> levels = new List<double>();

                    foreach (string s in strLevels)
                    {
                        if (double.TryParse(s, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out double elem))
                        {
                            levels.Add(elem);
                        }
                    }

                    foreach (string s in peaks)
                    {
                        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double elem))
                        {

                            peaksDouble.Add(elem);
                        }
                    }

                    if (peaksDouble.Count == levels.Count)
                    {
                        double maxLevel = levels.Max();
                        for (int i = 0; i < peaksDouble.Count; i++)
                        {
                            double minLevel = levels.Min();
                            int pos = levels.IndexOf(minLevel);
                            if ((maxLevel - minLevel) > 30)
                            {
                                levels.RemoveAt(pos);
                                peaksDouble.RemoveAt(pos);
                            }
                        }

                    }
                    else
                    {
                        logger.Trace("Несоответствие числа пиков и уровней (GetLambdaPeaks)");
                    }
                    peaksDouble.Sort();

                    if (peaksDouble.Count < 4)
                    {
                        if (!ZeroChannel.IsEnabled)
                        {
                            peaksDouble.Insert(0, 0.0);
                        }
                        if (!FirstChannel.IsEnabled)
                        {
                            peaksDouble.Insert(1, 0.0);
                        }
                        if (!SecondChannel.IsEnabled)
                        {
                            peaksDouble.Insert(2, 0.0);
                        }
                        if (!ThirdChannel.IsEnabled)
                        {
                            peaksDouble.Insert(3, 0.0);
                        }
                    logger.Trace($"Пиков получено {peaksDouble.Count}");
                    }
                    if (peaksDouble.Count > 4)
                    {
                        logger.Trace("Пиков больше 4-х (GetLambdaPeaks) берем первые 4");
                        peaksDouble = peaksDouble.Take(4).ToList();
                    }
                //} while (BigDeltas(peaksDouble));
            }
            //logger.Trace("Вычислили пики (GetLambdaPeaks)");
            return peaksDouble.ToArray();
        }
        //private bool BigDeltas(List<double> lambdas)
        //{
        //    double[] ideals = new double[4];
        //    if (lambdas.Count < 4) return true;

        //    ideals[0] = MainWindow.wlIdeal1;
        //    ideals[1] = MainWindow.wlIdeal2;
        //    ideals[2] = MainWindow.wlIdeal3;
        //    ideals[3] = MainWindow.wlIdeal4;
      
        //    for(int i = 0; i < 4; i++)
        //    {
        //        if (Math.Abs(lambdas[i] * 1000 - ideals[i]) > 3)
        //        {
        //            logger.Trace($"Большая дельта {i+1} канала");
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        private string[] PartSelect(string[] peaksRawData, int partNo)
        {
            string[] result = new string[4];
            if (partNo == 0)
            {
                for (int i = 0; i < peaksRawData.Length; i++)
                {
                    result[i] = peaksRawData[i].Substring(1, peaksRawData[i].IndexOf('E') - 1);
                }
            }
            else
            {
                for (int i = 0; i < peaksRawData.Length; i++)
                {
                    result[i] = peaksRawData[i].Substring(peaksRawData[i].IndexOf(',') + 1);
                    result[i] = result[i].Substring(0, result[i].Length - 1);
                }
            }
            return result;
        }
        #endregion

        // Команда, вызываемая при закрытии главного окна, для сохранения настроек приложения и отключения от устройств.
        private ICommand _quitCommand;
        public ICommand QuitCommand
        {
            get => _quitCommand ?? (_quitCommand = new RelayCommand(
                p => true,
                p => Quit()));
        }

        //Constructor
        public QsfpEyeDiagramViewModel()
        {
            IsUseAsApi = false;

            if (File.Exists("nlog.txt"))
            {
                File.Delete("nlog.txt");
            }

            RefreshComPortList();

            _model = new QsfpEyeDiagramModel(false);
            _model.UtbStatusChanged += UtbStatusChanged;
            _model.ModuleStatusChanged += ModuleStatusChanged;
            _model.EyeDiagramParametersChanged += EyeDiagramParametersChanged;
            _model.TecParametersChanged += TecParametersChanged;
            _model.OperationProgressStatusChanged += OperationProgressStatusChanged;
            _model.OperationSuccessChanged += OperationSuccessChanged;
            _model.OperationTypeChanged += OperationTypeChanged;
            _model.OsaStatusChanged += OsaStatusChanged;
            _model.BertwaveStatusChanged += BertwaveStatusChanged;
        }

        public QsfpEyeDiagramViewModel(bool fromApi = false):this()
        {
            IsUseAsApi = fromApi;
            //if (File.Exists("nlog.txt"))
            //{
            //    File.Delete("nlog.txt");
            //}

            //RefreshComPortList();

            //_model = new QsfpEyeDiagramModel(fromApi);
            //_model.UtbStatusChanged += UtbStatusChanged;
            //_model.ModuleStatusChanged += ModuleStatusChanged;
            //_model.EyeDiagramParametersChanged += EyeDiagramParametersChanged;
            //_model.TecParametersChanged += TecParametersChanged;
            //_model.OperationProgressStatusChanged += OperationProgressStatusChanged;
            //_model.OperationSuccessChanged += OperationSuccessChanged;
            //_model.OperationTypeChanged += OperationTypeChanged;
            //_model.OsaStatusChanged += OsaStatusChanged;
            //_model.BertwaveStatusChanged += BertwaveStatusChanged;
        }


        private void UtbStatusChanged(object sender, UtbStatusEventArgs e)
        {
            IsUtbConnected = e.IsConnected;
            if (IsUtbConnected)
            {
                logger.Trace("UTB подключена");
            }
            else
            {
                IsUidNotNull = false;
                logger.Trace("Потерян контакт с UTB");
            }
        }

        private bool _isUseAsApi = false;
        public bool IsUseAsApi
        {
            get => _isUseAsApi;
            set => Set(ref _isUseAsApi, value);
        }

        private async void ModuleStatusChanged(object sender, ModuleStatusEventArgs e)
        {
            _model.RefreshUtbParams();

            IsModuleConnected = e.IsConnected;
            if(IsModuleConnected && IsOsaConnected)
            {
                await Task.Run(()=>ReFreshOsa());
            }
            if(!IsModuleConnected && IsUtbConnected)
            {
                if (!IsModuleTuneEnded && !IsUseAsApi)
                {
                    MessageBox.Show("Вы не закончили настройку модуля.\n Вставтье модуль, нажмите кнопку 'Завершить' и ДОЖДИТЕСЬ СООБЩЕНИЯ ОБ ЗАВЕРШЕНИИ ПОДГОТОВКИ, только после этого можно менять модуль","Ошибка",
                        MessageBoxButton.OK,MessageBoxImage.Error);
                    IsCdrReading = true;
                }
                else
                {
                    //if (!IsBertwaveConnected)
                    //{
                    //    ConnectToBertwave(BertwaveIpAddress);
                    //    SetBertParamsToZero();
                    //    PutDataToParameters(_model.BertWaveParameters);
                    //}
                    logger.Trace("Модуль отсоединен");
                    IsUidNotNull = false;
                    IsAutoTecOk = false;
                    //StoreToDB();
                    //logger.Trace("Записи в БД при отсоединении модуля");
                    Power3v3 = 0;
                    Current3v3 = 0;
                    if(IsBertwaveConnected)
                        DisconnectFromBertwave();
                    ModDeSel = false;
                    Interrupt = false;
                    //ModAbsent = false;
                    ResetL = false;
                    LPMode = false;
                    Lambda1PeakValue = Lambda2PeakValue = Lambda3PeakValue = Lambda4PeakValue = 0;
                    MState = ModuleState.NotConnected;
                }
            }

            if (IsModuleConnected)
            {
               
                IsCdrReading = true;
                //logger.Trace("Модуль подключен");
                byte[] temp_uid = new byte[8];
                await Task.Run(() =>
                {
                    //IsCdrReading = true;
                    if (_model._uniqueId[0] == 0x00)
                    {
                        _ = _model.Utb.GetUniqueId(out temp_uid, true, true);
                        if ((temp_uid.Length == 8) && (temp_uid[0] != 0x00))
                        {
                            IsUidNotNull = true;
                            UID = temp_uid;
                        }
                    }
                    else
                    {
                        UID = _model._uniqueId;
                        IsUidNotNull = true;
                    }
                });
                //_model.Utb.WritePassword(UniversalTestBoard.Password);
                if (!IsAutotuneProcessing && !IsUseAsApi) //Если происходит автонастройка, то не читать данные CDR
                {
                    await ReadCdrData();//чтение данных CDR всех каналов, чтобы было что записывать в БД
                }
                IsCdrReading = false;
                IsTecSliderEnabled = true; //Разрешаем изменять уставку TEC
                IsModuleTuneEnded = false;
                MState = ModuleState.Connected;
            }

            if (IsModuleConnected && IsBertwaveConnected)
            {
                IsBertwaveConnected = false;
                SetBertParamsToZero();
                PutDataToParameters(_model.BertWaveParameters);
                IsBertwaveConnected = true;
            }
        }

        private void ShowEyeDiagramParameters(EyeDiagramParameters parameters)
        {
            byte statuses = _model.Utb.GetChannelStatuses();
            ZeroChannel = new EyeDiagramChannelViewModel(parameters.ZeroChannel, this)
            {
                IsEnabled = (statuses & (byte)0b1) == 0
            };
            FirstChannel = new EyeDiagramChannelViewModel(parameters.FirstChannel, this)
            {
                IsEnabled = (statuses & (byte)0b10) == 0
            };
            SecondChannel = new EyeDiagramChannelViewModel(parameters.SecondChannel, this)
            {
                IsEnabled = (statuses & (byte)0b100) == 0
            };
            ThirdChannel = new EyeDiagramChannelViewModel(parameters.ThirdChannel, this)
            {
                IsEnabled = (statuses & (byte)0b1000) == 0
            };
        }

        private void UpdateChannelStatuses()
        {
            byte statuses = _model.Utb.GetChannelStatuses();
            ZeroChannel.IsEnabled = (statuses & (byte)0b1) == 0;
            FirstChannel.IsEnabled = (statuses & (byte)0b10) == 0;
            SecondChannel.IsEnabled = (statuses & (byte)0b100) == 0;
            ThirdChannel.IsEnabled = (statuses & (byte)0b1000) == 0;
        }

        private void EyeDiagramParametersChanged(object sender, EyeDiagramParametersEventArgs e)
        {
            ShowEyeDiagramParameters(e.EyeDiagramParameters);
        }

        private void TecParametersChanged(object sender, TecParametersEventArgs e)
        {
            if (e.IsInitialParameters)
            {
                TecOptimalTemperatureVoltage = e.TecParameters.OptimalTemperatureVoltage;
            }

            TecDataGridItems[0].SetTecParameters(e.TecParameters);
            GetTemperature();//Из данных DDR
            GetDdmVoltage();
            GetDdmValues();
            //_model.RefreshUtbParams();
            Power3v3 = _model.ModuleVoltage * _model.ModuleCurrent/1000;
            IsPowerBig = Power3v3 > MaxPower;
            Current3v3 = _model.ModuleCurrent;
            ModAbsent = _model.ModuleAbsent;
            Interrupt = _model.ModuleInterrupt;
            ModDeSel = _model.ModuleDeSel;
            ResetL = _model.ModuleResetL;
            LPMode = _model.ModuleLPMode;

        }

        private void OperationProgressStatusChanged(object sender, OperationProgressStatusEventArgs e)
        {
            IsOperationInProgress = e.IsOperationInProgress;
        }

        private void OperationSuccessChanged(object sender, OperationSuccessEventArgs e)
        {
            OperationSuccess = e.Success;
        }

        private void OperationTypeChanged(object sender, OperationTypeEventArgs e)
        {
            IsIndefinitelyLongOperation = e.IsIndefinitelyLongOperation;
        }

        private void OsaStatusChanged(object sender, OsaStatusEventArgs e)
        {
            IsOsaConnected = e.IsConnected;
        }

        private void BertwaveStatusChanged(object sender, BertwaveStatusEventArgs e)
        {
            IsBertwaveConnected = e.IsConnected;
        }

        //Запись в БД
        public void  StoreToDB()
        {
            if (IsUtbConnected)
            {
                QsfpEyeRecord rec1 = new QsfpEyeRecord
                {
                    Uniqueid = _model._uniqueId,
                    Workercode = _worker.Code,
                    Channel = 1,
                    Avepower = Math.Round(AvePowerA, 2),
                    CdrRxDeemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(_zeroChannel.JitterAdjustRx, _zeroChannel.DeemphasisValueRx),
                    CdrRxMode = ChannelModeInfo.ComposeChannelModeConfigByte(_zeroChannel.InputOffsetCorrectionRx, _zeroChannel.AutoSquelchRx, _zeroChannel.HighSpeedDataPolarityInversionRx, _zeroChannel.DataInputSelectionRx,
            _zeroChannel.ClockRecoveryInCdrBypassModeRx, _zeroChannel.IsCdrBypassedAndPoweredDownRx, _zeroChannel.RateSelectionRx, _zeroChannel.PowerDownRx),
                    CdrRxSla = SlaInfo.ComposeSlaConfigByte(_zeroChannel.SlaEnRx, _zeroChannel.SlaRx),
                    CdrRxSwing = OutputSwingInfo.ComposeOutputSwingConfigByte(_zeroChannel.MuteForceRx, _zeroChannel.AutomuteRx, _zeroChannel.OutputSwingRx),
                    CdrTxCtle = CtleInfo.ComposeCtleConfigByte(_zeroChannel.CtleTx),
                    CdrTxDeemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(_zeroChannel.JitterAdjustTx, _zeroChannel.DeemphasisValueTx),
                    CdrTxMode = (int)ChannelModeInfo.ComposeChannelModeConfigByte(_zeroChannel.InputOffsetCorrectionTx, _zeroChannel.AutoSquelchTx, _zeroChannel.HighSpeedDataPolarityInversionTx, _zeroChannel.DataInputSelectionTx,
            _zeroChannel.ClockRecoveryInCdrBypassModeTx, _zeroChannel.IsCdrBypassedAndPoweredDownTx, _zeroChannel.RateSelectionTx, _zeroChannel.PowerDownTx),
                    CdrTxSwing = OutputSwingInfo.ComposeOutputSwingConfigByte(_zeroChannel.MuteForceTx, _zeroChannel.AutomuteTx, _zeroChannel.OutputSwingTx),
                    Crossing = Math.Round(CrossA, 2),
                    Extinctionratio = Math.Round(ExtRatioA, 2),
                    J2 = Math.Round(J2A, 2),
                    J9 = Math.Round(J9A, 2),
                    Jitterpp = Math.Round(JittPpA, 2),
                    LcBias = _zeroChannel.Bias,
                    LcCrossing = _zeroChannel.CrossingMagnitude,
                    LcEq = _zeroChannel.EqualizerPhaseWithMagnitude,
                    LcModul = _zeroChannel.Modulation,
                    LcOptim = (_zeroChannel.GeneralOptimization ? 0b10000 : 0b0) + (_zeroChannel.MinorTemperatureBoost ? 0b1000 : 0b0) + (_zeroChannel.MajorTemperatureBoost ? 0b100 : 0b0) + (_zeroChannel.MinorModulationBoost ? 0b10 : 00) + (_zeroChannel.MajorTemperatureBoost ? 0b1 : 0b0),//
                    Snr = Math.Round(SnrA, 2),
                    Wavelength = Math.Round(Lambda1PeakValue, 2),
                    ber = BerA != 0 ? BerA.ToString() : "0,0E-12",
                    TecCurrent = TecDataGridItems.First().Current,
                    TecDacVoltage = TecDataGridItems.First().DacVoltage,
                    TecValue = TecOptimalTemperatureVoltage,
                    TecVoltage = TecDataGridItems.First().Voltage,
                    MaskMargin = Math.Round(MaskMarginA, 2),
                    TecTemp = TecDataGridItems.First().TecTemp,
                    bertwaveip = BertwaveIpAddress,
                    osaip = OsaIpAddress,
                    moduleer = EyeA,
                    masktype = MaskRecall,
                    oma = Math.Round(MaxOmaDelta, 2)
                };

                QsfpEyeRecord rec2 = new QsfpEyeRecord
                {
                    Uniqueid = _model._uniqueId,
                    Workercode = _worker.Code,
                    Channel = 2,
                    Avepower = Math.Round(AvePowerB,2),
                    CdrRxDeemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(_firstChannel.JitterAdjustRx, _firstChannel.DeemphasisValueRx),
                    CdrRxMode = ChannelModeInfo.ComposeChannelModeConfigByte(_firstChannel.InputOffsetCorrectionRx, _firstChannel.AutoSquelchRx, _firstChannel.HighSpeedDataPolarityInversionRx, _firstChannel.DataInputSelectionRx,
                        _firstChannel.ClockRecoveryInCdrBypassModeRx, _firstChannel.IsCdrBypassedAndPoweredDownRx, _firstChannel.RateSelectionRx, _firstChannel.PowerDownRx),
                    CdrRxSla = SlaInfo.ComposeSlaConfigByte(_firstChannel.SlaEnRx, _firstChannel.SlaRx),
                    CdrRxSwing = OutputSwingInfo.ComposeOutputSwingConfigByte(_firstChannel.MuteForceRx, _firstChannel.AutomuteRx, _firstChannel.OutputSwingRx),
                    CdrTxCtle = CtleInfo.ComposeCtleConfigByte(_firstChannel.CtleTx),
                    CdrTxDeemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(_firstChannel.JitterAdjustTx, _firstChannel.DeemphasisValueTx),
                    CdrTxMode = ChannelModeInfo.ComposeChannelModeConfigByte(_firstChannel.InputOffsetCorrectionTx, _firstChannel.AutoSquelchTx, _firstChannel.HighSpeedDataPolarityInversionTx, _firstChannel.DataInputSelectionTx,
                        _firstChannel.ClockRecoveryInCdrBypassModeTx, _firstChannel.IsCdrBypassedAndPoweredDownTx, _firstChannel.RateSelectionTx, _firstChannel.PowerDownTx),
                    CdrTxSwing = OutputSwingInfo.ComposeOutputSwingConfigByte(_firstChannel.MuteForceTx, _firstChannel.AutomuteTx, _firstChannel.OutputSwingTx),
                    Crossing = Math.Round(CrossB,2),
                    Extinctionratio = Math.Round(ExtRatioB,2),
                    J2 = Math.Round(J2B, 2),
                    J9 = Math.Round(J9B, 2),
                    Jitterpp = Math.Round(JittPpB,2),
                    LcBias = _firstChannel.Bias,
                    LcCrossing = _firstChannel.CrossingMagnitude,
                    LcEq = _firstChannel.EqualizerPhaseWithMagnitude,
                    LcModul = _firstChannel.Modulation,
                    LcOptim = (_firstChannel.GeneralOptimization ? 0b10000 : 0b0) + (_firstChannel.MinorTemperatureBoost ? 0b1000 : 0b0) + (_firstChannel.MajorTemperatureBoost ? 0b100 : 0b0) + (_firstChannel.MinorModulationBoost ? 0b10 : 00) + (_firstChannel.MajorTemperatureBoost ? 0b1 : 0b0),//
                    Snr = Math.Round(SnrB,2),
                    Wavelength = Math.Round(Lambda2PeakValue, 2),
                    ber = BerB != 0 ? BerB.ToString() : "0,0E-12",
                    TecCurrent = TecDataGridItems.First().Current,
                    TecDacVoltage = TecDataGridItems.First().DacVoltage,
                    TecValue = TecOptimalTemperatureVoltage,
                    TecVoltage = TecDataGridItems.First().Voltage,
                    MaskMargin = Math.Round(MaskMarginB,2),
                    TecTemp = TecDataGridItems.First().TecTemp,
                    bertwaveip = BertwaveIpAddress,
                    osaip = OsaIpAddress,
                    moduleer = EyeB,
                    masktype = MaskRecall,
                    oma = Math.Round(MaxOmaDelta, 2)
                };

                QsfpEyeRecord rec3 = new QsfpEyeRecord
                {
                    Uniqueid = _model._uniqueId,
                    Workercode = _worker.Code,
                    Channel = 3,
                    Avepower = Math.Round(AvePowerC,2),
                    CdrRxDeemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(_secondChannel.JitterAdjustRx, _secondChannel.DeemphasisValueRx),
                    CdrRxMode = ChannelModeInfo.ComposeChannelModeConfigByte(_secondChannel.InputOffsetCorrectionRx, _secondChannel.AutoSquelchRx, _secondChannel.HighSpeedDataPolarityInversionRx, _secondChannel.DataInputSelectionRx,
                        _secondChannel.ClockRecoveryInCdrBypassModeRx, _secondChannel.IsCdrBypassedAndPoweredDownRx, _secondChannel.RateSelectionRx, _secondChannel.PowerDownRx),
                    CdrRxSla = SlaInfo.ComposeSlaConfigByte(_secondChannel.SlaEnRx, _secondChannel.SlaRx),
                    CdrRxSwing = OutputSwingInfo.ComposeOutputSwingConfigByte(_secondChannel.MuteForceRx, _secondChannel.AutomuteRx, _secondChannel.OutputSwingRx),
                    CdrTxCtle = CtleInfo.ComposeCtleConfigByte(_secondChannel.CtleTx),
                    CdrTxDeemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(_secondChannel.JitterAdjustTx, _secondChannel.DeemphasisValueTx),
                    CdrTxMode = ChannelModeInfo.ComposeChannelModeConfigByte(_secondChannel.InputOffsetCorrectionTx, _secondChannel.AutoSquelchTx, _secondChannel.HighSpeedDataPolarityInversionTx, _secondChannel.DataInputSelectionTx,
                        _secondChannel.ClockRecoveryInCdrBypassModeTx, _secondChannel.IsCdrBypassedAndPoweredDownTx, _secondChannel.RateSelectionTx, _secondChannel.PowerDownTx),
                    CdrTxSwing = OutputSwingInfo.ComposeOutputSwingConfigByte(_secondChannel.MuteForceTx, _secondChannel.AutomuteTx, _secondChannel.OutputSwingTx),
                    Crossing = Math.Round(CrossC,2),
                    Extinctionratio = ExtRatioC,
                    J2 = Math.Round(J2C, 2),
                    J9 = Math.Round(J9C, 2),
                    Jitterpp = Math.Round(JittPpC,2),
                    LcBias = _secondChannel.Bias,
                    LcCrossing = _secondChannel.CrossingMagnitude,
                    LcEq = _secondChannel.EqualizerPhaseWithMagnitude,
                    LcModul = _secondChannel.Modulation,
                    LcOptim = (_secondChannel.GeneralOptimization ? 0b10000 : 0b0) + (_secondChannel.MinorTemperatureBoost ? 0b1000 : 0b0) + (_secondChannel.MajorTemperatureBoost ? 0b100 : 0b0) + (_secondChannel.MinorModulationBoost ? 0b10 : 00) + (_secondChannel.MajorTemperatureBoost ? 0b1 : 0b0),//
                    Snr = Math.Round(SnrC,2),
                    Wavelength = Math.Round(Lambda3PeakValue, 2),
                    ber = BerC != 0 ? BerC.ToString() : "0,0E-12",
                    TecCurrent = TecDataGridItems.First().Current,
                    TecDacVoltage = TecDataGridItems.First().DacVoltage,
                    TecValue = TecOptimalTemperatureVoltage,
                    TecVoltage = TecDataGridItems.First().Voltage,
                    MaskMargin = Math.Round(MaskMarginC,2),
                    TecTemp = TecDataGridItems.First().TecTemp,
                    bertwaveip = BertwaveIpAddress,
                    osaip = OsaIpAddress,
                    moduleer = EyeC,
                    masktype = MaskRecall,
                    oma = Math.Round(MaxOmaDelta, 2)
                };

                QsfpEyeRecord rec4 = new QsfpEyeRecord
                {
                    Uniqueid = _model._uniqueId,
                    Workercode = _worker.Code,
                    Channel = 4,
                    Avepower = Math.Round(AvePowerD,2),
                    CdrRxDeemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(_thirdChannel.JitterAdjustRx, _thirdChannel.DeemphasisValueRx),
                    CdrRxMode = ChannelModeInfo.ComposeChannelModeConfigByte(_thirdChannel.InputOffsetCorrectionRx, _thirdChannel.AutoSquelchRx, _thirdChannel.HighSpeedDataPolarityInversionRx, _thirdChannel.DataInputSelectionRx,
                        _thirdChannel.ClockRecoveryInCdrBypassModeRx, _thirdChannel.IsCdrBypassedAndPoweredDownRx, _thirdChannel.RateSelectionRx, _thirdChannel.PowerDownRx),
                    CdrRxSla = SlaInfo.ComposeSlaConfigByte(_thirdChannel.SlaEnRx, _thirdChannel.SlaRx),
                    CdrRxSwing = OutputSwingInfo.ComposeOutputSwingConfigByte(_thirdChannel.MuteForceRx, _thirdChannel.AutomuteRx, _thirdChannel.OutputSwingRx),
                    CdrTxCtle = CtleInfo.ComposeCtleConfigByte(_thirdChannel.CtleTx),
                    CdrTxDeemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(_thirdChannel.JitterAdjustTx, _thirdChannel.DeemphasisValueTx),
                    CdrTxMode = ChannelModeInfo.ComposeChannelModeConfigByte(_thirdChannel.InputOffsetCorrectionTx, _thirdChannel.AutoSquelchTx, _thirdChannel.HighSpeedDataPolarityInversionTx, _thirdChannel.DataInputSelectionTx,
                        _thirdChannel.ClockRecoveryInCdrBypassModeTx, _thirdChannel.IsCdrBypassedAndPoweredDownTx, _thirdChannel.RateSelectionTx, _thirdChannel.PowerDownTx),
                    CdrTxSwing = OutputSwingInfo.ComposeOutputSwingConfigByte(_thirdChannel.MuteForceTx, _thirdChannel.AutomuteTx, _thirdChannel.OutputSwingTx),
                    Crossing = Math.Round(CrossD,2),
                    Extinctionratio = Math.Round(ExtRatioD,2),
                    J2 = Math.Round(J2D, 2),
                    J9 = Math.Round(J9D, 2),
                    Jitterpp = Math.Round(JittPpD,2),
                    LcBias = _thirdChannel.Bias,
                    LcCrossing = _thirdChannel.CrossingMagnitude,
                    LcEq = _thirdChannel.EqualizerPhaseWithMagnitude,
                    LcModul = _thirdChannel.Modulation,
                    LcOptim = (_thirdChannel.GeneralOptimization ? 0b10000 : 0b0) + (_thirdChannel.MinorTemperatureBoost ? 0b1000 : 0b0) + (_thirdChannel.MajorTemperatureBoost ? 0b100 : 0b0) + (_thirdChannel.MinorModulationBoost ? 0b10 : 00) + (_secondChannel.MajorTemperatureBoost ? 0b1 : 0b0),//
                    Snr = Math.Round(SnrD,2),
                    Wavelength = Math.Round(Lambda4PeakValue, 2),
                    ber = BerD != 0 ? BerD.ToString() : "0,0E-12",
                    TecCurrent = TecDataGridItems.First().Current,
                    TecDacVoltage = TecDataGridItems.First().DacVoltage,
                    TecValue = TecOptimalTemperatureVoltage,
                    TecVoltage = TecDataGridItems.First().Voltage,
                    MaskMargin = Math.Round(MaskMarginD,2),
                    TecTemp = TecDataGridItems.First().TecTemp,
                    bertwaveip = BertwaveIpAddress,
                    osaip = OsaIpAddress,
                    moduleer = EyeD,
                    masktype = MaskRecall,
                    oma = Math.Round(MaxOmaDelta,2)
                };
                if (_model._uniqueId != null && !Array.TrueForAll(_model._uniqueId, el => el == 0))//если нет UID - предположительно модуль неисправен - не сохраняем в БД
                {
                    using (var db = new QsfpEyeDataContext(Properties.Settings.Default.DatabaseServer, Properties.Settings.Default.DatabaseUser, Properties.Settings.Default.DatabasePassword,
                        Properties.Settings.Default.DatabaseName))
                    {
                        db.QsfpEye.Add(rec1);
                        db.QsfpEye.Add(rec2);
                        db.QsfpEye.Add(rec3);
                        db.QsfpEye.Add(rec4);
                        db.Database.BeginTransactionAsync();
                        db.SaveChanges();
                        db.Database.CommitTransaction();
                    }
                }
            }
        }

        private void Quit()
        {
            Properties.Settings.Default.sigma = Sigma;
            Properties.Settings.Default.Save();

            //if (IsModuleConnected)
            //{
            //    StoreToDB();
            //}

            if (bertTimer != null)
            {
                bertTimer.Dispose();
            }
            if (OsaTimer != null)
            {
                OsaTimer.Dispose();
            }
            _model.Quit();
        }

        public void UtbMonitoringPause()
        {
            //RefreshTecParameters = false;
            _model.RefreshStatusUtb = false;
            _model.StopMonitoringTasks();
            logger.Trace("Пауза мониторинга UTB");
        }

        public void UtbMonitoringRelease()
        {
            _model.InitializeMonitoringTasks();
            logger.Trace("Мониторинг UTB запущен");
            Trace.WriteLine("Мониторинг UTB запущен");
        }

        #region CDR 
        public void WriteCdrCtle(byte Value, Channel channel)
        {
            //object locker = new object();
            byte ctle = CtleInfo.ComposeCtleConfigByte(Value);

            _ = _model.Utb.WriteCdrCtle(ctle, channel, 1);

        }

        public void WriteSla(bool ena, int Value, Channel channel)
        {
            byte sla = SlaInfo.ComposeSlaConfigByte(ena, Value);
                _ = _model.Utb.WriteCdrCtle(sla, channel, 0);

        }

        public void WriteCdrSwing(bool mute, bool amute_dis, byte Value, Channel channel, byte TxRx)
        {
            object locker = new object();
            byte swing = OutputSwingInfo.ComposeOutputSwingConfigByte(mute, amute_dis, Value);
            EyeDiagramChannelViewModel channelViewModel = null; ;
            bool result;
            lock (locker)
            {
                result = _model.Utb.WriteCdrSwing(swing, channel, TxRx);
            }

            switch (channel)
            {
                case Channel.Zero:
                    channelViewModel = ZeroChannel;
                    break;
                case Channel.First:
                    channelViewModel = FirstChannel;
                    break;
                case Channel.Second:
                    channelViewModel = SecondChannel;
                    break;
                case Channel.Third:
                    channelViewModel = ThirdChannel;
                    break;
            }
            if (TxRx == 1)
            {
                channelViewModel.IsOperationCdrSuccessTx = result;
            }
            else
            {
                channelViewModel.IsOperationCdrSuccessRx = result;
            }

        }

        public void WriteCdrDeemphasis(bool jittAdjust,byte Value,Channel channel, byte TxRx)
        {
            object locker = new object();
            byte deemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(jittAdjust, Value);
            EyeDiagramChannelViewModel channelViewModel = null;
            bool result;
            lock(locker)
            {
                result = _model.Utb.WriteCdrDeemphasis(deemph, channel, TxRx);
            }
            switch (channel)
            {
                case Channel.Zero:
                    channelViewModel = ZeroChannel;
                    break;
                case Channel.First:
                    channelViewModel = FirstChannel;
                    break;
                case Channel.Second:
                    channelViewModel = SecondChannel;
                    break;
                case Channel.Third:
                    channelViewModel = ThirdChannel;
                    break;
            }
            if (TxRx == 1)
            {
                channelViewModel.IsOperationCdrSuccessTx = result;
            }
            else
            {
                channelViewModel.IsOperationCdrSuccessRx = result;
            }
        }

        private ICommand _readCdrCommand;
        public ICommand ReadCdrCommand
        {
            get => _readCdrCommand ?? (_readCdrCommand = new RelayCommand(
                p => IsModuleConnected,
                p => Task.Run(() => ReadCdrData())
                ));
        }

        public async Task ReadCdrData()
        {


            int waitTime = 50;
            if (IsModuleConnected)
            {
                IsCdrReading = true;

                //RefreshTecParameters = false;
                //UtbMonitoringPause();
                //logger.Trace("Начали чтение CDR");
                IsOperationInProgress = true;
                bool IsIndefine = IsIndefinitelyLongOperation;
                IsIndefinitelyLongOperation = true;
                try
                {
                    await Task.Run(() =>
                    { 
                    do
                    {
                        Std.Shared.Timing.Pause(waitTime - 10);
                        ZeroChannel.PutCdrParametersToProperties(Channel.Zero);
                        //await Task.Delay(waitTime);

                    } while (!ZeroChannel.IsOperationCdrSuccessTx);
                    //logger.Trace("Прочитали Tx I канала");

                    do
                    {
                        Std.Shared.Timing.Pause(waitTime - 10);
                        ZeroChannel.ReadChannelCdrRxParameters(Channel.Zero);
                        //await Task.Delay(waitTime);

                    } while (!ZeroChannel.IsOperationCdrSuccessRx);
                    //logger.Trace("Прочитали Rx I канала");

                    do
                    {
                        FirstChannel.PutCdrParametersToProperties(Channel.First);
                        //await Task.Delay(waitTime);
                        Std.Shared.Timing.Pause(waitTime - 10);
                    } while (!FirstChannel.IsOperationCdrSuccessTx);
                    //logger.Trace("Прочитали Tx II канала");

                    do
                    {
                        FirstChannel.ReadChannelCdrRxParameters(Channel.First);
                        //await Task.Delay(waitTime);
                        Std.Shared.Timing.Pause(waitTime - 10);
                    } while (!FirstChannel.IsOperationCdrSuccessRx);
                    //logger.Trace("Прочитали Rx II канала");

                    //if (SecondChannel.Parent == null)
                    //{
                    //    SecondChannel.Parent = this;
                    //}
                    do
                    {
                        SecondChannel.PutCdrParametersToProperties(Channel.Second);
                        //await Task.Delay(waitTime);
                        Std.Shared.Timing.Pause(waitTime - 10);
                    } while (!SecondChannel.IsOperationCdrSuccessTx);
                    //logger.Trace("Прочитали Tx III канала");
                    do
                    {
                        SecondChannel.ReadChannelCdrRxParameters(Channel.Second);
                        //await Task.Delay(waitTime);
                        Std.Shared.Timing.Pause(waitTime - 10);
                    } while (!SecondChannel.IsOperationCdrSuccessRx);

                    //logger.Trace("Прочитали Rx III канала");
                    do
                    {
                        ThirdChannel.PutCdrParametersToProperties(Channel.Third);
                        //await Task.Delay(waitTime);
                        Std.Shared.Timing.Pause(waitTime - 10);
                    } while (!ThirdChannel.IsOperationCdrSuccessTx);

                    //logger.Trace("Прочитали Tx IV канала");
                    do
                    {
                        ThirdChannel.ReadChannelCdrRxParameters(Channel.Third);
                        Std.Shared.Timing.Pause(waitTime - 10);
                    } while (!ThirdChannel.IsOperationCdrSuccessRx);
                    });
                    //logger.Trace("Прочитали Rx IV канала");
                }
                catch (Exception /*ex*/)
                {
                    MessageBox.Show("Данные CDR считаны не полностью, перечитайте их кнопками 'ReadCdr'", "Сбой чтения CDR", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                //} while (!successRead);
                IsIndefinitelyLongOperation = IsIndefine;
                IsOperationInProgress = false;

                //_model.UtbStatusChanged += UtbStatusChanged;
                //_model.ModuleStatusChanged += ModuleStatusChanged;
                RefreshTecParameters = true;
                //UtbMonitoringRelease();
                IsCdrReading = false;
            }

        }

        private bool _isCdrReading = false;//Для отображения надписи о чтении CDR
        public bool IsCdrReading
        {
            get => _isCdrReading;
            set
            {
                if (_isCdrReading != value)
                {
                    _isCdrReading = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Данные от BertWave

        private double _avePowerA;
        public double AvePowerA
        {
            get => _avePowerA;
            set
            {
                if (_avePowerA != value)
                {
                    _avePowerA = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _avePowerB;
        public double AvePowerB
        {
            get => _avePowerB;
            set
            {
                if (_avePowerB != value)
                {
                    _avePowerB = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _avePowerC;
        public double AvePowerC
        {
            get => _avePowerC;
            set
            {
                if (_avePowerC != value)
                {
                    _avePowerC = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _avePowerD;
        public double AvePowerD
        {
            get => _avePowerD;
            set
            {
                if (_avePowerD != value)
                {
                    _avePowerD = value;
                    OnPropertyChanged();
                }
            }
        }


        private double _extRatioA;
        public double ExtRatioA
        {
            get => _extRatioA;
            set
            {
                if (_extRatioA != value)
                {
                    _extRatioA = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _extRatioB;
        public double ExtRatioB
        {
            get => _extRatioB;
            set
            {
                if (_extRatioB != value)
                {
                    _extRatioB = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _extRatioC;
        public double ExtRatioC
        {
            get => _extRatioC;
            set
            {
                if (_extRatioC != value)
                {
                    _extRatioC = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _extRatioD;
        public double ExtRatioD
        {
            get => _extRatioD;
            set
            {
                if (_extRatioD != value)
                {
                    _extRatioD = value;
                    OnPropertyChanged();
                }
            }
        }


        private double _snrA;
        public double SnrA
        {
            get => _snrA;
            set
            {
                if (_snrA != value)
                {
                    _snrA = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _snrB;
        public double SnrB
        {
            get => _snrB;
            set
            {
                if (_snrB != value)
                {
                    _snrB = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _snrC;
        public double SnrC
        {
            get => _snrC;
            set
            {
                if (_snrC != value)
                {
                    _snrC = value;
                    OnPropertyChanged();
                }
            }
        }
        private double _snrD;
        public double SnrD
        {
            get => _snrD;
            set
            {
                if (_snrD != value)
                {
                    _snrD = value;
                    OnPropertyChanged();
                }
            }
        }


        private double _crossA;
        public double CrossA
        {
            get => _crossA;
            set
            {
                if (_crossA != value)
                {
                    _crossA = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _crossB;
        public double CrossB
        {
            get => _crossB;
            set
            {
                if (_crossB != value)
                {
                    _crossB = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _crossC;
        public double CrossC
        {
            get => _crossC;
            set
            {
                if (_crossC != value)
                {
                    _crossC = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _crossD;
        public double CrossD
        {
            get => _crossD;
            set
            {
                if (_crossD != value)
                {
                    _crossD = value;
                    OnPropertyChanged();
                }
            }
        }


        private double _jittPpA;
        public double JittPpA
        {
            get => _jittPpA;
            set
            {
                if (_jittPpA != value)
                {
                    _jittPpA = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _jittPpB;
        public double JittPpB
        {
            get => _jittPpB;
            set
            {
                if (_jittPpB != value)
                {
                    _jittPpB = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _jittPpC;
        public double JittPpC
        {
            get => _jittPpC;
            set
            {
                if (_jittPpC != value)
                {
                    _jittPpC = value;
                    OnPropertyChanged();

                }
            }
        }

        private double _jittPpD;
        public double JittPpD
        {
            get => _jittPpD;
            set
            {
                if (_jittPpD != value)
                {
                    _jittPpD = value;
                    OnPropertyChanged();
                }
            }
        }


        private double _j2A;
        public double J2A
        {
            get => _j2A;
            set
            {
                if (_j2A != value)
                {
                    _j2A = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _j2B;
        public double J2B
        {
            get => _j2B;
            set
            {
                if (_j2B != value)
                {
                    _j2B = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _j2C;
        public double J2C
        {
            get => _j2C;
            set
            {
                if (_j2C != value)
                {
                    _j2C = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _j2D;
        public double J2D
        {
            get => _j2D;
            set
            {
                if (_j2D != value)
                {
                    _j2D = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _j9A;
        public double J9A
        {
            get => _j9A;
            set
            {
                if (_j9A != value)
                {
                    _j9A = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _j9B;
        public double J9B
        {
            get => _j9B;
            set
            {
                if (_j9B != value)
                {
                    _j9B = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _j9C;
        public double J9C
        {
            get => _j9C;
            set
            {
                if (_j9C != value)
                {
                    _j9C = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _j9D;
        public double J9D
        {
            get => _j9D;
            set
            {
                if (_j9D != value)
                {
                    _j9D = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _mMA;
        public double MaskMarginA
        {
            get => _mMA;
            set
            {
                if(_mMA!=value)
                {
                    _mMA = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _mMB;
        public double MaskMarginB
        {
            get => _mMB;
            set
            {
                if (_mMB != value)
                {
                    _mMB = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _mMC;
        public double MaskMarginC
        {
            get => _mMC;
            set
            {
                if (_mMC != value)
                {
                    _mMC = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _mMD;
        public double MaskMarginD
        {
            get => _mMD;
            set
            {
                if (_mMD != value)
                {
                    _mMD = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _berA = -1.0;
        public double BerA
        {
            get => _berA;
            set => Set(ref _berA, value);
        }

        private double _berB = -1.0;
        public double BerB
        {
            get => _berB;
            set => Set(ref _berB, value);
        }

        private double _berC = -1.0;
        public double BerC
        {
            get => _berC;
            set => Set(ref _berC, value);
        }

        private double _berD = -1.0;
        public double BerD
        {
            get => _berD;
            set => Set(ref _berD, value);
        }

        private double _maxOmaDelta = default;
        public double MaxOmaDelta
        {
            get => _maxOmaDelta;
            set => Set(ref _maxOmaDelta, value);
        }

        #endregion

        #region Работа с BewrtWave
        public void PutDataToParameters(Dictionary<string, double> paramsDict)
        {
            //var paramsDict = new Dictionary<string, double>();

            if (_model.IsBertwaveConnected)
            {
                if (Is4C)
                {
                    //_model.Bertwave.DisplayAutoScale();
                    //Task.Delay(500).Wait();
                    //await Task.Run(()=> _model.BertWaveParameters = _model.RefreshBertData());
                    _model.BertWaveParameters = _model.RefreshBertData();
                    double[] Omas = new double[4];
                    int i = 0;
                    foreach(BertwaveChannels ch in Enum.GetValues(typeof(BertwaveChannels)))
                    {
                        _=_model.Bertwave.GetBertwaveOma(ch,out Omas[i]);
                        i++;
                    }
                    double max = Omas.Max();
                    double min = Omas.Min();
                    //for (int j = 0; j < 4; j++)
                    //    for (int k = j; k < 4; k++)
                    //        if (j != k)
                    //        {
                    //            double delta = Math.Abs(Omas[j] - Omas[k]);
                    //            if (delta > maxDelta) maxDelta = delta;
                    //            if (delta < minDelta) minDelta = delta;
                    //        }

                    MaxOmaDelta = 10 * Math.Log10(max/min);
                }

                double dv;
                    paramsDict = _model.BertWaveParameters;
                    _ = paramsDict.TryGetValue(nameof(CrossA), out dv);
                    CrossA = dv;
                    _ = paramsDict.TryGetValue(nameof(CrossB), out dv);
                    CrossB = dv;
                    _ = paramsDict.TryGetValue(nameof(CrossC), out dv);
                    CrossC = dv;
                    _ = paramsDict.TryGetValue(nameof(CrossD), out dv);
                    CrossD = dv;

                    _ = paramsDict.TryGetValue(nameof(SnrA), out dv);
                    SnrA = dv;
                    _ = paramsDict.TryGetValue(nameof(SnrB), out dv);
                    SnrB = dv;
                    _ = paramsDict.TryGetValue(nameof(SnrC), out dv);
                    SnrC = dv;
                    _ = paramsDict.TryGetValue(nameof(SnrD), out dv);
                    SnrD = dv;

                    _ = paramsDict.TryGetValue(nameof(AvePowerA), out dv);
                    AvePowerA = dv + ZeroChannel.Attenuation;
                    _ = paramsDict.TryGetValue(nameof(AvePowerB), out dv);
                    AvePowerB = dv + FirstChannel.Attenuation;
                    _ = paramsDict.TryGetValue(nameof(AvePowerC), out dv);
                    AvePowerC = dv + SecondChannel.Attenuation;
                    _ = paramsDict.TryGetValue(nameof(AvePowerD), out dv);
                    AvePowerD = dv + ThirdChannel.Attenuation;

                    _ = paramsDict.TryGetValue(nameof(ExtRatioA), out dv);
                    ExtRatioA = dv;
                    _ = paramsDict.TryGetValue(nameof(ExtRatioB), out dv);
                    ExtRatioB = dv;
                    _ = paramsDict.TryGetValue(nameof(ExtRatioC), out dv);
                    ExtRatioC = dv;
                    _ = paramsDict.TryGetValue(nameof(ExtRatioD), out dv);
                    ExtRatioD = dv;

                    _ = paramsDict.TryGetValue(nameof(JittPpA), out dv);
                    JittPpA = dv * 25.78125;
                    _ = paramsDict.TryGetValue(nameof(JittPpB), out dv);
                    JittPpB = dv * 25.78125;
                _ = paramsDict.TryGetValue(nameof(JittPpC), out dv);
                    JittPpC = dv * 25.78125;
                _ = paramsDict.TryGetValue(nameof(JittPpD), out dv);
                    JittPpD = dv * 25.78125;

                _ = paramsDict.TryGetValue(nameof(J2A), out dv);
                    J2A = dv;
                    _ = paramsDict.TryGetValue(nameof(J2B), out dv);
                    J2B = dv;
                    _ = paramsDict.TryGetValue(nameof(J2C), out dv);
                    J2C = dv;
                    _ = paramsDict.TryGetValue(nameof(J2D), out dv);
                    J2D = dv;

                    _ = paramsDict.TryGetValue(nameof(J9A), out dv);
                    J9A = dv;
                    _ = paramsDict.TryGetValue(nameof(J9B), out dv);
                    J9B = dv;
                    _ = paramsDict.TryGetValue(nameof(J9C), out dv);
                    J9C = dv;
                    _ = paramsDict.TryGetValue(nameof(J9D), out dv);
                    J9D = dv;

                    _ = paramsDict.TryGetValue(nameof(MaskMarginA), out dv);
                    MaskMarginA = dv;
                    _ = paramsDict.TryGetValue(nameof(MaskMarginB), out dv);
                    MaskMarginB = dv;
                    _ = paramsDict.TryGetValue(nameof(MaskMarginC), out dv);
                    MaskMarginC = dv;
                    _ = paramsDict.TryGetValue(nameof(MaskMarginD), out dv);
                    MaskMarginD = dv;

                    _ = paramsDict.TryGetValue(nameof(BerA), out dv);
                    BerA = dv;
                    _ = paramsDict.TryGetValue(nameof(BerB), out dv);
                    BerB = dv;
                    _ = paramsDict.TryGetValue(nameof(BerC), out dv);
                    BerC = dv;
                    _ = paramsDict.TryGetValue(nameof(BerD), out dv);
                    BerD = dv;
                
            }
        }

        public void SetBertParamsToZero()
        {
            CrossA = CrossB = CrossC = CrossD = 0.0;
            SnrA = SnrB = SnrC = SnrD = 0;
            AvePowerA = AvePowerB = AvePowerC = AvePowerD = 0;
            ExtRatioA = ExtRatioB = ExtRatioC = ExtRatioD = 0;
            JittPpA = JittPpB = JittPpC = JittPpD = 0;
            J2A = J2B = J2C = J2D = 0.0;
            J9A = J9B = J9C = J9D = 0.0;
            MaskMarginA = MaskMarginB = MaskMarginC = MaskMarginD = 0;
            BerA = BerB = BerC = BerD = 0.00;
        }
        #endregion

        #region Автонастройка TEC

        private ICommand _autoTuneTecCommand;
        public ICommand AutoTuneTecCommand
        {
             get => _autoTuneTecCommand ?? (_autoTuneTecCommand = new RelayCommand(
                p => !IsAutotuneProcessing,
                p => Task.Run(() => AutoTuneTec2())
                ));
        }


        private bool _isAutoTuneProcessing = false;
        public bool IsAutotuneProcessing
        {
            get => _isAutoTuneProcessing;
            set
            {
                if(_isAutoTuneProcessing!=value)
                {
                    _isAutoTuneProcessing = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _delta1 = 0.0;
        public double Delta1
        {
            get => _delta1;
            set
            {
                if (_delta1 != value)
                {
                    _delta1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _delta2 = 0.0;
        public double Delta2
        {
            get => _delta2;
            set
            {
                if(_delta2!=value)
                {
                    _delta2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _delta3 = 0.0;
        public double Delta3
        {
            get => _delta3;
            set
            {
                if(_delta3!=value)
                {
                    _delta3 = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _delta4 = 0.0;
        public double Delta4
        {
            get => _delta4;
            set
            {
                if(_delta4!=value)
                {
                    _delta4 = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isAutoTecOk = false;
        public bool IsAutoTecOk
        {
            get => _isAutoTecOk;
            set => Set(ref _isAutoTecOk, value);
        }
        public void RefreshDeltasVM(double[] deltas)
        {
            //await Task.Run(() =>
            //{

                Delta1 = deltas[0];
                Delta2 = deltas[1];
                Delta3 = deltas[2];
                Delta4 = deltas[3];
            //});
            //logger.Trace("Обновили дельты для отображения");
        }

        public double[] GetDeltas()
        {
            double[] result = new double[4];
            result[0] = Lambda1PeakValue - MainWindow.wlIdeal1;
            result[1] = Lambda2PeakValue - MainWindow.wlIdeal2;
            result[2] = Lambda3PeakValue - MainWindow.wlIdeal3;
            result[3] = Lambda4PeakValue - MainWindow.wlIdeal4;
            SumOfDeltas = result.Sum();
            return result;
        }

        public bool atBreak = false;
        //private static Semaphore sem = new Semaphore(1, 1);
        public async Task AutoTuneTec2()
        {
            //sem.WaitOne();
            IsAutoTecOk = false;
            IsAutotuneProcessing = true;
            IsTecSliderEnabled = false;
            //double sigma = 0.05;//предел автонастройки
            int deltaTec = 1000;
            double[] deltas = new double[4];
            double[] peaksValues = new double[4];
            //logger.Trace("Начали автонастройку");
            OsaTimer.Stop();
            Thread.Sleep(7500);
            double avg = 10000;
            int it = 1;
            while (it < 10 && avg > Sigma && !IsAutoTuneTecCancelled)
            {
                RefreshOsaData();
                //logger.Trace("Обновили пики с OSA");
                    //Thread.Sleep(600);
                    Std.Shared.Timing.Pause(590);
                    deltas[0] = Lambda1PeakValue - MainWindow.wlIdeal1;
                    deltas[1] = Lambda2PeakValue - MainWindow.wlIdeal2;
                    deltas[2] = Lambda3PeakValue - MainWindow.wlIdeal3;
                    deltas[3] = Lambda4PeakValue - MainWindow.wlIdeal4;
                logger.Trace($"Значение TEC {TecOptimalTemperatureVoltage}");
                //int i = 0;
                //foreach (double delta in deltas)
                //    {
                //    i++;
                //        logger.Trace($"Дельта {i} {delta:F2}");
                //    }
                    var maxDeltas = deltas.OrderBy(l => Math.Abs(l)).Skip(2).Take(2).ToArray();
                    avg = Math.Abs(maxDeltas.Average());
                    //logger.Trace($"Модуль средней дельты из двух максимальных {avg:F3}");
                if (avg > Sigma)
                {
                    int sign = maxDeltas.Last() > 0 ? 1 : -1;
                    //logger.Trace($"Направление {sign}");
                    deltaTec = sign * (int)Math.Round(avg * 20);
                    //logger.Trace($"Сдвигаем уставку на {deltaTec}");
                    TecOptimalTemperatureVoltage += deltaTec;
                    if (TecOptimalTemperatureVoltage < 2150) TecOptimalTemperatureVoltage = 2151;
                    if (TecOptimalTemperatureVoltage > 2250) TecOptimalTemperatureVoltage = 2249;

                    WriteTecOptimalTemperatureVoltage(TecOptimalTemperatureVoltage);
                    int tecSigma = (deltaTec > 20) ? 5 : 2;
                    //logger.Trace("Записали TEC, ждем когда успокоится");
                    await Task.Run(() => { while (Math.Abs(TecOptimalTemperatureVoltage - TecDataGridItems[0].TecTemp) > tecSigma) { } });
                    Task.Delay(2000).GetAwaiter().GetResult();
                    //logger.Trace("Дождались, вычисляем");
                    it++;
                    //logger.Trace($"Итерация {it}");
                }
                else break;
            }
            if (it >= 9) MessageBox.Show("Автонастройка TEC завершена принудительно из-за большой длительности");
            OsaTimer.Start();

            if (!IsAutoTuneTecCancelled)
            {
                IsAutoTecOk = true;
            }

            IsAutotuneProcessing = false;
            IsTecSliderEnabled = true;
            //logger.Trace("Завершили автонастройку");
            atBreak = false;
            IsAutoTuneTecCancelled = false;
        }



        private async Task ReFreshOsa()
        {
            //_model.Osa.RefreshOsa();
            DisconnectFromOsa();
            await ConnectToOsa(OsaIpAddress);
        }
        #endregion


        private ICommand _writeModuleParamsCommand;
        public ICommand WriteModuleParamsCommand
        {
            get => _writeModuleParamsCommand ?? (_writeModuleParamsCommand = new RelayCommand(
                p => true,
                p => WriteModuleParams()
                ));
        }

        void WriteModuleParams()
        {
            //RefreshTecParameters = false;
            _model.WriteModuleParameters(LPMode, ModDeSel, ResetL);
            //RefreshTecParameters = true;
        }

        #region DDM Data

        //private double _ddmTemperature;
        public double DdmTemperature
        {
            get => _model._ddmTemperature;
            set
            {
                if (_model._ddmTemperature != value)
                {
                    _model._ddmTemperature = value;
                    OnPropertyChanged();
                }
            }
        }

        public double DdmVoltage
        {
            get=> _model._ddmVoltage;
            set
            {
                if (value != _model._ddmVoltage)
                {
                    _model._ddmVoltage = value;
                    OnPropertyChanged();
                }
            }
        }

        public double DdmTxPower1
        {
            get=>_model._ddmTxPower1;
            set
            {
                if (value != _model._ddmTxPower1)
                {
                    _model._ddmTxPower1 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DdmTxPower2
        {
            get => _model._ddmTxPower2;
            set
            {
                if (value != _model._ddmTxPower2)
                {
                    _model._ddmTxPower2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DdmTxPower3
        {
            get => _model._ddmTxPower3;
            set
            {
                if (value != _model._ddmTxPower3)
                {
                    _model._ddmTxPower3 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DdmTxPower4
        {
            get => _model._ddmTxPower4;
            set
            {
                if (value != _model._ddmTxPower4)
                {
                    _model._ddmTxPower4 = value;
                    OnPropertyChanged();
                }
            }
        }

        public double DdmRxPower1
        {
            get => _model._ddmRxPower1;
            set
            {
                if (value != _model._ddmRxPower1)
                {
                    _model._ddmRxPower1 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DdmRxPower2
        {
            get => _model._ddmRxPower2;
            set
            {
                if (value != _model._ddmRxPower2)
                {
                    _model._ddmRxPower2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DdmRxPower3
        {
            get => _model._ddmRxPower3;
            set
            {
                if (value != _model._ddmRxPower3)
                {
                    _model._ddmRxPower3 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DdmRxPower4
        {
            get => _model._ddmRxPower4;
            set
            {
                if (value != _model._ddmRxPower4)
                {
                    _model._ddmRxPower4 = value;
                    OnPropertyChanged();
                }
            }
        }

        public double DdmTxBias1
        {
            get => _model._ddmTxBias1;
            set
            {
                if (value != _model._ddmTxBias1)
                {
                    _model._ddmTxBias1 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DdmTxBias2
        {
            get => _model._ddmTxBias2;
            set
            {
                if (value != _model._ddmTxBias2)
                {
                    _model._ddmTxBias2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DdmTxBias3
        {
            get => _model._ddmTxBias3;
            set
            {
                if (value != _model._ddmTxBias3)
                {
                    _model._ddmTxBias3 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double DdmTxBias4
        {
            get => _model._ddmTxBias4;
            set
            {
                if (value != _model._ddmTxBias4)
                {
                    _model._ddmTxBias4 = value;
                    OnPropertyChanged();
                }
            }
        }

        private void GetTemperature()
        {

            _model.Utb.GetDdmTemperature(out double temp);
            DdmTemperature = temp;
        }

        private void GetDdmVoltage()
        {
            DdmVoltage = _model.Utb.GetDdmVoltage();
        }

        private void GetDdmValues()
        {
            byte[] values = _model.Utb.GetDdmValues();
            DdmRxPower1 = Std.Data.Converters.DdmValuesConverter.ToDbm(values[0]*256+values[1]);
            DdmRxPower2 = Std.Data.Converters.DdmValuesConverter.ToDbm(values[2] * 256 + values[3]);
            DdmRxPower3 = Std.Data.Converters.DdmValuesConverter.ToDbm(values[4] * 256 + values[5]);
            DdmRxPower4 = Std.Data.Converters.DdmValuesConverter.ToDbm(values[6] * 256 + values[7]);

            DdmTxBias1 = Std.Data.Converters.DdmValuesConverter.ToMilliamps(values[8] * 256 + values[9]);
            DdmTxBias2 = Std.Data.Converters.DdmValuesConverter.ToMilliamps(values[10] * 256 + values[11]);
            DdmTxBias3 = Std.Data.Converters.DdmValuesConverter.ToMilliamps(values[12] * 256 + values[13]);
            DdmTxBias4 = Std.Data.Converters.DdmValuesConverter.ToMilliamps(values[14] * 256 + values[15]);

            DdmTxPower1 = Std.Data.Converters.DdmValuesConverter.ToDbm(values[16] * 256 + values[17]);
            DdmTxPower2 = Std.Data.Converters.DdmValuesConverter.ToDbm(values[18] * 256 + values[19]);
            DdmTxPower3 = Std.Data.Converters.DdmValuesConverter.ToDbm(values[20] * 256 + values[21]);
            DdmTxPower4 = Std.Data.Converters.DdmValuesConverter.ToDbm(values[22] * 256 + values[23]);

        }

        #endregion

        #region Файлы
        private ICommand _saveToFileCommand;
        public ICommand SaveToFileCommand
        {
            get => _saveToFileCommand ?? (_saveToFileCommand = new RelayCommand(
                p=>true,
                p=>SaveToFile()
                ));
        }

        public void SaveToFile()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Файлы настройки QSFP|*.qet"
            };
            bool res = (bool)dialog.ShowDialog();
            if (!res)
            {
                MessageBox.Show("Не удалось открыть файл", "Неудача", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            //далее запись файла
            using (BinaryWriter sw = new BinaryWriter(File.OpenWrite(dialog.FileName)))
            {
                //sw.Write(RefreshTecParameters);
                sw.Write(TecOptimalTemperatureVoltage);
                //LC ZeroChannel.IsEnabled
                sw.Write(ZeroChannel.Bias);
                sw.Write(ZeroChannel.Modulation);
                sw.Write(ZeroChannel.IsEqualizerEnabled);
                sw.Write(ZeroChannel.EqualizerPhaseWithMagnitude);
                sw.Write(ZeroChannel.GeneralOptimization);
                sw.Write(ZeroChannel.MinorTemperatureBoost);
                sw.Write(ZeroChannel.MajorTemperatureBoost);
                sw.Write(ZeroChannel.MinorModulationBoost);
                sw.Write(ZeroChannel.MajorModulationBoost);
                sw.Write(ZeroChannel.IsCrossingEnabled);
                sw.Write(ZeroChannel.CrossingMagnitude);

                sw.Write(FirstChannel.Bias);
                sw.Write(FirstChannel.Modulation);
                sw.Write(FirstChannel.IsEqualizerEnabled);
                sw.Write(FirstChannel.EqualizerPhaseWithMagnitude);
                sw.Write(FirstChannel.GeneralOptimization);
                sw.Write(FirstChannel.MinorTemperatureBoost);
                sw.Write(FirstChannel.MajorTemperatureBoost);
                sw.Write(FirstChannel.MinorModulationBoost);
                sw.Write(FirstChannel.MajorModulationBoost);
                sw.Write(FirstChannel.IsCrossingEnabled);
                sw.Write(FirstChannel.CrossingMagnitude);

                sw.Write(SecondChannel.Bias);
                sw.Write(SecondChannel.Modulation);
                sw.Write(SecondChannel.IsEqualizerEnabled);
                sw.Write(SecondChannel.EqualizerPhaseWithMagnitude);
                sw.Write(SecondChannel.GeneralOptimization);
                sw.Write(SecondChannel.MinorTemperatureBoost);
                sw.Write(SecondChannel.MajorTemperatureBoost);
                sw.Write(SecondChannel.MinorModulationBoost);
                sw.Write(SecondChannel.MajorModulationBoost);
                sw.Write(SecondChannel.IsCrossingEnabled);
                sw.Write(SecondChannel.CrossingMagnitude);

                sw.Write(ThirdChannel.Bias);
                sw.Write(ThirdChannel.Modulation);
                sw.Write(ThirdChannel.IsEqualizerEnabled);
                sw.Write(ThirdChannel.EqualizerPhaseWithMagnitude);
                sw.Write(ThirdChannel.GeneralOptimization);
                sw.Write(ThirdChannel.MinorTemperatureBoost);
                sw.Write(ThirdChannel.MajorTemperatureBoost);
                sw.Write(ThirdChannel.MinorModulationBoost);
                sw.Write(ThirdChannel.MajorModulationBoost);
                sw.Write(ThirdChannel.IsCrossingEnabled);
                sw.Write(ThirdChannel.CrossingMagnitude);

                //CDR ZeroChannel.PowerDownTx
                //cdr ZeroChannel.RateSelectionTx
                //CDR ZeroChannel.IsCdrBypassedAndPoweredDownTx
                //CDR ZeroChannel.ClockRecoveryInCdrBypassModeTx
                //CDR ZeroChannel.DataInputSelectionTx
                //CDR ZeroChannel.HighSpeedDataPolarityInversionTx
                //CDR ZeroChannel.AutoSquelchTx
                //CDR ZeroChannel.InputOffsetCorrectionTx

                //CDR ZeroChannel.CtleTx

                //CDR ZeroChannel.AutomuteTx
                //CDR ZeroChannel.MuteForceTx
                //CDR ZeroChannel.OutputSwingTx

                //CDR ZeroChannel.JitterAdjustTx
                //CDR ZeroChannel.DeemphasisValueTx

                //CDR ZeroChannel.PowerDownRx
                //CDR ZeroChannel.RateSelectionRx
                //CDR ZeroChannel.IsCdrBypassedAndPoweredDownRx
                //CDR ZeroChannel.ClockRecoveryInCdrBypassModeRx
                //CDR ZeroChannel.DataInputSelectionRx
                //CDR ZeroChannel.HighSpeedDataPolarityInversionRx
                //CDR ZeroChannel.AutoSquelchRx
                //CDR ZeroChannel.InputOffsetCorrectionRx

                //CDR ZeroChannel.SlaEnRx
                //CDR ZeroChannel.SlaRx

                //CDR ZeroChannel.AutomuteRx
                //CDR ZeroChannel.MuteForceRx
                //CDR ZeroChannel.OutputSwingRx

                //CDR ZeroChannel.JitterAdjustRx
                //CDR ZeroChannel.DeemphasisValueRx
            }
        }

        private ICommand _readFromFileCommand;
        public ICommand ReadFromFileCommand
        {
            get => _readFromFileCommand ?? (_readFromFileCommand = new RelayCommand(
                p => true,
                p => Task.Run(()=> LoadFromFile())
                ));
        }

        private async Task LoadFromFile()
        {
            //FileStream file = null;
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Файлы настройки QSFP|*.qet"
            };
            bool res = (bool)dialog.ShowDialog();
            if (!res)
            {
                MessageBox.Show("Не удалось прочитать данные из файла", "Неудача", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            //далее чтение файла
            using (BinaryReader file = new BinaryReader(File.OpenRead(dialog.FileName)))
            {
                try
                {
                    //RefreshTecParameters
                    TecOptimalTemperatureVoltage = file.ReadInt32();
                    await Task.Run(()=>WriteTecOptimalTemperatureVoltage(TecOptimalTemperatureVoltage));
                    //ZeroChannel.IsEnabled = file.ReadBoolean();
                    
                    ZeroChannel.Bias = file.ReadInt32();
                    await Task.Run(() => WriteBias(ZeroChannel.Bias, Channel.Zero));
                    ZeroChannel.Modulation = file.ReadInt32();
                    await Task.Run(() => WriteModulation(ZeroChannel.Modulation, Channel.Zero));
                    ZeroChannel.IsEqualizerEnabled = file.ReadBoolean();
                    await Task.Run(() => SwitchEqualizer(ZeroChannel.IsEqualizerEnabled, Channel.Zero));
                    ZeroChannel.EqualizerPhaseWithMagnitude = file.ReadInt32();
                    await Task.Run(() => WriteEqualizer(ZeroChannel.EqualizerPhaseWithMagnitude, Channel.Zero));
                    ZeroChannel.GeneralOptimization = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(ZeroChannel.GeneralOptimization, EyeOptimization.GeneralOptimization, Channel.Zero));
                    ZeroChannel.MinorTemperatureBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(ZeroChannel.MinorTemperatureBoost, EyeOptimization.MinorTemperatureBoost, Channel.Zero));
                    ZeroChannel.MajorTemperatureBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(ZeroChannel.MajorTemperatureBoost, EyeOptimization.MajorTemperatureBoost, Channel.Zero));
                    ZeroChannel.MinorModulationBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(ZeroChannel.MinorModulationBoost, EyeOptimization.MinorModulationBoost, Channel.Zero));
                    ZeroChannel.MajorModulationBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(ZeroChannel.MajorModulationBoost, EyeOptimization.MajorModulationBoost, Channel.Zero));
                    ZeroChannel.IsCrossingEnabled = file.ReadBoolean();
                    await Task.Run(() => SwitchCrossing(ZeroChannel.IsCrossingEnabled, Channel.Zero));
                    ZeroChannel.CrossingMagnitude = file.ReadInt32();
                    await Task.Run(() => WriteCrossing(ZeroChannel.CrossingMagnitude, Channel.Zero));

                    byte[] ch0 = new byte[16];
                    ch0[0] = (byte)ZeroChannel.CrossingMagnitude;
                    ch0[1] = (byte)(ZeroChannel.EqualizerPhaseWithMagnitude / 256);
                    ch0[2] = (byte)(ZeroChannel.EqualizerPhaseWithMagnitude % 256);
                    ch0[3] = (byte)(ZeroChannel.Modulation / 256);
                    ch0[4] = (byte)(ZeroChannel.Modulation % 256);
                    ch0[5] = (byte)(ZeroChannel.Bias / 256);
                    ch0[6] = (byte)(ZeroChannel.Bias % 256);
                    byte opt = (byte)(
                        (ZeroChannel.GeneralOptimization ? 1 : 0) +
                        (ZeroChannel.MinorModulationBoost ? 2 : 0) +
                        (ZeroChannel.MajorTemperatureBoost ? 4 : 0) +
                        (ZeroChannel.MinorModulationBoost ? 32:0) +
                        (ZeroChannel.MajorModulationBoost? 64 : 0)
                        );
                    ch0[7] = opt;

                    FirstChannel.Bias = file.ReadInt32();
                    await Task.Run(() => WriteBias(FirstChannel.Bias, Channel.First));
                    FirstChannel.Modulation = file.ReadInt32();
                    await Task.Run(() => WriteModulation(FirstChannel.Modulation, Channel.First));
                    FirstChannel.IsEqualizerEnabled = file.ReadBoolean();
                    await Task.Run(() => SwitchEqualizer(FirstChannel.IsEqualizerEnabled, Channel.First));
                    FirstChannel.EqualizerPhaseWithMagnitude = file.ReadInt32();
                    await Task.Run(() => WriteEqualizer(FirstChannel.EqualizerPhaseWithMagnitude, Channel.First));
                    FirstChannel.GeneralOptimization = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(FirstChannel.GeneralOptimization, EyeOptimization.GeneralOptimization, Channel.First));
                    FirstChannel.MinorTemperatureBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(FirstChannel.MinorTemperatureBoost, EyeOptimization.MinorTemperatureBoost, Channel.First));
                    FirstChannel.MajorTemperatureBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(FirstChannel.MajorTemperatureBoost, EyeOptimization.MajorTemperatureBoost, Channel.First));
                    FirstChannel.MinorModulationBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(FirstChannel.MinorModulationBoost, EyeOptimization.MinorModulationBoost, Channel.First));
                    FirstChannel.MajorModulationBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(FirstChannel.MajorModulationBoost, EyeOptimization.MajorModulationBoost, Channel.First));
                    FirstChannel.IsCrossingEnabled = file.ReadBoolean();
                    await Task.Run(() => SwitchCrossing(FirstChannel.IsCrossingEnabled, Channel.First));
                    FirstChannel.CrossingMagnitude = file.ReadInt32();
                    await Task.Run(() => WriteCrossing(FirstChannel.CrossingMagnitude, Channel.First));

                    byte[] ch1 = new byte[16];
                    ch1[0] = (byte)FirstChannel.CrossingMagnitude;
                    ch1[1] = (byte)(FirstChannel.EqualizerPhaseWithMagnitude / 256);
                    ch1[2] = (byte)(FirstChannel.EqualizerPhaseWithMagnitude % 256);
                    ch1[3] = (byte)(FirstChannel.Modulation / 256);
                    ch1[4] = (byte)(FirstChannel.Modulation % 256);
                    ch1[5] = (byte)(FirstChannel.Bias / 256);
                    ch1[6] = (byte)(FirstChannel.Bias % 256);
                    byte opt1 = (byte)(
                        (FirstChannel.GeneralOptimization ? 1 : 0) +
                        (FirstChannel.MinorModulationBoost ? 2 : 0) +
                        (FirstChannel.MajorTemperatureBoost ? 4 : 0) +
                        (FirstChannel.MinorModulationBoost ? 32 : 0) +
                        (FirstChannel.MajorModulationBoost ? 64 : 0)
                        );
                    ch1[7] = opt1;

                    SecondChannel.Bias = file.ReadInt32();
                    await Task.Run(() => WriteBias(SecondChannel.Bias, Channel.Second));
                    SecondChannel.Modulation = file.ReadInt32();
                    await Task.Run(() => WriteModulation(SecondChannel.Modulation, Channel.Second));
                    SecondChannel.IsEqualizerEnabled = file.ReadBoolean();
                    await Task.Run(() => SwitchEqualizer(SecondChannel.IsEqualizerEnabled, Channel.Second));
                    SecondChannel.EqualizerPhaseWithMagnitude = file.ReadInt32();
                    await Task.Run(() => WriteEqualizer(SecondChannel.EqualizerPhaseWithMagnitude, Channel.Second));
                    SecondChannel.GeneralOptimization = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(SecondChannel.GeneralOptimization, EyeOptimization.GeneralOptimization, Channel.Second));
                    SecondChannel.MinorTemperatureBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(SecondChannel.MinorTemperatureBoost, EyeOptimization.MinorTemperatureBoost, Channel.Second));
                    SecondChannel.MajorTemperatureBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(SecondChannel.MajorTemperatureBoost, EyeOptimization.MajorTemperatureBoost, Channel.Second));
                    SecondChannel.MinorModulationBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(SecondChannel.MinorModulationBoost, EyeOptimization.MinorModulationBoost, Channel.Second));
                    SecondChannel.MajorModulationBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(SecondChannel.MajorModulationBoost, EyeOptimization.MajorModulationBoost, Channel.Second));
                    SecondChannel.IsCrossingEnabled = file.ReadBoolean();
                    await Task.Run(() => SwitchCrossing(SecondChannel.IsCrossingEnabled, Channel.Second));
                    SecondChannel.CrossingMagnitude = file.ReadInt32();
                    await Task.Run(() => WriteCrossing(SecondChannel.CrossingMagnitude, Channel.Second));

                    byte[] ch2 = new byte[16];
                    ch2[0] = (byte)SecondChannel.CrossingMagnitude;
                    ch2[1] = (byte)(SecondChannel.EqualizerPhaseWithMagnitude / 256);
                    ch2[2] = (byte)(SecondChannel.EqualizerPhaseWithMagnitude % 256);
                    ch2[3] = (byte)(SecondChannel.Modulation / 256);
                    ch2[4] = (byte)(SecondChannel.Modulation % 256);
                    ch2[5] = (byte)(SecondChannel.Bias / 256);
                    ch2[6] = (byte)(SecondChannel.Bias % 256);
                    byte opt2 = (byte)(
                        (SecondChannel.GeneralOptimization ? 1 : 0) +
                        (SecondChannel.MinorModulationBoost ? 2 : 0) +
                        (SecondChannel.MajorTemperatureBoost ? 4 : 0) +
                        (SecondChannel.MinorModulationBoost ? 32 : 0) +
                        (SecondChannel.MajorModulationBoost ? 64 : 0)
                        );
                    ch2[7] = opt2;

                    ThirdChannel.Bias = file.ReadInt32();
                    await Task.Run(() => WriteBias(ThirdChannel.Bias, Channel.Third));
                    ThirdChannel.Modulation = file.ReadInt32();
                    await Task.Run(() => WriteModulation(ThirdChannel.Modulation, Channel.Third));
                    ThirdChannel.IsEqualizerEnabled = file.ReadBoolean();
                    await Task.Run(() => SwitchEqualizer(ThirdChannel.IsEqualizerEnabled, Channel.Third));
                    ThirdChannel.EqualizerPhaseWithMagnitude = file.ReadInt32();
                    await Task.Run(() => WriteEqualizer(ThirdChannel.EqualizerPhaseWithMagnitude, Channel.Third));
                    ThirdChannel.GeneralOptimization = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(ThirdChannel.GeneralOptimization, EyeOptimization.GeneralOptimization, Channel.Third));
                    ThirdChannel.MinorTemperatureBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(ThirdChannel.MinorTemperatureBoost, EyeOptimization.MinorTemperatureBoost, Channel.Third));
                    ThirdChannel.MajorTemperatureBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(ThirdChannel.MajorTemperatureBoost, EyeOptimization.MajorTemperatureBoost, Channel.Third));
                    ThirdChannel.MinorModulationBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(ThirdChannel.MinorModulationBoost, EyeOptimization.MinorModulationBoost, Channel.Third));
                    ThirdChannel.MajorModulationBoost = file.ReadBoolean();
                    await Task.Run(() => SwitchEyeOptimization(ThirdChannel.MajorModulationBoost, EyeOptimization.MajorModulationBoost, Channel.Third));
                    ThirdChannel.IsCrossingEnabled = file.ReadBoolean();
                    await Task.Run(() => SwitchCrossing(ThirdChannel.IsCrossingEnabled, Channel.Third));
                    ThirdChannel.CrossingMagnitude = file.ReadInt32();
                    await Task.Run(() => WriteCrossing(ThirdChannel.CrossingMagnitude, Channel.Third));

                    byte[] ch3 = new byte[16];
                    ch3[0] = (byte)ThirdChannel.CrossingMagnitude;
                    ch3[1] = (byte)(ThirdChannel.EqualizerPhaseWithMagnitude / 256);
                    ch3[2] = (byte)(ThirdChannel.EqualizerPhaseWithMagnitude % 256);
                    ch3[3] = (byte)(ThirdChannel.Modulation / 256);
                    ch3[4] = (byte)(ThirdChannel.Modulation % 256);
                    ch3[5] = (byte)(ThirdChannel.Bias / 256);
                    ch3[6] = (byte)(ThirdChannel.Bias % 256);
                    byte opt3 = (byte)(
                        (ThirdChannel.GeneralOptimization ? 1 : 0) +
                        (ThirdChannel.MinorTemperatureBoost ? 2 : 0) +
                        (ThirdChannel.MajorTemperatureBoost ? 4 : 0) +
                        (ThirdChannel.MinorModulationBoost ? 32 : 0) +
                        (ThirdChannel.MajorModulationBoost ? 64 : 0)
                        );
                    ch3[7] = opt1;

                    List<byte> data = new List<byte>();
                    data.AddRange(ch0);
                    data.AddRange(ch1);
                    data.AddRange(ch2);
                    data.AddRange(ch3);
                }
                catch (Exception /*ex*/)
                {
                    MessageBox.Show("Ошибка при загрузке настроек из файла", "Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
            MessageBox.Show("Данные загружены из файла","Информация",MessageBoxButton.OK,MessageBoxImage.Information);
        }
        #endregion

        private double _sigma = Properties.Settings.Default.sigma;//Для настроек
        public double Sigma
        {
            get => _sigma;
            set => Set(ref _sigma, value);
        }

        private byte[] _uid = new byte[8];
        public byte[] UID
        {
            get =>_uid;
            set => Set(ref _uid, value);
            
        }

        private bool _isUidNotNull = false;
        public bool IsUidNotNull
        {
            get => _isUidNotNull;
            set => Set(ref _isUidNotNull, value);
        }

        private bool _is4C = true;
        public bool Is4C
        {
            get => _is4C;
            set => Set(ref _is4C, value);
        }

        private int _selectedOscyll = 1;
        public int SelectedOscyll
        {
            get => _selectedOscyll;
            set
            {
                Set(ref _selectedOscyll, value);
                if (value == 0)
                {
                    Is4C = false;
                }
                else
                {
                    Is4C = true;
                }
            }
        }

        private ICommand _chSelectCommand;
        public ICommand ChSelectCommand
        {
            get => _chSelectCommand ?? (_chSelectCommand = new RelayCommand(
                p => true,
                p =>
                {
                    var channel = (string)p;
                    SelectChannel(channel);
                    logger.Trace($"Заполняем канал {channel}");

                }));
        }

        void SelectChannel(string ch)
        {
            if (IsBertwaveConnected)
            {
                _model.RefreshSingleChannel(ch, ref _model.BertWaveParameters);
                PutDataToParameters(_model.BertWaveParameters);
            }
        }

        private bool _isAutoTuneTecCancelled = false;
        public bool IsAutoTuneTecCancelled
        {
            get => _isAutoTuneTecCancelled;
            set => Set(ref _isAutoTuneTecCancelled, value);
        }

        private ICommand _cancelAutoTuningCommand;
        public ICommand CancelAutoTuningCommand
        {
            get => _cancelAutoTuningCommand ?? (_cancelAutoTuningCommand = new RelayCommand(
                p => true,
                p =>{
                    IsAutoTuneTecCancelled = true;
                    Trace.WriteLine("Отмена автонастройки вручную");
                    logger.Trace("Отмена автонастройки вручную");
                }));
        }

        private bool _isModuleTuneEnded = true;
        public bool IsModuleTuneEnded
        {
            get => _isModuleTuneEnded;
            set => Set(ref _isModuleTuneEnded, value);
        }

        private ICommand _completeTuneCommand;
        public ICommand CompleteTuneCommand
        {
            get => _completeTuneCommand ?? (_completeTuneCommand = new RelayCommand(
                p => !IsOperationInProgress,
                async p=> await CompleteTune()
                ));
        }

        //Скриншот как массив байт
        public byte[] EyeA { get; set; }
        public byte[] EyeB { get; set; }
        public byte[] EyeC { get; set; }
        public byte[] EyeD { get; set; }



        public string MaskRecall { get; set; }
        public async Task CompleteTune()
        {
            //DateTime start = DateTime.Now;
            IsOperationInProgress = true;
            IsIndefinitelyLongOperation = true;
            bool osaConnected = false;
            //OperationProgressPercent = 0;
            var tasks = new List<Task>();
            await Task.Run(() =>
            {
                if (!IsBertwaveConnected)
                {
                    tasks.Add(Task.Factory.StartNew(() => 
                    {
                        try
                        {
                            ConnectToBertwave(BertwaveIpAddress);
                        }
                        catch (Exception) 
                        {
                            Trace.WriteLine("неудача с осциллографом");
                            logger.Trace("Не удалось подключиться к осциллографу при завершении");
                        }
                    }));
                    Task.WaitAll(tasks.ToArray());
                }
                tasks.Add(Task.Factory.StartNew(()=>PutDataToParameters(_model.BertWaveParameters)));
                tasks.Add(Task.Factory.StartNew(() => 
                {
                    _model.Bertwave.DisplayAutoScale();
                    string tempStr = _model.Bertwave.GetMaskRecall();
                    MaskRecall = tempStr.Substring(1, tempStr.IndexOf('.') - 1);
                foreach (BertwaveChannels ch in Enum.GetValues(typeof(BertwaveChannels)))
                {
                    byte[] chImage = _model.GetBertwaveScreenshot(ch);

                    switch(ch)
                    {
                        case BertwaveChannels.A:
                            EyeA = CompressImage(chImage);
                            break;
                        case BertwaveChannels.B:
                            EyeB = CompressImage(chImage);
                            break;
                        case BertwaveChannels.C:
                            EyeC = CompressImage(chImage);
                            break;
                        case BertwaveChannels.D:
                            EyeD = CompressImage(chImage);
                            break;
                    }
                }

                }));
                Task.WaitAll(tasks.ToArray());
                //OperationProgressPercent = 20;
                if (!IsOsaConnected)
                {
                    try
                    {
                        tasks.Add(Task.Factory.StartNew(() => { osaConnected = ConnectToOsa1(OsaIpAddress); }));
                        //while (tasks[2].Status != TaskStatus.RanToCompletion) { }
                    }
                    catch(Exception)
                    {
                        logger.Trace("Не удалось подключиться к спектрографу при завершении");
                    }
                }
                //else
                //{
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        //Thread.Sleep(7500);
                        try
                        {
                            RefreshOsaData();
                        }
                        catch
                        {
                            logger.Trace("Не удалось получить данные со спектрографа при завершении");
                        }
                    }));
                //}
                
                //OperationProgressPercent = 40;

                Task.WaitAll(tasks.ToArray());
                StoreToDB();
                //Trace.WriteLine($"Статус: {tasks[0].Status},{tasks[1].Status},{tasks[2].Status}");
                //Trace.WriteLine($"Прочитали данные с OSA {Lambda1PeakValue},{Lambda2PeakValue},{Lambda3PeakValue},{Lambda4PeakValue}");
                if (IsBertwaveConnected)
                tasks.Add(Task.Factory.StartNew(() => 
                {
                    DisconnectFromBertwave();
                }));
                if (IsOsaConnected)
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        DisconnectFromOsa();
                    }));
                OperationProgressPercent = 100;
                Task.WaitAll(tasks.ToArray());
                IsOperationInProgress = false;
                IsModuleTuneEnded = true;
                IsIndefinitelyLongOperation = false;
                //DateTime finish = DateTime.Now;
                //TimeSpan time = finish - start;
                MessageBox.Show("Завершена подготовка значений параметров для записи в базу данных.Теперь модуль можно заменить");
            });
        }

        public bool ConnectToOsa1(string ipAddress)
        {
            OsaConnectionProgress = true;
            bool retValue = false;
            _model.ConnectToOsa(ipAddress);
            if (IsOsaConnected)
            {
                IsTecSliderEnabled = true;
                Thread.Sleep(200);//7500
                RefreshOsaData();
                retValue = true;
            }
            OsaConnectionProgress = false;
            return retValue;
        }

        private byte[] CompressImage(byte[] imgData)
        {
            Bitmap bmp;
            using(MemoryStream ms = new MemoryStream(imgData))
            {
                bmp = new Bitmap(ms);
            }
            //var r = new Rectangle(0, 0, 636, 608);
            //Bitmap nb = bmp.Clone(r, System.Drawing.Imaging.PixelFormat.DontCare);
            //bmp.Save("Image.bmp");

            using(MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageCodecInfo.GetImageEncoders()[1], new EncoderParameters()
                {
                    Param = new EncoderParameter[]
                    {
                        new EncoderParameter(Encoder.Quality,100L-40)
                    }
                });
                return ms.ToArray();
            }
        }

        private ICommand _ddmValueShowCommand;
        public ICommand DdmValueShowCommand
        {
            get => _ddmValueShowCommand ?? (_ddmValueShowCommand = new RelayCommand(
                p=>true,
                p =>
                {
                    DdmWindow wnd = new DdmWindow();
                    wnd.ShowDialog();
                }));
        }
    }
}
