using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class BertwaveStatusEventArgs : DeviceStatusEventArgs
    {
        public BertwaveStatusEventArgs(bool isConnected) : base(isConnected)
        {
        }
    }
}
