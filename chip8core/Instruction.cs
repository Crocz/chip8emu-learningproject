using System;
using System.Collections.Generic;
using System.Text;

namespace Chip8Core
{
    /// <summary>
    /// Chip8 instructions
    /// </summary>
    public enum Instruction
    {
        //notes: Instruction names & comments copied from: http://devernay.free.fr/hacks/chip8/C8TECH10.HTM

        /// <summary>
        /// Jump to a machine code routine at nnn. *This instruction is only used on the old computers on which Chip-8 was originally implemented. It is ignored by modern interpreters.*
        /// </summary>
        SYS_addr,
        /// <summary>
        /// Clear the display.
        /// </summary>
        CLS,
        /// <summary>
        /// Return from a subroutine.
        /// </summary>
        RET,
        /// <summary>
        /// Jump to location nnn.
        /// </summary>
        JP_addr,
        /// <summary>
        /// Call subroutine at nnn.
        /// </summary>
        CALL_addr,
        /// <summary>
        /// Skip next instruction if Vx = kk.
        /// </summary>
        SE_Vx_byte,
        /// <summary>
        /// Skip next instruction if Vx != kk.
        /// </summary>
        SNE_Vx_byte,
        /// <summary>
        /// Skip next instruction if Vx = Vy.
        /// </summary>
        SE_Vx_Vy,
        /// <summary>
        /// Set Vx = kk.
        /// </summary>
        LD_Vx_byte,
        /// <summary>
        /// Set Vx = Vx + kk.
        /// </summary>
        ADD_Vx_byte,
        /// <summary>
        /// Set Vx = Vy.
        /// </summary>
        LD_Vx_Vy,
        /// <summary>
        /// Set Vx = Vx OR Vy.
        /// </summary>
        OR_Vx_Vy,
        /// <summary>
        /// Set Vx = Vx AND Vy.
        /// </summary>
        AND_Vx_Vy,
        /// <summary>
        /// Set Vx = Vx XOR Vy.
        /// </summary>
        XOR_Vx_Vy,
        /// <summary>
        /// Set Vx = Vx + Vy, set VF = carry.
        /// </summary>
        ADD_Vx_Vy,
        /// <summary>
        /// Set Vx = Vx - Vy, set VF = NOT borrow.
        /// </summary>
        SUB_Vx_Vy,
        /// <summary>
        /// Set Vx = Vx SHR 1.
        /// </summary>
        SHR_Vx_Vy,
        /// <summary>
        /// Set Vx = Vy - Vx, set VF = NOT borrow.
        /// </summary>
        SUBN_Vx_Vy,
        /// <summary>
        /// Set Vx = Vx SHL 1.
        /// </summary>
        SHL_Vx_Vy,
        /// <summary>
        /// Skip next instruction if Vx != Vy.
        /// </summary>
        SNE_Vx_Vy,
        /// <summary>
        /// Set I = nnn.
        /// </summary>
        LD_I_addr,
        /// <summary>
        /// Jump to location nnn + V0.
        /// </summary>
        JP_V0_addr,
        /// <summary>
        /// Set Vx = random byte AND kk.
        /// </summary>
        RND_Vx_byte,
        /// <summary>
        /// Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
        /// </summary>
        DRW_Vx_Vy_nibble,
        /// <summary>
        /// Skip next instruction if key with the value of Vx is pressed.
        /// </summary>
        SKP_Vx,
        /// <summary>
        /// Skip next instruction if key with the value of Vx is not pressed.
        /// </summary>
        SKNP_Vx,
        /// <summary>
        /// Set Vx = delay timer value.
        /// </summary>
        LD_Vx_DT,
        /// <summary>
        /// Wait for a key press, store the value of the key in Vx.
        /// </summary>
        LD_Vx_K,
        /// <summary>
        /// Set delay timer = Vx.
        /// </summary>
        LD_DT_Vx,
        /// <summary>
        /// Set sound timer = Vx.
        /// </summary>
        LD_ST_Vx,
        /// <summary>
        /// Set I = I + Vx.
        /// </summary>
        ADD_I_Vx,
        /// <summary>
        /// Set I = location of sprite for digit Vx.
        /// </summary>
        LD_F_Vx,
        /// <summary>
        /// Store BCD (TODO:What?) representation of Vx in memory locations I, I+1, and I+2.
        /// </summary>
        LD_B_Vx,
        /// <summary>
        /// Store registers V0 through Vx in memory starting at location I.
        /// </summary>
        LD_I_Vx,
        /// <summary>
        /// Read registers V0 through Vx from memory starting at location I.
        /// </summary>
        LD_Vx_I,
    }
}

