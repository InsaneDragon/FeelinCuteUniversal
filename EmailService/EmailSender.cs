using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
using System.Collections.Generic;
using System.IO;

namespace EmailService
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguraion _emailConfig;

        public EmailSender(EmailConfiguraion emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public void SendEmail(Message message)
        { 
            var builder = new BodyBuilder();
            var emailMessage = CreateEmailMessage(message, builder);
            Send(emailMessage);
        }

        public void SendPromotion(string email, Promotion promotion)
        {
            var builder = new BodyBuilder();
            var promotionMessage = CreatePromotionMessage(email, promotion, builder);
            Send(promotionMessage);
        }

        private MimeMessage CreateEmailMessage(Message message, BodyBuilder builder)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailConfig.UserName, _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = builder.ToMessageBody();
            return emailMessage;
        }

        private MimeMessage CreatePromotionMessage(string email, Promotion promotion, BodyBuilder builder)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailConfig.UserName, _emailConfig.From));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = promotion.Title;

            // Add the promotion image
            var imagePath = promotion.ImagePath;
            var image = builder.LinkedResources.Add(imagePath);
            image.ContentId = MimeUtils.GenerateMessageId();

            // Set the HTML body
            builder.HtmlBody = $@"
                <html>
                <body>
                    <h1>{promotion.Title}</h1>
                    <p>{promotion.Description}</p>
                    <img src=""cid:{image.ContentId}"" alt=""Promotion Image"" style=""width:100%;height:auto;"" />
                </body>
                </html>";

            emailMessage.Body = builder.ToMessageBody();
            return emailMessage;
        }

        private void Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.StartTls);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
                    client.Send(mailMessage);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }


        public void SendOrdersToAdminEmail(
     string purchaseId,
     string clientName,
     string clientEmail,
     string clientPhoneNumber,
     string clientAddress,
     string state,
     string aptNumber,
     string zipCode,
     string purchaseDate,
     string status,
     List<string> productIds,
     List<double> productPrices,
     List<double?> productDiscounts,
     List<int> productCounts,
     List<string> imageNames,
     string AdminEmail)
        {
            var builder = new BodyBuilder();
            var purchaseDetailsHtml = string.Empty;

            string CurrentDirectory = Directory.GetCurrentDirectory();

            for (int i = 0; i < productIds.Count; i++)
            {
                string ImagePath = Path.Combine(CurrentDirectory, "wwwroot", "images", imageNames[i]);
                var image = builder.LinkedResources.Add(ImagePath);
                image.ContentId = MimeUtils.GenerateMessageId();

                purchaseDetailsHtml += string.Format(@"
            <tr>
                <td style=""text-align: center;"">
                    <img src=""cid:{0}"" style=""width: 100px; height: auto;"">
                </td>
                <td>{1}</td>
                <td>{2}</td>
                <td>{3}</td>
                <td>{4}</td>
            </tr>", image.ContentId, productIds[i], productPrices[i], productDiscounts[i] ?? 0, productCounts[i]);
            }

            builder.HtmlBody = string.Format(@"
        <html lang=""en"">
        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f8f8f8;"">
            <div style=""max-width: 600px; margin: 0 auto; padding: 20px; background-color: #ffffff; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
                <h1 style=""color: #333333;"">New Purchase Order</h1>
                <p>Order ID: {0}</p>
                <p>Client Name: {1}</p>
                <p>Client Email: {2}</p>
                <p>Client Phone Number: {3}</p>
                <p>Client Address: {4}, {5}, {6}, {7}</p>
                <p>Purchase Date: {8}</p>
                <p>Status: {9}</p>
                <h2>Purchase Details</h2>
                <table style=""width: 100%; border-collapse: collapse;"">
                    <thead>
                        <tr>
                            <th>Purchase Image</th>
                            <th>Product ID</th>
                            <th>Product Price</th>
                            <th>Product Discount</th>
                            <th>Product Count</th>
                        </tr>
                    </thead>
                    <tbody>
                        {10}
                    </tbody>
                </table>
            </div>
        </body>
        </html>",
                purchaseId, clientName, clientEmail, clientPhoneNumber, clientAddress, state, aptNumber, zipCode, purchaseDate, status, purchaseDetailsHtml);

            var message = new Message(new string[] { AdminEmail }, "New Order", string.Empty, null);
            var emailMessage = CreateEmailMessage(message, builder);

            Send(emailMessage);
        }
    }
}
