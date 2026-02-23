import { data } from "jquery";
import ActivePage from "../activePage";
import SearchPage from "../searchPage";

export default class DisclosureReviewPage extends SearchPage {

    constructor() {
        super();
        this.canRateDisclosure = false;
        this.ratings = [];

        this.valuationGridSelected = [];
        this.verificationTokenFormData = "__RequestVerificationToken";
        this.dmsReviewerEntityType = 1;
        this.isReviewer = false;
        this.ValuationSpreadSheetHeaderLabel = {
            ReviewerName: "",
            Category: "",
            Description: "",
            Rating: "",
            Weight: "",
            Remarks: "",
            RatingDate: ""
        };
    }

    initializeSearchResultPage(searchResultPage) {
        searchResultPage.showBreadCrumbTrail = true;
        super.initializeSearchResultPage(searchResultPage);
        this.sidebar.container.addClass("collapse-lg");

        $('#dmsDisclosureReviewResults').on("click", ".print-grid-record", () => {
            this.print();
        });
    }

    initializeSidebarPage(sidebarPage) {
        //$(".kendo-Grid .k-grid-toolbar").addClass("sidebar-link");
        super.initializeSidebarPage(sidebarPage);
        this.sidebar.container.addClass("collapse-lg");

        $('#dmsDisclosureReviewResults').on("click", ".print-grid-record", () => {
            this.print();
        });
    }

