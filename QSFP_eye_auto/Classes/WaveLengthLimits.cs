using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSFP_eye_auto.Classes
{
    public class WaveLengthLimits
    {
        public int Channel { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public decimal Ideal { get; set; }
    }
}
