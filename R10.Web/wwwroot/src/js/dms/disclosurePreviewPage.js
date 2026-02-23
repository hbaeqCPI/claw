import { data } from "jquery";
import ActivePage from "../activePage";
import SearchPage from "../searchPage";

export default class DisclosurePreviewPage extends SearchPage {

    constructor() {
        super();
        this.valuationGridSelected = [];
        this.verificationTokenFormData = "__RequestVerificationToken";
        this.dmsReviewerEntityType = 1;
    }

    initializeSearchResultPage(searchResultPage) {
        searchResultPage.showBreadCrumbTrail = true;
        super.initializeSearchResultPage(searchResultPage);
        this.sidebar.container.addClass("collapse-lg");

        $('#dmsDisclosurePreviewResults').on("click", ".print-grid-record", () => {
            this.print();
        });
    }

    initializeSidebarPage(sidebarPage) {
        //$(".kendo-Grid .k-grid-toolbar").addClass("sidebar-link");
        super.initializeSidebarPage(sidebarPage);
        this.sidebar.container.addClass("collapse-lg");

        $('#dmsDisclosurePreviewResults').on("click", ".print-grid-record", () => {
            this.print();
        });
    }
    
    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const grid = e.sender.element;            
            
            grid.find(".grid-remarks").on("click", this.remarksOnClick);            
            grid.find(".grid-preview").on("click", this.previewOnClick);       
        }
    }

    popUpTitle(item) {
        if (disclosurePreviewPage.dmsReviewerEntityType === 2) {
            return `<div class="h2"><div>${item.DisclosureNumber}</div></div>
                <div class="h6" style="margin-top: -10px;"><span class="pr-2">${item.AreaCode ? item.AreaCode : ''}</span></div>
                <div class="label mt-2">${item.DisclosureTitle ? item.DisclosureTitle : ''}</div>`;
        }
        else {
            return `<div class="h2"><div>${item.DisclosureNumber}</div></div>
                <div class="h6" style="margin-top: -10px;"><span class="pr-2">${item.ClientCode ? item.ClientCode : ''}</span><span>${item.ClientName ? item.ClientName  : ''}</span></div>
                <div class="label mt-2">${item.DisclosureTitle ? item.DisclosureTitle : ''}</div>`;
        }
        
    }
        
    remarksOnClick = (e) => {
        const el = $(e.target);
        this.showRemarksEditor(el.closest("tr"));
    }

    showRemarksEditor = (row) => {
        const form = row.closest("form");
        const url = form.data("save-remarks-url");
        const grid = this.searchResultGrid.data("kendoGrid");
        const item = grid.dataItem(row);
        const inputId = `remarks-${item.DMSId}`;
        const popUpContent = `
            <div class="form-group float-label">
                <label for="${inputId}">${form.data("label-remarks")}</label>
                <textarea rows="4" class="form-control form-control-sm" id="${inputId}" ${url ? "" : "disabled"}>${item.Remarks ? item.Remarks : ""}</textarea>
            </div>`;

        if (url) {
            cpiConfirm.save(this.popUpTitle(item), popUpContent,
                function () {
                    const remarks = $(`textarea[id=${inputId}]`).val();

                    if (remarks !== item.Remarks) {
                        cpiLoadingSpinner.show();

                        const remarksIcon = $(row).find(".grid-actions.grid-remarks");
                        const data = {
                            dmsId: item.DMSId,
                            remarks: remarks,
                            tStamp: item.tStamp,
                            __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                        };
                        $.post(url, data)
                            .done(function (result) {
                                cpiLoadingSpinner.hide();

                                item.Remarks = remarks;
                                item.tStamp = result.tStamp;

                                if (remarks)
                                    remarksIcon.show();
                                else
                                    remarksIcon.hide();
                            })
                            .fail((error) => {
                                cpiLoadingSpinner.hide();

                                cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                                    grid.dataSource.read();
                                });
                            });
                    }
                }
            );

            setTimeout(function () {
                $(`#${inputId}`).focus();
            }, 500);
        }
        else {
            cpiAlert.popUp(this.popUpTitle(item), popUpContent, null, true);
        }
    }

    previewOnClick = (e) => {
        const el = $(e.target);
        this.showPreviewEditor(el.closest("tr"));
    }

    showPreviewEditor = (row) => {
        const form = row.closest("form");
        const url = form.data("save-preview-url");
        const grid = this.searchResultGrid.data("kendoGrid");
        const item = grid.dataItem(row);
        const remarks = item.Preview.Remarks === null ? "" : item.Preview.Remarks;        
        const inputId = `preview-remarks-${item.DMSId}`;        
        const popUpContent = `            
            <div class="form-group float-label">
                <label for="${inputId}">${form.data("label-preview-remarks")}</label>
                <textarea rows="4" class="form-control form-control-sm" id="${inputId}" ${url ? "" : "disabled"}>${remarks}</textarea>
            </div>`;                    
        if (url) {
            cpiConfirm.save(this.popUpTitle(item), popUpContent,
                function () {                    
                    const newRemarks = $(`textarea[id=${inputId}]`).val();

                    if (newRemarks !== remarks) {
                        cpiLoadingSpinner.show();

                        const data = {
                            preview: {
                                DMSId: item.DMSId,
                                DMSPreviewId: item.Preview.DMSPreviewId,
                                PreviewerType: item.Preview.PreviewerType,
                                PreviewerId: item.Preview.PreviewerId,                                
                                Remarks: newRemarks,
                                tStamp: item.Preview.tStamp,
                                UserId: item.Preview.UserId
                            },
                            __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                        };
                        $.post(url, data)
                            .done(function (result) {
                                cpiLoadingSpinner.hide();

                                item.Preview.DMSPreviewId = result.DMSPreviewId;
                                item.Preview.PreviewerType = result.PreviewerType;
                                item.Preview.PreviewerId = result.PreviewerId;                                
                                item.Preview.Remarks = newRemarks;
                                item.Preview.tStamp = result.tStamp;
                                item.Preview.UserId = result.UserId;

                                grid.dataSource.read();

                                if (result && result.emailWorkflows) {
                                    pageHelper.handleEmailWorkflow(result);
                                }
                            })
                            .fail((error) => {
                                cpiLoadingSpinner.hide();

                                cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                                    grid.dataSource.read();
                                });
                            });
                    }
                }, false, function () {
                    //row.find(".star-rating").rating("reset");
                }
            );
            
            setTimeout(function () {
                $(`#${inputId}`).focus();
            }, 500);
        }
    }

    showPreviewHistory = (row) => {
        const form = row.closest("form");
        const url = form.data("get-preview-history-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const title = this.popUpTitle(item);

            const data = {
                dmsId: item.DMSId,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.open({ title: title, message: result, largeModal: true, noPadding: true });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }
    
    showStatusHistory = (row) => {
        const form = row.closest("form");
        const url = form.data("get-status-history-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const title = this.popUpTitle(item);

            const data = {
                dmsId: item.DMSId,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.open({ title: title, message: result, largeModal: true, noPadding: true });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    showRecommendationHistory = (row) => {
        const form = row.closest("form");
        const url = form.data("get-recommendation-history-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const title = this.popUpTitle(item);

            const data = {
                dmsId: item.DMSId,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.open({ title: title, message: result, noPadding: true });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }   
        
    contextMenuOnOpen = (e) => {
        const menu = e.sender;
        const row = $(e.target).closest("tr");
        const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
        const form = $(row).closest("form");

        var items = [];               
        
        if (form.data("save-remarks-url")) {
            if (item.Remarks) {
                items.push({
                    text: `<span class='fal fa-comment-alt-edit fa-fixed-width'></span>${form.data("label-edit-remarks")}`,
                    attr: { "data-action": "remarks" },
                    encoded: false
                });
            }
            else {
                items.push({
                    text: `<span class='fal fa-comment-alt-plus fa-fixed-width'></span>${form.data("label-add-remarks")}`,
                    attr: { "data-action": "remarks" },
                    encoded: false
                });
            }
        }

        items.push({
            text: `<span class='fal fa-search-plus fa-fixed-width'></span>${form.data("label-view-preview-history")}`,
            attr: { "data-action": "preview-history" },
            encoded: false
        });

        items.push({
            text: `<span class='fal fa-calendar-check fa-fixed-width'></span>${form.data("label-view-status-history")}`,
            attr: { "data-action": "status-history" },
            encoded: false
        });

        items.push({
            text: `<span class='fal fa-badge-check fa-fixed-width'></span>${form.data("label-view-recommendation-history")}`,
            attr: { "data-action": "recommendation-history" },
            encoded: false
        });

        menu.setOptions({
            dataSource: items
        });
    }

    contextMenuOnSelect = (e) => {
        const selected = e.item;
        const action = $(selected).data("action");
        const label = selected.innerText;
        const row = $(e.target).closest("tr");

        switch (action) {  
            case "preview-history":
                this.showPreviewHistory(row);
                return;

            case "status-history":
                this.showStatusHistory(row);
                return;

            case "recommendation-history":
                this.showRecommendationHistory(row);
                return;

            case "remarks":
                this.showRemarksEditor(row);
                return;            
        }
    }       

    print = () => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/DMS/Review/Print`;
        const criteria = JSON.stringify(this.getCriteria(this.refineSearchContainer));
        cpiLoadingSpinner.show();

        console.log(criteria);
        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body: criteria
        })
            .then(response => {
                if (!response.ok)
                    throw response;

                return response.blob();
            })
            .then(data => {
                cpiLoadingSpinner.hide();

                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.download = "Disclosure Review";
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            })
            .catch(error => {
                cpiLoadingSpinner.hide();
                if (error.status >= 500)
                    pageHelper.showErrors(error);
                else
                    error.text().then(errorMessage => {
                        pageHelper.showErrors(errorMessage);
                    })
            });
    }

    formDataToJson(form) {
        const values = form.serializeArray();
        const formData = {};
        let verificationToken = "";

        $.each(values, function () {
            if (this.name === this.verificationTokenFormData) {
                verificationToken = this.value;
            }
            else if ((this.value > "") && !this.name.endsWith("_input")) {
                const element = form.find("input[name='" + this.name + "']");

                if (element.data("role") === "datepicker") {
                    if (element.data("kendoDatePicker")) {
                        let dateValue = element.data("kendoDatePicker").value();
                        if (dateValue) {
                            dateValue = pageHelper.cpiDateFormatToSave(dateValue);
                        }
                        formData[this.name.substring(this.name.indexOf(".") + 1)] = dateValue;
                    }
                }
                else if (element.data("role") === "datetimepicker") {
                    if (element.data("kendoDateTimePicker")) {
                        let dateValue = element.data("kendoDateTimePicker").value();
                        if (dateValue) {
                            dateValue = pageHelper.cpiDateTimeFormatToSave(dateValue);
                        }
                        formData[this.name.substring(this.name.indexOf(".") + 1)] = dateValue;
                    }
                }
                else if (element.data("role") === "numerictextbox") {
                    //asp.net core model binder uses invariant culture
                    //always use "." decimal separator
                    //kendo already strips out thousands separator
                    formData[this.name.substring(this.name.indexOf(".") + 1)] = this.value.replace(",", ".");
                }
                else if (element.length > 0 && element[0].type === "checkbox") {
                    formData[this.name.substring(this.name.indexOf(".") + 1)] = element[0].checked;
                }
                else {
                    formData[this.name.substring(this.name.indexOf(".") + 1)] = this.value;
                }
            }
        });

        //from copy/paste without selecting from the dropdown (only the _input is populated)
        $.each(values, function () {
            if ((this.value > "") && this.name.endsWith("_input")) {
                const name = this.name.replace("_input", "").substring(this.name.indexOf(".") + 1);

                //use the _input value
                if (!formData[name]) {
                    formData[name] = this.value;
                }
            }
        });
        return { verificationToken: verificationToken, payLoad: formData };
    };

    getCriteria(name) {
        const data = this.formDataToJson($(name)).payLoad;
        return data;
    }       
       
}