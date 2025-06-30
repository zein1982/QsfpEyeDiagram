using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class DeviceStatusEventArgs : EventArgs
    {
        public bool IsConnected { get; private set; }

        public DeviceStatusEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
