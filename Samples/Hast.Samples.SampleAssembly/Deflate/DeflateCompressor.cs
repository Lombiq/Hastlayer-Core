using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.SampleAssembly.Deflate
{
    /// <summary>
    /// Sample demonstrating de/compression with the deflate algorithm. A port of this JS deflate/inflate implementation:
    /// https://github.com/dankogai/js-deflate (included in the js-deflate folder).
    /// </summary>
    public class DeflateCompressor
    {
        #region MemoryIndices
        public const int Deflate_InputOutputCountInt32Index = 0;
        public const int Deflate_InputOutputStartIndex = 1;
        #endregion

        #region ConfigurationConstants
        internal const int zip_WSIZE = 32768;      // Sliding Window size
        private const int zip_STORED_BLOCK = 0;
        private const int zip_STATIC_TREES = 1;
        private const int zip_DYN_TREES = 2;

        /* for deflate */
        private const int zip_DEFAULT_LEVEL = 6;
        private const bool zip_FULL_SEARCH = true;
        internal const int zip_INBUFSIZ = 32768;   // Input buffer size
        private const int zip_INBUF_EXTRA = 64;   // Extra buffer
        internal const int zip_OUTBUFSIZ = 1024 * 8;
        private const int zip_window_size = 2 * zip_WSIZE;
        private const int zip_MIN_MATCH = 3;
        internal const int zip_MAX_MATCH = 258;
        internal const int zip_BITS = 16;
        // for SMALL_MEM
        internal const int zip_LIT_BUFSIZE = 0x2000;
        internal const int zip_HASH_BITS = 13;
        // for MEDIUM_MEM
        // internal const int zip_LIT_BUFSIZE = 0x4000;
        // internal const int zip_HASH_BITS = 14;
        // for BIG_MEM
        // internal const int zip_LIT_BUFSIZE = 0x8000;
        // internal const int zip_HASH_BITS = 15;
        private const int zip_DIST_BUFSIZE = zip_LIT_BUFSIZE;
        private const int zip_HASH_SIZE = 1 << zip_HASH_BITS;
        private const int zip_HASH_MASK = zip_HASH_SIZE - 1;
        private const int zip_WMASK = zip_WSIZE - 1;
        private const int zip_NIL = 0; // Tail of hash chains
        private const int zip_TOO_FAR = 4096;
        private const int zip_MIN_LOOKAHEAD = zip_MAX_MATCH + zip_MIN_MATCH + 1;
        private const int zip_MAX_DIST = zip_WSIZE - zip_MIN_LOOKAHEAD;
        private const int zip_SMALLEST = 1;
        private const int zip_MAX_BITS = 15;
        private const int zip_MAX_BL_BITS = 7;
        private const int zip_LENGTH_CODES = 29;
        private const int zip_LITERALS = 256;
        private const int zip_END_BLOCK = 256;
        private const int zip_L_CODES = zip_LITERALS + 1 + zip_LENGTH_CODES;
        private const int zip_D_CODES = 30;
        private const int zip_BL_CODES = 19;
        private const int zip_REP_3_6 = 16;
        private const int zip_REPZ_3_10 = 17;
        private const int zip_REPZ_11_138 = 18;
        private const int zip_HEAP_SIZE = 2 * zip_L_CODES + 1;
        private const int zip_H_SHIFT = (zip_HASH_BITS + zip_MIN_MATCH - 1) / zip_MIN_MATCH;

        /* constant tables */
        private static readonly int[] zip_extra_lbits = new[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
        };
        private static readonly int[] zip_extra_dbits = new[]
        {
            0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13
        };
        private static readonly int[] zip_extra_blbits = new[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 7
        };
        private static readonly int[] zip_bl_order = new[]
        {
            16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15
        };
        private static readonly DeflateConfiguration[] zip_configuration_table = new[]
        {
            new DeflateConfiguration
            {
               good_length = 0,
               max_lazy = 0,
               nice_length = 0,
               max_chain = 0
            },
            new DeflateConfiguration
            {
               good_length = 4,
               max_lazy = 4,
               nice_length = 8,
               max_chain = 4
            },
            new DeflateConfiguration
            {
               good_length = 4,
               max_lazy = 5,
               nice_length = 16,
               max_chain = 8
            },
            new DeflateConfiguration
            {
               good_length = 4,
               max_lazy = 6,
               nice_length = 32,
               max_chain = 32
            },
            new DeflateConfiguration
            {
               good_length = 4,
               max_lazy = 4,
               nice_length = 16,
               max_chain = 16
            },
            new DeflateConfiguration
            {
               good_length = 8,
               max_lazy = 16,
               nice_length = 32,
               max_chain = 32
            },
            new DeflateConfiguration
            {
               good_length = 8,
               max_lazy = 16,
               nice_length = 128,
               max_chain = 128
            },
            new DeflateConfiguration
            {
               good_length = 8,
               max_lazy = 32,
               nice_length = 128,
               max_chain = 256
            },
            new DeflateConfiguration
            {
               good_length = 32,
               max_lazy = 128,
               nice_length = 258,
               max_chain = 1024
            },
            new DeflateConfiguration
            {
               good_length = 32,
               max_lazy = 258,
               nice_length = 258,
               max_chain = 4096
            }
        };
        #endregion


        public virtual void Deflate(SimpleMemory memory)
        {
            var inputCount = memory.ReadInt32(Deflate_InputOutputCountInt32Index);

            var z = zip_configuration_table[3];
        }
    }
}
