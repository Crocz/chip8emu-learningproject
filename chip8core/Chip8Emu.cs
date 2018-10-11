using System;

namespace chip8core {
    public sealed class Chip8Emu {
        //Chip-8 has 16 general purpose 8-bit registers,
        private const int NumberOfGeneralPurposeRegisters = 16;
        //Chip-8 allows for up to 16 levels of nested subroutines.
        private const int MaxStackDepth = 16;
        //The Chip-8 language is capable of accessing up to 4KB (4,096 bytes) of RAM, from location 0x000 (0) to 0xFFF (4095).
        private const int RAMSizeInBytes = 4096;
        //The first 512 bytes, from 0x000 to 0x1FF, are where the original interpreter was located, and should not be used by programs.
        private const short ProgramStart = 512;
        private const short ETI660ProgramStart = 1536;

        //Registers         
        private byte[] registers = new byte[NumberOfGeneralPurposeRegisters];
        //There is also a 16-bit register called I. This register is generally used to store memory addresses, so only the lowest (rightmost) 12 bits are usually used.
        private short i_register = new short();

        private byte delay_timer = new byte();
        private byte sound_timer = new byte();
        private byte stack_pointer = new byte();
        private short[] stack = new short[MaxStackDepth];
        private short program_counter = new short();

        private byte[] memory = new byte[RAMSizeInBytes];

        private bool done = false;

        public int Run(byte[] rom) {
            //determine which speed to run rom at
            //determine if ETI 660 computer
            Load(rom);
            program_counter = ProgramStart;
            while (!done) {
                EmulateStep();
            }
            return 0;
        }

        private void Load(byte[] rom) {
            Buffer.BlockCopy(rom, 0, memory, ProgramStart, rom.Length);
        }

        private void EmulateStep() {
            //process current instuction
            //decrement counters
            //increment Program counter
            //sleep?
        }


    }
}