    getRatingOptions = () => {
        const count = this.ratings.length - 1;
        const min = this.ratings[0].RatingValue;
        const max = this.ratings[count].RatingValue;
        const step = (max - min) / count;

        let starCaptions = {};
        for (var i = 0, item; i < this.ratings.length; i++) {
            item = this.ratings[i];
            starCaptions[item.RatingValue] = item.Rating;
        }

        return {
            min: min,
            max: max,
            step: step,
            starCaptions: starCaptions,
            starCaptionClasses: function (val) {
                return "rating-caption";
            },
            filledStar: "<i class='fas fa-star'></i>",
            emptyStar: "<i class='fa fa-star'></i>",
            clearButton: "<i class='fa fa-lg fa-minus-circle'></i>",
            showClear: this.canRateDisclosure,
            displayOnly: !this.canRateDisclosure,
            size: "xs",
            animate: false,
            hoverEnabled: false
        };
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const grid = e.sender.element;
            const showRatingsEditor = this.showRatingsEditor;

            grid.find(".star-rating")
                .rating(this.getRatingOptions())
                .on('rating:change', function (event, value, caption) {
                    showRatingsEditor($(this).closest("tr"), value);
                })
                .on('rating:clear', function () {
                    showRatingsEditor($(this).closest("tr"), 0);
                });

            grid.find(".grid-remarks").on("click", this.remarksOnClick);
            grid.find(".grid-recommendation").on("click", this.recommendationOnClick);
        }
    }

    popUpTitle(item) {
        if (disclosureReviewPage.dmsReviewerEntityType === 2) {
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

    showRatingsEditor = (row, value) => {
        const form = row.closest("form");
        const url = form.data("save-rating-url");
        const grid = this.searchResultGrid.data("kendoGrid");
        const item = grid.dataItem(row);
        const remarks = item.Review.Remarks === null ? "" : item.Review.Remarks;
        const rating = item.Review.RatingValue === null ? 0 : item.Review.RatingValue;
        const inputId = `rating-remarks-${item.DMSId}`;
        const ratingId = `rating-${item.DMSId}`;
        const popUpContent = `
            <div class="row mb-3">
                <div class="col text-center">
                    <input type="text" class="d-none" id="${ratingId}" value="${value}" title="">
                </div>
            </div>
            <div class="form-group float-label">
                <label for="${inputId}">${form.data("label-rating-remarks")}</label>
                <textarea rows="4" class="form-control form-control-sm" id="${inputId}" ${url ? "" : "disabled"}>${remarks}</textarea>
            </div>`;

        const ratings = this.ratings;
        if (url) {
            cpiConfirm.save(this.popUpTitle(item), popUpContent,
                function () {
                    const newRating = $(`#${ratingId}`).rating().val();
                    const newRemarks = $(`textarea[id=${inputId}]`).val();
                    
                    if (newRating !== rating || newRemarks !== remarks) {
                        cpiLoadingSpinner.show();

                        const ratingItem = $.grep(ratings, function (item) {
                            return item.RatingValue == newRating;
                        });
                        
                        const data = {
                            review: {
                                DMSId: item.DMSId,
                                DMSReviewId: item.Review.DMSReviewId, 
                                ReviewerType: item.Review.ReviewerType,
                                ReviewerId: item.Review.ReviewerId, 
                                RatingId: ratingItem[0].RatingId,
                                Remarks: newRemarks,
                                tStamp: item.Review.tStamp,
                                UserId: item.Review.UserId
                                },
                            __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                        };
                        $.post(url, data)
                            .done(function (result) {
                                cpiLoadingSpinner.hide();

                                item.Review.DMSReviewId = result.DMSReviewId;
                                item.Review.ReviewerType = result.ReviewerType;
                                item.Review.ReviewerId = result.ReviewerId;
                                item.Review.RatingValue = newRating;
                                item.Review.Remarks = newRemarks;
                                item.Review.tStamp = result.tStamp;
                                item.Review.UserId = result.UserId;

                                grid.dataSource.read();
                            })
                            .fail((error) => {
                                cpiLoadingSpinner.hide();

                                cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                                    grid.dataSource.read();
                                });
                            });
                    }
                }, false, function () {
                    row.find(".star-rating").rating("reset");
                }
            );

            let ratingOptions = this.getRatingOptions();
            ratingOptions.size = "md";

            $(`#${ratingId}`).rating(ratingOptions)
                .on('rating:change', function (event, value, caption) {
                    row.find(".star-rating").rating("update", value);
                }).on('rating:clear', function () {
                    row.find(".star-rating").rating("update", 0);
                });

            setTimeout(function () {
                $(`#${inputId}`).focus();
            }, 500);
        }
        //else {
        //    cpiAlert.popUp(this.popUpTitle(item), popUpContent, null, true);
        //}
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

    recommendationOnClick = (e) => {
        const el = $(e.target);
        this.getRecommendationEditor(el.closest("tr"));
    }

    getRecommendationEditor = (row) => {
        const form = row.closest("form");
        const url = form.data("edit-recommendation-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const showRecommendationEditor = this.showRecommendationEditor;
            
            const data = {
                dmsId: item.DMSId,                
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    showRecommendationEditor(row, result);
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    showRecommendationEditor = (row, editor) => {
        const form = row.closest("form");
        const url = form.data("save-recommendation-url");
        const missingCombineLbl = form.data("label-missing-combine");
        const missingRecommendationLbl = form.data("label-missing-recommendation");

        if (url) {
            const grid = this.searchResultGrid.data("kendoGrid");
            const item = grid.dataItem(row);

            $(".cpiContainerPopup").empty();
            const popupContainer = $(".cpiContainerPopup").last();
            popupContainer.html(editor);

            const dialog = $("#dmsReviewRecommendationDialog");
            const modalDialog = dialog.find(".modal-dialog");
            if (modalDialog) {
                if (item.Recommendation && item.Recommendation.toLowerCase() === "combine") {
                    modalDialog.addClass("modal-xl");
                }
                else {
                    modalDialog.removeClass("modal-xl");                    
                }
            }
            dialog.find(".dms-review-recommendation-combine-search").hide();            
            dialog.modal("show");            

            let entryForm = dialog.find("form")[0];
            $(entryForm).on("submit", (e) => {
                e.preventDefault();                
                //const recommendation = $("#Recommendation_disclosureReviewPage").data('kendoComboBox').value();
                const recommendation = $("#Recommendation").data('kendoDropDownList').value();

                if (recommendation <= "") {
                    alert(missingRecommendationLbl);
                    throw missingRecommendationLbl;
                }

                var combineValue = "";
                const combineDropDown = $("#CombineOption_disclosureReviewPage").data('kendoDropDownList');                        
                if (combineDropDown) combineValue = combineDropDown.value();

                if (recommendation.toLowerCase() === "combine" && !combineValue) {
                    alert(missingCombineLbl);
                    throw missingCombineLbl;
                }

                if ((recommendation !== item.Recommendation) || (recommendation === item.Recommendation && recommendation.toLowerCase() === "combine" && combineValue !== item.Combined)) {
                    const recommendationColumn = $(row).find(".grid-recommendation");

                    const data = {
                        dmsId: item.DMSId,
                        recommendation: recommendation,
                        combineOption: combineValue,
                        tStamp: item.tStamp,
                        __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                    };

                    cpiLoadingSpinner.show();
                    $.post(url, data)
                        .done(function (result) {
                            cpiLoadingSpinner.hide();
                            recommendationColumn.text(recommendation);
                            item.Recommendation = recommendation;
                            item.tStamp = result.tStamp;
                            if (combineValue > "") item.Combined = combineValue;

                            dialog.modal("hide");
                            pageHelper.showSuccess(result.message);

                            grid.dataSource.read();

                            //check workflows
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
                else {
                    dialog.modal("hide");
                }
            });         
        }
    }

    recommendationOnChange = (e) => {        
        const recommendation = e.sender;
        if (recommendation && recommendation.value()) {
            const recommendationValue = recommendation.value().toLowerCase();
            var combinedDiv = e.sender.element.closest(".recommendationDiv").find(".combineDiv");            
            if (combinedDiv && recommendationValue === "combine") combinedDiv.removeClass("d-none");
            else if (combinedDiv && recommendationValue !== "combine") combinedDiv.addClass("d-none");        

            var dialog = e.sender.element.closest("#dmsReviewRecommendationDialog");
            if (dialog) {
                var modal = dialog.find(".modal-dialog");
                if (modal && recommendationValue === "combine") modal.addClass("modal-xl");
                else if (modal && recommendationValue !== "combine") modal.removeClass("modal-xl");

                var recommendationCol = e.sender.element.closest(".recommendationCol");
                if (recommendationCol && recommendationValue === "combine") recommendationCol.removeClass("col").addClass("col-6");
                else if (recommendationCol && recommendationValue !== "combine") recommendationCol.removeClass("col-6").addClass("col");
            }
        }
    }

    onDMSCombinedGridDataBound = (e) => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/DMS/Review/`;
        const self = this;
        $(".k-grid-toolbar .dmsCombinedGridAdd").unbind("click");
        $(".k-grid-toolbar .dmsCombinedGridAdd").bind("click", function() {
            self.showCombineSearch();
        });        
    }

    showCombineSearch() {
        const container = $("#dmsReviewRecommendationDialog");
        container.find(".dms-review-recommendation-main").hide();
        container.find(".dms-review-recommendation-combine-search").show();
        
        container.find(".combine-search .k-combobox > input").each(function () {
            var comboBox = $(this).data("kendoComboBox");
            if (comboBox) {
                comboBox.value("");
                comboBox.bind("change", function () {
                    if (searchGrid.length > 0) {
                        searchGrid.data("kendoGrid").dataSource.page(1);
                    }
                });
            }
        });
        container.find(".combine-search .k-dropdowngrid > input").each(function () {
            var comboBox = $(this).data("kendoMultiColumnComboBox");
            if (comboBox) {
                comboBox.value("");
                comboBox.bind("change", function () {
                    if (searchGrid.length > 0) {
                        searchGrid.data("kendoGrid").dataSource.page(1);
                    }
                });
            }
        });
        container.find(".combine-search .k-multiselect > select").each(function () {
            const multiSelect = $(this).data("kendoMultiSelect");
            if (multiSelect) {
                multiSelect.value([]);
                multiSelect.bind("change", function () {                    
                    if (searchGrid.length > 0) {                            
                        searchGrid.data("kendoGrid").dataSource.page(1);
                    }
                });
            }
        });
        container.find(".combine-search .toggle-option").each(function () {
            $(this).bind("change", function () {                    
                if (searchGrid.length > 0) {                            
                    searchGrid.data("kendoGrid").dataSource.page(1);
                }
            });
        });

        const searchGrid = container.find("#disclosureReviewCombineSearchGrid");
        if (searchGrid.length > 0) {                            
            searchGrid.data("kendoGrid").dataSource.page(1);
            searchGrid.data("kendoGrid").clearSelection();
        }

        container.find(".combine-search-cancel").off("click");
        container.find(".combine-search-cancel").on("click", function () {
            container.find(".dms-review-recommendation-main").show();
            container.find(".dms-review-recommendation-combine-search").hide();
        });

        container.find(".combine-search-save").off("click");
        container.find(".combine-search-save").on("click", function () {            
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/DMS/Review/CombineDisclosures`;
                   
            const searchGrid = container.find("#disclosureReviewCombineSearchGrid");
            const selectedKeys = $(searchGrid).data("kendoGrid").selectedKeyNames();
            
            const parentDMSId = container.find("#DMSId");
            cpiLoadingSpinner.show();
            $.post(url, { parentId: $(parentDMSId).val(), combineDMSIds: selectedKeys })
                .done(function (result) {                    
                    cpiLoadingSpinner.hide();
                    const combineGrid = container.find("#disclosureReviewDMSCombinedsGrid");
                    $(combineGrid).data("kendoGrid").dataSource.read();
                    pageHelper.showSuccess(result.success);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error)
                }); 

            container.find(".dms-review-recommendation-main").show();
            container.find(".dms-review-recommendation-combine-search").hide();
        });
    }

    getCombineSearchCriteria = (e) => {
        const inputs = $("#dmsReviewRecommendationDialog").find("#combineSearchContainer :input");
        const criteria = pageHelper.formDataToCriteriaList(inputs);        
        return { criteria: criteria.payLoad };
    }

    combinedDisclosureOnChange = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const disclosureTitle = e.dataItem["DisclosureTitle"];
            const disclosureStatus = e.dataItem["DisclosureStatus"];
            const disclosureDate = e.dataItem["DisclosureDate"];

            const grid = $("#disclosureReviewDMSCombinedsGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.CombinedDMSId = e.dataItem["DMSId"];
            dataItem.CombinedDisclosureTitle = disclosureTitle;
            dataItem.CombinedDisclosureStatus = disclosureStatus;
            dataItem.CombinedDisclosureDate = disclosureDate;

            $(row).find(".title-field").html(disclosureTitle);
            $(row).find(".status-field").html(disclosureStatus);
            $(row).find(".date-field").html(disclosureDate);
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

    showValuationGrid = (row) => {
        const form = row.closest("form");
        const url = form.data("get-valuation-url");

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
                    cpiAlert.open({ title: title, message: result, extraLargeModal: true, noPadding: true });
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

        if (this.canRateDisclosure) {
            items.push({
                text: `<span class='fal fa-star fa-fixed-width'></span>${form.data("label-update-rating")}`,
                attr: { "data-action": "rating", "data-rating": item.Review.RatingValue },
                encoded: false
            });
        }

        if (form.data("save-recommendation-url")) {
            items.push({
                text: `<span class='fal fa-thumbs-up fa-fixed-width'></span>${form.data("label-update-recommendation")}`,
                attr: { "data-action": "recommendation" },
                encoded: false
            });
        }

        //if (form.data("get-valuation-url")) {
        //    items.push({
        //        text: `<span class='fal fa-abacus fa-fixed-width'></span>${form.data("label-update-valuation")}`,
        //        attr: { "data-action": "valuation" },
        //        encoded: false
        //    });
        //}

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
            case "rating":
                this.showRatingsEditor(row, $(selected).data("rating"));
                return;

            case "recommendation":
                this.getRecommendationEditor(row);
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

            case "valuation":
                this.showValuationGrid(row);
                return;
        }
    }
        
    //------------------------------------------------------------------
    //Valuation grid popup
    onValuationGridEdit = (e) => {        
        //Select editing row
        //$("#valuationGrid").data("kendoGrid").select(".k-grid-edit-row");    
        e.sender.element.select(".k-grid-edit-row");
        var row = e.container;
        var dataItem = e.sender.dataItem(row);
        this.valuationGridSelected = dataItem;        
    } 

    getValuationRating = (e) => {
        //const grid = $("#valuationGrid").data("kendoGrid");
        //const dataItem = grid.dataItem(grid.select());
        return {
            rateId: this.valuationGridSelected.RateId
        };
    }

    valuationRatingOnChange = (e) => {
        const cbDataItem = e.sender.dataItem();
        const grid = $("#" + $(`#${e.sender.element[0].id}`).closest("div")[0].id).data("kendoGrid");
        const row = $(`#${e.sender.element[0].id}`).closest("tr");
        var dataItem = grid.dataItem(row);
        if (cbDataItem) {
            dataItem.RateId = cbDataItem.RateId;
            dataItem.Weight = cbDataItem.WeightMin;
            $(row).find(".weight-field").html(cbDataItem.WeightMin);
        }
    }

    getValuationCategory = (e) => {          
        //const grid = $("#valuationGrid").data("kendoGrid");
        //const dataItem = grid.dataItem(grid.select());
        return {
            valId: this.valuationGridSelected.ValId
        };
    } 

    valuationGridRequestEnd = (e) => {
        var type = e.type;
        var data = e.response.Data;
        var dmsId = 0;
        if (data[0]) {
            dmsId = data[0].DMSId;
            var grid = $("#valuationGrid_" + dmsId).data('kendoGrid');
            if (type == 'create' || type == 'update') {
                grid.dataSource.read();
            } 
        }         
    }

    valuationGridDataBound = (e) => {
        var grid = e.sender;
        var gridPager = $('#' + grid.element[0].id + '.k-grid-pager')[0];
        if (gridPager) {
            gridPager.style.cssText = "background-color: #dedede !important";
        }
    }

    //------------------------------------------------------------------
    //Valuation spreadsheet
    populateRange = (start, stop, step) => Array.from({ length: (stop - start) / step + 1}, (_, i) => start + (i * step));

    valuationSpreadSheetDataBound(e) {
        //Weird row get added after update, so adding an empty row at the end and hide it;
        var rowSize = e.sheet.dataSource.data().filter(d => d.DMSValId > 0).length + 2;     
        if (rowSize > 2) {             
            const allKeys = Object.keys(e.sheet.dataSource.data()[0]);    
            const filteredKeys = allKeys.filter(key => key.charAt(0) === key.charAt(0).toUpperCase() && /^[A-Z]/.test(key));

            //Resize sheet size             
            e.sheet.resize(rowSize,filteredKeys.length);

            // Set headers dynamically based on displayKeys and mapping to labels
            const headerMap = {
                ReviewerName: disclosureReviewPage.ValuationSpreadSheetHeaderLabel.ReviewerName,
                Category: disclosureReviewPage.ValuationSpreadSheetHeaderLabel.Category,
                Description: disclosureReviewPage.ValuationSpreadSheetHeaderLabel.Description,
                Rating: disclosureReviewPage.ValuationSpreadSheetHeaderLabel.Rating,
                Weight: disclosureReviewPage.ValuationSpreadSheetHeaderLabel.Weight,
                Remarks: disclosureReviewPage.ValuationSpreadSheetHeaderLabel.Remarks,
                RatingDate: disclosureReviewPage.ValuationSpreadSheetHeaderLabel.RatingDate
            };
            filteredKeys.forEach((key, index) => {
                if (headerMap[key]) {
                    e.sheet.range(0, index).value(headerMap[key]).bold(true);
                }
            });

            //Hide column            
            const numberOfVisibleColumns = Object.keys(headerMap).length;
            const totalColumns = filteredKeys.length;
            for (let colIndex = numberOfVisibleColumns; colIndex < totalColumns; colIndex++) {
                e.sheet.hideColumn(colIndex);
            }

            //hide row
            e.sheet.hideRow(rowSize-1);

            //Disable columns    
            //var colARange = "A1:A" + (rowSize).toString();
            //var rangeA = e.sheet.range(colARange);
            //rangeA.enable(false).color("black");

            //var colBRange = "B1:B" + (rowSize).toString();        
            //var rangeB = e.sheet.range(colBRange);
            //rangeB.enable(false).color("black");          

            //var colFRange = "F1:F" + (rowSize).toString();        
            //var rangeF = e.sheet.range(colFRange);
            //rangeF.enable(false).color("black"); 

            //Disable all from A1 through G
            var colRange = "A1:G" + (rowSize).toString();
            var sheetRange = e.sheet.range(colRange);
            sheetRange.enable(false).color("black").textAlign("left");

            //Hide Reviewer Name column (1st column) for reviewer user
            if (disclosureReviewPage.isReviewer) {                
                e.sheet.hideColumn(0);
            }

            //Set format for Rating date column at G
            e.sheet.range("G1:G" + (rowSize).toString()).format("dd-mmm-yyyy");

            //Set WRAP for Description column at C
            e.sheet.range("C2:C" + (rowSize).toString()).wrap(true);

            //SET VALIDATION for each cell (custom editor)
            //Order in view model is important for below to work properly
            //rowSize minus 1 to remove empty row at the end
            //First row is header
            const ratingColIndx = 3;
            const WeightColIndx = 4;
            const remarksColIndx = 5;

            const dataItem = e.sheet.dataSource.data();
            
            //Set validation for each cell - loop by row
            for (var i = 1; i < rowSize-1; i++) {  
                var ratingCell = e.sheet.range(i, ratingColIndx);
                var weightCell = e.sheet.range(i, WeightColIndx);
                var remarksCell = e.sheet.range(i, remarksColIndx);
                        
                //DataItem is zero based index
                var rowData = dataItem[i - 1];

                var ratingOpts = rowData.RatingOpts;
                var ratingSystem = rowData.RatingSystem;

                if (rowData.CanEdit == false || rowData.CategoryInUse == false) {
                    continue;
                }
                
                remarksCell.enable(true).background("#e5eef4").color("black");
                
                if (ratingSystem == "Numeric Range") {
                    //Set validation/custom selection for Weight - START    
                    weightCell.enable(true).background("#e5eef4").color("black");
                    ratingCell.enable(false);

                    var weightRange = ratingOpts.filter(d => d.RateId == rowData.RateId)[0];  
                    var weightOpts = [];
                                        
                    let startValue, endValue;
                    const weightMin = weightRange.WeightMin;
                    const weightMax = weightRange.WeightMax;

                    // Check if both are numeric
                    if (/^\d+$/.test(weightMin) && /^\d+$/.test(weightMax)) {
                        startValue = parseInt(weightMin, 10);
                        endValue = parseInt(weightMax, 10);
                        weightOpts = disclosureReviewPage.populateRange(startValue, endValue, 1).map(x => x.toString()); // Keep as strings for consistency
                    }
                    // Check if both are alphabetic (single character)
                    else if (/^[A-Za-z]$/.test(weightMin) && /^[A-Za-z]$/.test(weightMax)) {
                        startValue = weightMin.toUpperCase().charCodeAt(0);
                        endValue = weightMax.toUpperCase().charCodeAt(0);
                        weightOpts = disclosureReviewPage.populateRange(startValue, endValue, 1).map(x => String.fromCharCode(x));
                    }
                    
                    weightOpts = weightOpts.join(',');
                    
                    weightCell.validation({
                        dataType: 'list',
                        from: `"${weightOpts}"`,
                        allowNulls: true,
                        type: 'reject',
                        titleTemplate: 'Invalid value',
                        messageTemplate: 'Please select from the list',
                        showButton: true
                    })                    
                    //Weight validation - END                     

                    //Weird issue with spreadsheet check datatype against list from custom editor
                    if ($.isNumeric(weightCell.value())) {
                        weightCell.value(parseInt(weightCell.value()));
                    }
                }                
                else {
                     //Set validation/custom selection for Rating - START
                    weightCell.enable(false);
                    ratingCell.enable(true).background("#e5eef4").color("black");
                   
                    var rateOpts = [];
                    $.each(ratingOpts, function(key, val) {                
                        rateOpts.push(val.Rating);
                    })
                    rateOpts = rateOpts.join(',');
                    ratingCell.validation({
                        dataType: 'list',
                        from: `"${rateOpts}"`,
                        allowNulls: true,
                        type: 'reject',
                        titleTemplate: 'Invalid value',
                        messageTemplate: 'Please select from the list',
                        showButton: true
                    })
                    //Rating validation - END
                }                
            }
        } 
    }

    valuationSpreadSheetDataBinding(e) {
        //console.log('Data is about to be bound to sheet "' + e.sheet.name() + '".');
        //console.log(e);
    }

    getValuationSpreadSheetDataSource(dmsId) {
        return $("#valuationSpreadsheet_" + dmsId).data("kendoSpreadsheet").activeSheet().dataSource;
    }

    valuationSpreadSheetSaveClick(e) { 
        disclosureReviewPage.valuationSpreadSheetToggleButtons(e, false);
        disclosureReviewPage.getValuationSpreadSheetDataSource(e).sync();
    }
    
    valuationSpreadSheetCancelClick(e) {    
        disclosureReviewPage.valuationSpreadSheetToggleButtons(e, false);
        disclosureReviewPage.getValuationSpreadSheetDataSource(e).cancelChanges();
    }

    valuationSpreadSheetChange(e) {         
        if (e.action == 'itemchange' && e.items[0].DMSValId) {

            disclosureReviewPage.valuationSpreadSheetToggleButtons(e.items[0].DMSId, true);

            //Spreadsheet
            var spreadSheet = $("#valuationSpreadsheet_" + e.items[0].DMSId).data("kendoSpreadsheet");
            var sheet = spreadSheet.activeSheet();
            var dataItem = sheet.dataSource.data();

            //Get row index on spreadsheet
            //Datasource is in the same order as in the spreadsheet, so using for loop to find row index
            //First row on spreadsheet is header, so add 1 to rowIndex
            var rowIndex = 0;
            for (var i = 0; i < dataItem.length; i++) {
                rowIndex = i;
                if (dataItem[i].uid == e.items[0].uid) { break; }
            }            
            var rowData = dataItem[rowIndex];
            //Handle Rating column change
            if (e.field == 'Rating') {       
                
                //RateId column index on spreadsheet - must match with the order on view model
                var rateIdColIndx = 14;
                var weightColIndx = 4;

                var rating = rowData.RatingOpts.filter(d => d.Rating == rowData.Rating); 
                if (rating.length > 0) {
                    //Update RateId field
                    sheet.range(rowIndex + 1, rateIdColIndx).value(rating[0].RateId);
                    //Update Weight if available
                    if (rating[0].WeightMin) {
                        sheet.range(rowIndex + 1, weightColIndx).value(rating[0].WeightMin);
                    }                    
                }
            }                      
        }
    }

    valuationSpreadSheetRead(options, dmsID) {
        cpiLoadingSpinner.show();

        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/DMS/Review/ValuationSpreadSheetGrid_Read`;
        $.ajax({
            url: url,
            data: { dmsId: dmsID},
            dataType: "json",
            success: function (result) {
                cpiLoadingSpinner.hide();
                if (result.Data.length > 0) {                    
                    
                }
                options.success(result.Data);   
            },
            error: function (result) {
                cpiLoadingSpinner.hide();
                options.error(result);
            }
        });
    }

    valuationSpreadSheetSubmit(e, dmsID) {
        cpiLoadingSpinner.show();

        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/DMS/Review/ValuationSpreadSheetGrid_Submit`;
        $.ajax({
            cache: false,
            type: 'POST',
            url: url,
            data: JSON.stringify(e.data.updated),            
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (result) {                
                cpiLoadingSpinner.hide();
                e.success(result.updated, "update");
                e.success(e.data.Created, "create");
                e.success(e.data.Destroyed, "destroy");

                if (result && result.success) {
                    pageHelper.showSuccess(result.success);
                }                
            },
            error: function (xhr, httpStatusMessage, customErrorMessage) {
                cpiLoadingSpinner.hide();
                alert(customErrorMessage);
            }
        });
    }

    valuationSpreadSheetExportExcel(e) {
        let url = $("body").data("base-url") + "/DMS/Review/ValuationSpreadSheetExportToExcel";        
        cpiLoadingSpinner.show();
        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(e)
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
            a.download = "Valuation";
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

    valuationSpreadSheetToggleButtons(dmsId, isChangeBtnVisible) {
        const changeButtons= `#spreadsheetButtons_${dmsId} .change-btn`;
        const exportButtons= `#spreadsheetButtons_${dmsId} .export-btn`;

        if (isChangeBtnVisible) {
            $(changeButtons).removeClass('d-none');
            $(exportButtons).addClass('d-none');
        }
        else {
            $(changeButtons).addClass('d-none');
            $(exportButtons).removeClass('d-none');
        }        
    }

    //------------------------------------------------------------------
    //Print
    print = () => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/DMS/Review/Print`;
        const criteria = JSON.stringify(this.getCriteria(this.refineSearchContainer));
        cpiLoadingSpinner.show();
                
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