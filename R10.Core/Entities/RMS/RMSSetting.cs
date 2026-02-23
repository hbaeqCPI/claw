using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Entities.RMS
{
    public class RMSSetting : DefaultSetting
    {
        //CHECKBOX DEFAULTS
        //instructions
        public bool IsInstrxIncludeInstructedChecked { get; set; }      //Include instructed in Instructions screen
        //portfolio review
        public bool IsPortfolioIncludeInstructedChecked { get; set; }   //Include instructed in Portfolio review screen
        //action closing
        public bool IsConfirmChecked { get; set; }      //Confirmation letter
        public bool IsAgentRespChecked { get; set; }    //Agent responsibility letter
        public bool IsStatusUpdateChecked { get; set; } //Status update

        //REMINDER SETTINGS
        public bool HasInstructByDate { get; set; } //Show Instruct By Date when sending reminders
        public bool IsInstructByDateRequired { get; set; } //Instruct By Date required field validation

        //CLIENT SETTINGS
        //override in client rms settings tab
        public bool HasConfirmation { get; set; }   //Send Confirmation Letter to clients on instructions sent to CPI                
        public bool HasDecisionMgt { get; set; }     //Enable Decision Management

        //AGENT SETTINGS
        //override in agent rms settings tab
        public bool HasAgentResp { get; set; }      //Generate Agent Responsibility Letter

        //PORTFOLIO REVIEW AND REMINDERS SETTINGS
        public bool PortfolioHasRemarks { get; set; }       //(HasTicklerRemarks) Allow ClientInstrxRemarks in Portfolio Review

        //COVER LETTER DEFAULTS
        public string ReminderCoverLetter { get; set; } = "Renewal Reminder";               //Default reminder cover letter
        public string ClientConfirmationCoverLetter { get; set; } = "Renewal Confirmation"; //Default client confirmation cover letter
        public string AgentConfirmationCoverLetter { get; set; } = "Renewal Agent Responsibility";  //Default agent responsibilty confirmation cover letter

        //NOTIFICATION EMAIL SETUP NAMES
        public string InstructionNotification { get; set; } = "Renewal Instruction Notification";                   //Instruction notification email
        public string ClearedInstructionNotification { get; set; } = "Cleared Renewal Instruction Notification";    //Blank instruction notification email

        //DECISION MAKER ACCOUNT SETTINGS
        public string DecisionMakerNotification { get; set; }           //Default new user account notification email.
                                                                        //Use NewPasswordNotification if blank/not found.
                                                                        //Use TemporaryPasswordNotification if blank/not found and DecisionMakerRequireChangePassword=true
        public bool DecisionMakerRequireChangePassword { get; set; }    //Require new decision maker to change password on first login.

        //REMINDER PREVIEW
        public bool HasReminderPreview { get; set; }

        //AGENT RESPONSIBILITY EMAIL PREVIEW
        public bool HasAgentRespPreview { get; set; } = true;

        //ATTACH REQUIRED DOCS IN AGENT RESPONSIBILITY EMAIL
        public bool HasAgentRespRequiredDocs { get; set; }

        //ATTACH REQUIRED DOCS IN CLIENT CONFIRMATION EMAIL
        public bool HasConfirmationRequiredDocs { get; set; }
    }
}
