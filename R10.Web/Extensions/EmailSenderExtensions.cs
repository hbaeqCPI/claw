using System.Threading.Tasks;
using R10.Core.Identity;
using R10.Web.Interfaces;

namespace R10.Web.Services
{
    public static class EmailSenderExtensions
    {
        public static async Task<EmailSenderResult> SendEmailConfirmationAsync(this IEmailSender emailSender, CPiUser user, string url)
        {
            return await emailSender.SendEmailAsync(user.Email, "Confirm your CPI account",
                $"<p>Hi {user.FirstName},</p>" +
                $"<p>Thanks for signing up. Please confirm your account by clicking the button below:</p>" +
                $"<p>" + LinkButton("Confirm Email", url) + "</p>");
        }

        public static async Task<EmailSenderResult> SendNeedsToConfirmEmailAsync(this IEmailSender emailSender, CPiUser user, string confirmEmailUrl, string forgotPasswordUrl)
        {
            return await emailSender.SendEmailAsync(user.Email, "Confirm your CPI account",
                $"<p>Hi {user.FirstName},</p>" +
                $"<p>We received a request to reset your CPI password. To proceed, please confirm your account by clicking the button below:</p>" +
                $"<p>" + LinkButton("Confirm Email", confirmEmailUrl) + "</p>" +
                $"<p>After confirming your account, please go back to the <a href=\"{forgotPasswordUrl}\">Forgot your password?</a> link to reset your password.</p>" +
                $"<p>If you did not request a password reset, you can safely ignore this email. No changes will be made to your account.</p>");
        }

        public static async Task<EmailSenderResult> SendResetPasswordLinkAsync(this IEmailSender emailSender, CPiUser user, string url)
        {
            return await emailSender.SendEmailAsync(user.Email, "CPI Password Reset",
                $"<p>Hi {user.FirstName},</p>" +
                $"<p>We received a request to reset your CPI password. To reset your password, please click the button below:</p>" +
                $"<p>" + LinkButton("Reset Password", url) + "</p>" +
                $"<p>If you did not request a password reset, you can safely ignore this email. No changes will be made to your account.</p>");
        }

        public static async Task<EmailSenderResult> SendTemporaryPassword(this IEmailSender emailSender, CPiUser user, string tempPassword, string loginUrl)
        {
            return await emailSender.SendEmailAsync(user.Email, "CPI Login Information",
                $"<p>Hi {user.FirstName},</p>" +
                $"<p>Your CPI account has been successfully setup. Please click the button below to login or copy and paste the URL to your browser's address bar:</p>" +
                $"<p>" + LinkButton("Login", loginUrl) + "</p>" +
                $"<p><strong>CPI URL:</strong> {loginUrl}<br><strong>Your temporary CPI password:</strong> {tempPassword}</p>" +
                $"<p>You will be asked to change your password after successfully logging in.</p>");
        }
        public static async Task<EmailSenderResult> SendNewPassword(this IEmailSender emailSender, CPiUser user, string tempPassword, string loginUrl)
        {
            return await emailSender.SendEmailAsync(user.Email, "CPI Login Information",
                $"<p>Hi {user.FirstName},</p>" +
                $"<p>Your CPI account has been successfully setup. Please click the button below to login or copy and paste the URL to your browser's address bar:</p>" +
                $"<p>" + LinkButton("Login", loginUrl) + "</p>" +
                $"<p><strong>CPI URL:</strong> {loginUrl}<br><strong>Your CPI password:</strong> {tempPassword}</p>");
        }

        public static string LinkButton(string label, string url, int width = 0, int height = 0)
        {
            width = (width == 0) ? 200 : width;
            height = (height == 0) ? 40 : height;

            return $"<div><!--[if mso]>" +
                $"  <v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"{url}\" style=\"height:{height}px;v-text-anchor:middle;width:{width}px;\" arcsize=\"10%\" strokecolor=\"#0078d4\" fillcolor=\"#0078d4\">" +
                $"    <w:anchorlock/>" +
                $"    <center style=\"color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:bold;\">{label}</center>" +
                $"  </v:roundrect>" +
                $"<![endif]--><a href=\"{url}\"" +
                $"style=\"background-color:#0078d4;border:1px solid #0078d4;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:bold;line-height:{height}px;text-align:center;text-decoration:none;width:{width}px;-webkit-text-size-adjust:none;mso-hide:all;\">{label}</a></div>";
        }
    }
}
