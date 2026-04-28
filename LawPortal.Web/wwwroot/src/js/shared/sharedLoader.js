import cpiStatusMessage from "../statusMessage";
import cpiLoadingSpinner from "../loadingSpinner";
import cpiConfirm from "../confirm";
import cpiPrintConfirm from "../printConfirm";
import cpiAlert from "../alert";
import ActivePage from "../activePage";
import * as pageHelper from "../pageHelper";
import DynamicGrid from "./dynamicGrid";
import FileUtility from "./fileUtility";
import GenSearch from "./genSearch";

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

if (!window.pageHelper) {
    window.pageHelper = pageHelper;
}

if (!window.dynamicGrid) {
    window.dynamicGrid = new DynamicGrid();
}

if (!window.fileUtility) {
    window.fileUtility = new FileUtility();
}

if (!window.genSearch) {
    window.genSearch = new GenSearch();
}
