import Image from "../image";
import ActivePage from "../activePage";

export default class DMSAgendaPage extends ActivePage {
    constructor() {
        super();
        this.image = new Image();        
    }

    init(screen, id) {

        this.editableGrids = [                        
            { name: "dmsAgendaReviewerGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: "dmsAgendaRelatedDisclosureGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps }
        ];

        this.tabsLoaded = [];
        this.tabChangeSetListener();

        $(document).ready(() => {
            const relatedDisclosureGrid = $("#dmsAgendaRelatedDisclosureGrid");
            relatedDisclosureGrid.on("click", ".disclosureLink", (e) => {
                e.stopPropagation();
                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = relatedDisclosureGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.DMSId);
                pageHelper.openLink(linkUrl, false);
            });

            const reviewerGrid = $("#dmsAgendaReviewerGrid");
            reviewerGrid.on("click", ".reviewerLink", (e) => {
                e.stopPropagation();
                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = reviewerGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.Reviewer.ReviewerId);
                pageHelper.openLink(linkUrl, false);
            });
        });

    }

    tabChangeSetListener() {
        $('#dmsAgendaTab a').on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
        });       
    }

    loadTabContent(tab) {
        const self = this;

        switch (tab) {
            case "dmsAgendaMeetingResultsTab":
                $(document).ready(() => {
                    const grid = $("#dmsAgendaRelatedDisclosureGrid").data("kendoGrid");                    
                    grid.dataSource.read();                        
                });
                break;
            case "dmsAgendaCorrenspondenceTab":
                $(document).ready(() => {
                    const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "":
                break;
        }        
    }

    relatedDisclosureOnChange = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const disclosureTitle = e.dataItem["DisclosureTitle"];
            const disclosureStatus = e.dataItem["DisclosureStatus"];
            const disclosureDate = e.dataItem["DisclosureDate"];
            const recommendation = e.dataItem["Recommendation"];
            const clientCode = e.dataItem["ClientCode"];
            const areaCode = e.dataItem["AreaCode"];

            const grid = $("#dmsAgendaRelatedDisclosureGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.DMSId = e.dataItem["DMSId"];
            dataItem.DisclosureTitle = disclosureTitle;
            dataItem.DisclosureStatus = disclosureStatus;
            dataItem.DisclosureDate = disclosureDate;
            dataItem.Recommendation = recommendation;
            dataItem.ClientCode = clientCode;
            dataItem.AreaCode = areaCode;

            $(row).find(".title-field").html(disclosureTitle);
            $(row).find(".status-field").html(disclosureStatus);
            $(row).find(".date-field").html(disclosureDate);
            $(row).find(".recommendation-field").html(recommendation);
            $(row).find(".client-field").html(clientCode);
            $(row).find(".area-field").html(areaCode);
        }
    }

    agendaRelatedDisclosureGridDataBound = (e) => {
        const data = e.sender.dataSource.data();
        if (data.length > 0) {
            const grid = e.sender.element;            
            grid.find(".recommendation-field").on("click", this.recommendationOnClick);

            for (var i = 0; i < data.length; i++) {
                const trow = $("#" + e.sender.element[0].id).data("kendoGrid").tbody.find("tr[data-uid='" + data[i].uid + "']");
                
                if (data[i].RelatedId <= 0) {
                    trow.children("td.editable-cell.recommendation-field").removeClass("editable-cell");
                }

                if (!data[i].CanEditRecommendation) {
                    trow.children("td.editable-cell.recommendation-field").removeClass("editable-cell").removeClass("recommendation-field");
                }

                if (!data[i].CanLookupRecord) {
                    trow.find("i.disclosureLink").addClass("d-none");
                }
            }
        }
    }

    recommendationOnClick = (e) => {
        const el = $(e.target);
        const item = $("#dmsAgendaRelatedDisclosureGrid").data("kendoGrid").dataItem(el.closest("tr"));
        if (item && !item.CanEditRecommendation) return;

        this.getRecommendationEditor(el.closest("tr"));
    }

    getRecommendationEditor = (row) => {
        const form = row.closest("form");
        const url = form.data("edit-recommendation-url");

        if (url) {
            const item = $("#dmsAgendaRelatedDisclosureGrid").data("kendoGrid").dataItem(row);

            if (item.RelatedId <= 0) {                
                return;
            }

            const showRecommendationEditor = this.showRecommendationEditor;
            
            const data = {
                dmsId: item.DMSId,                
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            cpiLoadingSpinner.show();                      
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
            const grid = $("#dmsAgendaRelatedDisclosureGrid").data("kendoGrid");
            const item = grid.dataItem(row);

            $(".cpiContainerPopup").empty();
            const popupContainer = $(".cpiContainerPopup").last();
            popupContainer.html(editor);

            const dialog = $("#dmsAgendaRecommendationDialog");
            const modalDialog = dialog.find(".modal-dialog");
            if (modalDialog) {
                if (item.Recommendation && item.Recommendation.toLowerCase() === "combine") {
                    modalDialog.addClass("modal-xl");
                }
                else {
                    modalDialog.removeClass("modal-xl");                    
                }
            }
            dialog.find(".dms-agenda-recommendation-combine-search").hide();            
            dialog.modal("show");            

            let entryForm = dialog.find("form")[0];
            $(entryForm).on("submit", (e) => {
                e.preventDefault();
                
                const recommendation = $("#Recommendation").data('kendoDropDownList').value();

                if (recommendation <= "") {
                    alert(missingRecommendationLbl);
                    throw missingRecommendationLbl;
                }

                var combineValue = "";
                const combineDropDown = $("#CombineOption_disclosureAgendaPage").data('kendoDropDownList');                        
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
                            recommendationColumn.text(recommendation);
                            item.Recommendation = recommendation;
                            item.tStamp = result.tStamp;
                            if (combineValue > "") item.Combined = combineValue;

                            dialog.modal("hide");
                            pageHelper.showSuccess(result.message);                           

                            //Process Workflow
                            if (result && result.emailWorkflows)
                                pageHelper.handleEmailWorkflow(result);

                            grid.dataSource.read();

                            cpiLoadingSpinner.hide();
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

            var dialog = e.sender.element.closest("#dmsAgendaRecommendationDialog");
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
        const mainUrl = `${baseUrl}/DMS/Agenda/`;
        const self = this;
        $(".k-grid-toolbar .dmsCombinedGridAdd").unbind("click");
        $(".k-grid-toolbar .dmsCombinedGridAdd").bind("click", function() {
            self.showCombineSearch();
        });        
    }

    showCombineSearch() {
        const container = $("#dmsAgendaRecommendationDialog");
        container.find(".dms-agenda-recommendation-main").hide();
        container.find(".dms-agenda-recommendation-combine-search").show();
        
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

        const searchGrid = container.find("#disclosureAgendaCombineSearchGrid");
        if (searchGrid.length > 0) {                            
            searchGrid.data("kendoGrid").dataSource.page(1);
            searchGrid.data("kendoGrid").clearSelection();
        }

        container.find(".combine-search-cancel").off("click");
        container.find(".combine-search-cancel").on("click", function () {
            container.find(".dms-agenda-recommendation-main").show();
            container.find(".dms-agenda-recommendation-combine-search").hide();
        });

        container.find(".combine-search-save").off("click");
        container.find(".combine-search-save").on("click", function () {            
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/DMS/Agenda/CombineDisclosures`;
                   
            const searchGrid = container.find("#disclosureAgendaCombineSearchGrid");
            const selectedKeys = $(searchGrid).data("kendoGrid").selectedKeyNames();
            
            const parentDMSId = container.find("#DMSId");
            cpiLoadingSpinner.show();
            $.post(url, { parentId: $(parentDMSId).val(), combineDMSIds: selectedKeys })
                .done(function (result) {                    
                    cpiLoadingSpinner.hide();
                    const combineGrid = container.find("#disclosureAgendaDMSCombinedsGrid");
                    $(combineGrid).data("kendoGrid").dataSource.read();
                    pageHelper.showSuccess(result.success);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error)
                }); 

            container.find(".dms-agenda-recommendation-main").show();
            container.find(".dms-agenda-recommendation-combine-search").hide();
        });
    }

    getCombineSearchCriteria = (e) => {
        const inputs = $("#dmsAgendaRecommendationDialog").find("#combineSearchContainer :input");
        const criteria = pageHelper.formDataToCriteriaList(inputs);        
        return { criteria: criteria.payLoad };
    }

    combinedDisclosureOnChange = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const disclosureTitle = e.dataItem["DisclosureTitle"];
            const disclosureStatus = e.dataItem["DisclosureStatus"];
            const disclosureDate = e.dataItem["DisclosureDate"];

            const grid = $("#disclosureAgendaDMSCombinedsGrid").data("kendoGrid");
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
}


