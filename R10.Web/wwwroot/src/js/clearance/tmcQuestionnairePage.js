import ActivePage from "../activePage";

export default class TmcQuestionnairePage extends ActivePage {

    constructor() {
        super();
    }

    initialize = (screen, id) => {
        this.editableGrids = [
            { name: "tmcQuestionGuideGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps }
        ];

        this.tabsLoaded = [];
        this.tabChangeSetListener();
    }

    tabChangeSetListener = () => {
        const self = this;
        $('#tmcQuestionnaire-tab a').on('click',
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
    //    var guideGrid = $("#tmcQuestionGuideGrid").data("kendoGrid");
    //    items.each(function (index) {
    //        var dataItem = guideGrid.dataItem(this);
    //        if (dataItem.QuestionId == 0 || dataItem.CanEdit) {
    //            this.cells[2].className += "editable-cell";
    //        }            
    //    })
    //}
}
