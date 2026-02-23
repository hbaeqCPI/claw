
export default class DMSReviewPage {

    getCriteria() {
        let form = $("#searchCriteriaForm").serializeArray();
        let data = {};
        $.each(form, function () {
            data[this.name] = this.value;
        });
        return data;
    }

    openLink(url) {
        $.get(url)
            .done(function (result) {
                const popupContainer = $("#reviewContainer").find(".cpiContainerPopup");
                popupContainer.html(result);
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });
    }

    editRemarks(e, dataItem) {
        e.preventDefault();
        const grid = $("#" + e.delegateTarget.id).data("kendoGrid");
        const selectedItem = dataItem.dataItem($(e.currentTarget).closest("tr"));
        const url = grid.dataSource.transport.options.update.url + "Remarks/" + selectedItem.DMSId;
        pageHelper.openLink(url);
    }

    editRecommendation(e, dataItem) {
        e.preventDefault();
        const grid = $("#" + e.delegateTarget.id).data("kendoGrid");
        const selectedItem = dataItem.dataItem($(e.currentTarget).closest("tr"));
        const url = grid.dataSource.transport.options.update.url + "Recommendation/" + selectedItem.DMSId;
        pageHelper.openLink(url);
    }

    editRating(e, dataItem) {
        e.preventDefault();
        const grid = $("#" + e.delegateTarget.id).data("kendoGrid");
        const selectedItem = dataItem.dataItem($(e.currentTarget).closest("tr"));
        const url = grid.dataSource.transport.options.update.url + "Rating/" + selectedItem.DMSId;
        pageHelper.openLink(url);
    }

    refreshSearchResultGrid() {
        const searchResultContainer = $("#searchResultContainer");
        const grid = searchResultContainer.find("#searchResultGrid");
        if (grid.length > 0) {
            const data = getCriteria();
            grid.data("kendoGrid").dataSource.read(data);
        }

    }
}


