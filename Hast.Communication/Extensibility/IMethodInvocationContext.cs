﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Hast.Common.Models;

namespace Hast.Communication.Extensibility
{
    /// <summary>
    /// The context of the invocation of a hardware-implemented method.
    /// </summary>
    public interface IMethodInvocationContext
    {
        /// <summary>
        /// Gets the context of the method invocation.
        /// </summary>
        IInvocation Invocation { get; }

        /// <summary>
        /// Gets the full name of the invoked method, including the full namespace of the parent type(s) as well as their return type 
        /// and the types of their (type) arguments.
        /// </summary>
        string MethodFullName { get; }

        /// <summary>
        /// Gets the hardware representation behind the hardware-implemented members.
        /// </summary>
        IHardwareRepresentation HardwareRepresentation { get; }
    }
}
