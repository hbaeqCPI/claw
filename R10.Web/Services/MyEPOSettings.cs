using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class EPOMailboxSettings
    {
        public string? GrantType { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? TokenUrl { get; set; }
        public string? Scope { get; set; }
        public int DiffTolerance { get; set; }
        public int SearchOption { get; set; }
        public bool IsAPIOn
        {
            get => !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret) && !string.IsNullOrEmpty(GrantType) && !string.IsNullOrEmpty(TokenUrl) && !string.IsNullOrEmpty(Scope);
        }
    }

    public class EPOOPSSettings
    {
        public string? GrantType { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? TokenUrl { get; set; }
        public string? Scope { get; set; }   
        public int MaxAttempts { get; set; }
        public string? CPIClientId { get; set; }
        public string? CPIClientSecret { get; set; }
        public bool IsAPIOn
        {
            get => !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret) && !string.IsNullOrEmpty(GrantType) && !string.IsNullOrEmpty(TokenUrl) && !string.IsNullOrEmpty(Scope) && !string.IsNullOrEmpty(CPIClientId) && !string.IsNullOrEmpty(CPIClientSecret);
        }
    }
}
