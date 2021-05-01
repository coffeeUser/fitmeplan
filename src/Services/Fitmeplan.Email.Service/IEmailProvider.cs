using System.Net.Mail;

namespace Fitmeplan.Email.Service
{
    /// <summary>
    ///  /// <summary>
    /// Represents an interface for using SMTP service.
    /// </summary>
    /// </summary>
    public interface IEmailProvider
    {
        bool IsBodyHtml { get; set; }

        /// <summary>
        /// Sends mail message.
        /// </summary>
        /// <param name="from">From address.</param>
        /// <param name="to">To address.</param>
        /// <param name="subject">Subject of mail.</param>
        /// <param name="body">Body of mail.</param>
        void SendMail(string from, string to, string subject, string body);

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="cc">The carbon copy.</param>
        /// <param name="bcc">The blind carbon copy.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="attachmentContent"></param>
        /// <param name="attachmentName"></param>
        void SendMail(string @from, string to, string cc, string bcc, string subject, string body, byte[] attachmentContent, string attachmentName);

        /// <summary>
        /// Sends mail from service account.
        /// </summary>
        /// <param name="to">To address.</param>
        /// <param name="subject">Subject of mail.</param>
        /// <param name="body">Body of mail.</param>
        void SendMail(string to, string subject, string body);
        /// <summary>
        /// Send mail message
        /// </summary>
        /// <param name="message">Message</param>
        void SendMail(MailMessage message);
    }
}
