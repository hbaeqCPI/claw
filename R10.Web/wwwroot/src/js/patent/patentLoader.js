import InventionPage from "./inventionPage";
import CountryAppPage from "./countryAppPage";
import IDSManage from "./idsManagePage";
import PatActionDuePage from "./patActionDuePage";
import PatActionDueInvPage from "./patActionDueInvPage";
import PatCountryLaw from "./patCountryLawPage";
import PatCostTrackingPage from "./costTrackingPage";
import PatCostTrackingInvPage from "./costTrackingInvPage";
import PatTaxSchedulePage from "./patTaxSchedulePage";
import PatGlobalUpdate from "./patGlobalUpdate";
import PatentWatch from "./patentWatch";
import PDTStatistics from "./pdtStatistics";
import PatCostImportPage from "./patCostImportPage";
import PatInventorPage from "./patInventorPage";
import PatWorkflowPage from "./patWorkflowPage";
import PatSearchPage from "./patSearchPage";
import PatInventionRemunerationPage from "./patInventionRemunerationPage";
import PatInventionFRRemunerationPage from "./patInventionFRRemunerationPage";

import rtsLib from "./rtsLib";
import idsLib from "./idsLib";

import PatCEAnnuitySetup from "./patCEAnnuitySetupPage";
import PatCECountrySetup from "./patCECountrySetupPage";
import PatCEGeneralSetup from "./patCEGeneralSetupPage";
import PatCostEstimator from "./patCostEstimatorPage";

import PatDelegationUtility from "./patDelegationUtility";
import RTSUpdatePage from "./rtsUpdatePage";
import RTSPTOUpdatePage from "./rtsPTOUpdatePage";
import RTSMappingPage from "./rtsMappingPage";
import RTSPTOMappingPage from "./rtsPTOMappingPage";

import EPOMappingPage from "./epoMappingPage";
import EPODAMappingPage from "./epoDAMappingPage";

if (!window.rtsLib) {
    window.rtsLib = rtsLib;
}

if (!window.idsLib) {
    window.idsLib = idsLib;
}

if (!window.inventionPage) {
    window.inventionPage = new InventionPage();
}

if (!window.patCountryAppPage) {
    window.patCountryAppPage = new CountryAppPage();
}

if (!window.patActionDuePage) {
    window.patActionDuePage = new PatActionDuePage();
}

if (!window.patActionDueInvPage) {
    window.patActionDueInvPage = new PatActionDueInvPage();
}

if (!window.costTrackingPage) {
    window.costTrackingPage = new PatCostTrackingPage();
}

if (!window.costTrackingInvPage) {
    window.costTrackingInvPage = new PatCostTrackingInvPage();
}

if (!window.idsManagePage) {
    window.idsManagePage = new IDSManage();
}

if (!window.patCountryLawPage) {
    window.patCountryLawPage = new PatCountryLaw();
}

if (!window.patTaxSchedulePage ) {
    window.patTaxSchedulePage = new PatTaxSchedulePage();
}

if (!window.patGlobalUpdate ) {
    window.patGlobalUpdate = new PatGlobalUpdate();
}

if (!window.patentWatch) {
    window.patentWatch = new PatentWatch();
}

if (!window.pdtStatistics) {
    window.pdtStatistics = new PDTStatistics();
}

if (!window.patCostImportPage) {
    window.patCostImportPage = new PatCostImportPage();
}

if (!window.patInventorPage) {
    window.patInventorPage = new PatInventorPage();
}

if (!window.patWorkflowPage) {
    window.patWorkflowPage = new PatWorkflowPage();
}

if (!window.patSearchPage) {
    window.patSearchPage = new PatSearchPage();
}

if (!window.patCEAnnuitySetupPage) {
    window.patCEAnnuitySetupPage = new PatCEAnnuitySetup();
}

if (!window.patCECountrySetupPage) {
    window.patCECountrySetupPage = new PatCECountrySetup();
}

if (!window.patCEGeneralSetupPage) {
    window.patCEGeneralSetupPage = new PatCEGeneralSetup();
}

if (!window.patCostEstimatorPage) {
    window.patCostEstimatorPage = new PatCostEstimator();
}

if (!window.patInventionRemunerationPage) {
    window.patInventionRemunerationPage = new PatInventionRemunerationPage();
}

if (!window.patInventionFRRemunerationPage) {
    window.patInventionFRRemunerationPage = new PatInventionFRRemunerationPage();
}

if (!window.patDelegationUtility) {
    window.patDelegationUtility = new PatDelegationUtility();
}
if (!window.rtsUpdatePage) {
    window.rtsUpdatePage = new RTSUpdatePage();
}
if (!window.rtsPTOUpdateBiblioPage) {
    window.rtsPTOUpdateBiblioPage = new RTSPTOUpdatePage();
}
if (!window.rtsMappingPage) {
    window.rtsMappingPage = new RTSMappingPage();
}
if (!window.rtsPTOIFWDocMappingPage) {
    window.rtsPTOIFWDocMappingPage = new RTSPTOMappingPage();
}
if (!window.rtsPTOTrnxHistoryMappingPage) {
    window.rtsPTOTrnxHistoryMappingPage = new RTSPTOMappingPage();
}

if (!window.epoMappingPage) {
    window.epoMappingPage = new EPOMappingPage();
}
if (!window.epoDocumentsMappingPage) {
    window.epoDocumentsMappingPage = new EPODAMappingPage();
}
if (!window.epoActionsMappingPage) {
    window.epoActionsMappingPage = new EPODAMappingPage();
}