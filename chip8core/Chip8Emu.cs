using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Chip8Core
{
    public sealed class Chip8Emu {
        private readonly IEmulatorHost Host;

        private int newKeysPressed; //incoming keypresses
        private int newKeysReleased; //incoming keyreleases

        private int keysHeld; //keys held this emustep        

        private static readonly int chip8CpuClockSpeedHz = 540;
        private static readonly int timerSpeedHz = 60;
        private static readonly Stopwatch stopwatch = new Stopwatch();
        private static int ticks = 0;
        private int cpuTicksSinceLastTimerDecrease;

        //Chip-8 has 16 general purpose 8-bit registers,
        private const byte NumberOfGeneralPurposeRegisters = 16;
        //Chip-8 allows for up to 16 levels of nested subroutines.
        private const int MaxStackDepth = 16;
        //The Chip-8 language is capable of accessing up to 4KB (4,096 bytes) of RAM, from location 0x000 (0) to 0xFFF (4095).
        private const int RAMSizeInBytes = 4096;
        //The first 512 bytes, from 0x000 to 0x1FF, are where the original interpreter was located, and should not be used by programs.
        private const ushort ProgramStart = 512;
        private const ushort ETI660ProgramStart = 1536; //TODO: Support ETI660
        
        //Registers         
        private byte[] registers = new byte[NumberOfGeneralPurposeRegisters];
        //There is also a 16-bit register called I. This register is generally used to store memory addresses, so only the lowest (rightmost) 12 bits are usually used.
        private ushort i_register = new ushort();
        private readonly IReadOnlyDictionary<InstructionType, Action<Instruction>> InstructionTable;
        private Random RandomSource = new Random(0); //todo: consider seed value for better randomization

        //We should store the sprites somewhere in the interpreter area; 0x000 to 0x200 (512 decimal)
        private int spriteStorageStart = 0;
        private const int spriteSizeInBytes = 5;

        private byte delay_timer = new byte();
        private byte sound_timer = new byte();
        private byte stack_pointer = new byte();
        private ushort[] stack = new ushort[MaxStackDepth];
        private ushort program_counter = new ushort();

        private byte[] memory = new byte[RAMSizeInBytes];
        private IDisplay display = new Display();

        private bool done = false;

        public Chip8Emu(IEmulatorHost host) {            
            InstructionTable = GetInstructionTable();
            Host = host;
        }

        private void LoadSpritesIntoMemory()
        {
            int i = 0;
            foreach(byte[] sprite in Sprites.List) {
                Buffer.BlockCopy(sprite, 0, memory, spriteStorageStart + spriteSizeInBytes * i, spriteSizeInBytes);                
                i++;
            }
        }

        public int Run(byte[] rom) {
            //determine which speed to run rom at
            //determine if ETI 660 computer
            LoadSpritesIntoMemory();
            Load(rom);
            program_counter = ProgramStart;
            stopwatch.Start();
            while (!done) {
                EmulateStep();
            }
            stopwatch.Stop();
            return 0;
        }

        private void Load(byte[] rom) {
            Buffer.BlockCopy(rom, 0, memory, ProgramStart, rom.Length);
        }

        private void EmulateStep() {
            ticks++;
            ReadInput();                                    
            ManipulateCounters();
            ProcessInstruction();

            //sleep if ahead
            while(ticks > (stopwatch.Elapsed.TotalSeconds * chip8CpuClockSpeedHz))
            {
                Task.Delay(10).Wait();
            }
        }

        /// <summary>
        /// Update keypresses this emustep with incoming keypresses, and reset incoming keypresses.
        /// </summary>
        private void ReadInput()
        {            
            keysHeld |= newKeysPressed;
            keysHeld &= ~newKeysReleased;            
            newKeysPressed = 0;
            newKeysReleased = 0;
        }

        private void ManipulateCounters()
        {            
            cpuTicksSinceLastTimerDecrease++;
            if(cpuTicksSinceLastTimerDecrease >= (chip8CpuClockSpeedHz / timerSpeedHz))
            {
                cpuTicksSinceLastTimerDecrease = 0;
                if(delay_timer > 0)
                {
                    delay_timer--;
                }
                if(sound_timer > 0)
                {
                    sound_timer--;
                }
            }
            
        }

        public void OnKeyPress(KeyMasks key)
        {            
            newKeysPressed |= (int)key;            
        }

        public void OnKeyRelease(KeyMasks key)
        {            
            newKeysReleased |= (int)key;         
        }

        private void IncreaseProgramCounter()
        {
            program_counter += 2;
        }

        private void ProcessInstruction() {
            Instruction instruction = GetInstruction();
            InstructionTable[instruction.Type].Invoke(instruction);
            if(instruction.Type != InstructionType.JP_addr && instruction.Type != InstructionType.JP_V0_addr && instruction.Type != InstructionType.CALL_addr)
            {
                IncreaseProgramCounter();
            }
        }

        private void SysAddr(Instruction arg) {
            //ignored
            return;
        }

        private void ClearScreen(Instruction argument) {
            display.Clear();
            Host.UpdateDisplay(display.GetCurrentPixels());
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
            while(newKeysPressed == 0)
            {
                Task.Delay(10).Wait();
            }
            registers[arg.XRegister] = DetermineKeyPressValue(newKeysPressed);
        }

        /// <summary>
        /// Returns the value of the first key pressed, going from 0 upwards. Cannot press half a key.
        /// </summary>
        /// <returns>Value [0-15]</returns>
        private byte DetermineKeyPressValue(int keysPressed)
        {
            if(keysPressed == 0)
            {
                throw new ArgumentException("Must be at least one pressed key");
            }
            byte x = 0;
            while(((keysPressed >> x) & 0b00000000_00000000_00000000_00000001) == 0)
            {
                x++;
            }
            return x;
        }

        private void LoadDelayTimer(Instruction arg) {
            registers[arg.XRegister] = delay_timer;
        }

        private void SkipIfNotPressed(Instruction arg) {
            var mask = Keypad.ByteToKeymask(registers[arg.XRegister]);
            if((keysHeld & (int)mask) == 0)
            {
                IncreaseProgramCounter();
            }
        }

        private void SkipIfPressed(Instruction arg) {
            var mask = Keypad.ByteToKeymask(registers[arg.XRegister]);
            if ((keysHeld & (int)mask) != 0)
            {
                IncreaseProgramCounter();
            }
        }
        
        private void Draw(Instruction arg) {            
            byte[] sprite = new byte[arg.LowestNibble];
            Buffer.BlockCopy(memory, i_register, sprite, 0, arg.LowestNibble);
            var sprajt = sprite.Select(b => new BitArray(new[] { b })).ToArray();
            foreach(BitArray b in sprajt)
            {
                Reverse(b);
            }
            registers[15] = display.Draw(registers[arg.XRegister], registers[arg.YRegister], sprajt) ? (byte)1 : (byte)0;
            Host.UpdateDisplay(display.GetCurrentPixels());
        }

        private void Reverse(BitArray array)
        {
            int length = array.Length;
            int mid = (length / 2);

            for (int i = 0; i < mid; i++)
            {
                bool bit = array[i];
                array[i] = array[length - i - 1];
                array[length - i - 1] = bit;
            }
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
            if (registers[arg.XRegister] != registers[arg.YRegister])
            {
                IncreaseProgramCounter();
            }
        }

        private void SkipIfEqualRegisters(Instruction arg) {
            if(registers[arg.XRegister] == registers[arg.YRegister])
            {
                IncreaseProgramCounter();
            }
        }

        private void SkipIfNotEqual(Instruction arg) {
            if(registers[arg.XRegister] != arg.KKValue)
            {
                IncreaseProgramCounter();
            }
        }

        private void SkipIfEqual(Instruction arg) {
            if(registers[arg.XRegister] == arg.KKValue)
            {
                IncreaseProgramCounter();
            }
        }

        private void LdVxI(Instruction arg) {
            for(int i = 0; i <= arg.XRegister; ++i)
            {
                registers[i] = memory[i_register + i];
            }            
        }

        private void LdIVx(Instruction arg) {
            for (int i = 0; i <= arg.XRegister; ++i)
            {
                memory[i_register + i] = registers[i];
            }
        }

        private void LdBVx(Instruction arg) {
            int value = registers[arg.XRegister];
            memory[i_register] = (byte)(value / (byte)100);
            value = value % 100;
            memory[i_register + 1] = (byte)(value / (byte)10);
            value = value % 10;
            memory[i_register + 2] = (byte)value;
        }

        private void LdFVx(Instruction arg) {
            i_register = (ushort)(spriteStorageStart + spriteSizeInBytes * registers[arg.XRegister]);
        }

        /// <summary>
        /// Returns the current instruction the PC points at.
        /// </summary>
        /// <returns>The current instruction the PC points at.</returns>
        /// <remarks>All instructions are 2 bytes long and are stored most-significant-byte first.</remarks>
        private Instruction GetInstruction() {
            return new Instruction((ushort)(((int)memory[program_counter] << 8) + (int)memory[program_counter + 1]));
        }

        private IReadOnlyDictionary<InstructionType, Action<Instruction>> GetInstructionTable()
        {
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
    }
}
