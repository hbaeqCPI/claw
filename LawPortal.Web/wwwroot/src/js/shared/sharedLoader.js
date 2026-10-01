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

// Browser history. Screens open in place (ajax) and push their url (see
// pageHelper.manageDetailPage), so Back/Forward land on an entry of THIS document:
// the browser only changes the address bar. Load the url it landed on — a
// full-page load of any search/add/detail url renders that screen.
if (!window.cpiHistoryWired) {
    window.cpiHistoryWired = true;

    window.addEventListener("popstate", function () {
        window.location.reload();
    });

    // Toolbar buttons are <a href="#">. Following that href adds a "#" history
    // entry per click, which Back then steps through without loading anything.
    $(document).on("click", 'a[href="#"]', function (e) {
        e.preventDefault();
    });
}
