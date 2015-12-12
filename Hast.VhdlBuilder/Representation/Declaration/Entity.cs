using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Entity : INamedElement, IDeclarableElement
    {
        private string _name;

        /// <summary>
        /// Gets or sets the name of the VHDL Entity. Keep in mind that Entity names can't be extended identifiers thus they can only contain
        /// alphanumerical characters.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (!Regex.IsMatch(value, "^[a-z0-9]*$", RegexOptions.IgnoreCase))
                {
                    throw new ArgumentException("VHDL Entity names can only contain alphanumerical characters.");
                }

                _name = value;
            }
        }

        public List<Generic> Generics { get; set; }
        public List<Port> Ports { get; set; }
        public List<IVhdlElement> Declarations { get; set; }


        public Entity()
        {
            Generics = new List<Generic>();
            Declarations = new List<IVhdlElement>();
            Ports = new List<Port>();
        }


        public string ToVhdl()
        {
            return
                "entity " +
                Name +
                " is " +
                (Generics != null && Generics.Any() ? Generics.ToVhdl() : string.Empty) +
                "port(" +
                string.Join("; ", Ports.Select(parameter => parameter.ToVhdl())) +
                ");" +
                Declarations.ToVhdl() +
                "end " +
                Name +
                ";";
        }


        /// <summary>
        /// Converts a string to be a safe Entity name, i.e. strips and substitutes everything not suited.
        /// </summary>
        /// <param name="name">The unsafe name to convert.</param>
        /// <returns>The cleaned name.</returns>
        public static string ToSafeEntityName(string name)
        {
            return Regex.Replace(name, "[^a-z0-9]", "I", RegexOptions.IgnoreCase);
        }
    }


    public enum PortMode
    {
        In,
        Out,
        Buffer,
        InOut
    }


    public class Port : TypedDataObjectBase
    {
        public PortMode Mode { get; set; }

        public Port()
        {
            DataObjectKind = DataObjectKind.Signal;
        }

        public override string ToVhdl()
        {
            return
                Name +
                ": " +
                Mode +
                " " +
                DataType.ToVhdl();
        }
    }
}
