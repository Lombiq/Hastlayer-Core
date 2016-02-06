using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Constants;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.ArchitectureComponentBuilding
{
    public class ArchitectureComponent : IArchitectureComponent
    {
        public string Name { get; private set; }
        public IList<Variable> LocalVariables { get; private set; }
        public IList<Variable> GlobalVariables { get; private set; }
        public IList<Signal> Signals { get; private set; }
        public IDictionary<string, int> OtherMemberMaxCallInstanceCounts { get; set; }


        public ArchitectureComponent(string name)
        {
            Name = name;

            LocalVariables = new List<Variable>();
            GlobalVariables = new List<Variable>();
            Signals = new List<Signal>();
            OtherMemberMaxCallInstanceCounts = new Dictionary<string, int>();
        }


        public IBlockElement BuildDeclarationsBlock(IVhdlElement beginWith = null, IVhdlElement endWith = null)
        {
            var declarationsBlock = new LogicalBlock(new LineComment(Name + " declarations start"));

            if (beginWith != null)
            {
                declarationsBlock.Add(beginWith);
            }

            if (Signals.Any())
            {
                declarationsBlock.Add(new LineComment("Signals:"));
            }
            declarationsBlock.Body.AddRange(Signals);

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

            return declarationsBlock;
        }

        public Process BuildProcess(IVhdlElement notInReset, IVhdlElement inReset = null)
        {
            var process = new Process { Name = Name };

            process.Declarations = LocalVariables.Cast<IVhdlElement>().ToList();

            var ifInResetBlock = new InlineBlock(new LineComment("Synchronous reset"));

            // Re-setting all signals and variables to their initial value.
            ifInResetBlock.Body.AddRange(Signals
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
                    Operator = Operator.Equality,
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
