using System;
using Ngol.Hytek;
using Ngol.Utilities.Reflection.Extensions;
using NUnit.Framework;

namespace Ngol.Hytek.Tests
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void TestFormatTime()
        {
            Assert.AreEqual("30:58.60", typeof(HytekFormatter).InvokeMethod("FormatTime", 1858.6));
            Assert.AreEqual("25:02.00", typeof(HytekFormatter).InvokeMethod("FormatTime", 25 * 60 + 2));
            Assert.AreEqual("25:02.10", typeof(HytekFormatter).InvokeMethod("FormatTime", 25 * 60 + 2.1));
        }
    }
}
