using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Identity;
using R10.Web.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
    public class CertificateRequest : BaseController
    {
        private readonly IConfiguration _configuration;

        public CertificateRequest(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return BadRequest();
        }

        public IActionResult OpenIddict(string? password, int? years)
        {
            password = string.IsNullOrEmpty(password) ? _configuration["OpenIddict:CertificatePassword"] : password;
            var validTo = DateTimeOffset.UtcNow.AddYears(years ?? 2);

            if (string.IsNullOrEmpty(password))
                return BadRequest("The password cannot be null or empty");

            OpenIddictEncryption(password, validTo);
            OpenIddictSigning(password, validTo);

            return Ok("Certificates have been successfully created.");
        }

        private void OpenIddictEncryption(string? password, DateTimeOffset validTo)
        {
            using var algorithm = RSA.Create(keySizeInBits: 2048);

            var subject = new X500DistinguishedName("CN=CPI OpenIddict Server Encryption Certificate");
            var request = new System.Security.Cryptography.X509Certificates.CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment, critical: true));

            var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, validTo);

            System.IO.File.WriteAllBytes("Resources/openiddict-encryption-certificate.pfx", certificate.Export(X509ContentType.Pfx, password));
        }

        private void OpenIddictSigning(string? password, DateTimeOffset validTo)
        {
            using var algorithm = RSA.Create(keySizeInBits: 2048);

            var subject = new X500DistinguishedName("CN=CPI OpenIddict Server Signing Certificate");
            var request = new System.Security.Cryptography.X509Certificates.CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

            var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, validTo);

            System.IO.File.WriteAllBytes("Resources/openiddict-signing-certificate.pfx", certificate.Export(X509ContentType.Pfx, password));
        }

        public IActionResult Saml2(string? password, int? years)
        {
            password = string.IsNullOrEmpty(password) ? _configuration["Authentication:Saml2:SP:ServiceCertificatePassword"] : password;
            var validTo = DateTimeOffset.UtcNow.AddYears(years ?? 2);

            if (string.IsNullOrEmpty(password))
                return BadRequest("The password cannot be null or empty");

            using var algorithm = RSA.Create(keySizeInBits: 2048);

            var subject = new X500DistinguishedName("CN=CPI SAML2 Signing Certificate");
            var request = new System.Security.Cryptography.X509Certificates.CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

            var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, validTo);
            var path = _configuration["Authentication:Saml2:SP:ServiceCertificate"] ?? "Resources/saml2-signing-certificate.pfx";

            System.IO.File.WriteAllBytes(path, certificate.Export(X509ContentType.Pfx, password));

            return Ok("Certificate has been successfully created.");
        }
    }
}