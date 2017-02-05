﻿using System;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IRecordComposer : IDependency
    {
        bool IsSupportedRecordMember(AstNode node);
        Record CreateRecordFromType(TypeDefinition typeDefinition);
    }


    public static class RecordComposerExtensions
    {
        public static Record CreateRecordFromType(this IRecordComposer recordComposer, TypeDeclaration typeDeclaration)
        {
            var typeDefinition = typeDeclaration.Annotation<TypeDefinition>();

            if (typeDefinition == null)
            {
                throw new ArgumentException("The given TypeDeclaration doesn't have a TypeDefinition annotation.");
            }

            return recordComposer.CreateRecordFromType(typeDefinition);
        }
    }
}
