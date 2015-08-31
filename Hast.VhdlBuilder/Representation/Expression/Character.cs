using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    public class Character : Value
    {
        public Character(char character)
        {
            DataType = KnownDataTypes.Character;
            Content = character.ToString();
        }
    }
}
