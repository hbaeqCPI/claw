import AMSMainPage from "./amsMainPage";
import AMSDuePage from "./amsDuePage";
import PortfolioReviewPage from "./portfolioReviewPage";
import InstructionsPage from "./instructionsPage";
import InstructionsToCPiPage from "./instructionsToCPiPage";
import InstructionsToCPiLogPage from "./instructionsToCPiLogPage";
import AMSRemindersPage from "./amsRemindersPage";
import AMSInstructionTypePage from "./amsInstructionTypePage";
import StatusUpdatePage from "./statusUpdatePage";
import CostExportPage from "./costExportPage";

if (!window.amsMainPage) {
    window.amsMainPage = new AMSMainPage();
}

if (!window.amsDuePage) {
    window.amsDuePage = new AMSDuePage();
}

if (!window.portfolioReviewPage) {
    window.portfolioReviewPage = new PortfolioReviewPage();
}

if (!window.instructionsPage) {
    window.instructionsPage = new InstructionsPage();
}

if (!window.instructionsToCPiPage) {
    window.instructionsToCPiPage = new InstructionsToCPiPage();
}

if (!window.instructionsToCPiLogPage) {
    window.instructionsToCPiLogPage = new InstructionsToCPiLogPage();
}

if (!window.amsRemindersPage) {
    window.amsRemindersPage = new AMSRemindersPage();
}

if (!window.amsInstructionTypePage) {
    window.amsInstructionTypePage = new AMSInstructionTypePage();
}

if (!window.statusUpdatePage) {
    window.statusUpdatePage = new StatusUpdatePage();
}

if (!window.costExportPage) {
    window.costExportPage = new CostExportPage();
}



