using Std.Modules.ConfigurationParameters.Qsfp.Tec;

namespace QsfpEyeDiagram.ViewModels
{
    public class TecDataGridRow : ViewModelBase
    {
        private int _dacVoltage;
        public int DacVoltage
        {
            get => _dacVoltage;
            set
            {
                if (_dacVoltage != value)
                {
                    _dacVoltage = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _tecTemp;
        public int TecTemp
        {
            get => _tecTemp;
            set => Set(ref _tecTemp, value);
        }

        private int _current;
        public int Current
        {
            get => _current;
            set
            {
                if (_current != value)
                {
                    _current = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _voltage;
        public int Voltage
        {
            get => _voltage;
            set
            {
                if (_voltage != value)
                {
                    _voltage = value;
                    OnPropertyChanged();
                }
            }
        }

        public TecDataGridRow()
        {
        }

        public void SetTecParameters(TecParameters parameters)
        {
            if (parameters != null)
            {
                PutParameterValuesToProperties(parameters);
            }
        }

        private void PutParameterValuesToProperties(TecParameters parameters)
        {
            DacVoltage = parameters.DacVoltage;
            TecTemp = parameters.CurrentTemperatureVoltage;
            Current = parameters.Current;
            Voltage = parameters.Voltage;
        }
    }
}
