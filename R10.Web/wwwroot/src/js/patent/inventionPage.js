import Image from "../image";
import TradeSecret from "../tradeSecret";
import ActivePage from "../activePage";

export default class InventionPage extends ActivePage {

    constructor() {
        super();
        this.image = new Image();
        this.tradeSecret = new TradeSecret();
        this.caseNumberSearchValueMapperUrl = "";
        this.previousClient = null;
        this.docServerOperation = true;
        this.isSharePointIntegrationOn = false;
    }

    initializeDetailContentPage(detailContentPage) {
        super.initializeDetailContentPage(detailContentPage);
        this.tradeSecret.initialize(detailContentPage);
    }

    init(addMode, isClientMatterOn, screen,isSharePointIntegrationOn) {
        this.tabsLoaded = [];
        this.tabChangeSetListener();
        this.docServerOperation = !isSharePointIntegrationOn;

        $(document).ready(() => {
            if (addMode) {

                if (isClientMatterOn) {
                    const client = this.getKendoComboBox("ClientID");
                    client.input.focus();
                }
                else {
                    const caseNumber = this.getElement("CaseNumber");
                    caseNumber.focus();
                }
                
            }

            const atty1 = this.getKendoComboBox("Attorney1ID");
            const atty2 = this.getKendoComboBox("Attorney2ID");
            const atty3 = this.getKendoComboBox("Attorney3ID");
            const atty4 = this.getKendoComboBox("Attorney4ID");
            const atty5 = this.getKendoComboBox("Attorney5ID");

            const self = this;
            $("input[name='ClientID']").change(function () {  
                const baseUrl = $("body").data("base-url");
                $.ajax({
                    url: `${baseUrl}/Shared/Client/GetPatDefaultAttorney`,
                    data: { clientId: this.value },
                    dataType: 'json',
                    success: function (data) {  
                        if (data.length > 0) {

                            if ((atty1.value() == '' && data[0].PatAttorney1ID != null) ||
                                (self.previousClient !=null && atty1.value() == self.previousClient[0].PatAttorney1ID))
                               {
                                atty1.value(data[0].PatAttorney1ID);
                                atty1.element.trigger("change");
                            }
                            if ((atty2.value() == '' && data[0].PatAttorney2ID != null) ||
                                (self.previousClient != null && atty2.value() == self.previousClient[0].PatAttorney2ID))
                               {
                                atty2.value(data[0].PatAttorney2ID);
                                atty2.element.trigger("change");
                            }
                            if ((atty3.value() == '' && data[0].PatAttorney3ID != null) ||
                                (self.previousClient != null && atty3.value() == self.previousClient[0].PatAttorney3ID))
                            {
                                atty3.value(data[0].PatAttorney3ID);
                                atty3.element.trigger("change");
                            }
                            if ((atty4.value() == '' && data[0].PatAttorney4ID != null) ||
                                (self.previousClient != null && atty4.value() == self.previousClient[0].PatAttorney4ID))
                            {
                                atty4.value(data[0].PatAttorney4ID);
                                atty4.element.trigger("change");
                            }
                            if ((atty5.value() == '' && data[0].PatAttorney5ID != null) ||
                                (self.previousClient != null && atty5.value() == self.previousClient[0].PatAttorney5ID))
                            {
                                atty5.value(data[0].PatAttorney5ID);
                                atty5.element.trigger("change");
                            }
                            self.previousClient = data;
                        }                        
                    }
                });
            })

            const ownersGrid = $(`#ownersGrid_${screen}`);
            ownersGrid.on("click", ".ownerLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = ownersGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.OwnerID);
                pageHelper.openLink(linkUrl, false);
            });
                        
            const inventorsGrid = $(`#inventorsGrid_${screen}`);
            inventorsGrid.on("click", ".inventorLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = inventorsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.InventorID);
                pageHelper.openLink(linkUrl, false);
            });

            const form = $($(`#${screen}`).find("form")[0]);
            const invId = form.find("#InvId").val();
            $(`#inventorsGrid_${screen}`).find(".k-grid-toolbar").on("click",
            ".ShowPaymentDateUpdateScreen",
            () => {
                const url = $(`#inventorsGrid_${screen}`).parent().data("url-mass-update");
                const data = {
                    invId: invId,
                };

                console.log(url);

                this.openAwardMassUpdateEntry(inventorsGrid, url, data, true);
            });

            const prioritiesGrid = $("#inventionPriorityGrid");
            prioritiesGrid.on("click", ".appLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = prioritiesGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.ParentAppId);
                pageHelper.openLink(linkUrl, false);
            });

