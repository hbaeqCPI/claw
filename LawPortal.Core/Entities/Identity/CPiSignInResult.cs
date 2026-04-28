using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Core.Identity
{
    public class CPiSignInResult : SignInResult
    {
        private static readonly CPiSignInResult _success = new CPiSignInResult { Succeeded = true };
        private static readonly CPiSignInResult _passwordChangeRequired = new CPiSignInResult { IsNotAllowed = true, RequiresPasswordChange = true };
        private static readonly CPiSignInResult _confirmedEmailRequired = new CPiSignInResult { IsNotAllowed = true, RequiresConfirmedEmail = true };
        private static readonly CPiSignInResult _disabled = new CPiSignInResult { IsNotAllowed = true, IsDisabled = true };
        private static readonly CPiSignInResult _inactive = new CPiSignInResult { IsNotAllowed = true, IsInactive = true };
        private static readonly CPiSignInResult _pending = new CPiSignInResult { IsNotAllowed = true, IsPending = true };
        private static readonly CPiSignInResult _notAllowed = new CPiSignInResult { IsNotAllowed = true };
        private static readonly CPiSignInResult _externalLoginOnly = new CPiSignInResult { IsNotAllowed = true, RequiresExternalLogin = true };

        public bool RequiresPasswordChange { get; set; }
        public bool RequiresConfirmedEmail { get; set; }
        public bool RequiresExternalLogin { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsInactive { get; set; }
        public bool IsPending { get; set; }

        public static CPiSignInResult PasswordChangeRequired => _passwordChangeRequired;
        public static CPiSignInResult ConfirmedEmailRequired => _confirmedEmailRequired;
        public static CPiSignInResult Disabled => _disabled;
        public static CPiSignInResult Inactive => _inactive;
        public static CPiSignInResult Pending => _pending;
        public static new CPiSignInResult NotAllowed => _notAllowed;
        public static new CPiSignInResult Success => _success;
        public static CPiSignInResult ExternalLoginOnly => _externalLoginOnly;
    }
}
