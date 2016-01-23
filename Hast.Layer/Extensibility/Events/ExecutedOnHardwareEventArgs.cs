using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Common.Models;
using Hast.Communication.Models;

namespace Hast.Layer.Extensibility.Events
{
    public class ExecutedOnHardwareEventArgs : EventArgs
    {
        private IHardwareRepresentation _hardwareRepresentation;
        public IHardwareRepresentation MaterializedHardware
        {
            get { return _hardwareRepresentation; }
        }

        private string _memberFullName;
        public string MemberFullName
        {
            get { return _memberFullName; }
        }

        private IHardwareExecutionInformation _hardwareExecutionInformation;
        public IHardwareExecutionInformation HardwareExecutionInformation
        {
            get { return _hardwareExecutionInformation; }
        }


        public ExecutedOnHardwareEventArgs(
            IHardwareRepresentation hardwareRepresentation,
            string memberFullName,
            IHardwareExecutionInformation hardwareExecutionInformation)
        {
            _hardwareRepresentation = hardwareRepresentation;
            _memberFullName = memberFullName;
            _hardwareExecutionInformation = hardwareExecutionInformation;
        }
    }
}
