using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Chip8Core {
    public sealed class Chip8Emu {
        //Chip-8 has 16 general purpose 8-bit registers,
        private const byte NumberOfGeneralPurposeRegisters = 16;
        //Chip-8 allows for up to 16 levels of nested subroutines.
        private const int MaxStackDepth = 16;
        //The Chip-8 language is capable of accessing up to 4KB (4,096 bytes) of RAM, from location 0x000 (0) to 0xFFF (4095).
        private const int RAMSizeInBytes = 4096;
        //The first 512 bytes, from 0x000 to 0x1FF, are where the original interpreter was located, and should not be used by programs.
        private const ushort ProgramStart = 512;
        private const ushort ETI660ProgramStart = 1536;
        
        //Registers         
        private byte[] registers = new byte[NumberOfGeneralPurposeRegisters];
        //There is also a 16-bit register called I. This register is generally used to store memory addresses, so only the lowest (rightmost) 12 bits are usually used.
        private ushort i_register = new ushort();
        private readonly IReadOnlyDictionary<InstructionType, Action<Instruction>> InstructionTable;
        private Random RandomSource = new Random(0); //todo: consider seed value for better randomization

        private byte delay_timer = new byte();
        private byte sound_timer = new byte();
        private byte stack_pointer = new byte();
        private ushort[] stack = new ushort[MaxStackDepth];
        private ushort program_counter = new ushort();

        private byte[] memory = new byte[RAMSizeInBytes];
        private IDisplay display = new Display();

        private bool done = false;

        public Chip8Emu() {
            InstructionTable = GetInstructionTable();
        }

        private IReadOnlyDictionary<InstructionType, Action<Instruction>> GetInstructionTable() {
            return new Dictionary<InstructionType, Action<Instruction>> {
                {InstructionType.SYS_addr, (arg) => SysAddr(arg) },
                {InstructionType.CLS, (arg) => ClearScreen(arg) },
                {InstructionType.RET, (arg) => ReturnFromSubRoutine(arg) },
                {InstructionType.JP_addr, (arg) => Jump(arg) },
                {InstructionType.CALL_addr, (arg) => Call(arg) },
                {InstructionType.SE_Vx_byte, (arg) => SkipIfEqual(arg) },
                {InstructionType.SNE_Vx_byte, (arg) => SkipIfNotEqual(arg) },
                {InstructionType.SE_Vx_Vy, (arg) => SkipIfEqualRegisters(arg) },
                {InstructionType.SNE_Vx_Vy, (arg) => SkipIfNotEqualRegisters(arg) },
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
                {InstructionType.LD_I_addr, (arg) => LoadIAddr(arg) },
                {InstructionType.JP_V0_addr, (arg) => JumpV0(arg) },
                {InstructionType.RND_Vx_byte, (arg) => SetRandom(arg) },
                {InstructionType.DRW_Vx_Vy_nibble, (arg) => Draw(arg) },
                {InstructionType.SKP_Vx, (arg) => SkipIfPressed(arg) },
                {InstructionType.SKNP_Vx, (arg) => SkipIfNotPressed(arg) },
                {InstructionType.LD_Vx_DT, (arg) => LoadDelayTimer(arg) },
                {InstructionType.LD_Vx_K, (arg) => WaitKeyPressThenStore(arg) },
                {InstructionType.LD_DT_Vx, (arg) => SetDelayTimer(arg) },
                {InstructionType.LD_ST_Vx, (arg) => SetSoundTimer(arg) },
                {InstructionType.ADD_I_Vx, (arg) => AddI(arg) },
                {InstructionType.LD_F_Vx, (arg) => LdFVx(arg) },
                {InstructionType.LD_B_Vx, (arg) => LdBVx(arg) },
                {InstructionType.LD_I_Vx, (arg) => LdIVx(arg) },
                {InstructionType.LD_Vx_I, (arg) => LdVxI(arg) }
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
            InstructionTable[instruction.Type].Invoke(instruction);
        }

        private void SysAddr(Instruction arg) {
            //ignored
            return;
        }

        private void ClearScreen(Instruction argument) {
            display.Clear();
        }

        private void ReturnFromSubRoutine(Instruction argument) {
            if(stack_pointer == 0) {
                throw new InvalidOperationException("Attempted to return at stack depth 0");
            }
            stack_pointer--;
            Jump(stack[stack_pointer]);
        }

        private void Call(Instruction arg) {
            if (stack_pointer == MaxStackDepth) {
                throw new InvalidOperationException($"Attempted to call method at maximum depth ({MaxStackDepth}).");
            }
            stack[stack_pointer] = program_counter;
            stack_pointer++;
            Jump(arg.Address);
        }

        private void Jump(Instruction arg) {
            Jump(arg.Address);
        }

        private void Jump(ushort arg) {
            program_counter = arg;
        }

        private void AddI(Instruction arg) {
            i_register += registers[arg.XRegister];
        }

        private void SetSoundTimer(Instruction arg) {
            sound_timer = registers[arg.XRegister];
        }

        private void SetDelayTimer(Instruction arg) {
            delay_timer = registers[arg.XRegister];
        }

        private void WaitKeyPressThenStore(Instruction arg) {
            throw new NotImplementedException();
        }

        private void LoadDelayTimer(Instruction arg) {
            registers[arg.XRegister] = delay_timer;
        }

        private void SkipIfNotPressed(Instruction arg) {
            throw new NotImplementedException();
        }

        private void SkipIfPressed(Instruction arg) {
            throw new NotImplementedException();
        }

        private void Draw(Instruction arg) {

            //var spriteData = memory.Skip(i_register).Take(arg.LowestNibble).ToList();
            //for (byte i = 0; i < arg.LowestNibble; ++i)
            //{
            //    var lineData = display[ypos];
            //    for(byte x = 0; x < sizeof(byte); ++x)
            //    {
            //        lineData[xpos + x]
            //    }
            //}
           
        }

        private void SetRandom(Instruction arg) {
            registers[arg.XRegister] = (byte)((byte)RandomSource.Next(256) & arg.KKValue);
        }

        private void JumpV0(Instruction arg) {
            program_counter = (ushort)(arg.Address + registers[0]);
        }

        private void ShlRegisters(Instruction arg) {
            registers[0xF] = (registers[arg.XRegister] & 0b1000_0000) == 0b1000_0000 ? (byte)1 : (byte)0;
            registers[arg.XRegister] *= 2;
        }

        private void SubXFromYRegisters(Instruction arg) {
            registers[0xF] = registers[arg.YRegister] > registers[arg.XRegister] ? (byte)1 : (byte)0;
            registers[arg.XRegister] = (byte)(registers[arg.YRegister] - registers[arg.XRegister]);
        }

        private void ShrRegisters(Instruction arg) {
            registers[0xF] = (registers[arg.XRegister] & 0b0000_0001) == 0b0000_0001 ? (byte)1 : (byte)0;
            registers[arg.XRegister] /= 2;
        }

        private void SubYFromXRegisters(Instruction arg) {
            registers[0xF] = registers[arg.XRegister] > registers[arg.YRegister] ? (byte)1 : (byte)0;
            registers[arg.XRegister] = (byte)(registers[arg.XRegister] - registers[arg.YRegister]);
        }

        private void XorRegisters(Instruction arg) {
            registers[arg.XRegister] = (byte)(registers[arg.XRegister] ^ registers[arg.YRegister]);
        }

        private void AddRegisters(Instruction arg) {
            int temp = registers[arg.XRegister] + registers[arg.YRegister];
            if(temp > 255)
            {
                temp = temp % 256;
                registers[0xF] = 1;
            }
            else
            {
                registers[0xF] = 0;
            }
            registers[arg.XRegister] = (byte)temp;
        }

        private void AndRegisters(Instruction arg) {
            registers[arg.XRegister] = (byte)(registers[arg.XRegister] & registers[arg.YRegister]);
        }

        private void LoadIAddr(Instruction arg) {
            i_register = arg.Address;
        }

        private void OrRegisters(Instruction arg) {
            registers[arg.XRegister] = (byte)(registers[arg.XRegister] | registers[arg.YRegister]);
        }

        private void LoadRegisters(Instruction arg) {
            registers[arg.XRegister] = registers[arg.YRegister];
        }

        private void AddByte(Instruction arg) {
            registers[arg.XRegister] += arg.KKValue;
        }

        private void LoadByte(Instruction arg) {
            registers[arg.XRegister] = arg.KKValue;
        }

        private void SkipIfNotEqualRegisters(Instruction arg) {
            program_counter += registers[arg.XRegister] != registers[arg.YRegister] ? (ushort)1 : (ushort)0; //todo: maybe increase by 2?
        }

        private void SkipIfEqualRegisters(Instruction arg) {
            program_counter += registers[arg.XRegister] == registers[arg.YRegister] ? (ushort)1 : (ushort)0; //todo: maybe increase by 2?
        }

        private void SkipIfNotEqual(Instruction arg) {
            program_counter += registers[arg.XRegister] != arg.KKValue ? (ushort)1 : (ushort)0; //todo: maybe increase by 2?
        }

        private void SkipIfEqual(Instruction arg) {
            program_counter += registers[arg.XRegister] == arg.KKValue ? (ushort)1 : (ushort)0; //todo: maybe increase by 2?
        }

        private void LdVxI(Instruction arg) {
            throw new NotImplementedException();
        }

        private void LdIVx(Instruction arg) {
            throw new NotImplementedException();
        }

        private void LdBVx(Instruction arg) {
            throw new NotImplementedException();
        }

        private void LdFVx(Instruction arg) {
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
