using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Code { get; set; }
    }
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get;set; }
        public string Content { get;set; }
        public List<string> ImageNames { get; set; }
        public List<Promotion>? Promotions { get; set; }
        public Message(IEnumerable<string> to,string subject,string content, List<Promotion>? promotions) {
        To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress("Guest",x)));
            Subject = subject;
            Content = content;
            Promotions = promotions;
        }
    }
}
