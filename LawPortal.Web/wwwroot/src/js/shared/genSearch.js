import SearchPage from "../searchPage";

export default class GenSearch extends SearchPage {

    constructor() {
        super();
        
    }

    initializeGenSearch() {
        $(document).ready(() => {
            $(".container-crumbs").addClass("d-none");
            $("#genSearch-tab").parent().removeClass("mt-2");

            this.configureColumnSort();
           
            $("#genSearchContainer").append($("#genSearchSearchResults-Grid_pager"));
        });
    }

    configureColumnSort() {
        const container = $("#genSearchHeaderContainer");

        const self = this;
        container.find("a").each(function () {
            const event = $(this);
            const id = $(this).attr("id");

            $("#" + id).on('click', function () {
                self.addSortIcon(event);
            });
        });
    }

    addSortIcon(event) {
        const sortAscIcon = "k-icon k-i-sort-asc-sm";
        const sortDescIcon = "k-icon k-i-sort-desc-sm";
        let sortOrder = "ASC";
        const sortColumn = event.attr("id");
        const sortIconId = sortColumn + "SortIcon";
        const sortIcon = $("#" + sortIconId);

        this.hideSortIcons(sortColumn);

        if (sortIcon.length === 0) {
            event.append("<span id=" + sortIconId + " class='" + sortAscIcon + "'></span>");
        }
        else {
            const spanClass = sortIcon.attr("class");
            if (spanClass === undefined) {
                sortIcon.addClass(sortAscIcon);
            }
            else if (spanClass === sortAscIcon) {
                sortIcon.removeClass().addClass(sortDescIcon);
                sortOrder = "DESC";
            }
            else {
                sortIcon.removeClass().addClass(sortAscIcon);
            }
        }

        this.refreshListView(sortColumn, sortOrder);
    }

    hideSortIcons(columnClicked) {
        const headerContainer = $("#genSearchHeaderContainer");
        headerContainer.find("a").each(function () {
            const column = $(this).attr("id");

            // hide Sort icons except column clicked 
            if (columnClicked !== column) {
                const sortIcon = $("#" + column + "SortIcon");
                if (sortIcon.length > 0) {
                    sortIcon.removeClass();
                }
            }
        });
    }

    refreshListView(sortColumn, sortOrder) {
        const dataSource = $('#genSearchSearchResults-Grid').data('kendoListView').dataSource;
        const sortDescriptor = [{ field: sortColumn, dir: sortOrder.toLowerCase() }];
        const query = {
            sort: sortDescriptor,
            page: dataSource.page(),
            pageSize: dataSource.pageSize()
        };
        dataSource.query(query);
    }
        
    dataBound() {
        let highlightRow = 2;
        $("#genSearchSearchResults-Grid tr").each(function (index) {
            const row = index + 1;

            if (row >= highlightRow && row <= highlightRow) {
                $(this).addClass("altItem");

                if (row === highlightRow)
                    highlightRow = highlightRow + 2;
            }
        });
        cpiLoadingSpinner.hide();
    }   

    getCriteria(name) {            
        $(".k-loading-mask").hide();
        cpiLoadingSpinner.show("",1);
        const data = {};
        const form = $(name).serializeArray();        

        $.each(form, function () {
            if (this.value) {
                data[this.name] = this.value;
            }
        });
        return data;
    }

    getSystems(name) {
        let systems = "|";
        const systemTypes = $(name).find(".system-types input");

        $.each(systemTypes, function () {
            if (this.checked) {
                systems += this.value + "|";
            }
        });
        return { systemType: systems };
    }

    getDefaultSystems(name) {
        let systems = "|";
        const systemTypes = $(name).find(".def-system-types input");

        $.each(systemTypes, function () {
            if (this.checked) {
                systems += this.value + "|";
            }
        });
        return { systemType: systems };
    }

    redirectToRecord(link) {
        let url = $("body").data("base-url") + "/";                
            url = url + link;
        window.open(url, '_blank');
    }
}





