using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace QSFP_eye_auto.Classes
{
    public class TuneLimits
    {
        [Key]
        public int Channel { get; set; }
        public int Min_bias { get; set; }
        public decimal Avg_bias { get; set; }
        public int Max_bias { get; set; }
        public int Min_mod { get; set; }
        public decimal Avg_mod { get; set; }
        public int Max_mod { get; set; }
        public int Min_cros { get; set; }
        public decimal Avg_cros { get; set; }
        public int Max_cros { get; set; }
        public int Min_eq { get; set; }
        public decimal Avg_eq { get; set; }
        public int Max_eq { get; set; }
        public int Min_tec { get; set; }
        public decimal Avg_tec { get; set; }
        public int Max_tec { get; set; }
    }
}
