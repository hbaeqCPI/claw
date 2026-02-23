using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSSetting : DefaultSetting
    {

        public bool DownloadOverwrite { get; set; }
        public bool HasStatusUpdate { get; set; }       //Generate Status Update
        public bool HasStatusDateUpdate { get; set; }   //Update Application Status Date

        //CLIENT SETTINGS
        //override in client ams settings tab
        public bool HasConfirmation { get; set; }   //Send Confirmation Letter to clients on instructions sent to CPI                
        public bool HasCcRemAttorney { get; set; }   //Send Reminder Summary to attorneys

        [Display(Description = "Decision Management", GroupName = "Modules")]
        public bool HasDecisionMgt { get; set; }     //Enable Decision Management

        //AGENT SETTINGS
        //override in agent ams settings tab
        public bool HasAgentResp { get; set; }      //Generate Agent Responsibility Letter

        //CHECKBOX DEFAULTS
        //instructions to cpi
        public bool IsConfirmChecked { get; set; }      //Confirmation letter
        public bool IsAgentRespChecked { get; set; }    //Agent responsibility letter
        public bool IsStatusUpdateChecked { get; set; } //Status update
        //instructions
        public bool IsInstrxIncludeInstructedChecked { get; set; }    //Include instructed in Instructions screen
        public bool IsInstrxIncludeServiceFeeChecked { get; set; }    //Include Service Fee in Instructions screen
        //portfolio review
        public bool IsPortfolioIncludeInstructedChecked { get; set; }    //Include instructed in Portfolio review screen
        //reports
        public bool IsIncludeFamilyChecked { get; set; }        //Include Family in Cost Projection report
        public bool IsIncludeServiceFeeChecked { get; set; }    //Include Service Fee
        public bool IsIncludeVATChecked { get; set; }           //Include VAT

        //COST SETTINGS
        public bool HasCPIExchangeRate { get; set; }//Show CPIExchangeRate and CPIExchangeRateAmt fields
        public bool HasServiceFee { get; set; }    //Include Service fee in Total Cost
        public bool HasVAT { get; set; }           //Include VAT in Total Cost
        public bool HasPercServiceFee { get; set; } //Service fee is percentage of AnnuityCost
        public bool RecalcOnClear { get; set; } //Recalculate service fee when client instruction is cleared

        //CLIENT REFERENCE SETTINGS
        public bool HasClientRef { get; set; }      //Show Client Reference Number.
        public bool HasClientRef2 { get; set; }     //Show ClientRef2 field.
        public string ClientRefFld { get; set; }    //Client Reference Number source field. Use AMSClientRefFieldOptions.
        public string ClientRefTbl { get; set; }    //Client Reference Number source table. Use AMSClientRefTableOptions.

        //PORTFOLIO REVIEW AND REMINDERS SETTINGS
        public bool IsSendRemindersToAttorney { get; set; } //(SendRemToAtty) Send Reminders and Confirmation to Attorneys instead of Client Contacts
        public bool PortfolioHasClientRef { get; set; }     //(HasClientRefRem) Show Client Reference Number in Reminders/Portfolio Review.
        public bool PortfolioHasCPIInstructed { get; set; } //(HasCPIInstructed) Include CPI Instructed in Instructions/Portfolio Review
        public bool PortfolioHasRemarks { get; set; }       //(HasTicklerRemarks) Allow ClientInstrxRemarks in Portfolio Review
        public bool PortfolioHasFamilyLayout { get; set; }  //Use family layout when Portoflio Review is sorted by CaseNumber
        public bool PortfolioIncludeFamily { get; set; }    //(HasFamilyRem) Include family in Portfolio Review for Corporations
        public bool PortfolioHasNP { get; set; }            //(HasNPonReminder) Include PaidThru == "NP" in Reminders/Portfolio Review
        public bool PortfolioHasCostToExpiration { get; set; }  //(HasCTE_Rem) Show Cost to Expiration in Reminders/Portfolio Review

        //INSTRUCTION SCREEN SETTINGS
        public bool InstructionsHasNP { get; set; }         //(HasNPonInstruction) Include PaidThru == "NP" in Instructions

        //COVER LETTER DEFAULTS
        public string ReminderCoverLetter { get; set; } = "Reminder";                               //Default reminder cover letter
        public string ClientConfirmationCoverLetter { get; set; } = "Instruction Confirmation";     //Default client confirmation cover letter
        public string PrepayReminderCoverLetter { get; set; } = "Prepay Reminder";                  //Default prepay reminder cover letter
        public string AttorneySummaryCoverLetter { get; set; } = "Attorney Reminder Summary";       //(AttorneyRemLetter) Default atty reminder summary cover letter
        public string AgentConfirmationCoverLetter { get; set; } = "Agent Responsibility";          //Default agent responsibilty confirmation cover letter
        public string AbandonmentCoverLetter { get; set; } = "Abandonment";                         //Default abandonment cover letter (WHERE IS THIS SET?)

        //NOTIFICATION EMAIL SETUP NAMES
        public string InstructionNotification { get; set; } = "Instruction Notification";                   //Instruction notification email
        public string ClearedInstructionNotification { get; set; } = "Cleared Instruction Notification";    //Blank instruction notification email
        public string InstructionGraceDateWarning { get; set; } = "Instruction Grace Date Warning";         //Instruction grace date warning notification email
        public string InstructionsToCPINotification { get; set; } = "Instructions to CPI Notification";

        //DECISION MAKER ACCOUNT SETTINGS
        public string DecisionMakerNotification { get; set; }           //Default new user account notification email.
                                                                        //Use NewPasswordNotification if blank/not found.
                                                                        //Use TemporaryPasswordNotification if blank/not found and DecisionMakerRequireChangePassword=true
        public bool DecisionMakerRequireChangePassword { get; set; }    //Require new decision maker to change password on first login.

        //COST PROJECTION SETTINGS
        public bool ProjectionHasCostToExpiration { get; set; }  //(HasCTE_Cost) Show Cost to Expiration in Cost Projection reports


        //SEND TO CPI WEB SERVICE URL
        public string SendToCPiURL { get; set; }    //(WebServiceAddress) Send To CPI web service URL
        public string CPiOnlineUrl { get; set; }    //Instruct CPI Url for Data Inquiry and Annuity Reports

        //SEND TO CPI API
        public bool IsSendToCPiAPIOn { get; set; }

        public bool AllowSendToCPi { get; set; }

        //AMS WIDGETS SETTINGS
        public int ProjectionYears { get; set; }    //Number of Projection Years, minimum is 2 years
        public string CPIClientCodeInvCurrency { get; set; }

        //REMINDER PREVIEW
        public bool HasReminderPreview {  get; set; }
    }

    public static class AMSClientRefFieldOptions
    {
        public const string ClientRef1 = "ClientRef1";
        public const string ClientRef2 = "ClientRef2";
        public const string CPIInvClientRef = "CPIInvClientRef";
    }

    public static class AMSClientRefTableOptions
    {
        public const string AMS = "tblAMSMain";
        public const string Invention = "tblPatInvention";
        public const string CountryApplication = "tblPatCountryApplication";
    }
}
