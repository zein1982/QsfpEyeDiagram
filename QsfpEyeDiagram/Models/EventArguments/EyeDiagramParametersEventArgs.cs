using Std.Modules.ConfigurationParameters.Qsfp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class EyeDiagramParametersEventArgs : EventArgs
    {
        public EyeDiagramParameters EyeDiagramParameters { get; private set; }

        public EyeDiagramParametersEventArgs(EyeDiagramParameters eyeDiagramParameters)
        {
            EyeDiagramParameters = eyeDiagramParameters;
        }
    }
}
