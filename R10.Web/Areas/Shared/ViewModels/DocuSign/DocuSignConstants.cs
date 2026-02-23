using R10.Core.DTOs;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public enum DocuSignRecipientStatus
    {
        [Display(Name ="AutoResponsed", Description = "The recipient’s email system auto-responded to the email from DocuSign. This status is used in the DocuSign web app (also known as the DocuSign console) to inform senders about the auto-responded email.")]
        autoresponded,

        [Display(Name ="Completed", Description = "The recipient has completed their actions (signing or other required actions if not a signer) for an envelope.")]
        completed,

        [Display(Name ="Created", Description = "The recipient's envelope is in a draft state (the envelope status is created).")]
        created,

        [Display(Name ="Declined", Description = "The recipient declined to sign the document(s) in the envelope.")]
        declined,

        [Display(Name ="Delivered", Description = "The recipient has viewed the document(s) in an envelope through the DocuSign signing website. This does not indicate an email delivery of the documents in an envelope.")]
        delivered,

        [Display(Name ="FaxPending", Description = "The recipient has finished signing, and the system is awaiting a fax attachment by the recipient before completing their signing step.")]
        faxpending,

        [Display(Name ="Sent", Description = "The recipient has been sent an email notification that it is their turn to sign an envelope.")]
        sent,

        [Display(Name ="Signed", Description = "The recipient has completed (performed all required interactions, such as signing or entering data) all required tags in an envelope. This is a temporary state during processing, after which the recipient status is automatically updated to completed.")]
        signed,
    }

    public static class DocuSignRecipientStatusExtensions
    {
        public static LookupDescDTO GetStatusAttributes(this DocuSignRecipientStatus val)
        {            
            DisplayAttribute[] displayAttributes = (DisplayAttribute[])val
                   .GetType()
                   .GetField(val.ToString())
                   .GetCustomAttributes(typeof(DisplayAttribute), false);

            var result = new LookupDescDTO();

            if (displayAttributes.Length > 0)
            {
                result.Text = displayAttributes[0].Name;
                result.Value = displayAttributes[0].Name;
                result.Description = displayAttributes[0].Description;
            }
            
            return result;
        }
    }

    public enum DocuSignEnvelopeStatus
    {
        [Display(Name = "AuthoritativeCopy", Description = "The envelope is in an authoritative state. Only copy views of the documents will be shown.")]
        authoritativecopy,

        [Display(Name = "Completed", Description = "The envelope has been completed by all the recipients.")]
        completed,

        [Display(Name = "Correct", Description = "The envelope has been opened by the sender for correction. The signing process is stopped for envelopes with this status.")]
        correct,

        [Display(Name = "Created", Description = "The envelope is in a draft state and has not been sent for signing.")]
        created,

        [Display(Name = "Declined", Description = "The envelope has been declined for signing by one of the recipients.")]
        declined,

        [Display(Name = "Deleted", Description = "This is a legacy status and is no longer used.")]
        deleted,

        [Display(Name = "Delivered", Description = "All recipients have viewed the document(s) in an envelope through the DocuSign signing website. This does not indicate an email delivery of the documents in an envelope.")]
        delivered,

        [Display(Name = "Sent", Description = "An email notification with a link to the envelope has been sent to at least one recipient. The envelope remains in this state until all recipients have viewed it at a minimum.")]
        sent,

        [Display(Name = "Signed", Description = "The envelope has been signed by all the recipients. This is a temporary state during processing, after which the envelope is automatically moved to completed status.")]
        signed,

        [Display(Name = "Template", Description = "The envelope is a template.")]
        template,

        [Display(Name = "Timedout", Description = "This is a legacy status and is no longer used.")]
        timedout,

        [Display(Name = "Transfercompleted", Description = "The envelope has been transferred out of DocuSign to another authority.")]
        transfercompleted,

        [Display(Name = "Voided", Description = "The envelope has been voided by the sender or has expired. The void reason indicates whether the envelope was manually voided or expired.")]
        voided,
    }

    public static class DocuSignEnvelopeStatusExtensions
    {
        public static LookupDescDTO GetStatusAttributes(this DocuSignEnvelopeStatus val)
        {
            DisplayAttribute[] displayAttributes = (DisplayAttribute[])val
                   .GetType()
                   .GetField(val.ToString())
                   .GetCustomAttributes(typeof(DisplayAttribute), false);

            var result = new LookupDescDTO();

            if (displayAttributes.Length > 0)
            {
                result.Text = displayAttributes[0].Name;
                result.Value = displayAttributes[0].Name;
                result.Description = displayAttributes[0].Description;
            }

            return result;
        }
    }

}
