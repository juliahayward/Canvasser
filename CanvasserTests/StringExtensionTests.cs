using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Canvasser.Extensions;

namespace CanvasserTests
{
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        public void TestToSurnameCase()
        {
            Assert.AreEqual("Foo", "foo".ToSurnameCase());
            Assert.AreEqual("Foo", "Foo".ToSurnameCase());
            Assert.AreEqual("Foo", "FOO".ToSurnameCase());
            Assert.AreEqual("Foo Bar", "FOO BAR".ToSurnameCase());
            Assert.AreEqual("Foo-Bar", "FOO-BAR".ToSurnameCase());
        }
    }
}
