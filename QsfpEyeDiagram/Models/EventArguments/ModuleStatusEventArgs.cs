using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class ModuleStatusEventArgs : DeviceStatusEventArgs
    {
        public ModuleStatusEventArgs(bool isConnected) : base(isConnected)
        {
        }
    }
}
