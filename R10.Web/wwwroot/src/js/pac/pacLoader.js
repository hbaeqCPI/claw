import PacClearancePage from "./pacClearancePage";
import PacWorkflowPage from "./pacWorkflowPage";
import PacReviewPage from "./pacReviewPage";
import PacQuestionnairePage from "./pacQuestionnairePage";

if (!window.pacClearancePage) {
    window.pacClearancePage = new PacClearancePage();
}

if (!window.pacWorkflowPage) {
    window.pacWorkflowPage = new PacWorkflowPage();
}

if (!window.pacReviewPage) {
    window.pacReviewPage = new PacReviewPage();
}

if (!window.pacQuestionnairePage) {
    window.pacQuestionnairePage = new PacQuestionnairePage();
}

