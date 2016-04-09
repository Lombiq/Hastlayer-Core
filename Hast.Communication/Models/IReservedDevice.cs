using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Models
{
    /// <summary>
    /// A connected hardware-executing device that is reserved for a session and thus can't handle anything else.
    /// </summary>
    public interface IReservedDevice : IDevice, IDisposable
    {
    }
}
