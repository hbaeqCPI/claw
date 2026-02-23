import SearchPage from "../searchPage";

export default class AuditTrail extends SearchPage {
    baseUrl = "";

    constructor() {
        super();
        this.headerGridResultRow = 0;
    }

    initializeAuditTrail(url) {
        this.baseUrl = url;
        //$("#logDataContainer").hide();          // hide log data section

        const searchCriteriaContainer = $("#auditTrailSearchMainTabContent");

        // refresh sub-criteria       
        const rdoSystem = searchCriteriaContainer.find("#AudSystemType input"); 

        this.refreshSubCriteria($("#AudSystemType").find("input:checked")[0].value);

        // attach event to system radio button
        rdoSystem.on("change", (e) => {
            const cbTableId = $("#TableId").data("kendoDropDownList");            
            cbTableId.dataSource.read({ systemType: $(e.target).val() });            

            this.refreshSubCriteria($(e.target).val());
        });
    }

    getSystemType() {       
        return {
            systemType: $("#AudSystemType").find("input:checked")[0].value
        };
    }

    refreshSubCriteria=(system)=> {
        $.ajax({
            url: this.baseUrl + 'GetSystemCriteria',
            type: "GET",
            dataType: "html",
            data: { systemType: system },
            success: function (data) {
                $('#searchSubCriteria').html(data);
            }
        });
    }

    search=(e)=> {
        const data = this.getCriteria('#auditSearchCriteriaForm');
        $("#auditTrailSearchResults-Grid").data("kendoGrid").dataSource.read(data);
        $("#searchPanel").data("kendoPanelBar").collapse($("li.k-state-active"));   // hide search panel
        $("#logDataContainer").show();          // show log data section
    }

    getCriteria(name) {
        $(".k-loading-mask").hide();
        cpiLoadingSpinner.show("", 1);
        const data = {};
        const form = $(name).serializeArray();

        $.each(form, function () {
            if (this.value) {
                data[this.name] = this.value;
            }
        });
        return data;
    }

    headerGridChange=()=> {
        const data = this.getSystemType();

        const grid = $("#auditTrailSearchResults-Grid").data("kendoGrid");
        const selectedItem = grid.dataItem(grid.select());
        if (selectedItem !== null) {
            data["audTrailId"] = selectedItem.AudTrailId;

            $('#auditKeyGrid').data('kendoGrid').dataSource.read(data);
            $('#auditDetailGrid').data('kendoGrid').dataSource.read(data);
        }
    }

    logHeaderdataBound = (e) => {
        e.sender.select('tr:eq(1)');
        cpiLoadingSpinner.hide();
        
        var gridElement = $(e.sender.element[0]);
        var newHeight = gridElement.height();               

        var headerHeight = $("#auditTrailSearchResults-Grid .k-grid-header").height();
        var pagerHeight = $("#auditTrailSearchResults-Grid .k-grid-pager").height();
        var rowHeight = $("#auditTrailSearchResults-Grid tr:last").height();

        var numberOfRows = Math.round((newHeight - headerHeight - pagerHeight) / rowHeight);

        if (Math.abs(this.headerGridResultRow - numberOfRows) < 2) {
            return;
        }

        gridElement.data("kendoGrid").dataSource.pageSize(numberOfRows); 
        gridElement.data("kendoGrid").resize();        

        this.headerGridResultRow = numberOfRows;
    } 

    exportToExcel = (e) => {
        var searchCriteria = this.getCriteria(this.refineSearchContainer);
        if (searchCriteria.__RequestVerificationToken) { delete searchCriteria.__RequestVerificationToken; }        
        var url = this.baseUrl + 'ExportFile?';
        for (var data in searchCriteria) {
            url = url + data + '=' + searchCriteria[data] + '&';
        }
        url = url.substring(0, url.length - 1);       
        
        window.location = url;
        cpiLoadingSpinner.hide();
    }
}

