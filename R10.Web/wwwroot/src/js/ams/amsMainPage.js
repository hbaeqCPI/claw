import ActivePage from "../activePage";

export default class AMSMainPage extends ActivePage {

    constructor() {
        super();
    }

    init() {
        this.tabsLoaded = [];
        this.tabChangeSetListener();

        const productsGrid = $(`#productsGrid_${this.mainDetailContainer}`);
        productsGrid.on("click", ".productLink", (e) => {
            e.stopPropagation();

            let url = $(e.target).data("url");
            const row = $(e.target).closest("tr");
            const dataItem = productsGrid.data("kendoGrid").dataItem(row);
            const linkUrl = url.replace("actualValue", dataItem.ProductId);
            pageHelper.openLink(linkUrl, false);
        });
    }

    tabChangeSetListener() {
        $('#amsMainDetailTab a').on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
        });
    }

    loadTabContent(tab) {
        switch (tab) {
            case "amsMainDetailAnnuitiesTab":
                $(document).ready(() => {
                    const grid = $(`#annuitiesGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    $(".grid-options #showOutstandingAnnuitiesOnly").prop('checked', this.showOutstandingAnnuitiesOnly);
                    if (parseInt(grid.options.dataSource.pageSize) > 0)
                        grid.dataSource.pageSize(grid.options.dataSource.pageSize);
                    else
                        grid.dataSource.read();
                });
                break;

            case "amsMainDetailWebLinksTab":
                $(document).ready(function () {
                    const webLinksGrid = $("#amsMainDetailWebLinksTabContent .webLinksGrid").data("kendoGrid");
                    webLinksGrid.dataSource.read();
                });
                break;

            case "amsMainDetailCorrespondenceTab":
                $(document).ready(() => {
                    const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "amsMainDetailProductsTab":
                $(document).ready(() => {
                    const grid = $(`#productsGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "amsMainDetailLicenseesTab":
                $(document).ready(() => {
                    const grid = $(`#licenseesGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "":
                break;
        }
    }

    showTaxSchedHistory(el) {
        const button = $(el);
        const annId = button.data("id");
        const url = button.data("url");
        const form = button.closest("form");

        if (url) {
            cpiLoadingSpinner.show();

            const data = {
                annId: annId,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };
            const title = button.data("title");
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

    getTaxSchedChangeReason = (callback) => {
        var taxSchedChangeReason = $(`#${this.detailContentContainer}`).find("#TaxSchedChangeReason");
        var cpiTaxScheduleComboBox = this.getKendoComboBox("CPITaxSchedule");
        var cpiTaxSchedule;

        if (cpiTaxScheduleComboBox == undefined)
            cpiTaxSchedule = $(`#${this.detailContentContainer}`).find("#CPITaxSchedule").val();
        else
            cpiTaxSchedule = cpiTaxScheduleComboBox.value();

        if (cpiTaxSchedule != taxSchedChangeReason.data("tax-schedule")) {
            var annId = $(`#${this.detailContentContainer}`).find("#AnnID");
            const inputId = `reason-for-change-${annId.val()}`;
            const validationError = taxSchedChangeReason.data("validation-error");
            cpiConfirm.save(window.cpiBreadCrumbs.getTitle(), `
                    <div class="form-group float-label">
                        <label for="${inputId}" class="required">${taxSchedChangeReason.data("label")}</label>
                        <textarea rows="4" class="form-control form-control-sm" id="${inputId}" maxlength="140" required>${taxSchedChangeReason.val()}</textarea>
                        <span class="field-validation-error" style="display:none;" data-valmsg-for="${inputId}"><span id="${inputId}-error">${validationError}</span></span>
                    </div>`,
                function () {
                    const reason = $(`textarea[id=${inputId}]`);
                    const error = $(`#${inputId}-error`).closest(".field-validation-error");

                    const reasonForChange = reason.val().trim();

                    if (reasonForChange === "") {
                        error.show();
                        reason.addClass("input-validation-error");
                        reason.focus();
                        throw validationError;
                    }
                    else {
                        error.hide();
                        reason.removeClass("input-validation-error");

                        taxSchedChangeReason.val(reasonForChange);
                        callback();
                    }
                },
                false,
                function () {
                    const reason = $(`textarea[id=${inputId}]`);
                    taxSchedChangeReason.val(reason.val().trim());
                }
            );

            //this.cpiStatusMessage.error(validationError);
        }
        else {
            callback();
        }
    }

    onChange_Product = (e) => {
        if (e.sender) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $(`#productsGrid_${this.mainDetailContainer}`).data("kendoGrid");
            const dataItem = grid.dataItem(row);

            var comboDataItem = e.sender.dataItem();
            dataItem.ProductId = comboDataItem["ProductId"];
            dataItem.ProductName = comboDataItem["ProductName"];

        }
    }

    productRefreshIndicator = (e) => {
        const data = e.sender.dataSource.data();
        if (data.length == 0)
            $("#amsMainDetailProductsTab").removeClass("has-products");
        else
            $("#amsMainDetailProductsTab").addClass("has-products");
    }

    licenseeRefreshIndicator = (e) => {
        const data = e.sender.dataSource.data();
        if (data.length == 0)
            $("#amsMainDetailLicenseesTab").removeClass("has-licensees");
        else
            $("#amsMainDetailLicenseesTab").addClass("has-licensees");
    }
}