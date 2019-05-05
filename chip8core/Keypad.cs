using System;

namespace Chip8Core
{
    interface IKeypad
    {
        UInt16 GetState();
    }

    public class Keypad : IKeypad
    {

        public Keypad() { }

        //public Keypad
        public ushort GetState()
        {
            
            throw new NotImplementedException();
        }
    }
}
