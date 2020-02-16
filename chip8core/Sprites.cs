using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Chip8Core
{
    public enum Sprite
    {
        _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _A, _B, _C, _D, _E, _F
    }
    /// <summary>
    /// Pre-defined sprites, representing hexadecimal digits 0 through F.
    /// </summary>
    public static class Sprites
    {
        public static BitArray[] AsBitArray(Sprite s) => sprites[s].Select(line => new BitArray(new[] { line }).Reverse()).ToArray();
        public static IList<byte[]> List => GetSortedList();

        internal static readonly IReadOnlyDictionary<Sprite, byte[]> sprites = new Dictionary<Sprite, byte[]> { {        Sprite._0, new byte[] { 0b11110000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b11110000, } },

                                                                                                            {   Sprite._1, new byte[] { 0b00100000,
                                                                                                                                        0b01100000,
                                                                                                                                        0b00100000,
                                                                                                                                        0b00100000,
                                                                                                                                        0b01110000, } },

                                                                                                            {   Sprite._2, new byte[] { 0b11110000,
                                                                                                                                        0b00010000,
                                                                                                                                        0b11110000,
                                                                                                                                        0b10000000,
                                                                                                                                        0b11110000, } },

                                                                                                            {   Sprite._3, new byte[] { 0b11110000,
                                                                                                                                        0b00010000,
                                                                                                                                        0b11110000,
                                                                                                                                        0b00010000,
                                                                                                                                        0b11110000, } },

                                                                                                            {   Sprite._4, new byte[] { 0b10010000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b11110000,
                                                                                                                                        0b00010000,
                                                                                                                                        0b00010000, } },

                                                                                                            {   Sprite._5, new byte[] { 0b11110000,
                                                                                                                                        0b10000000,
                                                                                                                                        0b11110000,
                                                                                                                                        0b00010000,
                                                                                                                                        0b11110000, } },

                                                                                                            {   Sprite._6, new byte[] { 0b11110000,
                                                                                                                                        0b10000000,
                                                                                                                                        0b11110000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b11110000, } },

                                                                                                            {   Sprite._7, new byte[] { 0b11110000,
                                                                                                                                        0b00010000,
                                                                                                                                        0b00100000,
                                                                                                                                        0b01000000,
                                                                                                                                        0b01000000, } },

                                                                                                            {   Sprite._8, new byte[] { 0b11110000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b11110000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b11110000, } },

                                                                                                            {   Sprite._9, new byte[] { 0b11110000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b11110000,
                                                                                                                                        0b00010000,
                                                                                                                                        0b11110000, } },

                                                                                                            {   Sprite._A, new byte[] { 0b11110000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b11110000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b10010000, } },

                                                                                                            {   Sprite._B, new byte[] { 0b11100000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b11100000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b11100000, } },

                                                                                                            {   Sprite._C, new byte[] { 0b11110000,
                                                                                                                                        0b10000000,
                                                                                                                                        0b10000000,
                                                                                                                                        0b10000000,
                                                                                                                                        0b11110000, } },

                                                                                                            {   Sprite._D, new byte[] { 0b11100000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b10010000,
                                                                                                                                        0b11100000, } },

                                                                                                            {   Sprite._E, new byte[] { 0b11110000,
                                                                                                                                        0b10000000,
                                                                                                                                        0b11110000,
                                                                                                                                        0b10000000,
                                                                                                                                        0b11110000, } },

                                                                                                            {   Sprite._F, new byte[] { 0b11110000,
                                                                                                                                        0b10000000,
                                                                                                                                        0b11110000,
                                                                                                                                        0b10000000,
                                                                                                                                        0b10000000, } },
        };

        private static IList<byte[]> GetSortedList()
        {
            List<byte[]> retval = new List<byte[]>(sprites.Count);
            var keys = sprites.Keys.ToList();
            keys.Sort();
            foreach (var key in keys)
            {
                retval.Add(sprites[key]);
            }
            return retval;
        }

        private static BitArray Reverse(this BitArray indata)
        {
            int mid = indata.Length / 2;
            int length = indata.Length;
            for (int i = 0; i < mid; ++i)
            {
                bool bit = indata[i];
                indata[i] = indata[length - i - 1];
                indata[length - i - 1] = bit;
            }
            return indata;
        }
    }
}
