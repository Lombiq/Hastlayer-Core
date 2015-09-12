using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "entity " + Name + " is " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    ((Generics != null && Generics.Any() ? Generics.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) : string.Empty) +
                    
                    "port(" + vhdlGenerationOptions.NewLineIfShouldFormat() +
                        Ports
                            .ToVhdl(vhdlGenerationOptions, Terminated.Terminator(vhdlGenerationOptions))
                            .IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                    Terminated.Terminate(")", vhdlGenerationOptions) +

                    Declarations.ToVhdl(vhdlGenerationOptions))
                    .IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "end " + Name, vhdlGenerationOptions);
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
}
