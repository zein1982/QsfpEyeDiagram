using QsfpEyeDiagram.ViewModels.Commands;
using Std.Equipment.FiberTrade.UniversalTestBoard;
using Std.Modules.ConfigurationParameters.Qsfp;
using Std.Modules.ConfigurationParameters.Qsfp.Cdr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QsfpEyeDiagram.ViewModels
{
    public class EyeDiagramChannelViewModel : ViewModelBase
    {
        //public static Semaphore semaphore = new Semaphore(1, 1);

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if(_isSelected!=value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }
        #region Основные параметры канала
        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isNotBuzzyTx = true;
        public bool IsNotBuzzyTx
        {
            get => _isNotBuzzyTx;
            set
            {
                if(_isNotBuzzyTx != value)
                {
                    _isNotBuzzyTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isCdrTxDataReaded = false; //данные Cdr Tx прочитаны
        public bool IsCdrTxReaded
        {
            get => _isCdrTxDataReaded;
            set
            {
                if(_isCdrTxDataReaded != value)
                {
                    _isCdrTxDataReaded = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isRxNotBuzzy = true;
        public bool IsRxNotBuzzy
        {
            get => _isRxNotBuzzy;
            set
            {
                if (_isRxNotBuzzy != value)
                {
                    _isRxNotBuzzy = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _bias;
        public int Bias
        {
            get => _bias;
            set
            {
                if (_bias != value)
                {
                    _bias = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _modulation;
        public int Modulation
        {
            get => _modulation;
            set
            {
                if (_modulation != value)
                {
                    _modulation = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Параметры эквалайзера
        private bool _isEqualizerEnabled;
        public bool IsEqualizerEnabled
        {
            get => _isEqualizerEnabled;
            set
            {
                if (_isEqualizerEnabled != value)
                {
                    _isEqualizerEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _equalizerPhaseWithMagnitude;
        public int EqualizerPhaseWithMagnitude
        {
            get => _equalizerPhaseWithMagnitude;
            set
            {
                if (_equalizerPhaseWithMagnitude != value)
                {
                    _equalizerPhaseWithMagnitude = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Параметры оптимизации глазковой диаграммы
        private bool _generalOptimization;
        public bool GeneralOptimization
        {
            get => _generalOptimization;
            set
            {
                if (_generalOptimization != value)
                {
                    _generalOptimization = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _minorTemperatureBoost;
        public bool MinorTemperatureBoost
        {
            get => _minorTemperatureBoost;
            set
            {
                if (_minorTemperatureBoost != value)
                {
                    _minorTemperatureBoost = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _majorTemperatureBoost;
        public bool MajorTemperatureBoost
        {
            get => _majorTemperatureBoost;
            set
            {
                if (_majorTemperatureBoost != value)
                {
                    _majorTemperatureBoost = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _minorModulationBoost;
        public bool MinorModulationBoost
        {
            get => _minorModulationBoost;
            set
            {
                if (_minorModulationBoost != value)
                {
                    _minorModulationBoost = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _majorModulationBoost;
        public bool MajorModulationBoost
        {
            get => _majorModulationBoost;
            set
            {
                if (_majorModulationBoost != value)
                {
                    _majorModulationBoost = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Параметры точки пересечения
        private bool _isCrossingEnabled;
        public bool IsCrossingEnabled
        {
            get => _isCrossingEnabled;
            set
            {
                if (_isCrossingEnabled != value)
                {
                    _isCrossingEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _crossingMagnitude;
        public int CrossingMagnitude
        {
            get => _crossingMagnitude;
            set
            {
                if (_crossingMagnitude != value)
                {
                    _crossingMagnitude = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Параметры CDR

        private bool _isOperationCdrSuccessTx;
        public bool IsOperationCdrSuccessTx
        {
            get => _isOperationCdrSuccessTx;
            set
            {
                if (_isOperationCdrSuccessTx != value)
                {
                    _isOperationCdrSuccessTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isOperationCdrSuccessRx;
        public bool IsOperationCdrSuccessRx
        {
            get => _isOperationCdrSuccessRx;
            set
            {
                if (_isOperationCdrSuccessRx != value)
                {
                    _isOperationCdrSuccessRx = value;
                    OnPropertyChanged();
                }
            }
        }

        #region ChannelMode(Tx)

        private bool _autoSquelchTx = false;
        public bool AutoSquelchTx
        {
            get => _autoSquelchTx;
            set
            {
                if (_autoSquelchTx != value)
                {
                    _autoSquelchTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _inputOffsetCorrectionTx = false;
        public bool InputOffsetCorrectionTx
        {
            get => _inputOffsetCorrectionTx;
            set
            {
                if (_inputOffsetCorrectionTx != value)
                {
                    _inputOffsetCorrectionTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _hsdPolarityInversionTx = false;
        public bool HighSpeedDataPolarityInversionTx
        {
            get => _hsdPolarityInversionTx;
            set
            {
                if (_hsdPolarityInversionTx != value)
                {
                    _hsdPolarityInversionTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _dataInputSelectionTx = false;
        public bool DataInputSelectionTx
        {
            get => _dataInputSelectionTx;
            set
            {
                if (_dataInputSelectionTx != value)
                {
                    _dataInputSelectionTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _crInCdrBypassModeTx = false;
        public bool ClockRecoveryInCdrBypassModeTx
        {
            get => _crInCdrBypassModeTx;
            set
            {
                if (_crInCdrBypassModeTx != value)
                {
                    _crInCdrBypassModeTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isCdrBypassedAndPoweredDownTx = false;
        public bool IsCdrBypassedAndPoweredDownTx
        {
            get => _isCdrBypassedAndPoweredDownTx;
            set
            {
                if (_isCdrBypassedAndPoweredDownTx != value)
                {
                    _isCdrBypassedAndPoweredDownTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _rateSelectionTx = false;
        public bool RateSelectionTx
        {
            get => _rateSelectionTx;
            set
            {
                if (_rateSelectionTx != value)
                {
                    _rateSelectionTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _powerDownTx;
        public bool PowerDownTx
        {
            get => _powerDownTx;
            set
            {
                if (_powerDownTx != value)
                {
                    _powerDownTx = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Ctle(Tx)
        private int _ctleTx = 0;
        public int CtleTx
        {
            get => _ctleTx;
            set
            {
                if (_ctleTx != value)
                {
                    _ctleTx = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Swing(Tx)

        private bool _automuteTx;
        public bool AutomuteTx
        {
            get => _automuteTx;
            set
            {
                if(_automuteTx!=value)
                {
                    _automuteTx = value;
                    IsOperationCdrSuccessTx = false;
                    OnPropertyChanged();
                }
            }
        }

        private bool _muteForceTx;
        public bool MuteForceTx
        {
            get => _muteForceTx;
            set
            {
                if(_muteForceTx!=value)
                {
                    _muteForceTx = value;
                    IsOperationCdrSuccessTx = false;
                    OnPropertyChanged();
                }
            }
        }

        private int _outputSwingTx;
        public int OutputSwingTx
        {
            get => _outputSwingTx;
            set
            {
                if(_outputSwingTx!=value)
                {
                    _outputSwingTx = value;
                    IsOperationCdrSuccessTx = false;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Deemphasis(Tx)

        private bool _jitterAdjustTx = false;
        public bool JitterAdjustTx
        {
            get => _jitterAdjustTx;
            set
            {
                if(_jitterAdjustTx!=value)
                {
                    _jitterAdjustTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _deemphasisValueTx = 0;
        public int DeemphasisValueTx
        {
            get => _deemphasisValueTx;
            set
            {
                if(_deemphasisValueTx!=value)
                {
                    _deemphasisValueTx = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region ChannelMode(Rx)

        private bool _autoSquelchRx;
        public bool AutoSquelchRx
        {
            get => _autoSquelchRx;
            set
            {
                if (_autoSquelchRx != value)
                {
                    _autoSquelchRx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _inputOffsetCorrectionRx;
        public bool InputOffsetCorrectionRx
        {
            get => _inputOffsetCorrectionRx;
            set
            {
                if (_inputOffsetCorrectionRx != value)
                {
                    _inputOffsetCorrectionRx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _hsdPolarityInversionRx;
        public bool HighSpeedDataPolarityInversionRx
        {
            get => _hsdPolarityInversionRx;
            set
            {
                if (_hsdPolarityInversionRx != value)
                {
                    _hsdPolarityInversionRx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _dataInputSelectionRx;
        public bool DataInputSelectionRx
        {
            get => _dataInputSelectionRx;
            set
            {
                if (_dataInputSelectionRx != value)
                {
                    _dataInputSelectionRx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _crInCdrBypassModeRx;
        public bool ClockRecoveryInCdrBypassModeRx
        {
            get => _crInCdrBypassModeRx;
            set
            {
                if (_crInCdrBypassModeRx != value)
                {
                    _crInCdrBypassModeRx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isCdrBypassedAndPoweredDownRx;
        public bool IsCdrBypassedAndPoweredDownRx
        {
            get => _isCdrBypassedAndPoweredDownRx;
            set
            {
                if (_isCdrBypassedAndPoweredDownRx != value)
                {
                    _isCdrBypassedAndPoweredDownRx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _rateSelectionRx;
        public bool RateSelectionRx
        {
            get => _rateSelectionRx;
            set
            {
                if (_rateSelectionRx != value)
                {
                    _rateSelectionRx = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _powerDownRx;
        public bool PowerDownRx
        {
            get => _powerDownRx;
            set
            {
                if (_powerDownRx != value)
                {
                    _powerDownRx = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region SLA(Rx)
        private bool _slaEnRx;

        public bool SlaEnRx
        {
            get => _slaEnRx;
            set
            {
                if(_slaEnRx!=value)
                {
                    _slaEnRx = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _slaRx;
        public int SlaRx
        {
            get => _slaRx;
            set
            {
                if (_slaRx != value)
                {
                    _slaRx = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Swing(Rx)

        private bool _automuteRx;
        public bool AutomuteRx
        {
            get => _automuteRx;
            set
            {
                if (_automuteRx != value)
                {
                    _automuteRx = value;
                    IsOperationCdrSuccessRx = false;
                    OnPropertyChanged();
                }
            }
        }

        private bool _muteForceRx;
        public bool MuteForceRx
        {
            get => _muteForceRx;
            set
            {
                if (_muteForceRx != value)
                {
                    _muteForceRx = value;
                    IsOperationCdrSuccessRx = false;
                    OnPropertyChanged();
                }
            }
        }

        private int _outputSwingRx;
        public int OutputSwingRx
        {
            get => _outputSwingRx;
            set
            {
                if (_outputSwingRx != value)
                {
                    _outputSwingRx = value;
                    IsOperationCdrSuccessRx = false;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Deemphasis(Rx)

        private bool _jitterAdjustRx;
        public bool JitterAdjustRx
        {
            get => _jitterAdjustRx;
            set
            {
                if (_jitterAdjustRx != value)
                {
                    _jitterAdjustRx = value;
                    IsOperationCdrSuccessRx = false;
                    OnPropertyChanged();
                }
            }
        }

        private int _deemphasisValueRx;
        public int DeemphasisValueRx
        {
            get => _deemphasisValueRx;
            set
            {
                if (_deemphasisValueRx != value)
                {
                    _deemphasisValueRx = value;
                    IsOperationCdrSuccessRx = false;
                    OnPropertyChanged();
                }
            }
        }
        #endregion


        #endregion

        public  QsfpEyeDiagramViewModel Parent { get; set; }


        public EyeDiagramChannelViewModel()
        {
            SetChannelParameters(EyeDiagramChannelParameters.Unassigned);
        }

        public EyeDiagramChannelViewModel(EyeDiagramChannelParameters parameters, QsfpEyeDiagramViewModel parent)
        {
            Parent = parent;
            SetChannelParameters(parameters);
            //parent.ZeroChannel.IsEnabled = IsChannelEnabled(Channel.Zero);
            //parent.FirstChannel.IsEnabled = IsChannelEnabled(Channel.First);
            //parent.SecondChannel.IsEnabled = IsChannelEnabled(Channel.Second);
            //parent.ThirdChannel.IsEnabled = IsChannelEnabled(Channel.Third);
        }

        public byte IsChannelEnabled()
        {
            byte res = this.Parent._model.Utb.GetChannelStatuses();
            return res;
        }

        public void SetChannelParameters(EyeDiagramChannelParameters parameters)
        {
            if (parameters != null)
            {
                PutParameterValuesToProperties(parameters);
            }
        }

        private void PutParameterValuesToProperties(EyeDiagramChannelParameters parameters)
        {
            // Основные параметры канала.

            //IsEnabled = parameters.IsEnabled;
            Bias = parameters.Bias;
            Modulation = parameters.Modulation;

            // Параметры эквалайзера.
            IsEqualizerEnabled = parameters.VariableEqualizer.IsEnabled;
            EqualizerPhaseWithMagnitude =
                ConvertVariableEqualizerPhaseAndMagnitudeToInt(parameters.VariableEqualizer.PhaseSelection, parameters.VariableEqualizer.Magnitude);

            // Параметры оптимизации глазковой диаграммы.
            GeneralOptimization = parameters.EyeOptimization.GeneralOptimization;

            MinorTemperatureBoost = parameters.EyeOptimization.MinorTemperatureBoost;
            MajorTemperatureBoost = parameters.EyeOptimization.MajorTemperatureBoost;

            MinorModulationBoost = parameters.EyeOptimization.MinorModulationBoost;
            MajorModulationBoost = parameters.EyeOptimization.MajorModulationBoost;

            // Параметры точки пересечения.
            IsCrossingEnabled = parameters.Crossing.IsEnabled;
            CrossingMagnitude = parameters.Crossing.Magnitude;
           
        }

        private int ConvertVariableEqualizerPhaseAndMagnitudeToInt(bool phaseSelection, int magnitude)
        {
            if (!phaseSelection)
            {
                return magnitude;
            }

            return (-magnitude) - 1;
        }

        public void PutCdrParametersToProperties(Channel channel)
        {
            //Mouse.OverrideCursor = null;
            //Mouse.OverrideCursor = Cursors.Wait;
            //bool refreshTecParamFlag;

            IsNotBuzzyTx = false;
            IsCdrTxReaded = false;
            CdrParameters cdrParametersTx=CdrParameters.Unassigned;
            UniversalTestBoard Utb;
            //Parent.logger.Trace($"Получаем параметры CDR Tx канала {(byte)channel}");
            //if (Parent != null)
            //{
            Utb = Parent._model.Utb;
            //new UniversalTestBoard();
            //refreshTecParamFlag = Parent.RefreshTecParameters;
            //Parent.RefreshTecParameters = false;//т.к чтение параметров CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен
                                                //}
                                                //else
                                                //{
                                                //    Utb = new UniversalTestBoard();
                                                //}
                                                //await Task.Run(() =>
                                                //{
            bool success=false;

            //Parent.UtbMonitoringPause(); //Отключаем мониторинг UTB
            try
                    {
                         cdrParametersTx = Utb.GetCdrParameters(channel, out success);
                    }
                    catch(Exception /*ex*/)
                    {
                        MessageBox.Show($"Не удалось получить параметры CDR Tx канала {((int)channel)}","Неудача",MessageBoxButton.OK,MessageBoxImage.Exclamation);
                    }
            //Parent.UtbMonitoringRelease();// Восстанавливаем мониторинг UTB
            IsOperationCdrSuccessTx = success;
                    IsNotBuzzyTx = true;

                    if (IsOperationCdrSuccessTx)
                    {
                        PowerDownTx = cdrParametersTx.ChannelMode.PowerDown;
                        RateSelectionTx = cdrParametersTx.ChannelMode.RateSelection;
                        InputOffsetCorrectionTx = cdrParametersTx.ChannelMode.InputOffsetCorrection;
                        AutoSquelchTx = cdrParametersTx.ChannelMode.AutoSquelch;
                        ClockRecoveryInCdrBypassModeTx = cdrParametersTx.ChannelMode.ClockRecoveryInCdrBypassMode;
                        DataInputSelectionTx = cdrParametersTx.ChannelMode.DataInputSelection;
                        HighSpeedDataPolarityInversionTx = cdrParametersTx.ChannelMode.HighSpeedDataPolarityInversion;
                        IsCdrBypassedAndPoweredDownTx = cdrParametersTx.ChannelMode.IsCdrBypassedAndPoweredDown;

                        CtleTx = cdrParametersTx.Ctle.Equalization;

                        AutomuteTx = cdrParametersTx.OutputSwing.AutoMuteDisabled;
                        MuteForceTx = cdrParametersTx.OutputSwing.ForceOutputMute;
                        OutputSwingTx = cdrParametersTx.OutputSwing.SwingValue;

                        JitterAdjustTx = cdrParametersTx.OutputDeemphasis.JitterAdjust;
                        DeemphasisValueTx = cdrParametersTx.OutputDeemphasis.DeemphasisValue;
                        IsCdrTxReaded = true;
                    }
            //});
            //if (refreshTecParamFlag)
            //{
            //    Parent.RefreshTecParameters = true;//возвратим режим мониторинга TEC
            //}
            //Mouse.OverrideCursor = null;
            //Mouse.OverrideCursor = Cursors.Arrow;
        }

        //Команды получения параметров CDR
        private ICommand _readChannelCdrTxCommand;
        public  ICommand ReadChannelCdrTxCommand
        {
            get => _readChannelCdrTxCommand ?? (_readChannelCdrTxCommand = new RelayCommand(
                p =>true,
                p =>
                {
                    var ch = (Channel)p;
                    IsOperationCdrSuccessTx = false;
                    //IsNotBuzzy = false;
                    Task.Run(() => PutCdrParametersToProperties(ch));
                })); 
                    
        }

        private ICommand _readChannelCdrRxCommand;
        public ICommand ReadChannelCdrRxCommand
        {
            get => _readChannelCdrRxCommand ?? (_readChannelCdrRxCommand = new RelayCommand(
                p=> true,
                p=>
                {
                    IsRxNotBuzzy = false;
                    var ch = (Channel)p;
                    IsOperationCdrSuccessRx = false;
                    IsEnabled = false;
                    Task.Run(()=>ReadChannelCdrRxParameters(ch));
                    IsEnabled = true;

                }
                ));
        }

        public void ReadChannelCdrRxParameters(Channel channel)
        {
            if (Parent != null)
            {
                //bool refreshTecParamFlag = Parent.RefreshTecParameters;
                //Parent.RefreshTecParameters = false;//т.к чтение параметров CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен

                IsRxNotBuzzy = false;
                CdrRxParameters cdrParametersRx=CdrRxParameters.Unassigned;
                UniversalTestBoard Utb = Parent._model.Utb;//new UniversalTestBoard();
                Parent.logger.Trace($"Получаем параметры Rx канала {(int)channel}");
                //await Task.Run(() =>
                //{
                try
                {
                    bool success = false;
                    //Parent.UtbMonitoringPause();
                    cdrParametersRx = Utb.GetCdrRxParameters(channel, out success);
                    //Parent.UtbMonitoringRelease();
                    IsOperationCdrSuccessRx = success;
                    PutCdrRxParametersToProps(cdrParametersRx);
                }
                catch(Exception /*ex*/)
                {
                    MessageBox.Show($"Не удалось получить параметры CDR Rx канала {(int)channel}", "Неудача", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                //});

                IsRxNotBuzzy = true;
            }
        }

        void PutCdrRxParametersToProps(CdrRxParameters cdrRxParameters)
        {
            if(IsOperationCdrSuccessRx)
            {
                PowerDownRx = cdrRxParameters.ChannelMode.PowerDown;
                RateSelectionRx = cdrRxParameters.ChannelMode.RateSelection;
                InputOffsetCorrectionRx = cdrRxParameters.ChannelMode.InputOffsetCorrection;
                AutoSquelchRx = cdrRxParameters.ChannelMode.AutoSquelch;
                ClockRecoveryInCdrBypassModeRx = cdrRxParameters.ChannelMode.ClockRecoveryInCdrBypassMode;
                DataInputSelectionRx = cdrRxParameters.ChannelMode.DataInputSelection;
                HighSpeedDataPolarityInversionRx = cdrRxParameters.ChannelMode.HighSpeedDataPolarityInversion;
                IsCdrBypassedAndPoweredDownRx = cdrRxParameters.ChannelMode.IsCdrBypassedAndPoweredDown;

                SlaEnRx = cdrRxParameters.Sla.SlaEn;
                SlaRx = cdrRxParameters.Sla.SlaValue;

                AutomuteRx = cdrRxParameters.OutputSwing.AutoMuteDisabled;
                MuteForceRx = cdrRxParameters.OutputSwing.ForceOutputMute;
                OutputSwingRx = cdrRxParameters.OutputSwing.SwingValue;

                JitterAdjustRx = cdrRxParameters.OutputDeemphasis.JitterAdjust;
                DeemphasisValueRx = cdrRxParameters.OutputDeemphasis.DeemphasisValue;
            }
        }

        private ICommand _saveSlaRxCommand;
        public ICommand SaveSlaRxCommand
        {
            get => _saveSlaRxCommand ?? (_saveSlaRxCommand = new RelayCommand(
                p=>true,
                p=>
                {
                    var parameters = (object[])p;
                    var ch = (Channel)parameters[0];
                    var eyeDiagramChannel = (EyeDiagramChannelViewModel)parameters[1];
                    Task.Run(()=>WriteSla(ch));
                }
        ));
        }

        public async Task WriteSla(Channel channel)
        {
            byte sla = SlaInfo.ComposeSlaConfigByte(SlaEnRx, SlaRx);
            IsRxNotBuzzy = false;
            IsOperationCdrSuccessRx = false;

            //bool refreshTecParamFlag = Parent.RefreshTecParameters;
            //Parent.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен

            UniversalTestBoard Utb = Parent._model.Utb;//new UniversalTestBoard();
            //Parent.UtbMonitoringPause();
            bool result = false;
            await Task.Run(() =>
            {
                result = Utb.WriteCdrCtle(sla, channel, 0);
            });
            IsOperationCdrSuccessRx = result;
            IsRxNotBuzzy = true;

            //Parent.UtbMonitoringRelease();
            //if (refreshTecParamFlag)
            //{
            //    Parent.RefreshTecParameters = true;
            //}
        }

        // Команда записи CDR Mode(Tx)
        private ICommand _saveCdrModeTxCommand;
        public ICommand SaveCdrModeTxCommand
        {
            get => _saveCdrModeTxCommand ?? (_saveCdrModeTxCommand = new RelayCommand(
                p => true,
                p=> 
                {
                    var ch = (Channel)p;
                    //var setValue = (bool)parameters[0];
                    Task.Run(()=>WriteCdrMode(ch, 1));
                }));
        }
        public async Task WriteCdrMode(Channel channel, byte TxRx)
        {
            byte mode;
            if (TxRx == 1)
            {
                mode = ChannelModeInfo.ComposeChannelModeConfigByte(InputOffsetCorrectionTx, AutoSquelchTx, HighSpeedDataPolarityInversionTx, DataInputSelectionTx,
                    ClockRecoveryInCdrBypassModeTx, IsCdrBypassedAndPoweredDownTx, RateSelectionTx, PowerDownTx);
                IsNotBuzzyTx = false;
                IsOperationCdrSuccessTx = false;
            }
            else
            {
                mode = ChannelModeInfo.ComposeChannelModeConfigByte(InputOffsetCorrectionRx, AutoSquelchRx, HighSpeedDataPolarityInversionRx, DataInputSelectionRx,
                    ClockRecoveryInCdrBypassModeRx, IsCdrBypassedAndPoweredDownRx, RateSelectionRx, PowerDownRx);
                IsRxNotBuzzy = false;
                IsOperationCdrSuccessRx = false;
            }
            //bool refreshTecParamFlag = Parent.RefreshTecParameters;
            //Parent.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен

            UniversalTestBoard Utb = Parent._model.Utb;//new UniversalTestBoard();
            //Parent.UtbMonitoringPause();

            await Task.Run(() =>
            {
                //semaphore.WaitOne();
                bool result = Utb.WriteCdrMode(mode, channel, TxRx/*, Parent.ProgressPercentHandler*/);
                //semaphore.Release(1);
                if (TxRx == 1)
                {
                    IsOperationCdrSuccessTx = result;
                    IsNotBuzzyTx = true;
                }
                else
                {
                    IsOperationCdrSuccessRx = result;
                    IsRxNotBuzzy = true;
                }
            });

            //Parent.UtbMonitoringRelease();
            //if (refreshTecParamFlag)
            //{
            //    Parent.RefreshTecParameters = true;
            //}
        }

        //Команда записи CdrSwing(Tx)

        private ICommand _saveCdrSwingTxCommand;
        public ICommand SaveCdrSwingTxCommand
        {
            get => _saveCdrSwingTxCommand ?? (_saveCdrSwingTxCommand = new RelayCommand(
                p=>true,
                p=>
                {
                    var parameters = (object[])p;
                    var ch = (Channel)parameters[0];
                    var eyeDiagramChannel = (EyeDiagramChannelViewModel)parameters[1];
                    Task.Run(()=>WriteCdrSwing(ch, 1));
                }
                ));
        }

        public async Task WriteCdrSwing(Channel channel, byte TxRx)
        {

            byte swing;
            if (TxRx == 1)
            {
                swing = OutputSwingInfo.ComposeOutputSwingConfigByte(MuteForceTx, AutomuteTx, OutputSwingTx);
                IsNotBuzzyTx = false;
                IsOperationCdrSuccessTx = false;
            }
            else
            {
                swing = OutputSwingInfo.ComposeOutputSwingConfigByte(MuteForceRx, AutomuteRx, OutputSwingRx);
                IsRxNotBuzzy = false;
                IsOperationCdrSuccessRx = false;
            }

            UniversalTestBoard Utb = Parent._model.Utb;//new UniversalTestBoard();

            //Parent.UtbMonitoringPause();
            await Task.Run(() =>
            {
                //IsRxNotBuzzy = false;
                bool result;
                //bool refreshTecParamFlag = Parent.RefreshTecParameters;
                //Parent.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен

                result = Utb.WriteCdrSwing(swing, channel, TxRx);

                //if (refreshTecParamFlag)
                //{
                //    Parent.RefreshTecParameters = true;
                //}

                if (TxRx == 1)
                {
                    IsOperationCdrSuccessTx = result;
                    IsNotBuzzyTx = true;
                }
                else
                {
                    IsOperationCdrSuccessRx = result;
                    IsRxNotBuzzy = true;
                }
            });
            //Parent.UtbMonitoringRelease();
        }

        //Команда записи Deemphasis(Tx)
        private ICommand _saveCdrDeemphasisTxCommand;
        public ICommand SaveCdrDeemphasisTxCommand
        {
            get => _saveCdrDeemphasisTxCommand ?? (_saveCdrDeemphasisTxCommand = new RelayCommand(
                p => true,
                p =>
                {
                    var parameters = (object[])p;
                    var ch = (Channel)parameters[0];
                    var eyeDiagramChannel = (EyeDiagramChannelViewModel)parameters[1];
                    Task.Run(()=>WriteCdrDeemphasis(ch, 1));
                }
                ));
        }


        public async Task WriteCdrDeemphasis(Channel channel, byte TxRx)
        {
            byte deemph;
            if (TxRx == 1)
            {
                deemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(JitterAdjustTx, DeemphasisValueTx);
                IsNotBuzzyTx = false;
                IsOperationCdrSuccessTx = false;
            }
            else
            {
                deemph = OutputDeemphasisInfo.ComposeOutputDeemphasisConfigByte(JitterAdjustRx, DeemphasisValueRx);
                IsRxNotBuzzy = false;
                IsOperationCdrSuccessRx = false;
            }

            UniversalTestBoard Utb = Parent._model.Utb;//new UniversalTestBoard();
            //bool refreshTecParamFlag = Parent.RefreshTecParameters;
            //Parent.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен

            //Parent.UtbMonitoringPause();
            await Task.Run(() =>
            {
                bool result;

                result = Utb.WriteCdrDeemphasis(deemph, channel, TxRx);

                //if (refreshTecParamFlag)
                //{
                //    Parent.RefreshTecParameters = true;
                //}

                if (TxRx == 1)
                {
                    IsOperationCdrSuccessTx = result;
                    IsNotBuzzyTx = true;
                }
                else
                {
                    IsOperationCdrSuccessRx = result;
                    IsRxNotBuzzy = true;
                }
            });
            //Parent.UtbMonitoringRelease();
        }

        //Команда записи CdrMode(Rx)
        private ICommand _saveCdrModeRxCommand;
        public ICommand SaveCdrModeRxCommand
        {
            get => _saveCdrModeRxCommand ?? (_saveCdrModeRxCommand = new RelayCommand(
                p => true,
                p=>
                {
                    var ch = (Channel)p;
                    Task.Run(()=>WriteCdrMode(ch, 0));
                }));
        }

        private ICommand _saveCdrSwingRxCommand;
        public ICommand SaveCdrSwingRxCommand
        {
            get => _saveCdrSwingRxCommand ?? (_saveCdrSwingRxCommand = new RelayCommand(
                p => true,
                p=>
                {
                    var parameters = (object[])p;
                    var ch = (Channel)parameters[0];
                    var eyeDiagramChannel = (EyeDiagramChannelViewModel)parameters[1];
                    Task.Run(() => WriteCdrSwing(ch, 0));
                }
                ));
        }

        private ICommand _saveCdrDeemphasisRxCommand;
        public ICommand SaveCdrDeemphasisRxCommand
        {
            get => _saveCdrDeemphasisRxCommand ?? (_saveCdrDeemphasisRxCommand = new RelayCommand(
                p => true,
                p=>
                {
                    var parameters = (object[])p;
                    var ch = (Channel)parameters[0];
                    var eyeDiagramChannel = (EyeDiagramChannelViewModel)parameters[1];
                    Task.Run(() => WriteCdrDeemphasis(ch, 0));
                }
                ));
        }

        

        private double _attenuation;
        public double Attenuation
        {
            get => _attenuation;
            set
            {
                if (_attenuation != value)
                {
                    _attenuation = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
