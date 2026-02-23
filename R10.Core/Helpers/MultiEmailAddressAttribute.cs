using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Helpers
{
    public class MultiEmailAddressAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is null)
                return true;

            var emailAddressAttribute = new EmailAddressAttribute();
            var emailAddresses = value.ToString().Replace(";", ",").Split(',');
            foreach(var emailAddress in emailAddresses)
            {
                if (!emailAddressAttribute.IsValid(emailAddress))
                    return false;
            }

            return true;
        }
    }
}
