using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Core.Models.ViewModels;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;


namespace Clean.Site.Controllers
{
    public class ContactSurfaceController : SurfaceController
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ContactSurfaceController> _logger;
        private readonly GlobalSettings _globalSettings;

        public ContactSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IEmailSender emailSender,
            ILogger<ContactSurfaceController> logger,
            IOptions<GlobalSettings> globalSettings)
            : base(umbracoContextAccessor, databaseFactory,
                  services, appCaches, profilingLogger,
                  publishedUrlProvider)
        {
            _emailSender = emailSender;
            _logger = logger;
            _globalSettings = globalSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitForm(ContactViewModel model)
        {
            if (!ModelState.IsValid) return CurrentUmbracoPage();

            TempData["Success"] = await SendEmail(model);

            return RedirectToCurrentUmbracoPage();
        }
        public async Task<Boolean> SendEmail(ContactViewModel model)
        {
        try
        {
            string senderEmail = "vladazero1@gmail.com";
            string senderPassword = "dnyhzhzfuojooaak";
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;

            MailMessage mail = new MailMessage();
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);

            mail.From = new MailAddress(senderEmail);
            mail.To.Add(model.Email);
            mail.Subject = model.Subject;
            mail.Body = model.Message;
            mail.IsBodyHtml = true;

            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtpClient.EnableSsl = true;

            smtpClient.Send(mail);

              _logger.LogInformation("Contact Form Submitted Successfully");
                return true;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error When Submitting Contact Form");
            return false;
        }
        }
    }
}
