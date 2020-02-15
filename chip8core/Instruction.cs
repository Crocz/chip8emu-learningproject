using System;
using System.Collections.Generic;
using System.Text;

namespace Chip8Core {
    public class Instruction {
        public InstructionType Type { get; }
        private readonly ushort argument;
        public ushort Address => argument;
        public byte XRegister => GetXVal(argument);
        public byte YRegister => GetYVal(argument);
        public byte KKValue => GetKKVal(argument);
        public byte HighestNibble => GetHighestNibble(argument);
        public byte LowestNibble => GetLowestNibble(argument);

        public Instruction(ushort data) : this(DecodeInstruction(data), GetLow12BitsArgument(data)) { }

        public Instruction(InstructionType type, ushort arg)
        {
            Type = type;
            argument = arg;
        }

        private static InstructionType DecodeInstruction(ushort instruction) {                        
            switch (GetHighestNibble(instruction)) {
                case 0x0: //SYS, CLS, RET
                    switch (instruction) {
                        case 0x00E0: return InstructionType.CLS;
                        case 0x00EE: return InstructionType.RET;
                        default: return InstructionType.SYS_addr;
                    }
                case 0x1: //JP
                    return InstructionType.JP_addr;
                case 0x2: //CALL
                    return InstructionType.CALL_addr;
                case 0x3: //SE (Vx, byte)
                    return InstructionType.SE_Vx_byte;
                case 0x4: //SNE
                    return InstructionType.SNE_Vx_byte;
                case 0x5: //SE (Vx, Vy)
                    return InstructionType.SE_Vx_Vy;
                case 0x6: //LD (Vx, byte)
                    return InstructionType.LD_Vx_byte;
                case 0x7: //ADD (Vx, byte)
                    return InstructionType.ADD_Vx_byte;
                case 0x8: //Various Vx, Vy instructions                    
                    switch (GetLowestNibble(instruction)) {
                        case 0x0: //LD (Vx, Vy)
                            return InstructionType.LD_Vx_Vy;
                        case 0x1:
                            return InstructionType.OR_Vx_Vy;
                        case 0x2:
                            return InstructionType.AND_Vx_Vy;
                        case 0x3:
                            return InstructionType.XOR_Vx_Vy;
                        case 0x4:
                            return InstructionType.ADD_Vx_Vy;
                        case 0x5:
                            return InstructionType.SUB_Vx_Vy;
                        case 0x6:
                            return InstructionType.SHR_Vx_Vy;
                        case 0x7:
                            return InstructionType.SUBN_Vx_Vy;
                        case 0xE:
                            return InstructionType.SHL_Vx_Vy;
                        default:
                            throw new UnrecognizedInstructionException(instruction);
                    }
                case 0x9: //SNE (Vx, Vy)
                    return InstructionType.SNE_Vx_Vy;
                case 0xA: //LD I
                    return InstructionType.LD_I_addr;
                case 0xB:
                    return InstructionType.JP_V0_addr;
                case 0xC:
                    return InstructionType.RND_Vx_byte;
                case 0xD:
                    return InstructionType.DRW_Vx_Vy_nibble;
                case 0xE:
                    switch (GetLowerByte(instruction)) {
                        case 0x9E:
                            return InstructionType.SKP_Vx;
                        case 0xA1:
                            return InstructionType.SKNP_Vx;
                        default:
                            throw new UnrecognizedInstructionException(instruction);
                    }
                case 0xF:
                    switch (GetLowerByte(instruction)) {
                        case 0x07:
                            return InstructionType.LD_Vx_DT;
                        case 0x0A:
                            return InstructionType.LD_Vx_K;
                        case 0x15:
                            return InstructionType.LD_DT_Vx;
                        case 0x18:
                            return InstructionType.LD_ST_Vx;
                        case 0x1E:
                            return InstructionType.ADD_I_Vx;
                        case 0x29:
                            return InstructionType.LD_F_Vx;
                        case 0x33:
                            return InstructionType.LD_B_Vx;
                        case 0x55:
                            return InstructionType.LD_I_Vx;
                        case 0x65:
                            return InstructionType.LD_Vx_I;
                        default:
                            throw new UnrecognizedInstructionException(instruction);
                    }
                default:
                    throw new UnrecognizedInstructionException(instruction);

            }            
        }

        private static byte GetHighestNibble(ushort instruction) => (byte)(instruction >> 12);
        private static byte GetLowestNibble(ushort instruction) => (byte)(instruction & LowestNibbleMask);
        private const ushort LowestNibbleMask = 0x000F;
        private static byte GetLowerByte(ushort instruction) => (byte)(instruction & LowerByteMask);
        private const ushort LowerByteMask = 0x00FF;
        private static ushort GetLow12BitsArgument(ushort instruction) => (ushort)(instruction & Low12BitsMask);
        private const ushort Low12BitsMask = 0x0FFF;


        private static byte GetXVal(ushort arg) => (byte)((arg & 0x0F00) >> 8);

        private byte GetYVal(ushort arg) => (byte)((arg & 0x00F0) >> 4);

        private byte GetKKVal(ushort instruction) => GetLowerByte(instruction);

        private class UnrecognizedInstructionException : Exception {
            private ushort instruction;

            public UnrecognizedInstructionException(ushort instruction) {
                this.instruction = instruction;
            }

            public override string ToString() {
                return $"Unrecognized instruction: {instruction}\nLeading nibble: {GetHighestNibble(instruction)}\nTrailing nibble: {GetLowestNibble(instruction)}";
            }

        }
    }
}
