using System;
using System.Collections.Generic;

namespace Chip8Core {
    public sealed class Chip8Emu {
        //Chip-8 has 16 general purpose 8-bit registers,
        private const int NumberOfGeneralPurposeRegisters = 16;
        //Chip-8 allows for up to 16 levels of nested subroutines.
        private const int MaxStackDepth = 16;
        //The Chip-8 language is capable of accessing up to 4KB (4,096 bytes) of RAM, from location 0x000 (0) to 0xFFF (4095).
        private const int RAMSizeInBytes = 4096;
        //The first 512 bytes, from 0x000 to 0x1FF, are where the original interpreter was located, and should not be used by programs.
        private const ushort ProgramStart = 512;
        private const ushort ETI660ProgramStart = 1536;
        //The original implementation of the Chip-8 language used a 64x32-pixel monochrome display
        private const ushort DisplayLineHeight = 32;
        //Registers         
        private byte[] registers = new byte[NumberOfGeneralPurposeRegisters];
        //There is also a 16-bit register called I. This register is generally used to store memory addresses, so only the lowest (rightmost) 12 bits are usually used.
        private short i_register = new short();
        private IDictionary<InstructionType, Action<ushort>> InstructionTable;

        private byte delay_timer = new byte();
        private byte sound_timer = new byte();
        private byte stack_pointer = new byte();
        private ushort[] stack = new ushort[MaxStackDepth];
        private ushort program_counter = new ushort();

        private byte[] memory = new byte[RAMSizeInBytes];
        private UInt64[] display = new UInt64[DisplayLineHeight];

        private bool done = false;

        public Chip8Emu() {
            InstructionTable
        }

        private IDictionary<InstructionType, Action<ushort>> GetMethodTable() {
            return new Dictionary<InstructionType, Action<ushort>> {
                {InstructionType.SYS_addr, (arg) => SysAddr(arg) },
                {InstructionType.CLS, (arg) => ClearScreen(arg) },
                {InstructionType.RET, (arg) => ReturnFromSubRoutine(arg) },
                {InstructionType.JP_addr, (arg) => Jump(arg) },
                {InstructionType.CALL_addr, (arg) => Call(arg) },
                {InstructionType.SE_Vx_byte, (arg) => SkipIfEqual(arg) },
                {InstructionType.SNE_Vx_byte, (arg) => SkipIfNotEqual(arg) },
                {InstructionType.SE_Vx_Vy, (arg) => SkipIfEqualRegisters(arg) },
                {InstructionType.LD_Vx_byte, (arg) => LoadByte(arg) },
                {InstructionType.ADD_Vx_byte, (arg) => AddByte(arg) },
                {InstructionType.LD_Vx_Vy, (arg) => LoadRegisters(arg) },
                {InstructionType.OR_Vx_Vy, (arg) => OrRegisters(arg) },
                {InstructionType.AND_Vx_Vy, (arg) => AndRegisters(arg) },
                {InstructionType.XOR_Vx_Vy, (arg) => XorRegisters(arg) },
                {InstructionType.ADD_Vx_Vy, (arg) => AddRegisters(arg) },
                {InstructionType.SUB_Vx_Vy, (arg) => SubYFromXRegisters(arg) },
                {InstructionType.SHR_Vx_Vy, (arg) => ShrRegisters(arg) },
                {InstructionType.SUBN_Vx_Vy, (arg) => SubXFromYRegisters(arg) },
                {InstructionType.SHL_Vx_Vy, (arg) => ShlRegisters(arg) },
                {InstructionType.SE_Vx_Vy, (arg) => SkipIfEqualXY(arg) },
                {InstructionType.SE_Vx_Vy, (arg) => SkipIfEqualXY(arg) },
                {InstructionType.SE_Vx_Vy, (arg) => SkipIfEqualXY(arg) },
                {InstructionType.SE_Vx_Vy, (arg) => SkipIfEqualXY(arg) },

             };
        }

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
            ProcessInstruction();
            //decrement counters
            //increment Program counter
            //sleep?
        }

        private void ProcessInstruction() {
            Instruction instruction = GetInstruction();
            DecodeInstruction(instruction);
        }

        private void DecodeInstruction(Instruction instruction) {
            //todo: implementation
        }

        private void SysAddr(ushort arg) {
            //ignore;
        }

        private void ClearScreen(ushort argument) {
            display = new UInt64[DisplayLineHeight];
        }

        private void ReturnFromSubRoutine(ushort argument) {
            //todo: implementation
        }
        private void Call(ushort arg) {
            throw new NotImplementedException();
        }

        private void Jump(ushort arg) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the current instruction the PC points at.
        /// </summary>
        /// <returns>The current instruction the PC points at.</returns>
        /// <remarks>All instructions are 2 bytes long and are stored most-significant-byte first.</remarks>
        private Instruction GetInstruction() {
            return new Instruction((ushort)(((int)memory[program_counter] << 8) + (int)memory[program_counter + 1]));
        }
    }
}
