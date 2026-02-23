import TmkTrademarkPage from "./tmkTrademarkPage";
import TmkActionDuePage from "./tmkActionDuePage";
import TmkCostTrackingPage from "./tmkCostTrackingPage";
import TmkConflictPage from "./tmkConflictPage";
import TmkCountryLaw from "./tmkCountryLawPage";
import PTOUpdateMainPage from "./tmkPTOUpdateMainPage";
import PTOUpdatePage from "./tmkPTOUpdatePage";
import TmkGlobalUpdate from "./tmkGlobalUpdate";
import TmkCostImportPage from "./tmkCostImportPage";
import TmkWorkflowPage from "./tmkWorkflowPage";
import TmkDelegationUtility from "./tmkDelegationUtility";
import TmkCECountrySetup from "./tmkCECountrySetupPage";
import TmkCEGeneralSetup from "./tmkCEGeneralSetupPage";
import TmkCostEstimator from "./tmkCostEstimatorPage";
import TLMappingPage from "./tlMappingPage";
import TLPTOMappingPage from "./tlPTOMappingPage";


if (!window.tmkTrademarkPage) {
    window.tmkTrademarkPage = new TmkTrademarkPage();
}

if (!window.tmkActionDuePage) {
    window.tmkActionDuePage = new TmkActionDuePage();
}

if (!window.tmkCostTrackingPage) {
    window.tmkCostTrackingPage = new TmkCostTrackingPage();
}

if (!window.tmkConflictPage) {
    window.tmkConflictPage = new TmkConflictPage();
}

if (!window.tmkCountryLawPage) {
    window.tmkCountryLawPage = new TmkCountryLaw();
}
if (!window.tmkPTOUpdateMainPage) {
    window.tmkPTOUpdateMainPage = new PTOUpdateMainPage();
}
if(!window.tmkPTOUpdateBiblioPage) {
    window.tmkPTOUpdateBiblioPage = new PTOUpdatePage();
}
if (!window.tmkPTOUpdateTrademarkNamePage) {
    window.tmkPTOUpdateTrademarkNamePage = new PTOUpdatePage();
}
if (!window.tmkPTOUpdateActionPage) {
    window.tmkPTOUpdateActionPage = new PTOUpdatePage();
}
if (!window.tmkGlobalUpdate) {
    window.tmkGlobalUpdate = new TmkGlobalUpdate();
}
if (!window.tmkCostImportPage) {
    window.tmkCostImportPage = new TmkCostImportPage();
}
if (!window.tmkWorkflowPage) {
    window.tmkWorkflowPage = new TmkWorkflowPage();
}
if (!window.tmkDelegationUtility) {
    window.tmkDelegationUtility = new TmkDelegationUtility();
}
if (!window.tmkCECountrySetupPage) {
    window.tmkCECountrySetupPage = new TmkCECountrySetup();
}
if (!window.tmkCEGeneralSetupPage) {
    window.tmkCEGeneralSetupPage = new TmkCEGeneralSetup();
}
if (!window.tmkCostEstimatorPage) {
    window.tmkCostEstimatorPage = new TmkCostEstimator();
}
if (!window.tlMappingPage) {
    window.tlMappingPage = new TLMappingPage();
}
if (!window.tlPTOIFWDocMappingPage) {
    window.tlPTOIFWDocMappingPage = new TLPTOMappingPage();
}
if (!window.tlPTOTrnxHistoryMappingPage) {
    window.tlPTOTrnxHistoryMappingPage = new TLPTOMappingPage();
}