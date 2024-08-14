using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using EmailService;
using FeelinCute.Controllers;
using Humanizer;
using MimeKit;

namespace EmailService
{
    public class ScheduledEmailService : BackgroundService
    {
        private readonly IEmailSender _emailSender;

        public ScheduledEmailService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                try
                {
                    // Step 1: Fetch all users with emails and IDs
                    var users = DbOperations.GetUsersEmailsAndIds();

                    // Step 2: Fetch and send one valid promotion to each user
                    foreach (var user in users)
                    {
                        var promotionToSend = DbOperations.GetOnePromotionToSend(user.Id);
                        List<string> mailboxAddress = new List<string> { user.Email };
                        if (promotionToSend != null)
                        {
                             _emailSender.SendPromotion(user.Email,promotionToSend);
                          DbOperations.RecordPromotionSent(user.Id, promotionToSend.Id);
                        }
                        else
                        {
                            Console.WriteLine($"No valid promotion found to send to user {user.Email}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions as needed
                    Console.WriteLine($"Error occurred: {ex.Message}");
                }
                // Delay until the next run time (e.g., daily execution at a specific time)
                var nextRunTime = new DateTime(now.Year, now.Month, now.Day, 11, 41, 0); // Example: 12:00 PM
                if (now > nextRunTime)
                {
                    nextRunTime = nextRunTime.AddDays(1);
                }
                var delay = nextRunTime - now;
                await Task.Delay(delay, stoppingToken);
            }
        }

        // Method to record promotion sent to user
       
    }
}
