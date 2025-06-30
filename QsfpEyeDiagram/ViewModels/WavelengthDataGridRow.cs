using System.Windows;
using System.Windows.Media;

namespace QsfpEyeDiagram.ViewModels
{
    public class WavelengthDataGridRow : ViewModelBase
    {
        private double _zeroChannelWavelength;
        public double ZeroChannelWavelength
        {
            get => _zeroChannelWavelength;
            set
            {
                if (_zeroChannelWavelength != value)
                {
                    _zeroChannelWavelength = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _firstChannelWavelength;
        public double FirstChannelWavelength
        {
            get => _firstChannelWavelength;
            set
            {
                if (_firstChannelWavelength != value)
                {
                    _firstChannelWavelength = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _secondChannelWavelength;
        public double SecondChannelWavelength
        {
            get => _secondChannelWavelength;
            set
            {
                if (_secondChannelWavelength != value)
                {
                    _secondChannelWavelength = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _thirdChannelWavelength;
        public double ThirdChannelWavelength
        {
            get => _thirdChannelWavelength;
            set
            {
                if (_thirdChannelWavelength != value)
                {
                    _thirdChannelWavelength = value;
                    OnPropertyChanged();
                }
            }
        }

        private Brush _background = Brushes.White;
        public Brush Background
        {
            get => _background;
            set
            {
                if (_background != value)
                {
                    _background = value;
                    OnPropertyChanged();
                }
            }
        }

        private FontWeight _fontWeight = FontWeights.Normal;
        public FontWeight FontWeight
        {
            get => _fontWeight;
            set
            {
                if (_fontWeight != value)
                {
                    _fontWeight = value;
                    OnPropertyChanged();
                }
            }
        }

        public WavelengthDataGridRow()
        {
        }

        public WavelengthDataGridRow(double zeroChannelWavelength, double firstChannelWavelength, double secondChannelWavelength, double thirdChannelWavelength)
        {
            ZeroChannelWavelength = zeroChannelWavelength;
            FirstChannelWavelength = firstChannelWavelength;
            SecondChannelWavelength = secondChannelWavelength;
            ThirdChannelWavelength = thirdChannelWavelength;
        }
    }
}
