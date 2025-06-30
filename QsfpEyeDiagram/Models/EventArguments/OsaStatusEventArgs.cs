using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class OsaStatusEventArgs : DeviceStatusEventArgs
    {
        public OsaStatusEventArgs(bool isConnected) : base(isConnected)
        {
        }
    }
}
