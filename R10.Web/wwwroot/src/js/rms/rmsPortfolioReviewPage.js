import Image from "../image";
import ActivePage from "../activePage";

export default class RMSPortfolioReviewPage extends ActivePage {

    constructor() {
        super();
        this.instructableIds = [];
        this.pageChanged = false;
        this.familyLayout = false;
        this.familyFooterTemplate = "";
        this.locale = "en";
        this.totalSpinner = "<i class='fa-spin fal fa-sync'></i>";
        this.allowRemarks = false;
        this.needsReasonForChange = false;
        this.isNotificationSent = false;
        this.notificationClientCodes = [];
        this.isDecisionManagementUser = false;
        this.image = new Image();
    }

    searchResultsInit(locale, allowRemarks, needsReasonForChange, listView, pager, footer, footerTemplate, sortColumns, pageSizes, isDecisionManagementUser) {
        this.locale = locale;
        this.allowRemarks = allowRemarks;
        this.needsReasonForChange = needsReasonForChange;
        this.isDecisionManagementUser = isDecisionManagementUser;
        this.familyFooterTemplate = kendo.template(footerTemplate.html())({ isFamilyFooter: true });

        footer.html(kendo.template(footerTemplate.html())({ isFamilyFooter: false }));
        sortColumns.sorter(listView, true);

        pager.kendoPager({
            dataSource: listView.dataSource,
            pageSizes: pageSizes ? pageSizes : [5, 10, 20, 50],
            refresh: true
        });
        pager.find(".k-pager-numbers-wrap, .k-pager-nav, .k-pager-refresh").bind("click", this.searchResultGridPage);
    }

    initializeSearchResultPage(searchResultPage) {
        searchResultPage.showBreadCrumbTrail = true;
        super.initializeSearchResultPage(searchResultPage);
        this.sidebar.container.addClass("collapse-lg");
        //hide .mark-all if all records are readonly
        //$(this.searchResultContainer).find(".mark-all").on("click", this.getBatchInstructionsEditor);
        $(this.searchResultContainer).find(".mark-all").hide();
        $(this.searchResultContainer).find(".k-grid-excel").on("click", this.exportToExcel);
        $(this.searchResultContainer).find(".get-report").on("click", this.getReport);
    }

    initializeSidebarPage(sidebarPage) {
        super.initializeSidebarPage(sidebarPage);
        this.sidebar.container.addClass("collapse-lg");
        $(this.searchResultContainer).find(".mark-all").hide();
        $(this.searchResultContainer).find(".k-grid-excel").on("click", this.exportToExcel);
        $(this.searchResultContainer).find(".get-report").on("click", this.getReport);
    }

    gridMainSearchFilters = (e) => {
        const filterContainer = typeof e === "string" ? e : this.refineSearchContainer;
        const ids = this.mainSearchRecordIds.slice(e.skip, e.skip + e.take);
        const total = this.mainSearchRecordIds.length;
        const instructionRequest = { IsPaging: this.pageChanged, Total: total, Ids: ids };

        $(this.searchResultContainer).find(".tickler-footer").hide();

        return { mainSearchFilters: pageHelper.formDataToCriteriaList($(filterContainer)).payLoad, instructionRequest: instructionRequest };
    }

    searchResultGridRequestEnd = (e) => {
        this.cpiStatusMessage.hide();
        this.pageChanged = false;

        if (e.response) {
            this.familyLayout = e.response.FamilyLayout;

            $(this.refineSearchContainer).find(".total-results-count").html(e.response.Total);

            if (e.response.Data.length > 0) {
                if (e.response.Ids.length > 0) {
                    this.mainSearchRecordIds = e.response.Ids;
                    //let dueIds = [];
                    //$.each(e.response.Ids, function (index, value) {
                    //    if (value[1] === 1)
                    //        dueIds.push(value[0]);
                    //});
                    //this.instructableIds = dueIds;
                    this.instructableIds = e.response.Ids
                }
                $(this.searchResultContainer).find(".no-results-hide").show();
            }
            else {
                const form = $(`${this.searchContainer}-MainSearch`);
                pageHelper.showErrors($(form).data("no-results") || $("body").data("no-results") ||"Error retrieving search results.");
                $(this.searchResultContainer).find(".no-results-hide").hide();
            }

            //remove DetailDueId
            //$(this.refineSearchContainer).find('input[name ="DetailDueId"]').val("");
        }
    }

