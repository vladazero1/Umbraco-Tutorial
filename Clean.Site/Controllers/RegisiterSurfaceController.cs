using System.Net;
using System.Net.Mail;
using Clean.Site.umbraco.models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace Clean.Site.Controllers
{
    public class RegisterSurfaceController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;
        private readonly ILogger<RegisterSurfaceController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly GlobalSettings _globalSettings;

        public RegisterSurfaceController(
            //these are required by the base controller
            IUmbracoContextAccessor umbracoContextAccessor, 
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services, 
            AppCaches appCaches,
            IProfilingLogger profilingLogger, 
            IPublishedUrlProvider publishedUrlProvider,
            //these are dependencies we've added
            IMemberManager memberManager,
            IMemberService memberService,
            ILogger<RegisterSurfaceController> logger,
            IEmailSender emailSender,
            IOptions<GlobalSettings> globalSettings
            ) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager ?? throw new ArgumentNullException(nameof(memberManager));
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _globalSettings = globalSettings?.Value ?? throw new ArgumentNullException(nameof(globalSettings));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var existingMember = _memberService.GetByEmail(model.Email);

            if (existingMember != null)
            {
                await SendAlreadyRegisteredEmail(model);
                _logger.LogInformation("Register: Member has already been registered");
            }
            else
            {
                if (!ModelState.IsValid) return RedirectToCurrentUmbracoPage();


                var fullName = $"{model.FirstName} {model.LastName}";

                var memberTypeAlias = CurrentPage.HasValue("memberType")
                    ? CurrentPage.Value<string>("memberType")
                    : Constants.Security.DefaultMemberTypeAlias;

                var identityUser = MemberIdentityUser.CreateNew(model.Email, model.Email, memberTypeAlias, isApproved: false, fullName);
                IdentityResult identityResult = await _memberManager.CreateAsync(
                    identityUser,
                    model.Password);

                var member = _memberService.GetByEmail(identityUser.Email);
                var token = member.Key;


                _logger.LogInformation("Register: Member created successfully");

                member.SetValue("token", token);
                member.SetValue("firstName", model.FirstName);
                member.SetValue("lastName", model.LastName);
                member.IsApproved = false;

                _memberService.Save(member);

                _memberService.AssignRoles(new[] { member.Username }, new[] {"Member"});
                TempData["Success"] = await SendEmailWithValidateToken(model,token);
            }

            return RedirectToCurrentUmbracoPage();
        }

        public async Task<bool> SendEmailWithValidateToken(RegisterViewModel model,Guid token)
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
                mail.Body = "https://localhost:44374/RegisterSurface/validate/" + token;
                mail.IsBodyHtml = true;

                smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                smtpClient.EnableSsl = true;

                smtpClient.Send(mail);

                _logger.LogInformation("Submitted Successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error When Trying To Send  Registered Email");
                return false;
            }
        }

        public async Task<bool> SendAlreadyRegisteredEmail(RegisterViewModel model)
        {
            try
            {
                var body =
                    "Somebody tried to register an account using your email address, but you are already registered.";

                var fromAddress = _globalSettings.Smtp.From;

                var subject = string.Format("Already Registered");
                var message = new EmailMessage(fromAddress, model.Email, subject, body, false);
                await _emailSender.SendAsync(message, emailType: "AlreadyRegistered");

                _logger.LogInformation("Already Registered Email Sent Successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error When Trying To Send Already Registered Email");
                return false;
            }
        }


        [HttpGet]
        public async Task<IActionResult> Validate(Guid token)
        {
            var member = _memberService.GetByKey(token);
            member.IsApproved = true;
            _memberService.Save(member);
            return RedirectToUmbracoPage(Guid.Parse("5dfe2eac-6e20-482f-9441-b973cd64fdf1"));
        }
    }
}
