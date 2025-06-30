using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class UtbStatusEventArgs : DeviceStatusEventArgs
    {
        public UtbStatusEventArgs(bool isConnected) : base(isConnected)
        {
        }
    }
}
