import ActivePage from "../activePage";

export default class PacQuestionnairePage extends ActivePage {

    constructor() {
        super();
    }

    initialize = (screen, id) => {
        this.editableGrids = [
            { name: "pacQuestionGuideGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps }
        ];

        this.tabsLoaded = [];
        this.tabChangeSetListener();
    }

    tabChangeSetListener = () => {
        const self = this;
        $('#pacQuestionnaire-tab a').on('click',
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

    //onQuestionGuideDataBound(e) {
    //    var items = e.sender.items();
    //    var guideGrid = $("#pacQuestionGuideGrid").data("kendoGrid");
    //    items.each(function (index) {
    //        var dataItem = guideGrid.dataItem(this);
    //        if (dataItem.QuestionId == 0 || dataItem.CanEdit) {
    //            this.cells[2].className += "editable-cell";
    //        }            
    //    })
    //}
}
