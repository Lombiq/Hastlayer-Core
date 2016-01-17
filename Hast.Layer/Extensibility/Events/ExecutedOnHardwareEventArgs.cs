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
        private IMaterializedHardware _materializedHardware;
        public IMaterializedHardware MaterializedHardware
        {
            get { return _materializedHardware; }
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
            IMaterializedHardware materializedHardware,
            string memberFullName,
            IHardwareExecutionInformation hardwareExecutionInformation)
        {
            _materializedHardware = materializedHardware;
            _memberFullName = memberFullName;
            _hardwareExecutionInformation = hardwareExecutionInformation;
        }
    }
}
