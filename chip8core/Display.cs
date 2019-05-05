using System.Collections;

namespace Chip8Core
{
    interface IDisplay
    {
        bool Draw(ushort x, ushort y, BitArray[] spriteData);
        void Clear();
        BitArray[] GetCurrentPixels();
    }

    public class Display : IDisplay
    {
        //The original implementation of the Chip-8 language used a 64x32-pixel monochrome display
        private readonly byte DisplayLineWidth;
        private readonly byte DisplayLineHeight;

        private BitArray[] displayData;
        
        public Display() : this(64, 32) { }

        public Display(byte width, byte heigth)
        {            
            DisplayLineWidth = width;
            DisplayLineHeight = heigth;
            displayData = new BitArray[DisplayLineHeight];
            for (int i = 0; i < DisplayLineHeight; ++i)
            {
                displayData[i] = new BitArray(DisplayLineWidth);
            }
        }

        public void Clear()
        {
            foreach (var line in displayData)
            {
                line.SetAll(false);
            }
        }

        public bool Draw(ushort x, ushort y, BitArray[] spriteData)
        {
            bool flipped = false;
            var xstartpos = (byte)(x % DisplayLineWidth);
            var ystartpos = (byte)(y % DisplayLineHeight);
            for(int i = 0; i < spriteData.Length; ++i)
            {
                var ycoord = (ystartpos + i) % DisplayLineHeight;
                for (int j = 0; j < spriteData[i].Length; ++j) {
                    var xcoord = (xstartpos + j) % DisplayLineWidth;
                    var oldval = displayData[ycoord].Get(xcoord);
                    var newval = oldval ^ spriteData[i].Get(j);
                    displayData[ycoord].Set(xcoord, newval);
                    if(!flipped && oldval && !newval)
                    {
                        flipped = true;
                    }
                }
            }
            return flipped;
        }

        public BitArray[] GetCurrentPixels() => displayData;
    }
}