    searchResultGridPage = (e) => {
        this.pageChanged = true;
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const listView = e.sender.element;
            const rows = listView.find(".tickler-row");
            const today = new Date();
            let family = "";

            for (var i = 0; i < rows.length; i++) {
                const row = rows[i];
                const rowData = data[i];

                if (i % 2)
                    $(row).addClass("k-alt");

                if (!rowData.Instructable)
                    $(row).addClass("portfolio-family");

                if (rowData.DueDate < today)
                    $(row).addClass("past-due");

                if (this.familyLayout) {
                    if (family !== rowData.CaseNumber) {
                        family = rowData.CaseNumber;
                        listView.append(`<div class="family-wrap" data-familyid="${family}"><div class="family-detail"></div><div class="family-footer">${this.familyFooterTemplate}</div></div>`);
                        $(row).appendTo($(`div[data-familyid='${family}'] .family-detail`));
                    }
                    else {
                        $(row).appendTo($(`div[data-familyid='${family}'] .family-detail`));
                    }
                }
            }

            if (this.familyLayout) {
                //RMS NOT NEEDED
                //const showFamilyTotals = this.showFamilyTotals;

                //rms has no cost
                //$.each(listView.find(".family-wrap"), function () {
                //    showFamilyTotals($(this));
                //});

                //hide .mark-family if all records are readonly
                //listView.find(".mark-family").on("click", this.getBatchInstructionsEditor);
                const getBatchInstructionsEditor = this.getBatchInstructionsEditor;
                $.each(listView.find(".mark-family"), function () {
                    const markFamily = $(this);
                    const family = markFamily.data("family");
                    const instructables = listView.find(`.tickler-instruction-options[data-family="${family}"]`);

                    if (instructables.length == 0)
                        markFamily.hide();
                    else
                        markFamily.on("click", getBatchInstructionsEditor);
                });
            }
            else {
                listView.find(".mark-family").hide();
            }

            listView.find(".tickler-instruction").on("click", this.instructionOnClick);
            listView.find(".tickler-remarks").on("click", this.remarksOnClick);
            listView.find(".tickler-licenses").on("click", this.licensesOnClick);
            listView.find(".tickler-products").on("click", this.productsOnClick);
            listView.find(".tickler-web-link").on("click", this.webLinksOnClick);
            listView.find(".tickler-case-info").on("click", this.caseInfoOnClick);
            listView.find(".tickler-conflict").on("click", this.conflictOnClick);
            listView.find(".tickler-documents").on("click", this.documentsOnClick);
            //listView.find(".tickler-manage-docs").on("click", this.manageDocsOnClick);
            listView.find(".tickler-cost-estimator").on("click", this.costEstimatorOnClick);
            listView.find(".tickler-countries").on("click", this.countriesOnClick);

            //default image thumbnail
            const image = listView.find(".image-default img.type-image");
            if (image.length > 0)
                image.lightBox();

            //default image url/document
            listView.find(".image-default img.type-link").on("click", this.imageOnClick);

            //abstract/inventors collapsible container
            $.each(listView.find(".tickler-collapsible"), function () {
                $(this).textOverflow();
            });

            //hide .mark-all if all records are readonly
            //turn off to prevent multiple click handlers
            $(this.searchResultContainer).find(".mark-all").off("click", this.getBatchInstructionsEditor);
            if (listView.find(".tickler-instruction-options").length == 0)
                $(this.searchResultContainer).find(".mark-all").hide();
            else {
                $(this.searchResultContainer).find(".mark-all").on("click", this.getBatchInstructionsEditor);
                $(this.searchResultContainer).find(".mark-all").show();
            }

            iManage.getDefaultGridImage(this);
            docViewer.getDefaultGridImage(this);

            //sp images
            const form = $(this.searchResultContainer).find(".tickler");
            const spThumbnailUrl = form.data("sp-thumbnail-url");

            if (spThumbnailUrl) {
                const spTokenUrl = form.data("sp-token-url");
                sharePointGraphHelper.getDefaultGridImage($(this.searchResultContainer), spThumbnailUrl, spTokenUrl, 'Trademark');
            }
        }

