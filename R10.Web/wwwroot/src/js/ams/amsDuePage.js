import ActivePage from "../activePage";

export default class AMSDuePage extends ActivePage {

    constructor() {
        super();
    }

    init() {
        this.tabsLoaded = [];
        this.tabChangeSetListener();
    }

    tabChangeSetListener() {
        $('#annuitiesDueDetailTab a').on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
        });
    }

    loadTabContent(tab) {
        switch (tab) {
            case "annuitiesDueDetailInstructionHistoryTab":
                $(document).ready(() => {
                    const grid = $(`#instructionHistoryGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "annuitiesDueDetailCorrespondenceTab":
                $(document).ready(() => {
                    const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "":
                break;
        }
    }
}