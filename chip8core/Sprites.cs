using System.Collections.Generic;

namespace Chip8core
{

    public enum Sprite
    {
        _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _A, _B, _C, _D, _E, _F
    }
    /// <summary>
    /// Pre-defined sprites, representing hexadecimal digits 0 through F.
    /// </summary>
    static class Sprites
    {
        static readonly IReadOnlyDictionary<Sprite, byte[]> sprites = new Dictionary<Sprite, byte[]> { {        Sprite._0, new byte[] { 0b11110000,
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
                                                                                                                                        0b11110000, } } };
    }
}
