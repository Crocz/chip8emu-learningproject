using System;
using System.Windows.Input;

namespace Chip8Core
{
    public interface IKeypad
    {
        int GetState();
    }

    public class Keypad : IKeypad
    {

        public Keypad() { }

        //public Keypad
        public int GetState()
        {
            int ret = 0;
            
            return 0;
        }

        public static KeyMasks ByteToKeymask(byte b)
        {
            switch (b)
            {
                case 0: return KeyMasks.Zero;
                case 1: return KeyMasks.One;
                case 2: return KeyMasks.Two;
                case 3: return KeyMasks.Three;
                case 4: return KeyMasks.Four;
                case 5: return KeyMasks.Five;
                case 6: return KeyMasks.Six;
                case 7: return KeyMasks.Seven;
                case 8: return KeyMasks.Eigth;
                case 9: return KeyMasks.Nine;
                case 10: return KeyMasks.A;
                case 11: return KeyMasks.B;
                case 12: return KeyMasks.C;
                case 13: return KeyMasks.D;
                case 14: return KeyMasks.E;
                case 15: return KeyMasks.F;
                default: throw new InvalidOperationException("err");
            }
        }
    }

    public class KeyStateArgs : EventArgs
    {
        public IKeypad State { get; }
        public KeyStateArgs(IKeypad newState)
        {
            State = newState;
        }
    }

    public enum KeyMasks
    {
        Zero =  0b00000000_00000000_00000000_00000001,
        One =   0b00000000_00000000_00000000_00000010,
        Two =   0b00000000_00000000_00000000_00000100,
        Three = 0b00000000_00000000_00000000_00001000,
        Four =  0b00000000_00000000_00000000_00010000,
        Five =  0b00000000_00000000_00000000_00100000,
        Six =   0b00000000_00000000_00000000_01000000,
        Seven = 0b00000000_00000000_00000000_10000000,
        Eigth = 0b00000000_00000000_00000001_00000000,
        Nine =  0b00000000_00000000_00000010_00000000,
        A =     0b00000000_00000000_00000100_00000000,
        B =     0b00000000_00000000_00001000_00000000,
        C =     0b00000000_00000000_00010000_00000000,
        D =     0b00000000_00000000_00100000_00000000,
        E =     0b00000000_00000000_01000000_00000000,
        F =     0b00000000_00000000_10000000_00000000,
    }
}
