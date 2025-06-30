using QsfpEyeDiagram.ViewModels.Commands;
using Std.Equipment.FiberTrade.UniversalTestBoard;
using Std.Modules.ConfigurationParameters.Qsfp;
using Std.Modules.ConfigurationParameters.Qsfp.Cdr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QsfpEyeDiagram.Models.EventArguments;
using Std.Data.Database.Domain;
using Std.Equipment;
using Std.Equipment.Anritsu.Bertwave;
using Std.Equipment.Anritsu.Osa;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using QsfpEyeDiagram.Models;

namespace QsfpEyeDiagram.ViewModels
{
    public class CdrViewModel : ViewModelBase
    {
        private bool _isOperationCdrSuccess;
        public bool IsOperationCdrSuccess
        {
            get => _isOperationCdrSuccess;
            set
            {
                if (_isOperationCdrSuccess != value)
                {
                    _isOperationCdrSuccess = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Параметры CDR

        #region ChannelMode(Tx)

        private bool _autoSquelchTx;
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

        private bool _inputOffsetCorrectionTx;
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

        private bool _hsdPolarityInversionTx;
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

        private bool _dataInputSelectionTx;
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

        private bool _crInCdrBypassModeTx;
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

        private bool _isCdrBypassedAndPoweredDownTx;
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

        private bool _rateSelectionTx;
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
        private int _ctleTx;
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
                if (_automuteTx != value)
                {
                    _automuteTx = value;
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
                if (_muteForceTx != value)
                {
                    _muteForceTx = value;
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
                if (_outputSwingTx != value)
                {
                    _outputSwingTx = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Deemphasis(Tx)

        private bool _jitterAdjustTx;
        public bool JitterAdjustTx
        {
            get => _jitterAdjustTx;
            set
            {
                if (_jitterAdjustTx != value)
                {
                    _jitterAdjustTx = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _deemphasisValueTx;
        public int DeemphasisValueTx
        {
            get => _deemphasisValueTx;
            set
            {
                if (_deemphasisValueTx != value)
                {
                    _deemphasisValueTx = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #endregion


        #region Commands

        private ICommand _saveCdrCtleCommand;
        public ICommand SaveCdrCtleCommand
        {
            get => _saveCdrCtleCommand ?? (_saveCdrCtleCommand = new RelayCommand(
                p => true,
                p => {
                    var ch = (Channel)p;
                    WriteCdrCtle(ch, 1);
                }));
        }
        public void WriteCdrCtle(Channel channel, Byte TxRx)
        {
            byte ctle = CtleInfo.ComposeCtleConfigByte(CtleTx);
            UniversalTestBoard Utb = new UniversalTestBoard();
            IsOperationCdrSuccess = Utb.WriteCdrCtle(ctle, channel, TxRx);
        }


        #endregion
    }
}
