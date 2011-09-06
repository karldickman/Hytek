using System;
using Ngol.Hytek;
using NUnit.Framework;

namespace Ngol.Hytek.Tests
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void TestFormatTime()
        {
            Assert.AreEqual("25:02.10", HytekFormatter.FormatTime(25 * 60 + 2.1));
        }
    }
}