        this.showCount();
    }

    totalsFormatter() {
        return new Intl.NumberFormat(this.locale, { minimumFractionDigits: 2 });
    }

    //use to show action closing count in intructions
    showCount() {}

    //RMS NOT NEEDED
    //showTotals() {
    //    const footer = $(this.searchResultContainer).find(".tickler-footer");

    //    const totalAmount = footer.find(".grand-total-amount");
    //    const totalCte = footer.find(".grand-total-cte");
    //    const savingsAmount = footer.find(".grand-savings-amount");
    //    const savingsCte = footer.find(".grand-savings-cte");

    //    const formatter = this.totalsFormatter(); //STILL WORKS AFTER REMOVING ARROW FUNCTION BUT SAME CALL IN showFamilyTotals DOES NOT WORK. ???
    //    const noTotal = formatter.format(0);

    //    footer.show();
    //    totalAmount.html(this.totalSpinner);
    //    totalCte.html(this.totalSpinner);
    //    savingsAmount.html(this.totalSpinner);
    //    savingsCte.html(this.totalSpinner);

    //    let data = { dueIds: this.instructableIds };
    //    const includeServiceFee = $(this.searchResultContainer).find("#IncludeServiceFee");
    //    if (includeServiceFee.length > 0)
    //        data.includeServiceFee = includeServiceFee[0].checked;

    //    const url = footer.data("totals-url");
    //    if (url) {
    //        $.post(url, data)
    //            .done(function (result) {
    //                totalAmount.html(formatter.format(result.TotalAmount));
    //                totalCte.html(formatter.format(result.TotalCostToExpiration));
    //                savingsAmount.html(formatter.format(result.SavingsAmount));
    //                savingsCte.html(formatter.format(result.SavingsCostToExpiration));
    //            })
    //            .fail(function (error) {
    //                console.error(error.responseText);
    //                totalAmount.html(noTotal);
    //                totalCte.html(noTotal);
    //                savingsAmount.html(noTotal);
    //                savingsCte.html(noTotal);
    //            });
    //    }
    //}

    //RMS NOT NEEDED
    //showFamilyTotals = (family) => {
    //    const totalAmount = $(family).find(".family-footer .family-total-amount").first();
    //    const totalCte = $(family).find(".family-footer .family-total-cte").first();
    //    const savingsAmount = $(family).find(".family-footer .family-savings-amount").first();
    //    const savingsCte = $(family).find(".family-footer .family-savings-cte").first();

    //    const formatter = this.totalsFormatter();
    //    const noTotal = formatter.format(0);

    //    totalAmount.html(this.totalSpinner);
    //    totalCte.html(this.totalSpinner);
    //    savingsAmount.html(this.totalSpinner);
    //    savingsCte.html(this.totalSpinner);

    //    let data = { familyId: family.data("familyid"), dueIds: this.instructableIds };
    //    const includeServiceFee = $(this.searchResultContainer).find("#IncludeServiceFee");
    //    if (includeServiceFee.length > 0)
    //        data.includeServiceFee = includeServiceFee[0].checked;

    //    const url = $(family).closest(".tickler").data("family-totals-url");
    //    if (url) {
    //        $.post(url, data)
    //            .done(function (result) {
    //                totalAmount.html(formatter.format(result.TotalAmount));
    //                totalCte.html(formatter.format(result.TotalCostToExpiration));
    //                savingsAmount.html(formatter.format(result.SavingsAmount));
    //                savingsCte.html(formatter.format(result.SavingsCostToExpiration));
    //            })
    //            .fail(function (error) {
    //                console.error(error.responseText);
    //                totalAmount.html(noTotal);
    //                totalCte.html(noTotal);
    //                savingsAmount.html(noTotal);
    //                savingsCte.html(noTotal);
    //            });
    //    }
    //}

    instructionOnClick = (e) => {
        const input = $(e.target);
        this.saveInstruction(input.val(), input.closest(".tickler-row"), input.data("country"));
    }

    countriesOnClick = (e) => {
        const el = $(e.target);
        const row = el.closest(".tickler-row");
        const listView = this.searchResultGrid.data("kendoListView");
        const item = listView.dataItem(row);
        const instruction = item.ClientInstructionType;

        this.saveInstruction(instruction, row, true, true);
    }

    remarksOnClick = (e) => {
        const el = $(e.target);
        this.showRemarksEditor(el.closest(".tickler-row"));
    }

    licensesOnClick = (e) => {
        const el = $(e.target);
        this.showLicenses(el.closest(".tickler-row"));
    }

    productsOnClick = (e) => {
        const el = $(e.target);
        this.showProducts(el.closest(".tickler-row"));
    }

    webLinksOnClick = (e) => {
        e.preventDefault();

        const el = $(e.target);
        const url = el.data("url");
        const data = {
            id: el.data("id"),
            module: "Trademark", 
            subModule: "FormLink" 
        };

        cpiLoadingSpinner.show();
        $.post(url, data)
            .done((result) => {
                cpiLoadingSpinner.hide();
                window.open(result.url, '_blank');
            })
            .fail((error) => {
                cpiLoadingSpinner.hide();
                cpiAlert.warning(pageHelper.getErrorMessage(error));
            });
    }

    caseInfoOnClick = (e) => {
        e.preventDefault();

        const el = $(e.target);
        const url = el.data("url");

        if(url)
            pageHelper.openLink(`${url}/${el.data("id")}`);
    }

    conflictOnClick = (e) => {
        const el = $(e.target);
        this.showDecisionManagement(el.closest(".tickler-row"));
    }

    imageOnClick = (e) => {
        const img = $(e.target);
        const form = img.closest(".tickler");
        const url = img.data("img-src");

        if (img.hasClass("type-url") && form.data("confirm-open-url")) {
            cpiConfirm.confirm(img.attr("title").toUpperCase(), form.data("message-open-url"), function () {
                window.open(url);
            });
        } else {
            window.open(url);
        }
    }

    documentsOnClick = (e) => {
        const el = $(e.target);
        this.showDocUploader(el.closest(".tickler-row"));
    }

    manageDocsOnClick = (e) => {
        const el = $(e.target);
        this.showDocsManagement(el.closest(".tickler-row"));
    }

    costEstimatorOnClick = (e) => {
        const el = $(e.target);
        this.openCostEstimator(el.closest(".tickler-row"));
    }


    saveInstruction = (instruction, row, selectCountries, edit) => {
        const tickler = row.closest(".tickler");
        const url = tickler.data("save-instruction-url");
        const getCountryUrl = tickler.data("show-countries-url");
        const listView = this.searchResultGrid.data("kendoListView");
        const item = listView.dataItem(row);

        const checkedInstruction = row.find(`#Instruction-${item.DueId}-${item.ClientInstructionType}`);
        const clickedInstruction = row.find(`#Instruction-${item.DueId}-${instruction}`);

        if (selectCountries === undefined) {
            selectCountries = false;
        }

        let reasonForChange = "";

        const searchResultGridPage = this.searchResultGridPage;
        const save = (ctry, reason) => {
            cpiLoadingSpinner.show();

            const searchResultGridPage = this.searchResultGridPage;
            const data = {
                dueId: item.DueId,
                instructionType: instruction,
                reason: reason,
                countries: ctry,
                source: tickler.data("source"),
                tStamp: item.tStamp,
                __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done((result) => {
                    cpiLoadingSpinner.hide();

                    item.ClientInstructionType = instruction;
                    item.ClientInstruction = clickedInstruction.next("label").text();
                    item.RMSDueId = result.dueId;
                    item.tStamp = result.tStamp;
                    item.HasCountries = selectCountries;

                    if (result && result.estimatedRenewalCost) {
                        item.EstimatedRenewalCost = result.estimatedRenewalCost;
                        const estimatedCostSpan = $(row).find(".estimatedCost");
                        const estimatedAsteriskSpan = $(row).find(".estimatedAsterisk");

                        if (item.EstimatedRenewalCost > 0) {
                            if (estimatedCostSpan)
                                estimatedCostSpan.html(kendo.format("{0:N}", item.EstimatedRenewalCost));

                            if (estimatedAsteriskSpan)
                                estimatedAsteriskSpan.html("*");
                        }
                        else {
                            if (estimatedCostSpan)
                                estimatedCostSpan.html("");

                            if (estimatedAsteriskSpan)
                                estimatedAsteriskSpan.html("");
                        }
                    }                        

                    //RMS NOT NEEDED
                    //if (this.familyLayout) {
                    //    this.showFamilyTotals(row.closest(".family-wrap"));
                    //}

                    //clear instruction
                    if (instruction === "" && checkedInstruction.length !== 0)
                        checkedInstruction.prop("checked", false);

                    //show/hide countries icon
                    const countriesIcon = $(row).find(".tickler-actions.tickler-countries");
                    if (selectCountries)
                        countriesIcon.show();
                    else
                        countriesIcon.hide();

                    this.showCount();

                    //RMS NOT NEEDED
                    //this.checkGraceDate(row).then(() => this.sendNotification(row));
                    this.sendNotification(row);
                    //RMS NOT NEEDED
                    //if (this.isDecisionManagementUser && item.UseDecisionMgt)
                    //    this.checkDecisionConflict(row);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();

                    undo();
                    console.log("error", error);
                    cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                        searchResultGridPage();
                        listView.dataSource.read();
                    });
                });
        };

        const undo = () => {
            if (instruction !== "") {
                if (checkedInstruction.length === 0)
                    clickedInstruction.prop("checked", false);
                else
                    checkedInstruction.prop("checked", true);
            }
        };

        if (instruction === "" && item.ClientInstructionSentToCPIFlag && !(this.isDecisionManagementUser && item.UseDecisionMgt)) {
            cpiAlert.warning(tickler.data("message-clear-not-allowed"));
            return;
        }

        const saveReason = (ctry) => {
            const inputId = `reason-for-change-${item.DueId}`;
            const validationError = tickler.data("message-reason-is-required");

            cpiConfirm.keepOpen = true;
            cpiConfirm.save(this.popUpTitle(item), `
                    <div class="form-group float-label">
                        <label for="${inputId}" class="required">${tickler.data("label-reason-for-change")}</label>
                        <textarea rows="4" class="form-control form-control-sm" id="${inputId}" maxlength="140" required></textarea>
                        <span class="field-validation-error" style="display:none;" data-valmsg-for="${inputId}"><span id="${inputId}-error">${validationError}</span></span>
                    </div>`,
                function () {
                    const reason = $(`textarea[id=${inputId}]`);
                    const error = $(`#${inputId}-error`).closest(".field-validation-error");

                    reasonForChange = reason.val().trim();

                    if (reasonForChange === "") {
                        error.show();
                        reason.addClass("input-validation-error");
                        reason.focus();
                        throw validationError;
                    }
                    else {
                        error.hide();
                        reason.removeClass("input-validation-error");
                        save(ctry, reasonForChange);

                        cpiConfirm.close();
                    }

                    cpiConfirm.keepOpen = false;
                }, false,
                function () {
                    undo();

                    cpiConfirm.keepOpen = false;
                });
        }

        const saveInstruction = (ctry, clientInstructionType, needsReasonForChange) => {
            if (needsReasonForChange && clientInstructionType)
                saveReason(ctry);
            else
                save(ctry, clientInstructionType ? tickler.data("reason") : "");
        }

        const needsReasonForChange = this.needsReasonForChange;
        const clientInstructionType = item.ClientInstructionType;

        if (url !== undefined && (item.ClientInstructionType !== instruction || edit)) {
            let ctry = [];
            if (selectCountries && getCountryUrl) {
                cpiLoadingSpinner.show();

                const data = {
                    dueId: item.DueId,
                    tmkId: item.TmkId,
                    __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
                };
                $.post(getCountryUrl, data)
                    .done((result) => {
                        cpiLoadingSpinner.hide();

                        cpiConfirm.save(this.popUpTitle(item), result,
                            function () {
                                const selected = $('.countries-content .country-check input:checked');
                                const label = $('.countries-content .label.validation');
                                if (selected.length == 0) {
                                    label.addClass("font-weight-bold text-danger");
                                    throw "No countries selected.";
                                }
                                else {
                                    label.removeClass("font-weight-bold text-danger");
                                    selected.each(function () {
                                        ctry.push($(this).attr('value'));
                                    });
                                    
                                    saveInstruction(ctry, clientInstructionType, needsReasonForChange);
                                }
                            }, true,
                            function () {
                                undo();
                            }, true);
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();

                        if (error.status == 404) { //mpr trademarks not found
                            saveInstruction(ctry, clientInstructionType, needsReasonForChange);
                        }
                        else {
                            undo();

                            cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                                searchResultGridPage();
                                listView.dataSource.read();
                            });
                        }
                    });
            }
            else {
                saveInstruction(ctry, clientInstructionType, needsReasonForChange);
            }
        }
    }

    sendNotification = (row) => {
        return new Promise((resolve, reject) => {
            const item = this.searchResultGrid.data("kendoListView").dataItem(row);
            const clientCode = item.ClientCode;
            const clientInstruction = item.ClientInstruction;
            const tickler = row.closest(".tickler");
            const url = tickler.data("send-notification-url");

            //send one notification per client code
            //always send notification when user clears the instruction
            if (url && (!this.notificationClientCodes.includes(clientCode) || clientInstruction === "" || clientInstruction === null)) {
                const data = {};

                data.ClientCode = clientCode;
                data.CaseNumber = item.CaseNumber;
                data.Country = item.CountryName;
                data.TrademarkName = item.TrademarkName;
                data.ActionDue = item.ActionDue;
                data.DueDate = pageHelper.cpiDateFormatToSave(item.DueDate);
                data.ClientInstruction = clientInstruction;

                $.post(url, { data, __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val() })
                    .done(() => {
                        if (!this.notificationClientCodes.includes(clientCode) && clientInstruction !== "")
                            this.notificationClientCodes.push(clientCode);

                        resolve();
                    })
                    .fail(function (error) {
                        console.error(pageHelper.getErrorMessage(error));
                        reject(error);                        
                    });
            }
            else {
                resolve();
            }
        })
    }

    //RMS NOT NEEDED
    //checkGraceDate = (row) => {
    //    return new Promise((resolve, reject) => {
    //        const tickler = row.closest(".tickler");
    //        const url = tickler.data("check-grace-date-url");

    //        //grace date checking is only on portfolio review
    //        //url is empty when called from instructions screen
    //        if (url) {
    //            const item = this.searchResultGrid.data("kendoListView").dataItem(row);
    //            const clientCode = item.ClientCode;
    //            const clientInstruction = item.ClientInstruction;
    //            const data = {};

    //            data.ClientCode = clientCode;
    //            data.CaseNumber = item.CaseNumber;
    //            data.Country = item.CountryName;
    //            data.AnnuityDueDate = pageHelper.cpiDateFormatToSave(item.AnnuityDueDate);
    //            data.ClientInstruction = clientInstruction;

    //            $.post(url, { dueId: item.DueId, data: data, __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val() })
    //                .done(() => {
    //                    resolve();
    //                })
    //                .fail(function (error) {
    //                    console.error(pageHelper.getErrorMessage(error));
    //                    reject(error);
    //                });
    //        }
    //        else {
    //            resolve();
    //        }
    //    })
    //}

    checkDecisionConflict = (row) => {
        const tickler = row.closest(".tickler");
        const url = tickler.data("decision-conflicts-url");
        const listView = this.searchResultGrid.data("kendoListView");
        const item = listView.dataItem(row);

        if (url) {
            $.post(url, { dueId: item.DueId, __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val() })
                .done((result) => {
                    const conflict = $(row).find(".tickler-actions.tickler-conflict");
                    if (result.conflict)
                        conflict.show();
                    else
                        conflict.hide();
                })
                .fail((error) => {
                    console.error(pageHelper.getErrorMessage(error));
                });
        }
    }

    popUpTitle(item) {
        return `<div class="h2"><div>${item.CaseNumber} ${item.SubCase}</div></div>
                <div class="label d-flex justify-content-between" style="margin-top: -10px;">${item.CountryName}</div>
                <div class="label mt-2">${item.TrademarkName}</div>
                <div class="row mt-1">
                    <div class="label col">${item.ActionDue}</div><div class="label col text-right">${pageHelper.cpiDateFormatToDisplay(item.DueDate)}</div>
                </div>
                <div class="rms-modal-status page-status alert alert-danger collapse" role="alert"><span class="message"></span></div>`;
    }

    popUpHeader(row) {
        const tickler = row.closest(".tickler");
        const item = this.searchResultGrid.data("kendoListView").dataItem(row);

        return `<div class="form-row">
                    <div class="col">
                        <div class="form-group float-label"><label>${tickler.data("label-due-date")}</label><input type="text" class="form-control form-control-sm" value="${pageHelper.cpiDateFormatToDisplay(item.DueDate)}" disabled></input></div>
                    </div>
                    <div class="col">
                        <div class="form-group float-label"><label>${tickler.data("label-amount")}</label><input type="text" class="form-control form-control-sm" value="${this.totalsFormatter().format(item.TotalCost)}" disabled></input></div>
                    </div>
                </div>`;
    }

    showRemarksEditor = (row) => {
        const tickler = row.closest(".tickler");
        const url = tickler.data("save-remarks-url");
        const listView = this.searchResultGrid.data("kendoListView");
        const item = listView.dataItem(row);
        const inputId = `remarks-${item.DueId}`;
        const popUpContent = `
            <div class="form-group float-label">
                <label for="${inputId}">${tickler.data("label-remarks")}</label>
                <textarea rows="4" class="form-control form-control-sm" id="${inputId}" ${url ? "" : "disabled"}>${item.ClientInstrxRemarks}</textarea>
            </div>`;

        const searchResultGridPage = this.searchResultGridPage;
        if (url) {
            cpiConfirm.save(this.popUpTitle(item), popUpContent,
                function () {
                    const remarks = $(`textarea[id=${inputId}]`).val();

                    if (remarks !== item.ClientInstrxRemarks) {
                        cpiLoadingSpinner.show();

                        const remarksIcon = $(row).find(".tickler-actions.tickler-remarks");
                        const data = {
                            dueId: item.DueId,
                            remarks: remarks,
                            tStamp: item.tStamp,
                            __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
                        };
                        $.post(url, data)
                            .done(function (result) {
                                cpiLoadingSpinner.hide();

                                item.ClientInstrxRemarks = remarks;
                                item.tStamp = result.tStamp;

                                if (remarks)
                                    remarksIcon.show();
                                else
                                    remarksIcon.hide();
                            })
                            .fail((error) => {
                                cpiLoadingSpinner.hide();

                                cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                                    searchResultGridPage();
                                    listView.dataSource.read();
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

    showHistory = (row) => {
        const tickler = row.closest(".tickler");
        const url = tickler.data("show-history-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoListView").dataItem(row);
            const title = this.popUpTitle(item);
            const header = this.popUpHeader(row);
            const largeModal = this.needsReasonForChange;
            const data = {
                dueId: item.RMSDueId,
                showReason: this.needsReasonForChange,
                __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    //cpiAlert.popUp(title, `${header}${result}`, null, largeModal);
                    cpiAlert.open({ title: title, message: result, largeModal: largeModal, noPadding: true });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    showDecisionManagement = (row) => {
        const tickler = row.closest(".tickler");
        const url = tickler.data("show-decision-mgt-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoListView").dataItem(row);
            const title = this.popUpTitle(item);
            const header = this.popUpHeader(row);
            const largeModal = this.allowRemarks;
            const data = {
                dueId: item.DueId,
                showReason: this.needsReasonForChange,
                __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    //cpiAlert.popUp(title, `${header}${result}`, null, largeModal);
                    cpiAlert.open({ title: title, message: result, largeModal: largeModal, noPadding: true });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    showLicenses = (row) => {
        const tickler = row.closest(".tickler");
        const url = tickler.data("show-licenses-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoListView").dataItem(row);
            const title = this.popUpTitle(item);
            const header = this.popUpHeader(row);
            const data = {
                tmkId: item.TmkId,
                __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
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

    //todo: refactor showHistory, showLicenses, showProducts 
    //      to use one method to open popup form.
    showProducts = (row) => {
        const tickler = row.closest(".tickler");
        const url = tickler.data("show-products-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoListView").dataItem(row);
            const title = this.popUpTitle(item);
            const header = this.popUpHeader(row);
            const data = {
                tmkId: item.TmkId,
                __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
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

    showDocUploader = (row, docId) => {
        const form = row.closest(".tickler");
        const item = this.searchResultGrid.data("kendoListView").dataItem(row);
        this.docUploader(form.data("upload-doc-url"), item, docId);
    }

    docUploader = (url, item, docId) => {
        if (url.toLowerCase().includes("imanagework"))
            iManage.saveDocumentRMS(this, url, item, docId)
        else if (url.toLowerCase().includes("netdocs"))
            docViewer.saveDocumentRMS(this, url, item, docId)
        else if (url) {
            this.image.container = $(this.searchResultContainer);
            this.image.hasViewScreen = false;
            this.image.openPopEditor(`${url}?documentLink=T|Act|ActId|${item.ActId}&dueId=${item.DueId}&docId=${docId || 0}`).then(() => {
                this.refreshPage();
            });
        }
    }

    showActionDueDocsManagement = (e, gridName) => {
        const grid = $(`#${gridName}`).data("kendoGrid");
        const row = $(e.currentTarget).closest("tr");
        const form = row.closest("form");
        const item = grid.dataItem(row);

        this.docsManagement({
            TmkId: item.ParentId,
            ActId: item.ActId,
            DueId: item.DDId,
            CaseNumber: item.CaseNumber,
            CountryName: item.CountryName,
            SubCase: item.SubCase,
            TrademarkName: item.Title,
            ActionDue: item.ActionDue,
            DueDate: item.DueDate
        }, form);
    }

    showDocsManagement = (row) => {
        const listView = this.searchResultGrid.data("kendoListView");
        const form = row.closest(".tickler");
        const item = listView.dataItem(row);

        this.docsManagement(item, form);
    }

    docsManagement = (item, form) => {
        const url = form.data("manage-docs-url");
        const verificationToken = form.find("input[name=__RequestVerificationToken]").val();
        let refresh = false;

        const saveDocument = (e, act) => {
            const el = $(e.currentTarget);
            const url = el.closest(".doc-management").data("save-url");

            if (url) {
                const due = el.closest(".due-doc");
                const val = el.is(":checked");

                cpiLoadingSpinner.show();                
                $.post(url, {
                    dueId: due.data("due-id"),
                    docId: due.data("doc-id"),
                    act: act,
                    val: val,
                    tStamp: due.data("tstamp"),
                    __RequestVerificationToken: verificationToken
                })
                    .done((result) => {
                        cpiLoadingSpinner.hide();
                        refresh = true;

                        if (due.find(".doc-is-required").is(":checked") &&
                            !due.find(".doc-is-uploaded").is(":checked"))
                            due.find(".doc-upload").show();
                        else
                            due.find(".doc-upload").hide();

                        due.data("tstamp", result.tStamp);

                        if (result.message)
                            this.showSuccess(result.message);
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        this.showError(pageHelper.getErrorMessage(error));

                        el.prop("checked", !val);
                    });
            }
            else
                this.showError();

        }

        if (url) {
            cpiLoadingSpinner.show();
            $.post(url, { dueId: item.DueId, __RequestVerificationToken: verificationToken })
                .done((result) => {
                    cpiLoadingSpinner.hide();

                    cpiAlert.open({
                        title: this.popUpTitle(item), message: result, largeModal: true, noPadding: true, onClose: () =>
                        {
                            if (refresh)
                                this.refreshPage();
                        }
                    });

                    setTimeout(() => {
                        $(".doc-management .doc-upload").on("click", (e) =>
                        {
                            const dueDoc = $(e.currentTarget).closest(".due-doc");

                            cpiAlert.close();
                            this.docUploader(form.data("upload-doc-url"), item, dueDoc.data("doc-id"));
                        });
                        $(".doc-management .doc-is-required").on("click", (e) => {
                            saveDocument(e, 1);
                        });
                        $(".doc-management .doc-is-uploaded").on("click", (e) => {
                            saveDocument(e, 2);
                        });
                    }, 500);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });            
        }
    }

    openCostEstimator = (row) => {
        const item = this.searchResultGrid.data("kendoListView").dataItem(row);
        const tickler = row.closest(".tickler");
        const url = tickler.data("view-cost-estimator-url");

        if (url)
            pageHelper.openLink(`${url}/${item.CostEstimatorId}?ddId=${item.DueId}`);
    }

    showUploadLogs = (row) => {
        const tickler = row.closest(".tickler");
        const url = tickler.data("show-upload-logs-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoListView").dataItem(row);
            const title = this.popUpTitle(item);
            const largeModal = true;
            const data = {
                dueId: item.DueId,
                __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.open({ title: title, message: result, largeModal: largeModal, noPadding: true });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    productsGridDataBound = (e) => {
        const data = e.sender.dataSource.data();
        const grid = e.sender.element;

        if (data.length > 0) {
            grid.find(".details-link").on("click", (e) => {
                $("#cpiAlert").modal("hide");
                e.preventDefault();
                pageHelper.openDetailsLink($(e.target));
            });
        }
    }

    getBatchInstructionsEditor = (e) => {
        e.preventDefault();

        const el = $(e.currentTarget);
        const url = el.data("url")
        const family = el.data("family");

        if (url) {
            cpiLoadingSpinner.show();
            const saveBatchInstructions = this.saveBatchInstructions;
            $.get(url, { family: family })
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    cpiConfirm.save(window.cpiBreadCrumbs.getTitle(), result,
                        function () {
                            const editor = $(".instructions-editor");
                            const instruction = editor.find("#BatchInstruction").data("kendoDropDownList");
                            let message = instruction.value() ? editor.data("confirm-update").replaceAll("{0}", instruction.text()) : editor.data("confirm-clear").replaceAll("{0}", editor.data("recipient"));
                            message = message.replaceAll("{1}", editor.data("family"));
                            setTimeout(saveBatchInstructions, 500, editor.data("title"), message, instruction.value(), editor.data("family"));
                        });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    saveBatchInstructions = (title, message, instruction, family) => {
        const listView = this.searchResultGrid;
        const searchResultGridPage = this.searchResultGridPage;
        //RMS NOT NEEDED
        //const checkGraceDate = this.checkGraceDate;
        const sendNotification = this.sendNotification;
        const isDecisionManagementUser = this.isDecisionManagementUser;

        cpiConfirm.warning(title, message,
            function () {
                const familySelector = family ? `[data-family="${family}"]` : "";
                const notSentSelector = instruction ? "" : isDecisionManagementUser ? ":not(.sent.dm-false)" : ":not(.sent)";
                const instructionsSelector = instruction ? ":not(:has(input:checked))" : ":has(input:checked)";
                const instructables = listView.find(`.tickler-instruction-options${notSentSelector}${familySelector}${instructionsSelector}`);

                //nothing to update
                if (instructables.length == 0)
                    return;

                let dueIds = [];
                let tStamps = []
                $.each(instructables, function () {
                    const row = $(this).closest(".tickler-row");
                    const item = listView.data("kendoListView").dataItem(row);

                    dueIds.push(item.DueId);
                    tStamps.push(item.tStamp);
                });

                const dataSource = listView.data("kendoListView").dataSource;
                const tickler = listView.closest(".tickler");
                const url = tickler.data("save-batch-instructions-url");
                const data = {
                    dueIds: dueIds,
                    instructionType: instruction,
                    reason: family ? "Mark Family" : "Mark All",
                    source: tickler.data("source"),
                    tStamps: tStamps,
                    __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
                };

                cpiLoadingSpinner.show()
                $.post(url, data)
                    .done((result) => {
                        cpiLoadingSpinner.hide();

                        searchResultGridPage();
                        dataSource.read().then(function () {
                            cpiAlert.success(title, result.message);

                            let sendNotifications = Promise.resolve();
                            $.each(dueIds, function (index, dueId) {
                                const el = listView.find(`.tickler-instruction-options[data-dueid="${dueId}"]`);
                                const row = $(el).closest(".tickler-row");

                                //RMS NOT NEEDED
                                //sendNotifications = sendNotifications.then(() => checkGraceDate(row)).then(() => sendNotification(row));
                                sendNotifications = sendNotifications.then(() => sendNotification(row));
                            });
                        });

                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();

                        cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                            searchResultGridPage();
                            dataSource.read();
                        });
                    });
            });
    }

    openDetails = (row) => {
        const item = this.searchResultGrid.data("kendoListView").dataItem(row);
        const tickler = row.closest(".tickler");
        const url = tickler.data("view-details-url");

        if (url)
            pageHelper.openLink(`${url}/${item.ActId}`);
    }

    contextMenuOnOpen = (e) => {
        const menu = e.sender;
        const row = $(e.target).closest(".tickler-row");
        const item = this.searchResultGrid.data("kendoListView").dataItem(row);
        const tickler = $(row).closest(".tickler");
        const saveRemarksUrl = tickler.data("save-remarks-url");
        const getCountryUrl = tickler.data("show-countries-url");
        const viewDetailsUrl = tickler.data("view-details-url");
        const manageDocsUrl = tickler.data("manage-docs-url");
        const viewCostEstimatorUrl = tickler.data("view-cost-estimator-url");
        const addCostEstimatorUrl = tickler.data("add-cost-estimator-url");

        var items = [];

        if (viewDetailsUrl) {
            items.push({
                text: `<span class='fal fa-search-plus fa-fixed-width'></span>${tickler.data("label-view-details")}`,
                attr: { "data-action": "details" },
                encoded: false
            });
        }

        if (item.Instructable) {
            if (item.ClientInstructionType) {
                items.push({
                    text: `<span class='fal fa-eraser fa-fixed-width'></span>${tickler.data("label-clear-instruction")}`,
                    attr: { "data-action": "clear" },
                    encoded: false
                });
            }

            if (item.HasCountries && getCountryUrl) {
                items.push({
                    text: `<span class='fal fa-globe fa-fixed-width'></span>${tickler.data("label-view-countries")}`,
                    attr: { "data-action": "countries" },
                    encoded: false
                });
            }

            if (this.allowRemarks && saveRemarksUrl) {
                if (item.ClientInstrxRemarks) {
                    items.push({
                        text: `<span class='fal fa-comment-alt-edit fa-fixed-width'></span>${tickler.data("label-edit-remarks")}`,
                        attr: { "data-action": "remarks" },
                        encoded: false
                    });
                }
                else {
                    items.push({
                        text: `<span class='fal fa-comment-alt-plus fa-fixed-width'></span>${tickler.data("label-add-remarks")}`,
                        attr: { "data-action": "remarks" },
                        encoded: false
                    });
                }
            }

            if (item.UseDecisionMgt) {
                items.push({
                    text: `<span class='fal fa-tasks fa-fixed-width'></span>${tickler.data("label-view-decision-management")}`,
                    attr: { "data-action": "decision" },
                    encoded: false
                });
            }

            if (!this.isDecisionManagementUser || !item.UseDecisionMgt) {
                items.push({
                    text: `<span class='fal fa-history fa-fixed-width'></span>${tickler.data("label-view-history")}`,
                    attr: { "data-action": "history" },
                    encoded: false
                });
            }

            if (item.HasTmkLicensee) {
                items.push({
                    text: `<span class='fal fa-file-certificate fa-fixed-width'></span>${tickler.data("label-view-licenses")}`,
                    attr: { "data-action": "licenses" },
                    encoded: false
                });
            }

            if (item.HasTmkProducts) {
                items.push({
                    text: `<span class='fal fa-cube fa-fixed-width'></span>${tickler.data("label-view-products")}`,
                    attr: { "data-action": "products" },
                    encoded: false
                });
            }

            if (item.NeedsRequiredDocs) {
                items.push({
                    text: `<span class='fal fa-cloud-upload fa-fixed-width'></span>${tickler.data("label-view-upload")}`,
                    attr: { "data-action": "upload" },
                    encoded: false
                });
            }

            if (manageDocsUrl) {
                items.push({
                    text: `<span class='fal fa-file-exclamation fa-fixed-width'></span>${tickler.data("label-manage-docs")}`,
                    attr: { "data-action": "docs" },
                    encoded: false
                });

                if (item.HasUploadedDocs) {
                    items.push({
                        text: `<span class='fal fa-file-archive fa-fixed-width'></span>${tickler.data("label-view-upload-logs")}`,
                        attr: { "data-action": "uploadLogs" },
                        encoded: false
                    });
                }
            }

            if (viewCostEstimatorUrl) {
                if (item.CostEstimatorId > 0) {
                    items.push({
                        text: `<span class='fal fa-file-invoice-dollar fa-fixed-width'></span>${tickler.data("label-view-cost-estimator")}`,
                        attr: { "data-action": "cost" },
                        encoded: false
                    });
                }
                else if (addCostEstimatorUrl) {
                    items.push({
                        text: `<span class='fal fa-file-invoice-dollar fa-fixed-width'></span>${tickler.data("label-add-cost-estimator")}`,
                        attr: { "data-action": "cost" },
                        encoded: false
                    });
                }
            }
        }

        menu.setOptions({
            dataSource: items
        });
    }

    contextMenuOnSelect = (e) => {
        const selected = e.item;
        const action = $(selected).data("action");
        const label = selected.innerText;
        const row = $(e.target).closest(".tickler-row");
        
        switch (action) {
            case "details":
                this.openDetails(row);
                return;

            case "history":
                this.showHistory(row);
                return;

            case "clear":
                this.saveInstruction("", row);
                return;

            case "countries":
                const listView = this.searchResultGrid.data("kendoListView");
                const item = listView.dataItem(row);
                const instruction = item.ClientInstructionType;

                this.saveInstruction(instruction, row, true, true);
                return;

            case "remarks":
                this.showRemarksEditor(row);
                return;

            case "licenses":
                this.showLicenses(row);
                return;

            case "products":
                this.showProducts(row);
                return;

            case "decision":
                this.showDecisionManagement(row);
                return;

            case "upload":
                this.showDocUploader(row);
                return;

            case "docs":
                this.showDocsManagement(row);
                return;

            case "uploadLogs":
                this.showUploadLogs(row);
                return;

            case "cost":
                this.openCostEstimator(row);
                return;
        }
    }

    exportToExcel = (e) => {
        e.preventDefault();

        const tickler = $(e.target).closest(".tickler");
        const verificationToken = tickler.find("input[name=__RequestVerificationToken]").val();
        const includeServiceFee = $(this.refineSearchContainer).find("input[name=IncludeServiceFee]");

        const exportCriteria = {};
        exportCriteria.Ids = this.mainSearchRecordIds;
        exportCriteria.Sort = pageHelper.getSortDescriptor(this.searchResultGrid.data("kendoListView").dataSource);
        exportCriteria.IncludeServiceFee = includeServiceFee.length > 0 && (includeServiceFee[0].checked || includeServiceFee[0].value == "1")

        pageHelper.fetchReport(tickler.data("export-to-excel-url"), exportCriteria, verificationToken, tickler.data("export-filename"));        
    }

    getReport = (e) => {
        const el = $(e.target);
        const url = el.data("url");
        const tokenUrl = el.data("token-url");
        const username = el.data("username");
        const form = el.closest("form")
        const verificationToken = form.find("input[name=__RequestVerificationToken]").val();
        const ids = this.mainSearchRecordIds;
        const downloadName = el.data("download-name");
        const sort = pageHelper.getSortDescriptor(this.searchResultGrid.data("kendoListView").dataSource);
        const includeServiceFee = $(this.refineSearchContainer).find("input[name=IncludeServiceFee]");
        const familyLayout = this.familyLayout;

        if (tokenUrl) {
            pageHelper.callWithAuthToken(tokenUrl, username, function (authToken) {
                pageHelper.fetchReport(url, {
                    ids: ids,
                    sort: sort,
                    includeServiceFee: includeServiceFee.length > 0 && (includeServiceFee[0].checked || includeServiceFee[0].value == "1"),
                    familyLayout: familyLayout,
                    token: authToken
                }, verificationToken, downloadName);
            })
        }
    }

    refreshPage = () => {
        const deferred = $.Deferred();
        if (this.searchResultGrid) {
            this.searchResultGridPage();
            this.searchResultGrid.data("kendoListView").dataSource.read().then(() => {
                deferred.resolve();
            });
        }
        return deferred.promise();
    }

    showError = (message) => {
        const status = $(".rms-modal-status");
        status.find(".message").text(message || "An error has occurred.");
        status.addClass("alert-danger");
        status.removeClass("alert-success");
        status.show();
    }

    showSuccess = (message) => {
        const status = $(".rms-modal-status");
        status.find(".message").text(message);
        status.removeClass("alert-danger");
        status.addClass("alert-success");
        status.show();
    }

    hideStatus = () => {
        $(".rms-modal-status").hide();
    }
    
    getSharePointThumbnails = (url, data) => {
        let driveId = "";

        data.forEach((r) => {
            const recKey = { Id: r.TmkId, RecKey: r.SharePointRecKey };
            $.post(url, { docLibraryFolder: 'Trademark', driveId, recKey })
                .done((result) => {
                    if (result && result.DriveId) {
                        driveId = result.DriveId;
                        const element = $(this.searchResultContainer).find(`.sp-img-${result.Id}`);

                        element.attr("src", result.ThumbnailUrl);
                        element.data("img-src", result.DisplayUrl);                        
                        element.closest(".image.border").removeClass("border");
                        element.closest(".image-default").find("i").hide();
                    }
                })
                .fail((e) => {
                    console.error(e.responseText);
                    //pageHelper.showErrors(e.responseText);
                });
        });
    }
}