import cpiStatusMessage from "../statusMessage";
import cpiLoadingSpinner from "../loadingSpinner";
import cpiConfirm from "../confirm";
import cpiPrintConfirm from "../printConfirm";
import cpiAlert from "../alert";
import ActivePage from "../activePage";
import AgentPage from "./agentPage";
import AttorneyPage from "./attorneyPage";
import ClientPage from "./clientPage";
import OwnerPage from "./ownerPage";
import QuickDocket from "./quickDocket";
import QuickEmail from "./quickEmail";
import QuickEmailSetup from "./quickEmailSetup";
import QuickEmailDataSourceSetup from "./quickEmailDataSourceSetup";
import ReportSchedulerPage from "./reportSchedulerPage";
import ReportCriteriaPage from "./reportCriteriaPage";
import AuditTrail from "./auditTrailPage";
import DataQuery from "./dataQuery";
import CustomReport from "./customReport";
import * as pageHelper from "../pageHelper";
import FamilyTreePage from "../shared/familyTreePage";
import FamilyTreeLinkPage from "../shared/familyTreeLinkPage";
import PTOMappingPage from "./ptoMappingPage";
import DynamicGrid from "./dynamicGrid";
import FileUtility from "./fileUtility";
import DataImportPage from "./dataImportPage";
import Letter from "./letter";
import LetterSetup from "./letterSetup";
import LetterDataSourceSetup from "./letterDataSourceSetup";
import DocumentPage from "./documentPage";
import * as documentHelper from "./documentHelper"; //r10
import GenSearch from "./genSearch";
import EmailSetupPage from "./emailSetupPage";
import EmailSetupDetailPage from "./emailSetupDetailPage";
import EmailTemplatePage from "./emailTemplatePage";
import GlobalSearch from "./globalSearch";
import ProductPage from "./productPage";
import ProductImportPage from "./productImportPage";
import FormExtract from "./formExtract";
import FormExtractIFW from "./formExtractIFW";
import MailPage from "./mailPage";
import ActionDelegation from "./actionDelegation";
import docx from "./docx";
import docxSetup from "./docxSetup";
import SharePointGraphHelper from "./sharePointGraphHelper";
import DocumentVerificationPage from "./documentVerificationPage";
import iManage from "./iManage";
import DocViewer from "./docViewer";
import RelatedTrademarkPage from "./relatedTrademarkPage";
import RelatedPatentPage from "./relatedPatentPage";
import RelatedMatterPage from "./relatedMatterPage";

if (!window.cpiStatusMessage) {
    window.cpiStatusMessage = cpiStatusMessage;
}

if (!window.cpiLoadingSpinner) {
    window.cpiLoadingSpinner = cpiLoadingSpinner;
}

if (!window.cpiConfirm) {
    window.cpiConfirm = cpiConfirm;
}

if (!window.cpiPrintConfirm) {
    window.cpiPrintConfirm = cpiPrintConfirm;
}

if (!window.cpiAlert) {
    window.cpiAlert = cpiAlert;
}

if (!window.ActivePage) {
    window.ActivePage = ActivePage;
}

//if (!window.image) {
//    window.image = new Image();
//}

if (!window.pageHelper) {
    window.pageHelper = pageHelper;
}

if (!window.agentPage) {
    window.agentPage = new AgentPage();
}

if (!window.attorneyPage) {
    window.attorneyPage = new AttorneyPage();
}

if (!window.clientPage) {
    window.clientPage = new ClientPage();
}
if (!window.ownerPage) {
    window.ownerPage = new OwnerPage();
}
if (!window.quickDocket) {
    window.quickDocket = new QuickDocket();
}

if (!window.ReportSchedulerPage) {
    window.ReportSchedulerPage = new ReportSchedulerPage();
}
if (!window.ReportCriteriaPage) {
    window.ReportCriteriaPage = new ReportCriteriaPage();
}
if (!window.quickEmail) {
    window.quickEmail = new QuickEmail();
}
if (!window.quickEmailSetup) {
    window.quickEmailSetup = new QuickEmailSetup();
}

