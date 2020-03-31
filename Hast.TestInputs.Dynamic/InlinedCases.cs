using Hast.Transformer.Abstractions.SimpleMemory;
using System.Runtime.CompilerServices;

namespace Hast.TestInputs.Dynamic
{
    /// <summary>
    /// Test cases for inlined methods.
    /// </summary>
    /// <remarks>
    /// While inlined methods with multiple returns utilize goto statements gotos will never be produced by ILSpy 
    /// during decompilation. So we can't test goto handling with source including gotos originally.
    /// </remarks>
    public class InlinedCases
    {
        public virtual void InlinedMultiReturn(SimpleMemory memory)
        {
            memory.WriteInt32(0, InlinedMultiReturnInternal(memory.ReadInt32(0)));
        }

        public virtual void NestedInlinedMultiReturn(SimpleMemory memory)
        {
            memory.WriteInt32(0, NestedInlinedMultiReturnInternal(memory.ReadInt32(0)));
        }

        public int InlinedMultiReturn(int input)
        {
            var memory = new SimpleMemory(1);
            memory.WriteInt32(0, input);
            InlinedMultiReturn(memory);
            return memory.ReadInt32(0);
        }

        public int NestedInlinedMultiReturn(int input)
        {
            var memory = new SimpleMemory(1);
            memory.WriteInt32(0, input);
            NestedInlinedMultiReturn(memory);
            return memory.ReadInt32(0);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int NestedInlinedMultiReturnInternal(int input)
        {
            return InlinedMultiReturnInternal(input) + InlinedMultiReturnInternal(input);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int InlinedMultiReturnInternal(int input)
        {
            int output;

            if (input > 0) output = 1;
            else output = 2;

            return output;
        }
    }
}
