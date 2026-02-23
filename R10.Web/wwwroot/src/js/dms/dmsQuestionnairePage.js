import ActivePage from "../activePage";

export default class DMSQuestionnairePage extends ActivePage {

    constructor() {
        super();
    }

    initialize = (screen, id) => {
        this.editableGrids = [
            { name: "dmsQuestionGuideGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps }
        ];

        this.tabsLoaded = [];
        this.tabChangeSetListener();
    }

    tabChangeSetListener = () => {
        const self = this;
        $('#dmsQuestionnaire-tab a').on('click',
            (e) => {
                e.preventDefault();
                const tab = e.target.id;
                if (this.tabsLoaded.indexOf(tab) === -1) {
                    this.tabsLoaded.push(tab);
                    this.loadTabContent(tab);
                }
            });

        $(document).ready(function () {
            
        });
    }

    loadTabContent(tab) {
        switch (tab) {

            case "":
                break;
        }
    }

    getParentTStamp = () => {
        const container = $(`#${this.detailContentContainer}`);
        const tStamp = container.find("input[name='tStamp']");
        return tStamp.val();
    }   

    questionChildAnswerTypeChange = (e) => {
        const selectedType = $('#' + e.sender.element[0].id).data('kendoDropDownList').text();
        const childPlaceHolder = document.getElementById('childPlaceHolder');

        if (selectedType.toLowerCase() !== 'string') {
            childPlaceHolder.style.display = 'none';            
        }
        else {
            childPlaceHolder.style.display = 'block';
        }
    }

    questionSubAnswerTypeChange = (e) => {
        const selectedType = $('#' + e.sender.element[0].id).data('kendoDropDownList').text();
        const subPlaceHolder = document.getElementById('subPlaceHolder');

        if (selectedType.toLowerCase() !== 'string') {
            subPlaceHolder.style.display = 'none';            
        }
        else {
            subPlaceHolder.style.display = 'block';
        }
    }

    clientEntityFilterValueMapper = (options) => {
        var multiSelect = $("#ReviewerEntityFilterList_questionnaireDetail").data("kendoMultiSelect");
        const data = multiSelect.dataSource.data();

        var value = options.value;
        var dataTemp = [];
        value = $.isArray(value) ? value : [value];
        for (var idx = 0; idx < value.length; idx++) {
            var filteredData = data.find(c => c.ClientID === parseInt(value[idx]));
            if (filteredData) {
                dataTemp.push({ ClientID: filteredData.ClientID, ClientCode: filteredData.ClientCode });
            }            
        }

        setTimeout(function () { options.success(dataTemp); }, 100);        
    }

    areaEntityFilterValueMapper = (options) => {
        var multiSelect = $("#ReviewerEntityFilterList_questionnaireDetail").data("kendoMultiSelect");
        const data = multiSelect.dataSource.data();

        var value = options.value;
        var dataTemp = [];
        value = $.isArray(value) ? value : [value];
        for (var idx = 0; idx < value.length; idx++) {
            var filteredData = data.find(c => c.AreaID === parseInt(value[idx]));
            if (filteredData) {
                dataTemp.push({ AreaID: filteredData.AreaID, Area: filteredData.Area });
            }            
        }

        setTimeout(function () { options.success(dataTemp); }, 100);        
    }

    addQuestionToCurrent = (e, grid, popupTitle, popupMessage) => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/DMS/Questionnaire/AddQuestionToCurrent`;
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const data = { questionId: dataItem.QuestionId };

        cpiConfirm.confirm(popupTitle, popupMessage, function () {
            cpiLoadingSpinner.show();
            $.post(url, data)
                .done(function (response) {
                    cpiLoadingSpinner.hide();                
                    pageHelper.showSuccess(response.success);
                    grid.dataSource.read();
                })
                .fail((error) => {                
                    cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error)
                });
        });
    }
}
