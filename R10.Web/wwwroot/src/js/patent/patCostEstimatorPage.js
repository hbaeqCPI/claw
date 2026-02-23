import ActivePage from "../activePage";

export default class PatCostEstimator extends ActivePage {

    constructor() {
        super();
        this.canChangeCountryDetailGridSelection = true;
        this.countryDetailGridSelectedCECountryId = 0;
        this.countryCostGridRefresh = false;
        this.countryDetailGridCurrentPage = 0;        
    }

    initialize = (screen, id) => {
        this.canChangeCountryDetailGridSelection = true;
        this.countryDetailGridSelectedCECountryId = 0;
        this.countryDetailGridCurrentPage = 0;

        this.tabsLoaded = [];
        this.tabChangeSetListener();

        $(document).ready(() => {

            const countriesGrid = $(`#patCostEstimatorCountriesGrid`);
            countriesGrid.on("click", ".countryLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = countriesGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.Country);
                pageHelper.openLink(linkUrl, false);
            });
        });
    }

    initializeSidebarPage(sidebarPage) {
        //$(".kendo-Grid .k-grid-toolbar").addClass("sidebar-link");
        super.initializeSidebarPage(sidebarPage);
        this.sidebar.container.addClass("collapse-lg");
    }

    tabChangeSetListener = () => {
        const self = this;
        $('#patCostEstimator-tab a').on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;            
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
            else if (tab == "costEstimatorDetailCountryTab") {
                const countryGrid = $("#patCostEstimatorCountriesGrid").data("kendoGrid");
                countryGrid.dataSource.read();
            }
        });

        this.fieldSetListener();
    }   

    fieldSetListener() {
        let form = $("#costEstimatorDetailsView-Content").find("form")[0];
        form = $(form);

        const self = this;
        $(document).ready(function () {
            
            form.find("input[name='Applicant']").each(function () {
                $(this).bind("click", function () {
                    var keyId = $("#KeyId").val();
                    if (keyId == 0) return true;

                    var confirmChange = confirm(form.data("label-applicant-change-confirm"));
                    if (confirmChange != true)
                        return false;
                });                
            });

            form.find("#chartShowBudget").each(function () {
                $(this).bind("click", function () {
                    const stackedChart = $("#countryCostStackedChart").data("kendoChart");
                    if (stackedChart) {
                        stackedChart.dataSource.read();
                    }
                });
            });
        });
    }       

    loadTabContent(tab) {
        switch (tab) {
            case "costEstimatorDetailCountryTab":
                $(document).ready(() => {
                    this.setCountriesGrid();
                });
                break;

            case "costEstimatorDetailAppDetailTab":
                $(document).ready(() => {
                    const countryGrid = $("#patCostEstimatorCountryDetailGrid").data("kendoGrid");
                    countryGrid.dataSource.read();
                });
                break;
            
            case "":
                break;
        }
    }

    getEstimateCosts() {        
        let mainForm = $(`#${this.mainDetailContainer}`).find("form")[0];
        mainForm = $(mainForm);
        const getEstimateCostsUrl = mainForm.data("total-estimate-cost-url");
        const data = {
            id: $("#KeyId").val()            
        };

        $.get(getEstimateCostsUrl, data)
            .done((result) => {
                if (result.totalCostEstimate) {
                    $("#totalCostLabel").text(kendo.toString(result.totalCostEstimate, 'n0'));
                }
                else {
                    $("#totalCostLabel").text(kendo.toString(0, 'n0'));
                }
                
                if (result.countryCosts) {
                    $("#viewModelEstimateCost").addClass('d-none');                
                    const countryCostTemplate = "<div class='col-4'><span style='text-decoration: none; font-weight: 500; color: var(--text-label);'><span>actualValue</span></span></div>";
                    const countryContainer = $("#countryCostsContainer");
                    countryContainer.empty();
                    $.each(JSON.parse(result.countryCosts), function (index, item) {                   
                        var itemTemplate = countryCostTemplate.replace('actualValue', item.Category);
                        countryContainer.append(itemTemplate);
                    });
                    $("#jsEstimateCost").removeClass('d-none');
                }
                
                pageHelper.updateRecordStamps(this);  
            })
            .fail((error) => {
                console.log('error: ', error);
            });

        var stackedChart = $("#countryCostStackedChart").data("kendoChart");
        if (stackedChart) {
            stackedChart.dataSource.read();
        }

        const countryDetailGrid = $("#patCostEstimatorCountryDetailGrid").data("kendoGrid");
        if (countryDetailGrid) {
            countryDetailGrid.dataSource.read();
            if (this.countryDetailGridCurrentPage > 0){
                countryDetailGrid.dataSource.page(this.countryDetailGridCurrentPage);
            }
        }        
    }

    getParentTStamp = () => {
        const container = $(`#${this.detailContentContainer}`);
        const tStamp = container.find("input[name='tStamp']");
        return tStamp.val();
    }    

    //onBaseAppSelect = (e) => {        
    //    const baseCaseNumber = $("#baseCaseNumber");
    //    const baseCountry = $("#baseCountry");
    //    const baseSubCase = $("#baseSubCase");
    //    const appStatus = $("#baseApplicationStatus");
    //    const priorityDate = $("#basePriorityDate");

    //    baseCaseNumber.val(e.dataItem.CaseNumber);
    //    baseCountry.val(e.dataItem.Country);
    //    baseSubCase.val(e.dataItem.SubCase);
    //    appStatus.val(e.dataItem.ApplicationStatus);
    //    priorityDate.val(e.dataItem.PriorityDate);

    //    $("#AppId").val(e.dataItem.AppId);
    //    $("#InvId").val(e.dataItem.InvId);
    //    if (e.dataItem.AppId != 0) $("#InvId").val(0);
    //}

    baseApplicationSearchValueMapper = (options) => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/CostEstimator/BaseApplicationSearchValueMapper`;
        
        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    onBaseAppChange = (e) => {       
        var dataItem = e.sender.dataItem();
        const baseCaseNumber = $("#baseCaseNumber");
        const baseCountry = $("#baseCountry");
        const baseSubCase = $("#baseSubCase");
        const appStatus = $("#baseApplicationStatus");
        const priorityDate = $("#basePriorityDate");        

        if (dataItem === undefined) {
            $("#AppId").val(0);
            $("#InvId").val(0);
            baseCaseNumber.val('');
            baseCountry.val('');
            baseSubCase.val('');
            appStatus.val('');
            priorityDate.val('');
        }
        else {
            baseCaseNumber.val(dataItem.CaseNumber);
            baseCountry.val(dataItem.Country);
            baseSubCase.val(dataItem.Subcase);
            appStatus.val(dataItem.ApplicationStatus);
            priorityDate.val(dataItem.ParentFilDate);

            $("#AppId").val(dataItem.AppId);
            $("#InvId").val(dataItem.InvId);
            if (dataItem.AppId != 0) $("#InvId").val(0);
        }
        baseCaseNumber.trigger("change");
        baseCountry.trigger("change");
        baseSubCase.trigger("change");
        appStatus.trigger("change");
        priorityDate.trigger("change");
    }

    onCountryDetailGridChange = (e) => {
        const countryDetailGrid = $('#patCostEstimatorCountryDetailGrid').data('kendoGrid');        
        var dataItem = countryDetailGrid.dataItem(e.sender.select())        
        if (dataItem) {
            if (this.canChangeCountryDetailGridSelection) {
                const questionGrid = $('#patCostEstimatorCountryCostGrid').data('kendoGrid');
                questionGrid.dataSource.read({ id: dataItem.KeyId, ceCountryId: dataItem.CECountryId });
                questionGrid.dataSource.page(1);
                questionGrid.table.off("click", ".answer-box");
                questionGrid.table.on("click", ".answer-box", function () { return false; });
                this.countryDetailGridSelectedCECountryId = dataItem.CECountryId;
            }
            else { 
                countryDetailGrid.unbind("change", this.onCountryDetailGridChange);
                countryDetailGrid.clearSelection();            
                var previousSelected = countryDetailGrid.dataSource.get(this.countryDetailGridSelectedCECountryId);
                if (previousSelected) {
                    var row = countryDetailGrid.table.find("tr[data-uid='" + previousSelected.uid + "']");
                    countryDetailGrid.select(row);
                }
                countryDetailGrid.bind("change", this.onCountryDetailGridChange);

                if (this.countryDetailGridSelectedCECountryId !== dataItem.CECountryId) {
                    let form = $("#costEstimatorDetailsView-Content").find("form")[0];
                    var errorMsg = $(form).data("dirty-cost-grid-warning");
                    pageHelper.showErrors(errorMsg);
                }                
            }
        }               
    }

    onCountryDetailGridDataBound = (e) => {
        var grid = e.sender;
        if (grid.dataSource.view().length > 0) { 
            if (this.countryDetailGridSelectedCECountryId > 0) {                
                var previousSelected = grid.dataSource.get(this.countryDetailGridSelectedCECountryId);
                if (previousSelected) {
                    var row = grid.table.find("tr[data-uid='" + previousSelected.uid + "']");
                    grid.select(row);
                }
            }
            else {
                grid.select(grid.tbody.find('tr:first')); 
            }            
        }
        else {
            const countryCostGrid = $("#patCostEstimatorCountryCostGrid").data("kendoGrid");
            if (countryCostGrid) {
                countryCostGrid.dataSource.read();
            }
        }
    }

    onCountryDetailGridPage = (e) => {
        this.countryDetailGridCurrentPage = e.page;
        var grid = e.sender;
        grid.clearSelection();
        this.countryDetailGridSelectedCECountryId = 0;
    }    

    getSelectedCountry() {
        const countriesGrid = $('#patCostEstimatorCountryDetailGrid').data('kendoGrid');        
        var dataItem = countriesGrid.dataItem(countriesGrid.select())
        var ceCountryId = 0;
        if (dataItem) {
            ceCountryId = dataItem.CECountryId;
        }
        return { ceCountryId: ceCountryId };
    }
    
    setCountriesGrid() {
        const countriesMap = $("#costEstCountry-map").data("kendoMap");
        const el = $("#patCostEstimatorCountriesGrid")
        const grid = el.data("kendoGrid");
        const self = this;
        const popUpTitle = `<div class="h2"><div>Cost Estimator Countries</div></div>`;

        if (grid) {
            grid.dataSource.read();
            const parent = el.parent();

            el.find(".k-grid-toolbar").on("click",
                ".k-grid-AddCountries",
                function (e) {
                    self.generateCountryies("All", false, true);                    
                })               
        }
    }

    onEPOClick(cbx, e) {      
        if (cbx.checked) {
            e.preventDefault();
            this.generateCountryies("EPO", cbx.checked);  
        }        
    }

    onPCTClick(cbx, e) {        
        if (cbx.checked) {
            e.preventDefault();
            this.generateCountryies("PCT", cbx.checked);  
        }        
    }

    generateCountryies(countrySource, isChecked, isManual = false) {
        let mainForm = $(`#${this.mainDetailContainer}`).find("form")[0];
        mainForm = $(mainForm);
        const getCountryUrl = mainForm.data("show-countries-url");        
        const data = {
            id: $("#KeyId").val(),
            countrySource: countrySource,
            isManual: isManual,
            __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
        };

        const el = $("#patCostEstimatorCountriesGrid")
        const grid = el.data("kendoGrid");
        const countriesMap = $("#costEstCountry-map").data("kendoMap");

        const popUpTitle = mainForm.data("label-country-popup");        
        $.post(getCountryUrl, data)
            .done((result) => {
                cpiLoadingSpinner.hide();

                cpiConfirm.save(popUpTitle, result,
                    function () {

                        // get checked countries
                        let countries = [];
                        let countryList = [];
                        const countryTab = $("#costEstiamtorAllCountries");

                        $(countryTab.find("input")).each(function () {
                            //const ctryCode = $(this).attr("name");
                            const ceCountry = $(this).data();
                            if (this.checked && ceCountry !== undefined && ceCountry.cecountryid !== undefined) {
                                //countries += ctryCode + "|";
                                countries.push(ceCountry.cecountryid);
                                countryList.push(ceCountry);
                            }
                        });

                        if (countries.length <= 0) {
                            alert("No countries selected.");
                            throw "No countries selected.";
                        }
                        else {  
                            //Add new row(s) to grid and let user changes CaseType
                            if (isManual) {
                                var gridDS = grid.dataSource;     
                                countryList.forEach((item => {
                                    //Follow fields from view model used on the grid
                                    gridDS.insert(0, {
                                        CECountryId: item.cecountryid,
                                        CaseType: item.casetype,
                                        Country: item.code,
                                        CountryName: item.name,
                                        EntityStatus: item.entitystatus,
                                        EstimatedCost: 0,
                                        PatCostEstimator: null,
                                        PatCountry: null,
                                        CreatedBy: null,
                                        UpdatedBy: null,
                                        DateCreated: null,
                                        LastUpdate: null,
                                        tStamp: null,
                                        KeyId: $("#KeyId").val(),
                                        Source: "All",
                                        EntityId: 0,
                                    }).set("dirty", true);
                                }));    
                                                               
                                var lastCaseTypeCell = $("#patCostEstimatorCountriesGrid td:eq(2)");
                                if (lastCaseTypeCell && lastCaseTypeCell.length > 0) {                                          
                                    setTimeout(() => {                                        
                                        grid.editCell(lastCaseTypeCell[0]); 
                                        grid.select(grid.tbody.find("tr:first"));                                        
                                        grid.trigger("change");
                                    }, 50)
                                }                                
                            }
                            //Auto-save default countries if adding countries after saving/creating new costestimator record
                            else {
                                const saveCountryUrl = mainForm.data("save-countries-url");
                                const data = {
                                    id: $("#KeyId").val(),
                                    countries: countries,
                                    isChecked: isChecked,
                                    countrySource: countrySource,
                                    __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                                }

                                $.post(saveCountryUrl, data)
                                    .done(function (result) {
                                        cpiLoadingSpinner.hide();
                                        patCostEstimatorPage.showDetails(patCostEstimatorPage.currentRecordId);
                                    })
                                    .fail((error) => {
                                        cpiLoadingSpinner.hide();

                                        cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                                            grid.dataSource.read();
                                            countriesMap.layers[1].dataSource.read();
                                        });
                                    });
                            }
                            
                        }
                    }, true,
                    function () {
                        //patCostEstimatorPage.showDetails(patCostEstimatorPage.currentRecordId);
                    }, true);
            })
            .fail((error) => {
                cpiLoadingSpinner.hide();
                cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                    grid.dataSource.read();
                    countriesMap.layers[1].dataSource.read();
                });
            });
    }

    loadDesCountries = (e) => {
        const el = $(e.currentTarget);
        const keyId = $("#KeyId").val();
        const url = el.data("url");

        var clientCode = $("#DesClientCode").data("kendoDropDownList");
        var clientId = 0;
        if (clientCode)
            clientId = clientCode.value();

        if (url) {
            cpiLoadingSpinner.show();
            $.get(url, { keyId: keyId, clientId: clientId })
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    if (result && result.length > 0) {
                        for (const country of result) {
                            const chk = $(`#country_${country}:not(:disabled)`);
                            chk.prop("checked", false);
                            chk.click();
                        }
                    }
                    else {
                        alert("No countries to load.");
                    }
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    console.error(pageHelper.getErrorMessage(error));
                });
        }
    }

    markCopyCountries(check, container) {
        $("#" + container + " input").each(function () {
            this.checked = check;
        });
    }

    getCountry = (e) => {
        var grid = $("#patCostEstimatorCountriesGrid").data("kendoGrid");
        var selectedData = grid.dataItem(grid.select());
        return {
            country: selectedData.Country,
            keyId: selectedData.KeyId,
            request: e
        };
    }

    onChange_CountryCaseType = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $("#patCostEstimatorCountriesGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);

            const entityStatus = e.dataItem["EntityStatus"];   
            dataItem.EntityStatus = entityStatus;
            dataItem.CECountryId = e.dataItem["CECountryId"];

            //$(row).find(".entitystatus-field").html(entityStatus);
        }
    }

    afterInsert = (result) => {
        let id = result;
        if (isNaN(result))
            id = result.id;

        pageHelper.showDetails(this, id, () => {
            let isNewRec = result.isNewRec;
            if (isNewRec) {
                $(`#${this.mainDetailContainer}`).find("#costEstimatorDetailCountryTab").click();
                setTimeout(() => {
                    //$('.k-grid-AddCountries').trigger('click');
                    this.generateCountryies("All", false);
                }, 500)
                
            }            
        });
    }

    showCopyScreen() {
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#patCostEstimatorCopyDialog");
        let entryForm = dialogContainer.find("form")[0];
        dialogContainer.modal("show");
        const self = this;

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

    countryCostGridOnDirty = () => {
        this.canChangeCountryDetailGridSelection = false;
    }

    countryCostGridOnCancel = () => {
        this.canChangeCountryDetailGridSelection = true;
    }

    countryCostGridAfterSubmit = () => {
        this.canChangeCountryDetailGridSelection = true; 
        this.getEstimateCosts();
    }

    countryCostGridDataBound(e) {
        const gridId = e.sender.element[0].id;
        const grid = $("#" + gridId).data("kendoGrid");

        var dataSource = grid.dataSource;
        if (dataSource && !dataSource.filter()) {
            dataSource.filter({
                field: "IsVisible",
                operator: "eq",
                value: true
            });
        }

        grid.tbody.find('tr').each(function () {
            var item = grid.dataItem(this);
            kendo.bind(this, item);
        });
    }

    countryCostGridChange(e)  {
        // if (e.field === 'Answer' && (e.items[0].AnswerType == 'Date' || e.items[0].AnswerType == 'Date Range')) { 
        //     const answer = e.items[0].Answer; 
        //     e.items[0].Answer = kendo.toString(kendo.parseDate(answer, "ddd MMM dd yyyy HH:mm:ss 'GMT'zzz", 'en-US'),'yyyy-MM-ddTHH:mm:ss');
        // }; 

        if (e.field === 'Answer') {
            var grid = $("#patCostEstimatorCountryCostGrid").data("kendoGrid");        
            var dataItemList = grid.dataSource.data();
            var currentPage = grid.dataSource.page();
            const dataItem = e.items[0];
            const answer = dataItem.Answer;
            
            var needRefresh = false;
            if (dataItem.AnswerType.toLowerCase().indexOf('date') > -1) 
            {
                dataItem.Answer = kendo.toString(kendo.parseDate(answer, "ddd MMM dd yyyy HH:mm:ss 'GMT'zzz", 'en-US'), 'yyyy-MM-ddTHH:mm:ss');
            } 
            // else if (dataItem.AnswerType.toLowerCase() === 'bool' && !dataItem.CECCId && dataItem.FollowUp === true) 
            // {
            //     dataItemList.forEach(item => {
            //         if (item.CECCId > 0 && item.CECostId === dataItem.CECostId && item.uid !== dataItem.uid) {
            //             item.IsVisible = answer && answer.toLowerCase() === 'yes';
            //         }
            //     });
            //     needRefresh = true;
            // } 
            else if (dataItem.AnswerType.toLowerCase() === 'selection' 
                && ((!dataItem.CECCId || dataItem.CECCId <= 0) 
                && (!dataItem.CESubId || dataItem.CESubId <= 0))
                && dataItemList.some(item => item.CESubId > 0 && item.CECostId === dataItem.CECostId)) 
            {
                dataItemList.forEach(item => {
                    if (item.CESubId > 0 && item.CECostId === dataItem.CECostId && item.uid !== dataItem.uid) {
                        item.IsVisible = (answer && item.FollowUpSelection && answer.toLowerCase() === item.FollowUpSelection.toLowerCase()) ? true : false;
                    }
                });
                needRefresh = true;
            }

            if (needRefresh) {
                patCostEstimatorPage.countryCostGridRefresh = true;
                grid.dataSource.filter({
                    field: 'IsVisible',
                    operator: 'eq',
                    value: true
                });
                grid.refresh();
                grid.dataSource.page(currentPage);
            }
        }
    }
}
