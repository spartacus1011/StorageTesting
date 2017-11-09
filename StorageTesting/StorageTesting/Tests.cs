using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace StorageTesting
{
    //Use these guys to make speed tests abit easier
    [TestFixture]
    class Tests
    {
        private MainWindowViewModel mainWindow;
        [SetUp]
        public void Setup()
        {
            mainWindow = new MainWindowViewModel();
        }

        [Test]
        public void testMethod1()
        {
        }
    }
}
