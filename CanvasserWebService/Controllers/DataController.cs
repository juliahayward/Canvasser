using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CanvasserWebService.Controllers
{
    public class DataController : Controller
    {
        [HttpGet]
        public ActionResult Test()
        {
            return new ContentResult() { Content = "test" };
        }

        [HttpPost]
        public ActionResult Upload()
        {
            try
            {
                foreach (string filename in Request.Files)
                {
                    var file = Request.Files[filename];
                    if (file.ContentLength > 0)
                    {
                        var receivedFolder = Server.MapPath(@"..\Received");
                        using (var fs = new FileStream(
                            receivedFolder + "\\" + file.FileName, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            file.InputStream.CopyTo(fs);
                            fs.Flush();
                        }
                        ConfirmByMail(file.FileName);
                    }
                }
                return new EmptyResult();
            }
            catch (Exception e)
            {
                return new ContentResult() { Content = e.Message + " " + e.StackTrace };
            }
        }


        private void ConfirmByMail(string filename)
        {
            var client = new SmtpClient();
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("**REDACTEDSmtpId**", "**REDACTEDSmtpPassword**");
            client.Host = "**REDACTEDSmtpServer**";
            client.Port = 587;
            var message = new MailMessage("**REDACTEDSmtpId**", "**REDACTEDSmtpId**");
            message.Subject = "File upload notification";
            message.Body = "You have received the following file from Canvasser:"
                    + Environment.NewLine + Environment.NewLine
                    + filename;
            client.Send(message);
        }
    }
}
