using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CanvasserTests
{
    [TestClass]
    public class FileUploadTests
    {
        [TestMethod]
        public void TestSendingAFile()
        {
            WebClient myWebClient = new WebClient();
            // Alter this depending on temporary web server port
            var uri = new Uri("**REDACTEDCanvasserTestUrl**");
            var fileName = @"D:\src\canvasser\canvassertests\test.txt";
            byte[] response = myWebClient.UploadFile(uri, "POST", fileName);
            var response1 = string.Join("", response.Select(x => (char)x));
            Assert.AreEqual(response1, "");
        }
    }
}
