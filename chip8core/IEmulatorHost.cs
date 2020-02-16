using System.Collections;

namespace Chip8Core
{
    /// <summary>
    /// To be implemented by code that will host the emulator. The host has to provide input as well as output in the form of graphics and sound.
    /// </summary>
    public interface IEmulatorHost
    {
        void UpdateDisplay(BitArray[] pixels);
    }
}
