using Canvasser.Schema;
using Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanvasserTests
{
    [TestClass]
    public class ExcelNumberListReaderTests
    {
        [TestMethod]
        public void ReadTestFile()
        {
            var path = "Test Data/TestNumberList.xlsx";
            var reader = new ExcelNumberReader();
            var result = reader.Read(path);

            Assert.AreEqual(result.Count(), 1174);
            var elt = result.First();
            Assert.AreEqual(elt.PD, "ER");
            Assert.AreEqual(elt.PN, 9);
            Assert.AreEqual(elt.PNs, 0);
            elt = result.ElementAt(37);
            Assert.AreEqual(elt.PD, "ER");
            Assert.AreEqual(elt.PN, 140);
            Assert.AreEqual(elt.PNs, 2);
        }
    }

    
}
