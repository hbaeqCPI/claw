import FFInstructionTypePage from "./ffInstructionTypePage";
import FFRemindersPage from "./ffRemindersPage";
import FFPortfolioReviewPage from "./ffPortfolioReviewPage";
import FFInstructionsPage from "./ffInstructionsPage";
import FFActionClosePage from "./ffActionClosePage";
import FFActionCloseLogPage from "./ffActionCloseLogPage";
import FFGenAppPage from "./ffGenAppPage";
import FFReminderSetupPage from "./ffReminderSetupPage";

if (!window.ffInstructionTypePage) {
    window.ffInstructionTypePage = new FFInstructionTypePage();
}

if (!window.ffReminderSetupPage) {
    window.ffReminderSetupPage = new FFReminderSetupPage();
}

if (!window.ffRemindersPage) {
    window.ffRemindersPage = new FFRemindersPage();
}

if (!window.ffPortfolioReviewPage) {
    window.ffPortfolioReviewPage = new FFPortfolioReviewPage();
}

if (!window.ffInstructionsPage) {
    window.ffInstructionsPage = new FFInstructionsPage();
}

if (!window.ffActionClosePage) {
    window.ffActionClosePage = new FFActionClosePage();
}

if (!window.ffActionCloseLogPage) {
    window.ffActionCloseLogPage = new FFActionCloseLogPage();
}

if (!window.ffGenAppPage) {
    window.ffGenAppPage = new FFGenAppPage();
}


