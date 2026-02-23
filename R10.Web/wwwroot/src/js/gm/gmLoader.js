import MatterPage from "./gmMatterPage";
import GMCostTrackingPage from "./gmCostTrackingPage";
import GMActionDuePage from "./gmActionDuePage";
import GMGlobalUpdate from "./gmGlobalUpdate";
import GMWorkflowPage from "./gmWorkflowPage";
import GMCostImportPage from "./gmCostImportPage";
import GMDelegationUtility from "./gmDelegationUtility";

if (!window.gmMatterPage) {
    window.gmMatterPage = new MatterPage();
}

if (!window.gmCostTrackingPage) {
    window.gmCostTrackingPage = new GMCostTrackingPage();
}

if (!window.gmActionDuePage) {
    window.gmActionDuePage = new GMActionDuePage();
}

if (!window.gmGlobalUpdate) {
    window.gmGlobalUpdate = new GMGlobalUpdate();
}

if (!window.gmWorkflowPage) {
    window.gmWorkflowPage = new GMWorkflowPage();
}

if (!window.gmCostImportPage) {
    window.gmCostImportPage = new GMCostImportPage();
}

if (!window.gmDelegationUtility) {
    window.gmDelegationUtility = new GMDelegationUtility();
}