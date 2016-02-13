﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class ConditionalSignalAssignment : IVhdlElement
    {
        private IDataObject _assignTo;
        public IDataObject AssignTo
        {
            get { return _assignTo; }
            set
            {
                if (value.DataObjectKind != DataObjectKind.Signal)
                {
                    throw new ArgumentException("The target of a conditional signal assignment should be a signal.");
                }
                _assignTo = value;
            }
        }

        public List<SignalAssignmentWhen> Whens { get; set; }


        public ConditionalSignalAssignment()
        {
            Whens = new List<SignalAssignmentWhen>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            if (Whens == null && Whens.Count < 2)
            {
                throw new InvalidOperationException("There must be at least two whens for a conditional signal assignment.");
            }

            return new Assignment
            {
                AssignTo = AssignTo,
                Expression = new Raw(Whens.ToVhdl(vhdlGenerationOptions, " else ", string.Empty))
            }.ToVhdl(vhdlGenerationOptions);
        }
    }


    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class SignalAssignmentWhen : IVhdlElement
    {
        public Value Value { get; set; }
        public IVhdlElement Expression { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Value.ToVhdl(vhdlGenerationOptions) + " when " + Expression.ToVhdl(vhdlGenerationOptions);
        }
    }
}
