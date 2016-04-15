﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Constants;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;
using Orchard.Validation;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public abstract class ArchitectureComponentBase : IArchitectureComponent
    {
        public string Name { get; private set; }
        public IList<Variable> LocalVariables { get; private set; }
        public IList<Variable> GlobalVariables { get; private set; }
        public IList<Signal> InternallyDrivenSignals { get; private set; }
        public IList<Signal> ExternallyDrivenSignals { get; private set; }
        public IDictionary<string, int> OtherMemberMaxInvokationInstanceCounts { get; private set; }


        protected ArchitectureComponentBase(string name)
        {
            Name = name;

            LocalVariables = new List<Variable>();
            GlobalVariables = new List<Variable>();
            InternallyDrivenSignals = new List<Signal>();
            ExternallyDrivenSignals = new List<Signal>();
            OtherMemberMaxInvokationInstanceCounts = new Dictionary<string, int>();
        }


        public abstract IVhdlElement BuildDeclarations();

        public abstract IVhdlElement BuildBody();


        protected IBlockElement BuildDeclarationsBlock(IVhdlElement beginWith = null, IVhdlElement endWith = null)
        {
            var declarationsBlock = new LogicalBlock(new LineComment(Name + " declarations start"));

            if (beginWith != null)
            {
                declarationsBlock.Add(beginWith);
            }

            if (InternallyDrivenSignals.Any() || ExternallyDrivenSignals.Any())
            {
                declarationsBlock.Add(new LineComment("Signals:"));
            }
            declarationsBlock.Body.AddRange(InternallyDrivenSignals.Union(ExternallyDrivenSignals));

            foreach (var variable in GlobalVariables)
            {
                variable.Shared = true;
            }
            if (GlobalVariables.Any())
            {
                declarationsBlock.Add(new LineComment("Shared (global) variables:"));
            }
            declarationsBlock.Body.AddRange(GlobalVariables);

            if (endWith != null)
            {
                declarationsBlock.Add(endWith);
            }

            declarationsBlock.Add(new LineComment(Name + " declarations end"));

            return declarationsBlock.Body.Count > 2 ? declarationsBlock : new InlineBlock();
        }

        protected Process BuildProcess(IVhdlElement notInReset, IVhdlElement inReset = null)
        {
            Argument.ThrowIfNull(notInReset, "notInReset");


            var process = new Process { Name = Name.ToExtendedVhdlId() };

            process.Declarations = LocalVariables.Cast<IVhdlElement>().ToList();

            var ifInResetBlock = new InlineBlock(new LineComment("Synchronous reset"));

            // Re-setting all internally driven signals and variables to their initial value.
            ifInResetBlock.Body.AddRange(InternallyDrivenSignals
                .Where(signal => signal.InitialValue != null)
                .Select(signal =>
                    new Assignment { AssignTo = signal.ToReference(), Expression = signal.InitialValue }));
            ifInResetBlock.Body.AddRange(LocalVariables
                .Union(GlobalVariables)
                .Where(variable => variable.InitialValue != null)
                .Select(variable =>
                    new Assignment { AssignTo = variable.ToReference(), Expression = variable.InitialValue }));

            if (inReset != null)
            {
                ifInResetBlock.Add(inReset);
            }

            var resetIf = new IfElse
            {
                Condition = new Binary
                {
                    Left = CommonPortNames.Reset.ToVhdlSignalReference(),
                    Operator = BinaryOperator.Equality,
                    Right = Value.OneCharacter
                },
                True = ifInResetBlock,
                Else = notInReset
            };
            process.Add(resetIf);

            return process;
        }
    }
}