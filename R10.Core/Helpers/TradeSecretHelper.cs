using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Configuration;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Helpers
{
    public static class TradeSecretHelper
    {
        public static char RedactedChar { get; private set; } = '█';
        public static int MinRedacted { get; private set; } = 5;
        public static int MaxRedacted { get; private set; } = 50;
        public static double RequestExpiration { get; private set; } = 60;
        public static string TimeStampFormat { get; private set; } = "yyyy-MM-ddTHH:mm:ss";
        public static int AccessTokenLength { get; private set; } = 6;
        public static int MaxValidationFailedCount { get; private set; } = 5;
        public static bool AutoApproveAdmins { get; private set; } = true;

        public static void SetTradeSecret(this IConfiguration configuration)
        {
            var tsRequestExpiry = configuration["TradeSecret:RequestExpiration"];
            if (!string.IsNullOrEmpty(tsRequestExpiry))
                RequestExpiration = Double.Parse(tsRequestExpiry);
        }

        /// <summary>
        /// Restore all or redacted trade secret
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="ts"></param>
        /// <param name="restoreAll"></param>
        public static void RestoreTradeSecret(this object dest, object ts, bool restoreAll = false)
        {
            foreach (var destProp in dest.GetType().GetProperties())
            {
                var attributes = destProp.GetCustomAttributes(typeof(TradeSecretAttribute), false);
                if (attributes != null && attributes.Any())
                {
                    var tsProp = ts.GetType().GetProperty(destProp.Name);
                    if (tsProp != null)
                    {
                        var tsVal = tsProp.GetValue(ts, null)?.ToString() ?? "";
                        var destVal = destProp.GetValue(dest, null)?.ToString() ?? "";

                        //restore redacted value in dest
                        if (destVal.StartsWith(new String(RedactedChar, MinRedacted)) || (restoreAll && !string.IsNullOrEmpty(tsVal)))
                            destProp.SetValue(dest, tsVal, null);
                    }
                }
            }
        }

        /// <summary>
        /// Create trade secret with values from source
        /// Redact trade secret data in source
        /// Returns trade secret object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="ts"></param>
        /// <returns>Trade secret object</returns>
        public static T CreateTradeSecret<T>(this object source, T ts)
        {
            var rnd = new Random();

            foreach (var sourceProp in source.GetType().GetProperties())
            {
                var attributes = sourceProp.GetCustomAttributes(typeof(TradeSecretAttribute), false);
                if (attributes != null && attributes.Any())
                {
                    var tsProp = typeof(T).GetProperty(sourceProp.Name);
                    if (tsProp != null)
                    {
                        var value = sourceProp.GetValue(source, null)?.ToString() ?? "";

                        //get value from ts if already redacted to refresh redacted string length
                        if (value.StartsWith(new String(RedactedChar, MinRedacted)))
                            value = tsProp.GetValue(ts)?.ToString() ?? "";

                        //ignore if value is empty or if already redacted
                        if (!string.IsNullOrEmpty(value) && !value.StartsWith(new String(RedactedChar, MinRedacted)))
                        {
                            //save value to ts
                            tsProp.SetValue(ts, value, null);

                            //randomize redacted char length
                            var maxRedacted = MaxRedacted;
                            var strLenAttr = sourceProp.GetCustomAttributes(typeof(StringLengthAttribute), false).Cast<StringLengthAttribute>().SingleOrDefault();
                            if (strLenAttr != null)
                                maxRedacted = strLenAttr.MaximumLength;
                            var lenRedacted = rnd.Next(MinRedacted, maxRedacted < MaxRedacted ? maxRedacted : MaxRedacted);

                            //replace source value with redacted chars
                            sourceProp.SetValue(source, new String(RedactedChar, lenRedacted));
                        }
                    }
                }
            }

            return ts;
        }
    }
}
