using Std.Equipment.Anritsu.Bertwave;
using System;

namespace QsfpEyeDiagram.Models.EventArguments
{
    public class BertwaveEyeDiagramParametersEventArgs : EventArgs
    {
        public BertwaveEyeDiagramParameters BertwaveEyeDiagramParameters { get; private set; }

        public BertwaveEyeDiagramParametersEventArgs(BertwaveEyeDiagramParameters bertwaveEyeDiagramParameters)
        {
            BertwaveEyeDiagramParameters = bertwaveEyeDiagramParameters;
        }
    }
}