            const applicationsGrid = $("#inventionApplicationGrid");
            applicationsGrid.on("click", ".appLink", (e) => {
                e.stopPropagation();
                e.preventDefault();

                if (inventionPage.applicationAppIds && inventionPage.applicationAppIds.length > 0) {
                    patCountryAppPage.mainSearchRecordIds = inventionPage.applicationAppIds;
                }
                const link = $(e.target);
                pageHelper.openDetailsLink(link);
            });

            const productsGrid = $(`#productsGrid_${screen}`);
            productsGrid.on("click", ".productLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = productsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.ProductId);
                pageHelper.openLink(linkUrl, false);
            });
            const actionsGrid = $(`#actionsGrid_${screen}`);
            actionsGrid.on("click", ".action-link", (e) => {
                e.stopPropagation();
                e.preventDefault();

                if (this.actionActIds && this.actionActIds.length > 0) {
                    patActionDuePage.mainSearchRecordIds = this.actionActIds;
                }
                const link = $(e.target);
                pageHelper.openDetailsLink(link);
            });


            if (isSharePointIntegrationOn) {
                this.image.refreshSharePointDefaultImage(this);
            }
            else {
                this.image.refreshDefaultImage(this);
            }
        });
    }

    caseNumberSearchValueMapper=(options)=> {
        let url = this.getCaseNumberSearchValueMapper();

        if (!url)
            url = `${$("body").data("base-url")}/Patent/Invention/CaseNumberSearchValueMapper`;
        
        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    getCaseNumberSearchValueMapper = () => {
        if (this.caseNumberSearchValueMapperUrl === "") {
            this.caseNumberSearchValueMapperUrl = $("#inventionSearchCaseInfoTabContent").data("case-number-mapper-url");
            if (this.caseNumberSearchValueMapperUrl === undefined || this.caseNumberSearchValueMapperUrl === "")
                this.caseNumberSearchValueMapperUrl = $("#docMgtMainTabContent").data("case-number-mapper-url");
        }
        return this.caseNumberSearchValueMapperUrl;
    }

    titleSearchValueMapper(options) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/Invention/TitleSearchValueMapper`;
        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    tabChangeSetListener = () => {
        $('#inventionDetailTab a').on('click',
            (e)=> {
                e.preventDefault();
                const tab = e.target.id;
                if (this.tabsLoaded.indexOf(tab) === -1) {
                    this.tabsLoaded.push(tab);
                    this.loadTabContent(tab);
                }
            });
    }

    loadTabContent(tab) {
        const pageId = this.mainDetailContainer;

        switch (tab) {
            case "inventionDetailEntitiesTab":
                $(document).ready(function () {                    
                    const ownersGrid = $(`#ownersGrid_${pageId}`).data("kendoGrid");
                    if (ownersGrid) {
                        ownersGrid.dataSource.read().then(function () {
                            const inventorsGrid = $(`#inventorsGrid_${pageId}`).data("kendoGrid");
                            inventorsGrid.dataSource.read();
                        });
                    }
                });
                break;
            case "inventionActionsTab":
                console.log("2");

                $(document).ready(() => {
                    const actionsGrid = $(`#actionsGrid_${this.mainDetailContainer}`);
                    const grid = actionsGrid.data("kendoGrid");
                    $(".grid-options #showOutstandingActionsOnly").prop('checked', this.showOutstandingActionsOnly);
                    if (parseInt(grid.options.dataSource.pageSize) > 0)
                        grid.dataSource.pageSize(grid.options.dataSource.pageSize);
                    else
                        grid.dataSource.read();

                    pageHelper.addBreadCrumbsRefreshHandler(actionsGrid, () => {
                        grid.dataSource.read();
                        this.updateRecordStamps();
                        iManage.initializeViewer(this);
                        docViewer.initializeViewer(this);
                    });
                });
                console.log("2");

                break;
            case "inventionDetailCostsTab":
                $(document).ready(() => {
                    const costsGrid = $(`#costsGrid_${this.mainDetailContainer}`);
                    const grid = costsGrid.data("kendoGrid");
                    grid.dataSource.read();
                    pageHelper.addBreadCrumbsRefreshHandler(costsGrid, () => {
                        grid.dataSource.read();
                        this.updateRecordStamps();
                    });
                });
                break;
            case "inventionDetailPrioritiesTab":
                $(document).ready(function() {
                    const grid = $("#inventionPriorityGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "inventionDetailAbstractTab":
                $(document).ready(function() {
                    const grid = $("#inventionAbstractGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "inventionDetailKeywordsTab":
                $(document).ready(function() {
                    const grid = $(`#keywordsGrid${pageId}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "inventionProductsTab":
                $(document).ready(() => {
                    const productsGrid = $(`#productsGrid_${pageId}`).data("kendoGrid");
                    if (productsGrid)
                        productsGrid.dataSource.read();
                });
                break;
            case "inventionDetailDocumentsTab":
                $(document).ready(() => {
                    this.image.initializeImage(this, this.docServerOperation);
                });
                break;
            case "inventionDetailApplicationsTab":
                    $(document).ready(()=> {
                        const appGrid = $("#inventionApplicationGrid");
                        const grid = appGrid.data("kendoGrid");
                        grid.dataSource.read();
                        pageHelper.addBreadCrumbsRefreshHandler(appGrid, () => {
                            grid.dataSource.read();
                            const prioritiesGrid = $("#inventionPriorityGrid").data("kendoGrid");
                            prioritiesGrid.dataSource.read();
                            this.refreshCaseInfo();
                            this.updateRecordStamps();
                            iManage.initializeViewer(this);
                            docViewer.initializeViewer(this);
                        });
                });
                break;
            case "inventionDetailRelatedInventionsTab":
                $(document).ready(function () {
                    const grid = $("#inventionRelatedInventionsGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "inventionDetailRelatedDisclosuresTab":
                $(document).ready(function() {
                    const grid = $("#inventionRelatedDisclosuresGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "inventionDetailRelatedMatterTab":
                $(document).ready(()=> {
                    const relatedMattersGrid = $(`#relatedMattersGrid_${this.mainDetailContainer}`);
                    const grid = relatedMattersGrid.data("kendoGrid");
                    grid.dataSource.read();
                    pageHelper.addBreadCrumbsRefreshHandler(relatedMattersGrid, () => { grid.dataSource.read() });
                });
                break;
            case "inventionDetailCorrespondenceTab":
                $(document).ready(()=> {
                    const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "inventionInventorRemunerationTab":
                $(document).ready(()=> {
                    //const grid = $(`#inventionInventorRemunerationCostGrid`).data("kendoGrid");
                    //grid.dataSource.read();
                });
                break;
            case "":
                break;
        }
    }

    onComboBoxSelect(e, name) {
        if (e.item) {
            const element = e.sender.element.closest('td').next('td');
            if (element.length > 0) {
                const value = e.dataItem[name];
                if (element.is("input"))
                    element.val(value);
                else
                    element.html(value);
            }
        }
    }

    onAppNumberComboBoxSelect(e) {
        if (e.dataItem) {
            const grid = $("#inventionPriorityGrid").data("kendoGrid");
            const dataItem = grid.dataItem(grid.select());
            dataItem.ParentAppId = e.dataItem["AppId"];

            dataItem.set('FilDate', e.dataItem["FilDate"]);
            dataItem.set('CaseType', e.dataItem["CaseType"]);
            dataItem.set('Country', e.dataItem["Country"]);
        }
    }

    refreshGrid = function(name) {
        const grid = $("#" + name);
        grid.data("kendoGrid").dataSource.read();
    };

    onAddCountryApplication = function() {
        return $.ajax({
            url: "/r9/Patent/CountryApplication/Add",
            data: { fromSearch: false }
        });
    }


    applicationsTabFilter(element, property) {
        element.kendoDropDownList({
            dataSource: {
                transport: {
                    read: '../../CountryApplication/GetPicklistData?property=Property'.replace('Property', property)
                }
            },
            optionLabel: "--Select Value--"
        });
    }
    
    hintInventor(element) {
        const table = $('<table style="width: 600px;" class="k-grid k-widget"></table>');
        table.append(element.clone()); //append the dragged element
        table.css("opacity", 0.7);
        return table; //return the hint element
    }

    placeholderInventor() {
        return $('<tr colspan="4" class="placeholder"></tr>');
    }

    onEditInventor(e) {
        const gridId = e.sender.element[0].id;
        if (e.model.isNew()) {
            //Remove move class on newly added row as we do not want to move record which is not yet saved.
            $("#" + gridId + " tbody").find("tr[data-uid=" + e.model.uid + "]")
                .css({ 'cursor': "default", 'HintHandler': '', });
            $("#" + gridId + " tbody").find("tr[data-uid=" + e.model.uid + "]").removeAttr("onChange");
        }
    }

    onChange_RelatedDisclosure = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
 
            const disclosureStatus = e.dataItem["DisclosureStatus"];
            const disclosureDate = e.dataItem["DisclosureDate"];
            const clientCode = e.dataItem["ClientCode"];
            const title = e.dataItem["DisclosureTitle"];
            const recommendation = e.dataItem["Recommendation"];
          
            const grid = $("#inventionRelatedDisclosuresGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);

            dataItem.DMSId = e.dataItem["DMSId"];
            dataItem.DisclosureStatus = disclosureStatus;
            dataItem.DisclosureDate = disclosureDate;
            dataItem.ClientCode = clientCode;
            dataItem.DiscTitle = title;
            dataItem.Recommendation = recommendation;
            
            $(row).find(".status-field").html(kendo.htmlEncode(disclosureStatus));
            $(row).find(".date-field").html(kendo.htmlEncode(disclosureDate));
            $(row).find(".client-field").html(kendo.htmlEncode(clientCode));
            $(row).find(".title-field").html(kendo.htmlEncode(title));
            $(row).find(".recommendation-field").html(kendo.htmlEncode(recommendation));
        }
    }

    //------------------------------------------------------------ COPY
    showCopyScreen() {
        //const popupContainer = $(".cpiContainerPopup").last();
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#patInvCopyDialog");
        let entryForm = dialogContainer.find("form")[0];
        dialogContainer.modal("show");
        const self = this;

        //$(dialogContainer.find("#tmkCopyButton")).click(function () { self.copyTrademarks(); });

        entryForm = $(entryForm);
        const afterSubmit = function (result) {
            const dataContainer = $('#' + self.mainDetailContainer).find(".cpiDataContainer");
            if (dataContainer.length > 0) {
                setTimeout(function () {
                    dataContainer.empty();
                    dataContainer.html(result);
                }, 1000);
            }
        };
        entryForm.cpiPopupEntryForm({ dialogContainer: dialogContainer, afterSubmit: afterSubmit });
    }

    mainCopyInitialize = (copyInvId) => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/Patent/Invention/`;

        $(document).ready(() => {
            const container = $("#patInvCopyDialog");
            container.find(".case-info-settings").hide();

            container.on("click", ".case-info-set", () => {
                container.find(".case-info-settings").show();
                container.find(".data-to-copy").hide();

                const url = `${mainUrl}GetCopySettings`;
                $.get(url, { copyInvId: copyInvId })
                    .done(function (result) {
                        container.find(".case-info-settings-fields").html(result);
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
            container.on("click", ".case-info-set-cancel,.case-info-set-save", () => {
                container.find(".case-info-settings").hide();
                container.find(".data-to-copy").show();
            });
            container.on("change", ".case-info-settings-fields input[type='checkbox']", function () {
                const checkbox = $(this);
                const name = checkbox.attr("name");
                const value = checkbox.prop("checked");
                const url = `${mainUrl}UpdateCopySetting`;
                $.post(url, { copySettingId: name, copy: value })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
            container.on("change", ".data-to-copy input[type='checkbox']", function () {
                const checkbox = $(this);
                const name = checkbox.attr("name");
                const value = checkbox.prop("checked");
                const url = `${mainUrl}UpdateCopyMainSetting`;
                $.post(url, { name: name, copy: value })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
            
        });
    }

    searchResultDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            iManage.getDefaultGridImage(this);
            docViewer.getDefaultGridImage(this);
        }
    }

    refreshCaseInfo() {
        const baseUrl = $("body").data("base-url");
        $.get(`${baseUrl}/Patent/Invention/GetDetails`, { id: this.currentRecordId })
         .done((data)=> {
             const comboBox = this.getKendoComboBox("DisclosureStatus");
             comboBox.value(data.DisclosureStatus);
         })
         .fail(function (e) {
                pageHelper.showErrors(e.responseText);
         });  
    }

    applicationsGridDataBound = (e) => {
        const grid = $("#inventionApplicationGrid").data("kendoGrid");
        const dataSource = grid.dataSource;
        const data = dataSource.data();
        const sort = dataSource.sort();
        const query = new kendo.data.Query(data);
        const sortedData = query.sort(sort).data;
        this.applicationAppIds = sortedData.map(e => e.AppId);
    }

    /* related invention */
    onChange_RelatedInvention = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const invTitle = e.dataItem["InvTitle"];
            const disclosureStatus = e.dataItem["DisclosureStatus"];
            const disclosureDate = e.dataItem["DisclosureDate"];
            
            const grid = $("#inventionRelatedInventionsGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.RelatedInvId = e.dataItem["InvId"];
            dataItem.InvTitle = invTitle;
            dataItem.DisclosureStatus = disclosureStatus;
            dataItem.DisclosureDate = disclosureDate;

            $(row).find(".title-field").html(kendo.htmlEncode(invTitle));
            $(row).find(".status-field").html(kendo.htmlEncode(disclosureStatus));
            $(row).find(".date-field").html(kendo.htmlEncode(disclosureDate));
        }
    }

    /* related gm */
    onChange_RelatedMatter = (e,gridName) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const caseNumber = e.dataItem["CaseNumber"];
            const subCase = e.dataItem["SubCase"];
            const matterType = e.dataItem["MatterType"];
            const matterTitle = e.dataItem["MatterTitle"];

            const grid = $(`#${gridName}`).data("kendoGrid");
            const dataItem = grid.dataItem(row);

            dataItem.MatId = e.dataItem["MatId"];
            dataItem.RelatedCaseNumber = caseNumber;
            dataItem.RelatedSubCase = subCase;
            dataItem.MatterType = matterType;
            dataItem.MatterTitle = matterTitle;
            dataItem.dirty = true;

            $(row).find(".subCase-field").html(kendo.htmlEncode(subCase));
            $(row).find(".matterType-field").html(kendo.htmlEncode(matterType));
            $(row).find(".matterTitle-field").html(kendo.htmlEncode(matterTitle));

            grid.dataSource.trigger("change");
        }
    }

    showDetails(result) {
        let id = result;
        if (isNaN(result))
            id = result.id;

        pageHelper.showDetails(this, id, () => {
            pageHelper.handleEmailWorkflow(result);
        });
    }

    countryApplicationSearchParam = () => {
        const CountryApplicationInvId = document.getElementById("CountryApplicationInvId").value;
        const CountryApplicationCaseNumber = document.getElementById("CountryApplicationCaseNumber_patInventionDetail").value;
        const CountryApplicationCountry = document.getElementById("CountryApplicationCountry_patInventionDetail").value;
        const CountryApplicationSubCase = document.getElementById("CountryApplicationSubCase_patInventionDetail").value;
        const CountryApplicationCaseType = document.getElementById("CountryApplicationCaseType_patInventionDetail").value;
        const CountryApplicationStatus = document.getElementById("CountryApplicationStatus_patInventionDetail").value;
        const CountryApplicationAppNumber = document.getElementById("CountryApplicationAppNumber_patInventionDetail").value;
        const FilDateFrom = document.getElementById("FilDateFrom_patInventionDetail").value !== "" ? new Date(document.getElementById("FilDateFrom_patInventionDetail").value) : "";
        const FilDateTo = document.getElementById("FilDateTo_patInventionDetail").value !== "" ? new Date(document.getElementById("FilDateTo_patInventionDetail").value) : "";
        const CountryApplicationPatNumber = document.getElementById("CountryApplicationPatNumber_patInventionDetail").value;
        const IssDateFrom = document.getElementById("IssDateFrom_patInventionDetail").value !== "" ? new Date(document.getElementById("IssDateFrom_patInventionDetail").value) : "";
        const IssDateTo = document.getElementById("IssDateTo_patInventionDetail").value !== "" ? new Date(document.getElementById("IssDateTo_patInventionDetail").value) : "";
        return {
            CountryApplicationInvId: CountryApplicationInvId,
            CountryApplicationCaseNumber: CountryApplicationCaseNumber,
            CountryApplicationCountry: CountryApplicationCountry,
            CountryApplicationSubCase: CountryApplicationSubCase,
            CountryApplicationCaseType: CountryApplicationCaseType,
            CountryApplicationStatus: CountryApplicationStatus,
            CountryApplicationAppNumber: CountryApplicationAppNumber,
            FilDateFrom: FilDateFrom,
            FilDateTo: FilDateTo,
            CountryApplicationPatNumber: CountryApplicationPatNumber,
            IssDateFrom: IssDateFrom,
            IssDateTo: IssDateTo
        };
    };

    CountryApplicationGridRead = () => {
        const countryApplicationsGrid = $("#inventionApplicationGrid").data("kendoGrid");
        countryApplicationsGrid.dataSource.read();
    };

    onChange_Product = (e) => {
        if (e.sender) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const pageId = this.mainDetailContainer;
            const productsGrid = $(`#productsGrid_${pageId}`).data("kendoGrid");
            const dataItem = productsGrid.dataItem(row);

            var comboDataItem = e.sender.dataItem();
            dataItem.ProductId = comboDataItem["ProductId"];
            dataItem.ProductName = comboDataItem["ProductName"];

        }
    }

    productRefreshIndicator = (e) => {
        const data = e.sender.dataSource.data();
        if (data.length == 0)
            $("#inventionProductsTab").removeClass("has-products");
        else
            $("#inventionProductsTab").addClass("has-products");
    }

    invProductsGrid_AfterSubmit = (result) => {
        this.updateRecordStamps();
        pageHelper.handleEmailWorkflow(result);
    }

    inventorsGrid_AfterSubmit = () => {
        this.updateRecordStamps();
        const pageId = this.mainDetailContainer;
        const inventorsGrid = $(`#inventorsGrid_${pageId}`).data("kendoGrid");
        inventorsGrid.dataSource.read();
    }

    showAward = (e, grid) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        e.preventDefault();
        const parent = $("#" + e.delegateTarget.id).parent();
        const url = parent.data("url-award") + "?id=" + dataItem.ParentId + "&inventorid=" + dataItem.InventorID + "&inventor=" + dataItem.InventorDetail.Inventor + "&module=inv";

        $.ajax({
            url: url,
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                $("#awardDialog").modal('show');;
            },
            error: function (e) {
                pageHelper.showErrors(e.responseText);
            }
        });
    }

    openAwardMassUpdateEntry(grid, url, data, closeOnSave) {
        const self = this;

        $.ajax({
            url: url,
            data: data,
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#inventorAwardMassUpdateEntryDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: closeOnSave,
                        beforeSubmit: function () {
                        },
                        afterSubmit: function (e) {
                            grid.dataSource.read();
                            self.updateRecordStamps();
                            dialogContainer.modal("hide");

                            if (e.emailWorkflows) {
                                const promise = pageHelper.handleEmailWorkflow(e);
                                promise.then(() => {
                                });
                            }
                        }
                    }
                );
            },
        });
    }

    onDueDateAttorneyChange = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $(`#actionsGrid_${this.mainDetailContainer}`).data("kendoGrid");
            const dataItem = grid.dataItem(row);

            dataItem.AttorneyID = e.dataItem["AttorneyID"];
        }
    }

    clearDueDateAttorney = (e) => {
        const row = $(`#${e.sender.element[0].id}`).closest("tr");
        const grid = $(`#actionsGrid_${this.mainDetailContainer}`).data("kendoGrid");
        const dataItem = grid.dataItem(row);

        if (dataItem.DueDateAttorneyName == null || dataItem.DueDateAttorneyName == "") {
            dataItem.AttorneyID = null;
            dataItem.DueDateAttorneyName = "";
        }
    }

    showDueDateEmailScreen(e, grid) {
        const form = $("#" + this.detailContentContainer).find("form");
        const url = form.data("duedate-email-url");
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const ddId = dataItem.DDId;

        $.ajax({
            url: url,
            data: { id: ddId },
            success: function (result) {
                const popupContainer = $(".site-content .popup");
                popupContainer.html(result);
            },
            error: function (e) {
                pageHelper.showErrors(e);
            }
        });
    }

    deleteDueDate = (e, grid) => {
        this.deleteGridRow(e, grid);
    }

    searchResultGridRequestEnd = (e) => {
        cpiStatusMessage.hide();
        this.mainSearchRecordIds = [];

        if (e.response) {
            $(this.refineSearchContainer).find(".total-results-count").html(e.response.Total);

            if (e.response.Data.length > 0) {
                if (this.isSharePointIntegrationOn) {

                    const baseUrl = $("body").data("base-url");
                    const authenticatedCheckUrl = `${baseUrl}/Shared/SharePointGraph/IsAuthenticated`;

                    $.get(authenticatedCheckUrl)
                        .done(function () {
                            getThumbnails();
                        })
                        .fail(function (e) {
                            if (e.status == 401) {
                                const baseUrl = $("body").data("base-url");
                                const url = `${baseUrl}/Graph/SharePoint`;

                                sharePointGraphHelper.getGraphToken(url, () => {
                                    getThumbnails();
                                });
                            }
                            else {
                                pageHelper.showErrors(e.responseText);
                            }
                        });

                    function getThumbnails() {
                        const url = `${baseUrl}/Shared/SharePointGraph/GetDefaultWithThumbnailUrl?docLibrary=Patent`;
                        let driveId = "";

                        e.response.Data.forEach((r) => {
                            const recKey = { Id: r.InvId, RecKey: r.SharePointRecKey };
                            $.post(url, { docLibraryFolder: 'Invention', driveId, recKey })
                                .done(function (result) {
                                    if (result && result.DriveId) {
                                        driveId = result.DriveId;
                                        const element = $(`#inv-sr-${result.Id}`);
                                        element.attr("src", result.ThumbnailUrl);
                                        element.data("display-url", result.DisplayUrl);
                                    }
                                })
                                .fail(function (e) {
                                    pageHelper.showErrors(e.responseText);
                                });
                        });

                    }

                }

                this.mainSearchRecordIds = e.response.Ids;
                $(this.searchResultContainer).find(".no-results-hide").show();
            }
            else if (this.showNoRecordError) {
                const form = $(`${this.searchContainer}-MainSearch`);
                pageHelper.showErrors($(form).data("no-results") || $("body").data("no-results"));
                $(this.searchResultContainer).find(".no-results-hide").hide();
            }
        }
    }

    compensationEndDateChange = (e) => {
        const input = $(e.sender.element);
        const dateValue = input.data('kendoDatePicker').value();
        const form = $(this.refineSearchContainer);
        const compensationEndDate = document.getElementById("CompensationEndDate");

        if (dateValue)
            compensationEndDate.value = pageHelper.cpiDateFormatToSave(dateValue);
        else
            compensationEndDate.value = null;
    }

    onRemunerationChange = (e) => {
        var comboBox = e.sender;
        let selectedValue = comboBox.value();
        const compensationEndDate = document.getElementById("CompensationEndDate");
        if (selectedValue != "German")
            compensationEndDate.value = null;
    }

    onRemunerationCheckboxChange = (e) => {
        var checkbox = document.getElementById("UseInventorRemuneration");
        const compensationEndDate = document.getElementById("CompensationEndDate");
        if (!checkbox.checked)
            compensationEndDate.value = null;
    }
}