if (!window.auditTrail) {
    window.auditTrail = new AuditTrail();
}

if (!window.dataQuery) {
    window.dataQuery = new DataQuery();
}

if (!window.customReport) {
    window.customReport = new CustomReport();
}

if (!window.familyTreePage) {
    window.familyTreePage = new FamilyTreePage
}

if (!window.familyTreeLinkPage) {
    window.familyTreeLinkPage = new FamilyTreeLinkPage()
}

if (!window.ptoMappingPage) {
    window.ptoMappingPage = new PTOMappingPage();
}

if (!window.dynamicGrid) {
    window.dynamicGrid = new DynamicGrid();
}

if (!window.fileUtility) {
    window.fileUtility = new FileUtility();
}

if (!window.dataImportPage) {
    window.dataImportPage = new DataImportPage();
}

if (!window.letter) {
    window.letter = new Letter();
}

if (!window.letterSetup) {
    window.letterSetup = new LetterSetup();
}

if (!window.letterDataSourceSetup) {
    window.letterDataSourceSetup = new LetterDataSourceSetup();
}

if (!window.documentPage) {
    window.documentPage = new DocumentPage();
}

if (!window.documentHelper) {
    window.documentHelper = documentHelper;
}

if (!window.genSearch) {
    window.genSearch = new GenSearch();
}

if (!window.emailSetupPage) {
    window.emailSetupPage = new EmailSetupPage();
}

if (!window.emailSetupDetailPage) {
    window.emailSetupDetailPage = new EmailSetupDetailPage();
}

if (!window.emailTemplatePage) {
    window.emailTemplatePage = new EmailTemplatePage();
}

if (!window.quickEmailDataSourceSetup) {
    window.quickEmailDataSourceSetup = new QuickEmailDataSourceSetup();
}

if (!window.globalSearch) {
    window.globalSearch = new GlobalSearch();
}

if (!window.productPage) {
    window.productPage = new ProductPage();
}

if (!window.productImportPage) {
    window.productImportPage = new ProductImportPage();
}

if (!window.formExtract) {
    window.formExtract = new FormExtract();
}

if (!window.formExtractIFW) {
    window.formExtractIFW = new FormExtractIFW();
}

if (!window.mailPage) {
    window.mailPage = new MailPage();
}

if (!window.actionDelegation) {
    window.actionDelegation = new ActionDelegation();
}

if (!window.docx) {
    window.docx = new docx();
}

if (!window.docxSetup) {
    window.docxSetup = new docxSetup();
}

//if (!window.sharePointHelper) {
//    window.sharePointHelper = new SharePointHelper();
//}

if (!window.sharePointGraphHelper) {
    window.sharePointGraphHelper = new SharePointGraphHelper();
}

if (!window.documentVerificationPage) {
    window.documentVerificationPage = new DocumentVerificationPage();
}

if (!window.iManage) {
    window.iManage = new iManage();
}

if (!window.docViewer) {
    window.docViewer = new DocViewer();
}

if (!window.relatedTrademarkPage) {
    window.relatedTrademarkPage = new RelatedTrademarkPage();
}

if (!window.relatedPatentPage) {
    window.relatedPatentPage = new RelatedPatentPage();
}

if (!window.relatedMatterPage) {
    window.relatedMatterPage = new RelatedMatterPage();
}


//signalR codes
//$(document).ready(function () {
//    initializeSignalR();
//});

function initializeSignalR() {
    const baseUrl = $("body").data("base-url");
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`${baseUrl}/notify`, {
            skipNegotiation: true,
            transport: signalR.HttpTransportType.WebSockets
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on('refreshCount', (sender, messageText) => {
        pageHelper.refreshMessageCount();
    });

    connection.start()
        .then(() => console.log('SignalR connection active'))
        .catch(console.error);

    //connection.invoke('SendMessage', "message deleted...");
};



