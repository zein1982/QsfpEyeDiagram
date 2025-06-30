using QsfpEyeDiagram.Models.EventArguments;
using Std.Data.Database.Domain;
using Std.Equipment.Anritsu.Bertwave;
using Std.Equipment.Anritsu.Osa;
using Std.Equipment.FiberTrade.UniversalTestBoard;
using Std.Equipment.FiberTrade.UniversalTestBoard.Tables;
using Std.Modules.ConfigurationParameters.Qsfp;
using Std.Modules.ConfigurationParameters.Qsfp.Tec;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QsfpEyeDiagram.Models
{
    public class QsfpEyeDiagramModel
    {
        public event EventHandler<UtbStatusEventArgs> UtbStatusChanged;
        public event EventHandler<ModuleStatusEventArgs> ModuleStatusChanged;
        public event EventHandler<BertwaveStatusEventArgs> BertwaveStatusChanged;
        public event EventHandler<OsaStatusEventArgs> OsaStatusChanged;

        public event EventHandler<EyeDiagramParametersEventArgs> EyeDiagramParametersChanged;
        public event EventHandler<TecParametersEventArgs> TecParametersChanged;
        public event EventHandler<BertwaveEyeDiagramParametersEventArgs> BertwaveEyeDiagramParametersChanged;

        public event EventHandler<OperationProgressStatusEventArgs> OperationProgressStatusChanged;
        public event EventHandler<OperationSuccessEventArgs> OperationSuccessChanged;
        public event EventHandler<OperationTypeEventArgs> OperationTypeChanged;

        public WorkerRecord Worker { get; set; }

        private bool IsOperationInProgress
        {
            set
            {
                OnOperationProgressStatusChanged(new OperationProgressStatusEventArgs(value));
            }
        }

        private bool OperationSuccess
        {
            set
            {
                OnOperationSuccessChanged(new OperationSuccessEventArgs(value));
            }
        }

        private bool _isIndefinitelyLongOperation;
        private bool IsIndefinitelyLongOperation
        {
            get => _isIndefinitelyLongOperation;
            set
            {
                if (_isIndefinitelyLongOperation != value)
                {
                    _isIndefinitelyLongOperation = value;
                    OnOperationTypeChanged(new OperationTypeEventArgs(_isIndefinitelyLongOperation));
                }
            }
        }

        private bool _utbMonitoringInProgress;

        private bool _isModuleConnected;
        private bool IsModuleConnected
        {
            get { return _isModuleConnected; }
            set
            {
                if (_isModuleConnected != value)
                {
                    _isModuleConnected = value;
                    OnModuleStatusChanged(new ModuleStatusEventArgs(_isModuleConnected));
                }
            }
        }

        private bool _isUtbConnected;
        public bool IsUtbConnected
        {
            get { return _isUtbConnected; }
            private set
            {
                if (_isUtbConnected != value)
                {
                    _isUtbConnected = value;
                    OnUtbStatusChanged(new UtbStatusEventArgs(_isUtbConnected));
                }
            }
        }

        private bool _isBertwaveConnected;
        public bool IsBertwaveConnected
        {
            get { return _isBertwaveConnected; }
            set
            {
                if (_isBertwaveConnected != value)
                {
                    _isBertwaveConnected = value;
                    OnBertwaveStatusChanged(new BertwaveStatusEventArgs(_isBertwaveConnected));
                }
            }
        }

        private bool _isOsaConnected;
        public bool IsOsaConnected
        {
            get => _isOsaConnected;
            set
            {
                if (_isOsaConnected != value)
                {
                    _isOsaConnected = value;
                    OnOsaStatusChanged(new OsaStatusEventArgs(_isOsaConnected));
                }
            }
        }

        private bool _isModuleDataChanged;

        public byte[] _uniqueId;

        //Параметры модуля
        public int ModuleCurrent { get; set; }
        public int ModuleVoltage { get; set; }
        public bool ModuleInterrupt { get; set; }
        public bool ModuleAbsent { get; set; }
        public bool ModuleDeSel { get; set; }
        public bool ModuleResetL { get; set; }
        public bool ModuleLPMode { get; set; }

        private EyeDiagramParameters _eyeDiagramParameters = EyeDiagramParameters.Unassigned;
        private EyeDiagramParameters EyeDiagramParameters
        {
            get { return _eyeDiagramParameters; }
            set
            {
                _eyeDiagramParameters = value;
                OnEyeDiagramParametersChanged(new EyeDiagramParametersEventArgs(_eyeDiagramParameters));
            }
        }

        public bool RefreshTecParameters;

        private TecParameters _tecParameters = TecParameters.Unassigned;
        public TecParameters TecParameters
        {
            get => _tecParameters;
            set
            {
                _tecParameters = value;
                OnTecParametersChanged(new TecParametersEventArgs(_tecParameters));
            }
        }

        public UniversalTestBoard Utb { get; private set; }

        private string _bertwaveIpAddress;
        public BertwaveMP2100A Bertwave { get; private set; }

        private string _osaIpAddress;
        public OsaMS9740 Osa { get; private set; }

        public Dictionary<string, double> BertWaveParameters = new Dictionary<string, double>();
        public QsfpEyeDiagramModel(bool IsUseAsApi = false)
        {
            //TryInitializeQsfpEyeService();

            InitializeUtb();
            InitializeBertwave();
            InitializeOsa();
            if (!IsUseAsApi)
            {
                InitializeMonitoringTasks();
            }
        }

        private void InitializeUtb()
        {
            Utb = new UniversalTestBoard();
        }

        private void InitializeBertwave()
        {
            Bertwave = new BertwaveMP2100A();
        }

        private void InitializeOsa()
        {
            Osa = new OsaMS9740();
        }

        public void InitializeMonitoringTasks()
        {
            _refreshUtbStatus = true;
            //RefreshTecParameters = false;
            //_refreshBertwaveStatus = true;
            //_refreshOsaStatus = true;

            _utbMonitoringCancelSource = new CancellationTokenSource();
            //_bertwaveMonitoringCancelSource = new CancellationTokenSource();
            //_osaMonitoringCancelSource = new CancellationTokenSource();

            UtbMonitoringTask(_utbMonitoringCancelSource.Token);
            //BertwaveMonitoringTask(_bertwaveMonitoringCancelSource.Token);
            //OsaMonitoringTask(_osaMonitoringCancelSource.Token);
        }

        public void StopMonitoringTasks()
        {
            _utbMonitoringCancelSource.Cancel();
            //Std.Shared.Timing.Pause(1000);
            //_bertwaveMonitoringCancelSource.Cancel();
            //_osaMonitoringCancelSource.Cancel();
        }

        private bool _refreshUtbStatus;
        public bool RefreshStatusUtb
        {
            get => _refreshUtbStatus;
            set => _refreshUtbStatus = value;
        }

        private CancellationTokenSource _utbMonitoringCancelSource;
        private Task UtbMonitoringTask(CancellationToken cancellationToken)
        {

            
            return Task.Run(() =>
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (_refreshUtbStatus)
                    {
                        RefreshUtbStatus();
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    Thread.Sleep(200);
                }
        });
        }

        private bool RefreshModuleValues()
        {
            RefreshUtbParams();
            // Проверка подключения модуля.
            var isModuleConnectedNow = Utb.IsModuleConnected();

            if (isModuleConnectedNow)
            {
                // Если модуль ещё не был ранее подключен, с него читаются уникальный номер и параметры глазковой диаграммы.
                if (!IsModuleConnected)
                {
                    IsOperationInProgress = true;
                    IsIndefinitelyLongOperation = true;

                    // Ожидание включения модуля.
                    //Thread.Sleep(200);
                    Std.Shared.Timing.Pause(150);
                    // Чтение уникального идентификатора.
                    var uniqueIdReadSuccess = Utb.GetUniqueId(out _uniqueId, true, true);

                    // Чтение параметров глазковой диаграммы, если предыдущая операция прошла успешно.
                    var eyeDiagramReadSuccess = false;
                    if (uniqueIdReadSuccess)
                    {
                        // Чтение параметров глазковой диаграммы.
                        eyeDiagramReadSuccess = Utb.GetEyeDiagramParameters(out var eyeDiagramParameters,
                            usePageSelection: true, usePassword: true);
                        if (eyeDiagramReadSuccess)
                        {
                            EyeDiagramParameters = eyeDiagramParameters;
                        }
                    }

                    // Чтение параметров TEC, если предыдущая операция прошла успешно.
                    var tecParametersReadSuccess = false;
                    if (eyeDiagramReadSuccess)
                    {
                        // Чтение параметров TEC.
                        tecParametersReadSuccess = Utb.GetTecParameters(out var tecParameters);
                        if (tecParametersReadSuccess)
                        {
                            OnTecParametersChanged(new TecParametersEventArgs(tecParameters, isInitialParameters: true));
                            TecParameters = tecParameters;
                        }
                    }

                    if (uniqueIdReadSuccess && eyeDiagramReadSuccess && tecParametersReadSuccess)
                    {
                        OperationSuccess = true;
                    }
                    else
                    {
                        OperationSuccess = false;
                    }

                    IsOperationInProgress = false;
                    IsIndefinitelyLongOperation = false;
                }
                else
                {
                    // Иначе с модуля считываются параметры TEC.
                    if (RefreshTecParameters)
                    {
                        if (Utb.GetTecParameters(out var tecParameters,
                            usePassword: true, usePageSelection: true))
                        {
                            TecParameters = tecParameters;
                        }
                        Utb.GetDdmTemperature(out _ddmTemperature);
                    }
                }
            }

            
            IsModuleConnected = isModuleConnectedNow;
            return IsModuleConnected;
        }

        public void RefreshUtbStatus()
        {
            _utbMonitoringInProgress = true;

            IsUtbConnected = Utb.IsConnected();
            if (IsUtbConnected)
            {
                var isModuleConnected = RefreshModuleValues();
                // Если модуль не подключен и его данные были изменены, просходит запись собранной информации в базу данных.
                if (!isModuleConnected && _isModuleDataChanged)
                {
                    //WriteLastReceivedParametersToDatabase();
                    _isModuleDataChanged = false;
                }
            }
            else
            {
                IsModuleConnected = false;
            }
            _utbMonitoringInProgress = false;
        }

        public OperationResult ConnectToUtb(string comPortName)
        {
            if (IsUtbConnected)
            {
                
                return new OperationResult(OperationStatuses.Success, "Плата уже подключена.");
            }

            var comPortNumberParseResult = ComPortStringHelper.GetComPortNumber(comPortName, out var comPortNumber);
            if (!comPortNumberParseResult)
            {
                return new OperationResult(OperationStatuses.Failure, "Указано неверное имя COM порта.");
                
            }

            if (Utb.Connect(comPortNumber))
            {
                IsUtbConnected = true;
                return OperationResult.Success;
            }

            return new OperationResult(OperationStatuses.Failure, "Не удалось подключиться к плате.");
        }

        public OperationResult DisconnectFromUtb()
        {
            if (!IsUtbConnected)
            {
                return new OperationResult(OperationStatuses.Success, "Плата уже отключена.");
            }

            if (Utb.Disconnect())
            {
                IsUtbConnected = false;
                return OperationResult.Success;
            }

            return new OperationResult(OperationStatuses.Failure, "Не удалось отключиться от платы.");
        }

        public bool ConnectToBertwave(string ipAddress)
        {
            if (Bertwave.Connect(ipAddress))
            {
                _bertwaveIpAddress = ipAddress;
                IsBertwaveConnected = true;
                if(_is4chOscyll)
                    BertWaveParameters = RefreshBertData();

                return true;
            }

            return false;
        }

        public Dictionary<string, double> RefreshBertData()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            double dValue;
            //int ivalue;
            bool isCorrect = true;
            //Bertwave.DisplayAutoScale();
            foreach (BertwaveChannels ch in Enum.GetValues(typeof(BertwaveChannels)))
            {
                isCorrect &= Bertwave.GetBertWaveAvePower(ch, out dValue);
                string keyName = "AvePower" + ch.ToString();
                result.Add(keyName, dValue);

                isCorrect &= Bertwave.GetBertWaveER(ch, out dValue);
                keyName = "ExtRatio" + ch.ToString();
                result.Add(keyName, dValue);

                isCorrect &= Bertwave.GetBertWaveSNR(ch, out dValue);
                keyName = "Snr" + ch.ToString();
                result.Add(keyName, dValue);

                isCorrect &= Bertwave.GetBertWaveCrossing(ch, out dValue);
                keyName = "Cross" + ch.ToString();
                result.Add(keyName, dValue);

                isCorrect &= Bertwave.GetBertWaveJitPP(ch, out dValue);
                keyName = "JittPp" + ch.ToString();
                result.Add(keyName, dValue/*/38.7897*/);

                isCorrect &= Bertwave.GetBertWaveJ2(ch, out dValue);
                keyName = "J2" + ch.ToString();
                result.Add(keyName, dValue/*/38.7897*/);

                isCorrect &= Bertwave.GetBertWaveJ9(ch, out dValue);
                keyName = "J9" + ch.ToString();
                result.Add(keyName, dValue /*/ 38.7897*/);

                isCorrect &= Bertwave.GetBertWaveMM(ch, out dValue);
                keyName = "MaskMargin" + ch.ToString();
                result.Add(keyName, dValue);

                isCorrect &= Bertwave.GetBertWaveBer(ch, out dValue);
                keyName = "Ber" + ch.ToString();
                result.Add(keyName, dValue);
            }
            return result;
        }

        public string GetMaskRecall()
        {
            return Bertwave.GetMaskRecall();
        }
        public void RefreshSingleChannel(string channel, ref Dictionary<string,double> result)
        {
            double dvalue;
            bool isCorrect = Bertwave.GetAvePower(out dvalue);
            string keyName = "AvePower" + channel;
            result.Remove(keyName);//На всякий случай, если в словаре есть значение по этому ключу
            result.Add(keyName, dvalue);

            isCorrect &= Bertwave.GetEr(out dvalue);
            keyName = "ExtRatio" + channel;
            result.Remove(keyName);
            result.Add(keyName, dvalue);

            isCorrect &= Bertwave.GetSnr(out dvalue);
            keyName = "Snr" + channel;
            result.Remove(keyName);
            result.Add(keyName, dvalue);

            isCorrect &= Bertwave.GetCrossing(out dvalue);
            keyName = "Cross" + channel;
            result.Remove(keyName);
            result.Add(keyName, dvalue);

            isCorrect &= Bertwave.GetJitPp(out dvalue);
            keyName = "JittPp" + channel;
            result.Remove(keyName);
            result.Add(keyName, dvalue);

            isCorrect &= Bertwave.GetJ2(out dvalue);
            keyName = "J2" + channel;
            result.Remove(keyName);
            result.Add(keyName, dvalue / 38.7897);

            isCorrect &= Bertwave.GetJ9(out dvalue);
            keyName = "J9" + channel;
            result.Remove(keyName);
            result.Add(keyName, dvalue / 38.7897);

            isCorrect &= Bertwave.GetMm(out dvalue);
            keyName = "MaskMargin" + channel;
            result.Remove(keyName);
            result.Add(keyName, dvalue);

            isCorrect &= Bertwave.GetBer(out dvalue);
            keyName = "Ber" + channel;
            result.Remove(keyName);
            result.Add(keyName, dvalue);
        }

        public bool DisconnectFromBertwave()
        {
            if (Bertwave.StopSampling() && Bertwave.ReleaseRemoteConnectionStatus() && Bertwave.Disconnect())
            {
                IsBertwaveConnected = false;
                return true;
            }

            return false;
        }

        public bool ConnectToOsa(string ipAddress)
        {

            if (Osa.Connect(ipAddress))
            {
                _osaIpAddress = ipAddress;
                IsOsaConnected = true;


                return true;
            }
            return false;

        }

        //Semaphore semModel = new Semaphore(1, 1);
        public string[] RefreshOsaData()
        {
            
            return Osa.GetPeaks(4);
        }
        public bool DisconnectFromOsa()
        {
            if (Osa.StopSweeping() && Osa.Disconnect())
            {
                IsOsaConnected = false;
                return true;
            }
            return false;
        }

        public bool ReadEyeDiagramParameters(out EyeDiagramParameters parameters)
        {
            IsIndefinitelyLongOperation = true;
            IsOperationInProgress = true;
            _refreshUtbStatus = false;

            if (!WaitForUtbStatusRefresh())
            {
                IsOperationInProgress = false;
                _refreshUtbStatus = true;
            }
            
            var readSuccess = Utb.GetEyeDiagramParameters(out parameters, usePageSelection: true, usePassword: true);

            IsIndefinitelyLongOperation = true;
            IsOperationInProgress = false;
            _refreshUtbStatus = true;

            return readSuccess;
        }

        public bool SwitchChannel(bool isEnabled, Channel channel, IProgress<int> progress = null)
        {
            int statusByte = Utb.GetChannelStatuses();
            //MessageBox.Show($"статус до изменения {statusByte.ToString("X4")}");
            switch (channel)
            {
                case Channel.Zero:
                    statusByte = isEnabled ? statusByte | 0b00000001 : statusByte & 0b11111110;
                    break;
                case Channel.First:
                    statusByte = isEnabled ? statusByte | 0b00000010 : statusByte & 0b11111101;
                    break;
                case Channel.Second:
                    statusByte = isEnabled ? statusByte | 0b00000100 : statusByte & 0b11111011;
                    break;
                case Channel.Third:
                    statusByte = isEnabled ? statusByte | 0b00001000 : statusByte & 0b11110111;
                    break;
            }
            //var statusByteValue = isEnabled ? 0x00 : 0x01;
            //if (WriteValueToModule(Utb.SetChannelStatus, statusByteValue, channel, progress))
            if (WriteValueToModule(Utb.SetChannelsStatuses, statusByte, /*channel,*/ progress))
            {
                EyeDiagramParameters[channel].IsEnabled = isEnabled;
                //statusByte = Utb.GetChannelStatuses();
                //MessageBox.Show($"статус после изменения {statusByte.ToString("X4")}");

                return true;
            }
            
            return false;
        }

        public bool WriteBias(int bias, Channel channel, IProgress<int> progress = null)
        {
            if (WriteValueToModule(Utb.WriteBias, bias, channel, progress))
            {
                EyeDiagramParameters[channel].Bias = bias;
                return true;
            }

            return false;
        }

        public bool WriteModulation(int modulation, Channel channel, IProgress<int> progress = null)
        {
            if (WriteValueToModule(Utb.WriteModulation, modulation, channel, progress))
            {
                EyeDiagramParameters[channel].Modulation = modulation;
                return true;
            }

            return false;
        }

        public bool SwitchEqualizer(bool isEnabled, Channel channel, IProgress<int> progress = null)
        {
            if (WriteValueToModule(Utb.WriteVariableEqualizer,
                ComposeEqualizerConfigByte(isEnabled, EyeDiagramParameters[channel].VariableEqualizer.PhaseSelection, EyeDiagramParameters[channel].VariableEqualizer.Magnitude),
                channel, progress))
            {
                EyeDiagramParameters[channel].VariableEqualizer.IsEnabled = isEnabled;
                return true;
            }

            return false;
        }

        public bool WriteEqualizer(int equalizer, Channel channel, IProgress<int> progress = null)
        {
            GetPhaseAndMagnitudeFromEqualizerValue(equalizer, out var phaseSelection, out var magnitude);

            if (WriteValueToModule(Utb.WriteVariableEqualizer,
                ComposeEqualizerConfigByte(EyeDiagramParameters[channel].VariableEqualizer.IsEnabled, phaseSelection, magnitude),
                channel, progress))
            {
                EyeDiagramParameters[channel].VariableEqualizer.PhaseSelection = phaseSelection;
                EyeDiagramParameters[channel].VariableEqualizer.Magnitude = magnitude;

                return true;
            }

            return false;
        }

        public bool SwitchEyeOptimization(bool isEnabled, EyeOptimization optimizationType, Channel channel, IProgress<int> progress = null)
        {
            int eyeOptimization;

            // Формирование байта настройки оптимизации глаза для отправки в модуль.
            switch (optimizationType)
            {
                case EyeOptimization.GeneralOptimization:
                    {
                        eyeOptimization = ComposeEyeOptimizationConfigByte(generalOptimization: isEnabled,
                            minorTemperatureBoost: EyeDiagramParameters[channel].EyeOptimization.MinorTemperatureBoost,
                            majorTemperatureBoost: EyeDiagramParameters[channel].EyeOptimization.MajorTemperatureBoost,
                            minorModulationBoost: EyeDiagramParameters[channel].EyeOptimization.MinorModulationBoost,
                            majorModulationBoost: EyeDiagramParameters[channel].EyeOptimization.MajorModulationBoost);
                        break;
                    }

                case EyeOptimization.MinorTemperatureBoost:
                    {
                        eyeOptimization = ComposeEyeOptimizationConfigByte(generalOptimization: EyeDiagramParameters[channel].EyeOptimization.GeneralOptimization,
                            minorTemperatureBoost: isEnabled,
                            majorTemperatureBoost: EyeDiagramParameters[channel].EyeOptimization.MajorTemperatureBoost,
                            minorModulationBoost: EyeDiagramParameters[channel].EyeOptimization.MinorModulationBoost,
                            majorModulationBoost: EyeDiagramParameters[channel].EyeOptimization.MajorModulationBoost);
                        break;
                    }

                case EyeOptimization.MajorTemperatureBoost:
                    {
                        eyeOptimization = ComposeEyeOptimizationConfigByte(generalOptimization: EyeDiagramParameters[channel].EyeOptimization.GeneralOptimization,
                            minorTemperatureBoost: EyeDiagramParameters[channel].EyeOptimization.MinorTemperatureBoost,
                            majorTemperatureBoost: isEnabled,
                            minorModulationBoost: EyeDiagramParameters[channel].EyeOptimization.MinorModulationBoost,
                            majorModulationBoost: EyeDiagramParameters[channel].EyeOptimization.MajorModulationBoost);
                        break;
                    }

                case EyeOptimization.MinorModulationBoost:
                    {
                        eyeOptimization = ComposeEyeOptimizationConfigByte(generalOptimization: EyeDiagramParameters[channel].EyeOptimization.GeneralOptimization,
                            minorTemperatureBoost: EyeDiagramParameters[channel].EyeOptimization.MinorTemperatureBoost,
                            majorTemperatureBoost: EyeDiagramParameters[channel].EyeOptimization.MajorTemperatureBoost,
                            minorModulationBoost: isEnabled,
                            majorModulationBoost: EyeDiagramParameters[channel].EyeOptimization.MajorModulationBoost);
                        break;
                    }

                case EyeOptimization.MajorModulationBoost:
                    {
                        eyeOptimization = ComposeEyeOptimizationConfigByte(generalOptimization: EyeDiagramParameters[channel].EyeOptimization.GeneralOptimization,
                            minorTemperatureBoost: EyeDiagramParameters[channel].EyeOptimization.MinorTemperatureBoost,
                            majorTemperatureBoost: EyeDiagramParameters[channel].EyeOptimization.MajorTemperatureBoost,
                            minorModulationBoost: EyeDiagramParameters[channel].EyeOptimization.MinorModulationBoost,
                            majorModulationBoost: isEnabled);
                        break;
                    }

                default:
                    throw new ArgumentException("Неизвестный тип оптимизации.", nameof(optimizationType));
            }

            if (WriteValueToModule(Utb.WriteEyeOptimization, eyeOptimization, channel, progress))
            {
                EyeDiagramParameters[channel].EyeOptimization[optimizationType] = isEnabled;

                return true;
            }

            return false;
        }

        public bool SwitchCrossing(bool isEnabled, Channel channel, IProgress<int> progress = null)
        {
            if (WriteValueToModule(Utb.WriteCrossing, ComposeCrossingConfigByte(isEnabled, EyeDiagramParameters[channel].Crossing.Magnitude),
                channel, progress))
            {
                EyeDiagramParameters[channel].Crossing.IsEnabled = isEnabled;
                return true;
            }

            return false;
        }

        public bool WriteCrossing(int magnitude, Channel channel, IProgress<int> progress = null)
        {
            if (WriteValueToModule(Utb.WriteCrossing, ComposeCrossingConfigByte(EyeDiagramParameters[channel].Crossing.IsEnabled, magnitude),
                channel, progress))
            {
                EyeDiagramParameters[channel].Crossing.Magnitude = magnitude;
                return true;
            }

            return false;
        }

        public bool WriteTecOptimalTemperatureVoltage(int voltage, IProgress<int> progress = null)
        {
            if (WriteValueToModule(Utb.WriteTecOptimalTemperatureVoltage, voltage, progress))
            {
                TecParameters.OptimalTemperatureVoltage = voltage;
                return true;
            }

            return false;
        }

        private bool WriteValueToModule(Func<int, IProgress<int>, bool> writeDelegate, int value, IProgress<int> progress = null)
        {
            IsOperationInProgress = true;
            _refreshUtbStatus = false;

            if (!WaitForUtbStatusRefresh())
            {
                IsOperationInProgress = false;
                _refreshUtbStatus = true;
                return false;
            }

            if (!writeDelegate.Invoke(value, progress))
            {
                IsOperationInProgress = false;
                _refreshUtbStatus = true;
                return false;
            }

            _isModuleDataChanged = true;
            IsOperationInProgress = false;
            _refreshUtbStatus = true;
            return true;
        }

        private bool WriteValueToModule(Func<int, Channel, IProgress<int>, bool> writeDelegate, int value, Channel channel, IProgress<int> progress = null)
        {
            IsOperationInProgress = true;
            _refreshUtbStatus = false;

            if (!WaitForUtbStatusRefresh())
            {
                IsOperationInProgress = false;
                _refreshUtbStatus = true;
                return false;
            }

            if (!writeDelegate.Invoke(value, channel, progress))
            {
                IsOperationInProgress = false;
                _refreshUtbStatus = true;
                return false;
            }

            _isModuleDataChanged = true;
            IsOperationInProgress = false;
            _refreshUtbStatus = true;
            return true;
        }

        private bool WaitForUtbStatusRefresh()
        {
            const int maxWaitTime = 1000;
            const int waitStep = 20;

            var waitTime = 0;
            while (waitTime < maxWaitTime)
            {
                if (!_utbMonitoringInProgress)
                {
                    return true;
                }

                waitTime += waitStep;
                Thread.Sleep(waitStep);
            }

            return false;
        }

        #region Конвертеры
        private byte ComposeEqualizerConfigByte(bool isEnabled, bool phaseSelection, int magnitude)
        {
            if (magnitude < 0)
            {
                magnitude = 0;
            }
            else if (magnitude > 31)
            {
                magnitude = 31;
            }

            var configByte = (byte)((magnitude & 0b0001_1111) << 3);

            if (isEnabled)
            {
                configByte |= 0b0000_0001;
            }

            if (phaseSelection)
            {
                configByte |= 0b0000_0010;
            }

            return configByte;
        }

        private void GetPhaseAndMagnitudeFromEqualizerValue(int value, out bool phaseSelection, out int magnitude)
        {
            if (value < -32)
            {
                value = -32;
            }
            else if (value > 31)
            {
                value = 31;
            }

            if (value >= 0)
            {
                phaseSelection = false;
                magnitude = value;
            }
            else
            {
                phaseSelection = true;
                magnitude = -(value + 1);
            }
        }

        private byte ComposeEyeOptimizationConfigByte(bool generalOptimization, bool minorTemperatureBoost, bool majorTemperatureBoost,
            bool minorModulationBoost, bool majorModulationBoost)
        {
            byte configByte = 0x00;

            if (generalOptimization)
            {
                configByte |= 0b0000_0001;
            }

            if (minorTemperatureBoost)
            {
                configByte |= 0b0000_0010;
            }

            if (majorTemperatureBoost)
            {
                configByte |= 0b0000_0100;
            }

            if (minorModulationBoost)
            {
                configByte |= 0b0010_0000;
            }

            if (majorModulationBoost)
            {
                configByte |= 0b0100_0000;
            }

            return configByte;
        }

        private int ComposeCrossingConfigByte(bool isEnabled, int magnitude)
        {
            if (magnitude < 0)
            {
                magnitude = 0;
            }
            else if (magnitude > 63)
            {
                magnitude = 63;
            }

            var configByte = (byte)((magnitude & 0b0011_1111) << 2);

            if (isEnabled)
            {
                configByte |= 0b0000_0001;
            }

            return configByte;
        }
        #endregion

        protected virtual void OnUtbStatusChanged(UtbStatusEventArgs e)
        {
            UtbStatusChanged?.Invoke(this, e);
        }

        protected virtual void OnModuleStatusChanged(ModuleStatusEventArgs e)
        {
            RefreshUtbParams();
            ModuleStatusChanged?.Invoke(this, e);
        }

        protected virtual void OnBertwaveStatusChanged(BertwaveStatusEventArgs e)
        {
            BertwaveStatusChanged?.Invoke(this, e);
        }

        protected virtual void OnOsaStatusChanged(OsaStatusEventArgs e)
        {
            OsaStatusChanged?.Invoke(this, e);
        }

        protected virtual void OnEyeDiagramParametersChanged(EyeDiagramParametersEventArgs e)
        {
            EyeDiagramParametersChanged?.Invoke(this, e);
        }

        protected virtual void OnTecParametersChanged(TecParametersEventArgs e)
        {
            TecParametersChanged?.Invoke(this, e);
        }

        //protected virtual void OnLaserValuesChanged(LaserValuesEventArgs e)
        //{
        //    LaserValuesChanged?.Invoke(this, e);
        //}

        protected virtual void OnBertwaveEyeDiagramParametersChanged(BertwaveEyeDiagramParametersEventArgs e)
        {
            BertwaveEyeDiagramParametersChanged?.Invoke(this, e);
        }

        protected virtual void OnOperationProgressStatusChanged(OperationProgressStatusEventArgs e)
        {
            OperationProgressStatusChanged?.Invoke(this, e);
        }

        protected virtual void OnOperationSuccessChanged(OperationSuccessEventArgs e)
        {
            OperationSuccessChanged?.Invoke(this, e);
        }

        protected virtual void OnOperationTypeChanged(OperationTypeEventArgs e)
        {
            OperationTypeChanged?.Invoke(this, e);
        }



        public void Quit()
        {
            // Если модуль подключен и его данные были изменены, просходит запись собранной информации в базу данных.
            if (IsModuleConnected && _isModuleDataChanged)
            {
                //WriteLastReceivedParametersToDatabase();
            }

            //SaveApplicationSettings();
            StopMonitoringTasks();
            DisconnectFromUtb();
            DisconnectFromBertwave();
            DisconnectFromOsa();
        }

        public double _ddmTemperature = 0.0;
        public double _ddmVoltage = 0.0;
        public double _ddmTxPower1 = 0.0;
        public double _ddmTxPower2 = 0.0;
        public double _ddmTxPower3 = 0.0;
        public double _ddmTxPower4 = 0.0;
        public double _ddmRxPower1 = 0.0;
        public double _ddmRxPower2 = 0.0;
        public double _ddmRxPower3 = 0.0;
        public double _ddmRxPower4 = 0.0;
        public double _ddmTxBias1 = 0.0;
        public double _ddmTxBias2 = 0.0;
        public double _ddmTxBias3 = 0.0;
        public double _ddmTxBias4 = 0.0;


        public void RefreshUtbParams()
        {
            //await Task.Run(() =>
            //{
                bool res = Utb.GetPortsTable(out PortsTable portsTable);
                Utb.GetPowerSourceTable(out PowerSourceTable psTable);
                ModuleVoltage = psTable.Voltage3V3;
                ModuleCurrent = psTable.Current3V3;
                if (res)
                {
                    ModuleAbsent = portsTable.ExternalModuleInputSignal4 == 1;
                    ModuleInterrupt = portsTable.ExternalModuleInputSignal2 == 1;
                }
                ModuleDeSel = portsTable.ExternalModuleOutputSignal2 == 1;
                ModuleResetL = portsTable.ExternalModuleOutputSignal4 == 1;
                ModuleLPMode = portsTable.ExternalModuleOutputSignal1 == 1;
            //});
        }

        public bool WriteModuleParameters(bool lp, bool ds, bool rl)
        {
            bool res;
            //RefreshTecParameters = false;
            res = Utb.SetPortsTable(lp, ds, rl);
            RefreshUtbParams();
            //RefreshTecParameters = true;
            return res;
        }

        public bool _is4chOscyll = false;


        public byte[] GetBertwaveScreenshot(BertwaveChannels ch)
        {
            byte[] byteImage = Bertwave.GetScreenshot(ch);
            //string strImage = BitConverter.ToString(byteImage);
            int digits = int.Parse(((char)byteImage[1]).ToString());
            string tmp_str = "";
            byte[] tmp_arr = new Span<byte>(byteImage, 2, digits).ToArray();
            foreach (byte b in tmp_arr)
                tmp_str += (char)b;
            int len = int.Parse(tmp_str);
            //string strImg = strImage.Substring(8);
            var strImg = new Span<byte>(byteImage, 2 + digits, len);
            //if (strImg.EndsWith("\n"))
            //    strImg.Remove(len - 1);
            //byte[] res = Encoding.ASCII.GetBytes(strImage);
            //byte end = strImg[len - 1];
            return strImg.ToArray();
        }
    }
}
