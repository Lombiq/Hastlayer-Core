﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer
{
    public interface IMemberSuitabilityChecker : IDependency
    {
        /// <summary>
        /// Checks whether a member is suitable to be part of the hardware implementation's interface.
        /// </summary>
        bool IsSuitableInterfaceMember(EntityDeclaration member, ITypeDeclarationLookupTable typeDeclarationLookupTable);
    }
}
