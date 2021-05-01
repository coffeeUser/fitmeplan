using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using NLog;

namespace Fitmeplan.Email.Service
{
    /// <summary>
    /// Represents an provider for using SMTP service.
    /// </summary>
    public class EmailProvider : IEmailProvider
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        private readonly SmtpClient _client;

        public string ServiceEmail { get; set; }

        public string DefaultBcc { get; set; }

        /// <summary>
        /// Creates new instance of an object <see cref="EmailProvider"/>
        /// </summary>
        public EmailProvider(string hostName, string userName, string password, int timeOut, string serviceEmail, bool enableSsl, int port)
        {
            ServiceEmail = serviceEmail;
            _client = new SmtpClient(hostName, port)
            {
                Timeout = timeOut
            };

            _client.EnableSsl = enableSsl;

            if (String.IsNullOrEmpty(userName))
            {
                _client.UseDefaultCredentials = true;
            }
            else
            {
                _client.UseDefaultCredentials = false;
                _client.Credentials = new NetworkCredential(userName, password);
            }
        }

        /// <summary>
        /// Creates new instance of an object <see cref="EmailProvider"/>.
        /// </summary>
        public EmailProvider(string hostName, int timeOut)
        {
            _client = new SmtpClient(hostName)
            {
                Timeout = timeOut,
                UseDefaultCredentials = true
            };
        }

        private string RemoveHtmlTags(string message)
        {
            return message.Replace("\r\n", "<br/>");
        }

        public bool IsBodyHtml { get; set; }

        /// <summary>
        /// Sends mail message.
        /// </summary>
        /// <param name="from">From address.</param>
        /// <param name="to">To address.</param>
        /// <param name="subject">Subject of mail.</param>
        /// <param name="body">Body of mail.</param>
        public virtual void SendMail(string from, string to, string subject, string body)
        {
            SendMail(@from, to, null, null, subject, body, null, null);
        }

        public void SendMail(string @from, string to, string cc, string bcc, string subject, string body, byte[] attachmentContent, string attachmentName)
        {
            var message = new MailMessage(String.IsNullOrEmpty(from) ? ServiceEmail : from, to, subject, IsBodyHtml ? RemoveHtmlTags(body) : body)
            {
                IsBodyHtml = true
            };
            if (attachmentContent != null && !string.IsNullOrEmpty(attachmentName))
            {
                Attachment attachment = new Attachment(new MemoryStream(attachmentContent), attachmentName);
                message.Attachments.Add(attachment);
            }
            if (!string.IsNullOrEmpty(cc))
            {
                message.CC.Add(cc);
            }
            if (!string.IsNullOrEmpty(bcc))
            {
                message.Bcc.Add(bcc);
            }
            if (!string.IsNullOrEmpty(DefaultBcc))
            {
                message.Bcc.Add(DefaultBcc);
            }

            SendMail(message);
        }

        /// <summary>
        /// Sends mail from service account.
        /// </summary>
        /// <param name="to">To address.</param>
        /// <param name="subject">Subject of mail.</param>
        /// <param name="body">Body of mail.</param>
        public void SendMail(string to, string subject, string body)
        {
            SendMail(String.Empty, to, subject, body);
        }

        /// <summary>
        /// Send mail message
        /// </summary>
        /// <param name="message">Message</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void SendMail(MailMessage message)
        {
            _log.Info($"Send email. Subject: {message.Subject}. To: {message.To.First().Address}");

            Task.Factory.StartNew(() =>
            {
                try
                {
                    //NOTE:KOV: Possible HTML format for body.
                    message.IsBodyHtml = true;
                    if (string.IsNullOrEmpty(message.From.Address))
                    {
                        message.From = new MailAddress(ServiceEmail);
                    }
                    lock (_client)
                    {
                        _client.Send(message);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Exception during Send Email");
                }
            }).Wait();
        }
    }
}
