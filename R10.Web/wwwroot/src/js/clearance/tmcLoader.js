import TmcClearancePage from "./tmcClearancePage";
import TmcWorkflowPage from "./tmcWorkflowPage";
import TmcReviewPage from "./tmcReviewPage";
import TmcQuestionnairePage from "./tmcQuestionnairePage";

if (!window.tmcClearancePage) {
    window.tmcClearancePage = new TmcClearancePage();
}

if (!window.tmcWorkflowPage) {
    window.tmcWorkflowPage = new TmcWorkflowPage();
}

if (!window.tmcReviewPage) {
    window.tmcReviewPage = new TmcReviewPage();
}

if (!window.tmcQuestionnairePage) {
    window.tmcQuestionnairePage = new TmcQuestionnairePage();
}

