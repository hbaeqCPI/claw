import ActivePage from "../activePage";

export default class AttorneyPage extends ActivePage {
    constructor() {
        super();
        this.timeTrackerParam = { SystemType: "", CaseNumber: "", Country: "", SubCase: "" };
    }

    initialize(attorneyId) {
        this.editableGrids = [
            {
                name: 'timeTrackersGrid', isDirty: false, filter: { attorneyId: attorneyId }, afterSubmit: this.updateRecordStamps
            }
        ];
    }

    caseNumberSearchValueMapper = (options) => {
        var result = { CaseNumber: options.value }
        setTimeout(function () { options.success(result); }, 300);
    }

    timeTrackerGetParam = () => {
        const systemType = this.timeTrackerParam.SystemType !== undefined ? this.timeTrackerParam.SystemType : "";
        const caseNumber = this.timeTrackerParam.CaseNumber !== undefined ? this.timeTrackerParam.CaseNumber : "";
        const country = this.timeTrackerParam.Country !== undefined ? this.timeTrackerParam.Country : "";
        return {
            systemType: systemType,
            caseNumber: caseNumber,
            country: country
        };
    }

    onEdit = (e) => {
        this.timeTrackerParam.SystemType = e.model.SystemType;
        this.timeTrackerParam.CaseNumber = e.model.CaseNumber;
        this.timeTrackerParam.Country = e.model.Country;
        this.timeTrackerParam.SubCase = e.model.SubCase;
    };

    timeTrackerSearchOnChange = () => {

    };

    timeTrackerSearchParam = () => {
        const searchAttorneyId = document.getElementById("SearchAttorneyId").value;
        const searchOutstandingOnly = document.getElementById("ShowOutstandingOnly").checked ? "0" : (document.getElementById("ShowExportedOnly").checked?"1":"2");
        const searchSystemType = document.getElementById("SearchSystemType_attorneyDetail").value;
        const searchCaseNumber = document.getElementById("SearchCaseNumber_attorneyDetail").value;
        const searchCountry = document.getElementById("SearchCountry_attorneyDetail").value;
        const searchSubCase = document.getElementById("SearchSubCase_attorneyDetail").value;
        const searchClientCode = document.getElementById("SearchClientCode_attorneyDetail").value;
        const entryDateFrom = document.getElementById("EntryDateFrom_attorneyDetail").value !== "" ? new Date(document.getElementById("EntryDateFrom_attorneyDetail").value) : "";
        const entryDateTo = document.getElementById("EntryDateTo_attorneyDetail").value !== "" ? new Date(document.getElementById("EntryDateTo_attorneyDetail").value) : "";
        return {
            SearchAttorneyId: searchAttorneyId,
            SearchOutstandingOnly: searchOutstandingOnly,
            SearchSystemType: searchSystemType,
            SearchCaseNumber: searchCaseNumber,
            SearchCountry: searchCountry,
            SearchSubCase: searchSubCase,
            SearchClientCode: searchClientCode,
            EntryDateFrom: entryDateFrom,
            EntryDateTo: entryDateTo
        };
    };

    TimeTrackerGridRead = () => {
        const timeTrackersGrid = $("#timeTrackersGrid").data("kendoGrid");
        timeTrackersGrid.dataSource.read();
    };

    RecordEditable = (e) => {
        return e.TimeTrackerId == 0;
    };  

    TimeTrackerEditable = (e) => {
        return !e.Exported;
    };

    EditableCellHandler = () => {

    document.querySelectorAll('.data-EntryDate').forEach(element => {
        var currentDataItem = $("#timeTrackersGrid").data("kendoGrid").dataItem($(element).closest("tr"));
        if (currentDataItem.TimeTrackerId != 0 && currentDataItem.Exported) {
            element.classList.remove("editable-cell");
        }
    });
    //document.querySelectorAll('.data-Duration').forEach(element => {
    //    var currentDataItem = $("#timeTrackersGrid").data("kendoGrid").dataItem($(element).closest("tr"));
    //    if (currentDataItem.TimeTrackerId != 0 && currentDataItem.Exported) {
    //        element.classList.remove("editable-cell");
    //    }
    //});
    document.querySelectorAll('.data-Description').forEach(element => {
        var currentDataItem = $("#timeTrackersGrid").data("kendoGrid").dataItem($(element).closest("tr"));
        if (currentDataItem.TimeTrackerId != 0 && currentDataItem.Exported) {
            element.classList.remove("editable-cell");
        }
    });
    document.querySelectorAll('.data-SystemType').forEach(element => {
        var currentDataItem = $("#timeTrackersGrid").data("kendoGrid").dataItem($(element).closest("tr"));
        if (currentDataItem.TimeTrackerId != 0) {
            element.classList.remove("editable-cell");
        }
    });
    document.querySelectorAll('.data-CaseNumber').forEach(element => {
        var currentDataItem = $("#timeTrackersGrid").data("kendoGrid").dataItem($(element).closest("tr"));
        if (currentDataItem.TimeTrackerId != 0) {
            element.classList.remove("editable-cell");
        }
        if (currentDataItem.CostTrackId == null || currentDataItem.CostTrackId == 0) {
            element.innerHTML = currentDataItem.CaseNumber;
        } else if (currentDataItem.SystemType == "Trademark") {
            element.children[0].setAttribute("href", element.children[0].getAttribute("href").replace("Patent/CostTracking", "Trademark/CostTracking"));
        } else if (currentDataItem.SystemType == "General Matter") {
            element.children[0].setAttribute("href", element.children[0].getAttribute("href").replace("Patent/CostTracking", "GeneralMatter/CostTracking"));
        }
    });
    document.querySelectorAll('.data-Country').forEach(element => {
        var currentDataItem = $("#timeTrackersGrid").data("kendoGrid").dataItem($(element).closest("tr"));
        if (currentDataItem.TimeTrackerId != 0) {
            element.classList.remove("editable-cell");
        }
    });
    document.querySelectorAll('.data-SubCase').forEach(element => {
        var currentDataItem = $("#timeTrackersGrid").data("kendoGrid").dataItem($(element).closest("tr"));
        if (currentDataItem.TimeTrackerId != 0) {
            element.classList.remove("editable-cell");
        }
    });
};
}





