﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class MethodDeclarationExtensions
    {
        public static bool IsConstructor(this MethodDeclaration methodDeclaration) =>
            methodDeclaration.Annotation<MethodDefinition>()?.IsConstructor == true;
    }
}
