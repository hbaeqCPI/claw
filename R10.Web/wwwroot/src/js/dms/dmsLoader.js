import DisclosurePage from "./disclosurePage";
import DisclosureReviewPage from "./disclosureReviewPage";
import DMSActionDuePage from "./dmsActionDuePage";
import DMSWorkflowPage from "./dmsWorkflowPage";
import DMSQuestionnairePage from "./dmsQuestionnairePage";
import DMSValuationMatrixPage from "./dmsValuationMatrixPage";
import DisclosurePreviewPage from "./disclosurePreviewPage";
import DMSAgendaPage from "./dmsAgendaPage";
import DMSFAQDocPage from "./dmsFAQDocPage";

if (!window.disclosurePage) {
    window.disclosurePage = new DisclosurePage();
}

if (!window.disclosureReviewPage) {
    window.disclosureReviewPage = new DisclosureReviewPage();
}

if (!window.dmsActionDuePage) {
    window.dmsActionDuePage = new DMSActionDuePage();
}

if (!window.dmsWorkflowPage) {
    window.dmsWorkflowPage = new DMSWorkflowPage();
}

if (!window.dmsQuestionnairePage) {
    window.dmsQuestionnairePage = new DMSQuestionnairePage();
}

if (!window.dmsValuationMatrixPage) {
    window.dmsValuationMatrixPage = new DMSValuationMatrixPage();
}

if (!window.disclosurePreviewPage) {
    window.disclosurePreviewPage = new DisclosurePreviewPage();
}

if (!window.dmsAgendaPage) {
    window.dmsAgendaPage = new DMSAgendaPage();
}

if (!window.dmsFAQDocPage) {
    window.dmsFAQDocPage = new DMSFAQDocPage();
}
