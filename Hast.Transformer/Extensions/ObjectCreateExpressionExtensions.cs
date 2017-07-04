﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class ObjectCreateExpressionExtensions
    {
        public static string GetConstructorFullName(this ObjectCreateExpression objectCreateExpression) =>
            objectCreateExpression.Annotation<MethodReference>()?.FullName;
    }
}