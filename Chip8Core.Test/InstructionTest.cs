using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chip8Core.Test
{
    [TestClass]
    public class InstructionTest
    {
        [TestMethod]
        public void JumpCorrectAdress()
        {
            var inst = new Instruction(0b0001_1010_1111_0010); // 0x1nnn == JUMP to nnn
            Assert.AreEqual(InstructionType.JP_addr, inst.Type);            
            Assert.AreEqual(2802, inst.Address);
        }

        [TestMethod]
        public void Load_CorrectXRegAndValue()
        {
            var inst = new Instruction(0b0110_0010_1111_0010); // 0x6xkk == The interpreter puts the value kk into register Vx.
            Assert.AreEqual(InstructionType.LD_Vx_byte, inst.Type);
            Assert.AreEqual(2, inst.XRegister);
            Assert.AreEqual(242, inst.KKValue);
        }

        [TestMethod]
        public void Load_CorrectXRegAndYReg()
        {
            var inst = new Instruction(0b1000_0100_1000_0000); // 0x8xy0 == Stores the value of register Vy in register Vx.
            Assert.AreEqual(InstructionType.LD_Vx_Vy, inst.Type);
            Assert.AreEqual(4, inst.XRegister);
            Assert.AreEqual(8, inst.YRegister);
        }

        [TestMethod]
        public void NibbleCorrect()
        {
            var inst = new Instruction(0b0110_1001_0101_0010); // 0x6xkk == The interpreter puts the value kk into register Vx.            
            Assert.AreEqual(2, inst.LowestNibble);            
        }

        [TestMethod]
        public void XOR_Decode_Correct()
        {                        
            var inst = new Instruction(0b_1000_0100_0010_0011); // XOR op
            Assert.AreEqual(4, inst.XRegister);
            Assert.AreEqual(2, inst.YRegister);         
        }
    }
}
