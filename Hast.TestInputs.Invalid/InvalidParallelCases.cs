using System.Threading.Tasks;

namespace Hast.TestInputs.Invalid
{
    public class InvalidParallelCases
    {
        public void InvalidExternalVariableAssignment(uint input)
        {
            var task = Task.Factory.StartNew(
                () =>
                {
                    input = 4;
                    return input == 5;
                });
        }
    }
}
