using EmailService;
using FeelinCute.Models;
using FluentEmail.Core;
using Microsoft.AspNetCore.Mvc;

namespace FeelinCute.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailSender _emailSender;

        public EmailController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SendEmailAction(string email)
        {
            bool newUser = DbOperations.AddGuestEmail(email);
            if (newUser)
            {
                var message = new Message(new string[] { email }, "Welcome to Arion", "We have some exciting new promotions for you. Don't miss out!", null);
                _emailSender.SendEmail(message);
                return Ok(new { success = true, message = "Email has been successfully registered" });
            }
            else
            {
                return Ok(new { success = false, message = "The Email Already Exists" });
            }
        }
        
    }
}
