using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    /// <summary>
    /// Stores dependency relations between VDHL types, for custom types that need this. E.g. if MyArray is an array 
    /// type that stores elements of type MyRecord then that will be stored as MyArray depending on MyRecord. This is
    /// needed because in VHDL MyArray should come after MyRecord in the code file.
    /// </summary>
    public class DependentTypesTable
    {
        private readonly Dictionary<DataType, HashSet<string>> _dependencies =
            new Dictionary<DataType, HashSet<string>>(new DataTypeEqualityComparer());


        /// <summary>
        /// Declare a dependency between two types.
        /// </summary>
        public void AddDependency(DataType type, string dependencyTypeName)
        {
            HashSet<string> dependencies;

            if (!_dependencies.TryGetValue(type, out dependencies))
            {
                dependencies = _dependencies[type] = new HashSet<string>();
            }

            dependencies.Add(dependencyTypeName);
        }

        /// <summary>
        /// Add a type that doesn't depend on any other type but other types depend on it.
        /// </summary>
        public void AddBaseType(DataType type)
        {
            AddDependency(type, null);
        }

        /// <summary>
        /// Fetch all types stored in this table.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataType> GetTypes()
        {
            return _dependencies.Keys;
        }

        /// <summary>
        /// Fetch the dependencies of the given type.
        /// </summary>
        public IEnumerable<string> GetDependencies(DataType type)
        {
            HashSet<string> dependencies;
            if (!_dependencies.TryGetValue(type, out dependencies)) return Enumerable.Empty<string>();
            return dependencies;
        }


        private class DataTypeEqualityComparer : IEqualityComparer<DataType>
        {
            public bool Equals(DataType x, DataType y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(DataType dataType)
            {
                return dataType.Name.GetHashCode();
            }
        }
    }
}
