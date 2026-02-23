import SearchEntryPage from "../searchEntryPage";

export default class CustomReport extends SearchEntryPage {

    constructor() {
        super();
    }

    initializeDetailContentHeader(detailContentHeader) {

        this.detailContainer = $(`#${detailContentHeader.detailHeaderContainer}`);
        this.detailContentContainer = $(this.detailContainer).find(".cpiDataContainer");

        const detailHeaderContainer = this.detailContainer;                                                     // customReportDetail

        const self = this;
        const mainControlButtons = $(detailHeaderContainer).find(".cpiButtonsDetail");
        const saveCancelButtons = $(detailHeaderContainer).find("#editActionButtons");                          // actionsButtons

        let addMode = detailContentHeader.addMode;
        let id = detailContentHeader.id;

        let entryForm = detailHeaderContainer.find("form")[0];

        // reset dirty flags
        this.isParentDirty = false;
        cpiBreadCrumbs.markLastNode({ dirty: false });

        // note: breadcrumbs moved up to avoid detail content refresh issues
        //pageHelper.moveBreadcrumbs(`#${detailContentHeader.detailHeaderContainer}`);
        self.moveSideBar(false);

        // attach jquery validator
        $.validator.unobtrusive.parse(entryForm);
        entryForm = $(entryForm);
        entryForm.data("validator").settings.ignore = "";               // include hidden fields (kendo controls)
        pageHelper.addMaxLength(entryForm);                             // auto add maxlength to entry fields

        //initialize buttons
        const form = $("#CustomReportForm");
        form.find("#downloadCustomReport").on("click", function (e) { downloadCustomReport(form) })
        form.find(".print-record").on("click", function (e) { printReport(form) })
        form.find(".email-record").on("click", function (e) { emailReport(form) })

        // main buttons
        if (detailHeaderContainer.length > 0) {
            self.manageDetailPageButtons({ detailContainer: detailHeaderContainer, id: id });
        }

        // record stamps - in separate section of page
        const recordStampsUrl = entryForm.data("recordstamp-url");
        self.refreshRecordStamps(recordStampsUrl, id, addMode);

        // show save/cancel buttons, hide other buttons
        const setToSaveMode = function () {
            saveCancelButtons.removeClass("d-none");
            mainControlButtons.hide();
        };

        const setToViewMode = function () {
            saveCancelButtons.addClass("d-none");
            mainControlButtons.show();
            cpiBreadCrumbs.markLastNode({ dirty: false });
            //refreshNodeDirtyFlag();                           // n/a - refreshes grid
        };

        const markDirty = function () {
            if (self.isParentDirty)
                return;
            self.isParentDirty = true;
            cpiBreadCrumbs.markLastNode({ dirty: true });
            detailHeaderContainer.addClass("dirty");
            setToSaveMode();
        };

        // attach markDirty to input fields
        entryForm.on("input", ".cpiMainEntry input, .cpiMainEntry textarea", function () {
            markDirty();
        });

        const submitForm = function () {
            cpiLoadingSpinner.show();

            if (!addMode)
                self.fillRecordStamp(entryForm);                    // update hidden stamps field inside the form

            const formData = new FormData(entryForm[0]);

            $.ajax({
                type: "POST",
                url: entryForm.attr("action"),
                data: formData,
                contentType: false, // needed for file upload
                processData: false, // needed for file upload
                success: (result) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showSuccess(entryForm.data("save-message"));

                    self.isParentDirty = false;

                    setToViewMode();
                    detailHeaderContainer.removeClass("dirty");

                    // refresh search grid client side (w/o resetting everything)
                    self.selectSearchResultRow = false;
                    if (addMode) {
                        const data = { ReportId: result.ReportId, ReportName: result.ReportName };
                        self.insertSearchGridRow(self.searchResultGridId, data);
                        addMode = false;
                    } else {
                        const grid = $(self.searchResultGridId).data("kendoGrid");
                        const select = grid.select();
                        const data = grid.dataItem(select);
                        data.set("ReportName", result.ReportName);
                        $(self.lastGridSelection).addClass("k-state-selected");
                    }

                    // refresh timestamps
                    id = result.ReportId;
                    const recordStampsUrl = entryForm.data("recordstamp-url");
                    self.refreshRecordStamps(recordStampsUrl, id, addMode);

                    pageHelper.resetComboBoxes(self.searchForm);        // refresh search buttons to reflect saved data
                    var files = document.getElementsByClassName("k-upload-files")[0];
                    if(files)
                        files.parentElement.removeChild(files);
                },
                error: function (e) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(e);
                    //cpiStatusMessage.error(e.responseJSON.Value);
                    self.refreshSearchGrid();
                }
            });

            //pageHelper.postJson(entryForm.attr("action"), json)
            ////document.getElementById("CustomReportForm").submit()
            //    .done(function (result) {
            //        cpiLoadingSpinner.hide();
            //        pageHelper.showSuccess(entryForm.data("save-message"));

            //        self.isParentDirty = false;

            //        setToViewMode();
            //        detailHeaderContainer.removeClass("dirty");

            //        // refresh search grid client side (w/o resetting everything)
            //        self.selectSearchResultRow = false;
            //        if (addMode) {
            //            const data = { ReportName: result.ReportName };
            //            self.insertSearchGridRow(self.searchResultGridId, data);
            //            addMode = false;
            //        } else {
            //            const grid = $(self.searchResultGridId).data("kendoGrid");
            //            const select = grid.select();
            //            const data = grid.dataItem(select);
            //            data.set("ReportName", result.ReportName);
            //            $(self.lastGridSelection).addClass("k-state-selected");
            //        }

            //        // refresh timestamps
            //        id = result.ReportName;
            //        const recordStampsUrl = entryForm.data("recordstamp-url");
            //        self.refreshRecordStamps(recordStampsUrl, id, addMode);

            //        pageHelper.resetComboBoxes(self.searchForm);        // refresh search buttons to reflect saved data

            //    })
            //    .fail(function (e) {
            //        cpiLoadingSpinner.hide();
            //        pageHelper.showErrors(e);

            //        self.refreshSearchGrid();
            //    });
        };

        entryForm.on("submit", function (e) {
            e.preventDefault();
            //client side validation (using jquery validation)
            if (entryForm.valid()) {
                submitForm();
            }
            else {
                cpiLoadingSpinner.hide();
                entryForm.wasValidated();
            }
        });


    }

    refreshRecordStamps(url, id, addMode) {
        const stampContainer = $("#customReportContent");
        if (addMode) {
            stampContainer.find(".content-stamp").hide();
        }
        else {
            const recordStampsUrl = url.replace("recid", id);
            const activePage = { recordStampsUrl: recordStampsUrl, infoContainer: stampContainer };
            pageHelper.updateRecordStamps(activePage);
            const c = stampContainer.find(".content-stamp");
            stampContainer.find(".content-stamp").show();
        }
    }

    fillRecordStamp(entryForm) {
        // record stamp is shown at the footer separate from the form body; 
        // update the empty tStamp inside the form before posting form to server
        const tStampForm = entryForm.find("#tStamp");
        tStampForm.val(customReport.getRecordStamp());
    }

    ReportFormatChangeForCustomReport() {
        if (document.getElementById("ReportFormatForCustomReport") != null) {
            var reportFormat = document.getElementById("ReportFormatForCustomReport").value;
            document.querySelectorAll('.email-record').forEach(element => {
                if (reportFormat == 0 || reportFormat == 1 || reportFormat == 2) {
                    element.removeAttribute("hidden");
                } else {
                    element.setAttribute("hidden", "hidden");
                }
            });
        }
    }
}