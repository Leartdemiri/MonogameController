using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PeriphericalControl
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void StickConfig_InsideDeadZone_ReturnsZero()
        {
            var stick = new StickConfig("Test");
            stick.DeadZone = 0.2f;

            float result = stick.Apply(0.1f);

            Assert.AreEqual(0f, result, 0.0001f);
        }

        [TestMethod]
        public void Profile_Constructor_CreatesEmptyList()
        {
            var profile = new Profile();
            Assert.IsNotNull(profile.Sticks);
            Assert.AreEqual(0, profile.Sticks.Count);
        }

        [TestMethod]
        public void InputEvent_Constructor_StoresDateAndText()
        {
            DateTime now = DateTime.Now;
            string msg = "Test";

            var e = new InputEvent(now, msg);

            Assert.AreEqual(now, e.Date);
            Assert.AreEqual(msg, e.Text);
        }
    }
}
