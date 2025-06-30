using Std.Modules.ConfigurationParameters.Qsfp.Tec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class TecParametersEventArgs : EventArgs
    {
        public bool IsInitialParameters { get; private set; }

        public TecParameters TecParameters { get; private set; }

        public TecParametersEventArgs(TecParameters tecParameters, bool isInitialParameters = false)
        {
            IsInitialParameters = isInitialParameters;
            TecParameters = tecParameters;
        }
    }
}
