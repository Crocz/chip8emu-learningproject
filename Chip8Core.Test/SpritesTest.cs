using Chip8Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chip8Core.Test
{
    [TestClass]
    public class SpritesTest
    {
        [TestMethod]
        public void SpriteDataToBitArrayWorks()
        {
            var result = Sprites.AsBitArray(Sprite._0);
            Assert.AreEqual(5, result.Length);
            CollectionAssert.AreEqual(new[] { true, true, true, true, false, false, false, false }, result[0]);
            CollectionAssert.AreEqual(new[] { true, false, false, true, false, false, false, false }, result[1]);
            CollectionAssert.AreEqual(new[] { true, false, false, true, false, false, false, false }, result[2]);
            CollectionAssert.AreEqual(new[] { true, false, false, true, false, false, false, false }, result[3]);
            CollectionAssert.AreEqual(new[] { true, true, true, true, false, false, false, false }, result[4]);
        }
    }
}
