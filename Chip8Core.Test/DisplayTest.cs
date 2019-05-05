using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace Chip8Core.Test
{
    [TestClass]
    public class DisplayTest
    {
        [TestMethod]
        public void ConstructorWorkAndNewDisplayIsBlank()
        {
            var display = new Display(1, 1);
            Assert.AreEqual(1, display.GetCurrentPixels().Length);
            Assert.AreEqual(1, display.GetCurrentPixels()[0].Length);
            Assert.AreEqual(false, display.GetCurrentPixels()[0].Get(0));
        }

        [TestMethod]
        public void DrawDoesNotAffectDisplayDimensions()
        {
            byte size = 1;
            var display = new Display(size, size);
            display.Draw(0, 0, new[] { new BitArray(new[] { true }) });            
            Assert.AreEqual(size, display.GetCurrentPixels().Length);
            Assert.AreEqual(size, display.GetCurrentPixels()[0].Length);
        }

        [TestMethod]
        public void TestDraw1ToUndrawnPixel()
        {
            var display = new Display(1, 1);
            var ret = display.Draw(0, 0, new[] { new BitArray(new[] { true }) });
            Assert.IsFalse(ret);
            Assert.AreEqual(true, display.GetCurrentPixels()[0].Get(0));            
        }

        [TestMethod]
        public void TestDraw0ToUndrawnPixel()
        {
            var display = new Display(1, 1);
            var ret = display.Draw(0, 0, new[] { new BitArray(new[] { false }) });
            Assert.IsFalse(ret);
            Assert.AreEqual(false, display.GetCurrentPixels()[0].Get(0));
        }

        [TestMethod]
        public void TestDraw1ToDrawnPixel()
        {
            var display = new Display(1, 1);
            display.Draw(0, 0, new[] { new BitArray(new[] { true }) });
            var ret = display.Draw(0, 0, new[] { new BitArray(new[] { true }) });
            Assert.IsTrue(ret);
            Assert.AreEqual(false, display.GetCurrentPixels()[0].Get(0));
        }

        [TestMethod]
        public void TestDraw0ToDrawnPixel()
        {
            var display = new Display(1, 1);
            display.Draw(0, 0, new[] { new BitArray(new[] { true }) });
            var ret = display.Draw(0, 0, new[] { new BitArray(new[] { false }) });
            Assert.IsFalse(ret);
            Assert.AreEqual(true, display.GetCurrentPixels()[0].Get(0));
        }

        [TestMethod]
        public void XWrapsAround()
        {
            var display = new Display(3, 1);
            display.Draw(2, 0, new[] { new BitArray(new[] { true, true }) });                        
            Assert.AreEqual(true, display.GetCurrentPixels()[0].Get(0));
            Assert.AreEqual(false, display.GetCurrentPixels()[0].Get(1));
            Assert.AreEqual(true, display.GetCurrentPixels()[0].Get(2));
        }

        [TestMethod]
        public void YWrapsAround()
        {
            var display = new Display(1, 3);
            display.Draw(0, 2, new[] { new BitArray(new[] { true }), new BitArray(new[] { true }) });
            Assert.AreEqual(true, display.GetCurrentPixels()[0].Get(0));
            Assert.AreEqual(false, display.GetCurrentPixels()[1].Get(0));
            Assert.AreEqual(true, display.GetCurrentPixels()[2].Get(0));
        }
    }
}
