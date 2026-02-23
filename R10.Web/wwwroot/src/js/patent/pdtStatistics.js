import CompactSearchPage from "../compactSearchPage";

export default class PDTStatistics extends CompactSearchPage {

    constructor() {
        super();
        this.mainContainer = "pdtStatisticsSearch";
        this.reportType = 0;
        this.basedOn = 0;
        this.symbolTypeLabel = "";
        this.symbolType = "C";
        this.colorList = ['#52828b', '#4682B4']; //world, yours --#094ab2
        this.initSearch = false;
    }

    onSearchInitialized = () => {
        const container = $(`#${this.mainContainer}`);
        container.find("input[name='ReportType']").data("change-on-load", true);
        this.symbolTypeLabel = $("#symbolTypeCPC").text();

        container.on("change", "input[name='ReportType']", (e) => {
            const ipcCriteria = container.find(".by-ipc-criteria");
            this.basedOn = +(container.find("input[name='BasedOn']").data("kendoDropDownList").value());
            this.reportType = +(e.target.value);
            
            if (this.reportType === 3 || this.reportType === 5) {
                ipcCriteria.removeClass("d-none");
                ipcCriteria.find(".cpc-req").show();

                if (this.reportType === 5) {
                    ipcCriteria.find("#CPC").hide();
                    ipcCriteria.find("#CPC").val("");
                    ipcCriteria.find("#CPC2").show();
                    ipcCriteria.find("#cpc-val-msg").hide();
                }
                else {
                    ipcCriteria.find("#CPC").show();
                    ipcCriteria.find("#CPC2").hide();
                    ipcCriteria.find("#CPC2").val("");
                    ipcCriteria.find("#cpc-val-msg").show();
                }
                    
            }
            else if (this.reportType === 4 || this.reportType === 7) {
                ipcCriteria.addClass("d-none");
            }
            else if (this.reportType === 6) {
                ipcCriteria.removeClass("d-none");
                ipcCriteria.find("#CPC").show();
                ipcCriteria.find("#CPC2").hide();
                ipcCriteria.find("#CPC2").val("");
                ipcCriteria.find("#cpc-val-msg").show();
            }
            else {
                ipcCriteria.addClass("d-none");
               //ipcCriteria.find("#CPC").addClass("data-val-ignore");
            }
            this.toggleApplicantVisibility(this.basedOn);
        });
        container.on("change", "input[name='SymbolType']", (e) => {
            const container = $(`#${this.mainContainer}`);

            const cpcLabel = container.find("#symbolTypeCPC");
            const ipcLabel = container.find("#symbolTypeIPC");
            cpcLabel.toggleClass("d-none");
            ipcLabel.toggleClass("d-none");

            if (e.target.value === "C") {
                this.symbolTypeLabel = cpcLabel.text();
            }
            else {
                this.symbolTypeLabel = ipcLabel.text();
            }
            this.symbolType = e.target.value;
        });
        container.on("click", ".ipc-help", () => {
            const baseUrl = $("body").data("base-url");

            let url;
            if (this.symbolType==="C")
                url = `${baseUrl}/Shared/Weblinks/GetCPCHelpUrl`;
            else
                url = `${baseUrl}/Shared/Weblinks/GetIPCHelpUrl`;

            $.get(url)
                .done((result) => {
                    window.open(result, "_blank");
                })
                .fail((e => {
                    pageHelper.showErrors(e);
                }));
        });

        container.on("click", ".applicant-help", () => {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/PDTStatistics/ApplicantSelection`;

            $.get(url)
                .done((result) => {

                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(result);
                    const dialogContainer = $("#patStatApplicantSelectionDialog");
                    dialogContainer.modal("show");

                    dialogContainer.find("#apply").on("click", () => {
                        const grid = $("#pdtStatApplicantsGrid").data("kendoGrid");
                        const selected = grid.selectedKeyNames();

                        const allData = grid.dataSource.data();
                        const selection = allData.filter(item=> selected.findIndex(f=> +f===+item.Id) > -1);
                        const applicants = [... new Set(selection.map(({ Applicant }) => Applicant))].join('~');
                        const existingApplicants = container.find("#Applicant").val();

                        const applicant = container.find("#Applicant");
                        applicant.val(existingApplicants ? existingApplicants + "~" + applicants : applicants);
                        applicant.closest(".float-label").removeClass("inactive").addClass("active");
                        dialogContainer.modal("hide");
                    });
                })
                .fail((e => {
                    pageHelper.showErrors(e);
                }));
        });

        container.on("click", ".patstat-help", () => {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Shared/Weblinks/GetPatStatHelpUrl`;

            $.get(url)
                .done((result) => {
                    window.open(result, "_blank");
                })
                .fail((e => {
                    pageHelper.showErrors(e);
                }));
        });

        container.on("click", ".nav-chart", () => {
            container.find(".result-chart").removeClass("d-none");
            const grid = container.find(".kendo-Grid");
            grid.addClass("d-none");

            //const reportType = container.find("input[name='ReportType']:checked");
            //const reportTypeValue = +reportType.val();

            container.find("#viz").empty();
            container.find("#viz-legend").empty();

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/PDTStatistics/LoadChartParam?reportType=${this.reportType}`;

            $.get(url)
                .done((result) => {
                    container.find(".viz-params-detail").html(result);

                    //if (this.basedOn === 3 && this.reportType === 4) //w vs y, applicants
                    //    $("#viz-opt-4-based-on").closest("div").removeClass("d-none");

                    //if (this.basedOn === 3 && this.reportType === 5) //w vs y, applicants
                    //    $("#viz-opt-5-based-on").closest("div").removeClass("d-none");

                })
                .fail((e => {
                    pageHelper.showErrors(e);
                }));

        });

        container.on("submit", "#viz-params-form", (e) => {
            e.preventDefault();
            const json = pageHelper.formDataToJson($(e.target));

            //const reportType = container.find("input[name='ReportType']:checked");
            //const reportTypeValue = +reportType.val();
            //const selectedBasedOn = container.find("input[name='BasedOn']");
            //const basedOnValue = +selectedBasedOn.val();

            container.find("#viz").empty();
            container.find("#viz-legend").empty();
            this.drawChart(json.payLoad);
        });

        container.on("click", ".nav-grid", () => {
            container.find(".result-chart").addClass("d-none");
            container.find(".kendo-Grid").removeClass("d-none");
        });

        container.on("click", ".app-search-submit", () => {
            const searchText = $("#patStatApplicantSelectionDialog").find("#ApplicantSearch").val();
            if (searchText && searchText.length > 2) {
                const grid = $("#pdtStatApplicantsGrid").data("kendoGrid");
                grid.dataSource.read();
            }
        });
    }

    applicantSearchSelectionChange(parent) {
        const applyButton = $(`#${parent}`).find("#apply");

        const grid = $("#pdtStatApplicantsGrid").data("kendoGrid");
        if (grid.selectedKeyNames().length > 0)
            applyButton.removeAttr("disabled");
        else
            applyButton.attr("disabled", "disabled");
    }

    onSearchSubmit = () => {
        const container = $(`#${this.mainContainer}`);
        const reportType = container.find("input[name='ReportType']:checked");

        const reportTypeValue = +reportType.val();
        if (reportTypeValue === 3 || reportTypeValue === 5) {
            const ipcCriteria = container.find(".by-ipc-criteria");

            const ipc = ipcCriteria.find("#CPC"); //ipcCriteria.find("#IPC");
            ipc.removeClass("data-val-ignore");
            ipc.removeClass("input-validation-error");
            ipc.data("val", true);

            //if both are empty, display error
            //if (ipc.val().length === 0) {
            //    const cpc = ipcCriteria.find("#CPC");
            //    if (cpc.val().length > 0) {
            //        ipc.addClass("data-val-ignore");
            //        ipc.attr("data-val", "false");

            //    }
            //}
        }
        this.reportType = reportTypeValue;
        this.initSearch = true;
        return true;
    }

    setDefaults = () => {
        const container = $(`${this.searchForm}`);
        const basedOn = container.find("input[name='BasedOn']").data("kendoDropDownList");
        basedOn.value("3");
        container.find("#reportType1").prop("checked", true);
        container.find(".by-year-criteria .float-label").addClass("active").removeClass("inactive");
    }

    onSearchCleared = () => {
        this.setDefaults();
    }

    onSearchCriteriaLoaded = () => {
        const container = $(`#${this.mainContainer}`);
        const applicantCriteria = container.find(".by-applicant-criteria");
        this.basedOn = +(container.find("input[name='BasedOn']").data("kendoDropDownList").value());
        this.toggleApplicantVisibility(this.basedOn);

        //if (this.reportType === 4 || this.reportType === 5 || this.reportType === 7) {
        //    if (this.basedOn !== 1)
        //        applicantCriteria.removeClass("d-none");
        //    else
        //        applicantCriteria.addClass("d-none");
        //}
        //else {
        //    applicantCriteria.addClass("d-none");
        //}

    }

    onBasedOnSelect = (e) => {
        this.toggleApplicantVisibility(+e.dataItem.Value);
    }

    toggleApplicantVisibility = (basedOn) => {
        const container = $(`#${this.mainContainer}`);
        const reportType = container.find("input[name='ReportType']:checked").val();
        const applicantCriteria = container.find(".by-applicant-criteria");

        if (+reportType === 4 || +reportType === 5 || +reportType === 7) {
            applicantCriteria.removeClass("d-none");

            //your data
            if (basedOn === 1) {
                applicantCriteria.find(".ww-applicant").addClass("d-none");
                applicantCriteria.find(".your-applicant").removeClass("d-none");
            }
            else if (basedOn === 2) {
                applicantCriteria.find(".ww-applicant").removeClass("d-none");
                applicantCriteria.find(".your-applicant").addClass("d-none");
            }
            else {
                applicantCriteria.find(".ww-applicant").removeClass("d-none");
                applicantCriteria.find(".your-applicant").removeClass("d-none");
            }
        }
        else
            applicantCriteria.addClass("d-none");
    }

    applicantSearchParams = () => {
        return { searchText: $("#patStatApplicantSelectionDialog").find("#ApplicantSearch").val() };
    }

    onGridDataBound = () => {
        const container = $(`#${this.mainContainer}`);
        const selectedReportType = container.find("input[name='ReportType']:checked");
        const reportType = parseFloat(selectedReportType.val());
        const grid = $(this.searchResultGrid).data("kendoGrid");

        if (this.initSearch) {
            const page = grid.pager.page();
            if (page !== 1 && page > 0)
                grid.pager.page(1);
        }

        const label = $.trim(selectedReportType.next("label").text());
        $(".breadcrumb-item.active").text(label);

        container.find(".nav-chart").removeClass("d-none");
        container.find(".result-chart").addClass("d-none");
        container.find(".kendo-Grid").removeClass("d-none");

        const selectedBasedOn = container.find("input[name='BasedOn']");
        this.basedOn = +selectedBasedOn.val();

        $(this.searchResultGrid).find("thead [data-field='IPC'] .k-link").html(this.symbolTypeLabel);    

        if (reportType === 3 || reportType === 6) {
            grid.hideColumn(0);
            grid.showColumn(1);
            grid.showColumn(2);
            grid.showColumn(3);
        }
        else if (reportType === 4) {
            grid.showColumn(0);
            grid.hideColumn(1);
            grid.showColumn(2);
            grid.showColumn(3);
        }

        else if (reportType === 5 || reportType === 7) {
            grid.showColumn(0);
            grid.showColumn(1);
            grid.showColumn(2);
            grid.showColumn(3);

            if (reportType === 7)
                $(this.searchResultGrid).find("thead [data-field='IPC'] .k-link").html($("#symbolTypeCPC").text()); 
        }

        else {
            grid.hideColumn(0);
            grid.hideColumn(1);
            grid.showColumn(2);
            grid.showColumn(3);
        }

        //world vs yours
        if (this.basedOn === 3) {
            grid.hideColumn(5);
            grid.showColumn(6);
            grid.showColumn(7);
        }
        else {
            grid.showColumn(5);
            grid.hideColumn(6);
            grid.hideColumn(7);
        }
        this.initSearch = false;
    }

    /* Chart data accumulators */
    accumulateYear = (data) => {
        const helper = {};

        data.forEach((item) => {

            if (!helper[item.Year]) {
                helper[item.Year] = { Year: +item.Year, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            }
            else {
                helper[item.Year].Count += item.Count;
                helper[item.Year].WorldDataCount += item.WorldDataCount;
                helper[item.Year].YourDataCount += item.YourDataCount;
            }
        });
        const accuData = Object.keys(helper).map((k) => helper[k]);
        return accuData;
    }

    accumulateCountry = (data) => {
        const helper = {};

        data.forEach((item) => {
            if (!helper[item.Country]) {
                helper[item.Country] = { Country: item.Country, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            }
            else {
                helper[item.Country].Count += item.Count;
                helper[item.Country].WorldDataCount += item.WorldDataCount;
                helper[item.Country].YourDataCount += item.YourDataCount;
            }
        });
        const accuData = Object.keys(helper).map((k) => helper[k]);
        return accuData;
    }

    accumulateIPC = (data) => {
        const helper = {};

        data.forEach((item) => {

            if (!helper[item.IPC]) {
                helper[item.IPC] = { IPC: item.IPC, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            }
            else {
                helper[item.IPC].Count += item.Count;
                helper[item.IPC].WorldDataCount += item.WorldDataCount;
                helper[item.IPC].YourDataCount += item.YourDataCount;
            }
        });
        const accuData = Object.keys(helper).map((k) => helper[k]);
        return accuData;
    }

    accumulateCountryYear = (data) => {
        const accumulatedData = data.reduce((previous, item) => {
            const currentItem = { Country: item.Country, Year: item.Year, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            if (previous.length > 0) {
                var prevItem = previous.find(pItem => pItem.Country === item.Country && pItem.Year === item.Year);
                if (prevItem) {
                    prevItem.Count += item.Count;
                    prevItem.WorldDataCount += item.WorldDataCount;
                    prevItem.YourDataCount += item.YourDataCount;

                }
                else {
                    previous.push(currentItem);
                }
            }
            else
                previous.push(currentItem);

            return previous;
        }, []);
        return accumulatedData;
    }

    accumulateIPCYear = (data) => {
        const accumulatedData = data.reduce((previous, item) => {
            const currentItem = { IPC: item.IPC, Year: item.Year, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            if (previous.length > 0) {
                var prevItem = previous.find(pItem => pItem.IPC === item.IPC && pItem.Year === item.Year);
                if (prevItem) {
                    prevItem.Count += item.Count;
                    prevItem.WorldDataCount += item.WorldDataCount;
                    prevItem.YourDataCount += item.YourDataCount;

                }
                else {
                    previous.push(currentItem);
                }
            }
            else
                previous.push(currentItem);

            return previous;
        }, []);
        return accumulatedData;
    }

    accumulateIPCCountry = (data) => {
        const accumulatedData = data.reduce((previous, item) => {
            const currentItem = { IPC: item.IPC, Country: item.Country, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            if (previous.length > 0) {
                var prevItem = previous.find(pItem => pItem.IPC === item.IPC && pItem.Country === item.Country);
                if (prevItem) {
                    prevItem.Count += item.Count;
                    prevItem.WorldDataCount += item.WorldDataCount;
                    prevItem.YourDataCount += item.YourDataCount;

                }
                else {
                    previous.push(currentItem);
                }
            }
            else
                previous.push(currentItem);

            return previous;
        }, []);
        return accumulatedData;
    }

    //faster than using reducer
    accumulateCompany = (data) => {
        const helper = {};

        data.forEach((item) => {
            if (!helper[item.Company]) {
                helper[item.Company] = { Company: item.Company, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            }
            else {
                helper[item.Company].Count += item.Count;
                helper[item.Company].WorldDataCount += item.WorldDataCount;
                helper[item.Company].YourDataCount += item.YourDataCount;
            }
        });
        const accuData = Object.keys(helper).map((k) => helper[k]);
        return accuData;
    }

    accumulateCompanyCountry = (data) => {
        const accumulatedData = data.reduce((previous, item) => {
            const currentItem = { Company: item.Company, Country: item.Country, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            if (previous.length > 0) {
                var prevItem = previous.find(pItem => pItem.Company === item.Company && pItem.Country === item.Country);
                if (prevItem) {
                    prevItem.Count += item.Count;
                    prevItem.WorldDataCount += item.WorldDataCount;
                    prevItem.YourDataCount += item.YourDataCount;
                }
                else {
                    previous.push(currentItem);
                }
            }
            else
                previous.push(currentItem);

            return previous;
        }, []);
        return accumulatedData;
    }

    accumulateCompanyIPC = (data) => {
        const accumulatedData = data.reduce((previous, item) => {
            const currentItem = { Company: item.Company, IPC: item.IPC, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            if (previous.length > 0) {
                var prevItem = previous.find(pItem => pItem.Company === item.Company && pItem.IPC === item.IPC);
                if (prevItem) {
                    prevItem.Count += item.Count;
                    prevItem.WorldDataCount += item.WorldDataCount;
                    prevItem.YourDataCount += item.YourDataCount;
                }
                else {
                    previous.push(currentItem);
                }
            }
            else
                previous.push(currentItem);

            return previous;
        }, []);
        return accumulatedData;
    }

    accumulateCompanyYear = (data) => {
        const accumulatedData = data.reduce((previous, item) => {
            const currentItem = { Company: item.Company, Year: item.Year, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            if (previous.length > 0) {
                var prevItem = previous.find(pItem => pItem.Company === item.Company && pItem.Year === item.Year);
                if (prevItem) {
                    prevItem.Count += item.Count;
                    prevItem.WorldDataCount += item.WorldDataCount;
                    prevItem.YourDataCount += item.YourDataCount;
                }
                else {
                    previous.push(currentItem);
                }
            }
            else
                previous.push(currentItem);

            return previous;
        }, []);
        return accumulatedData;
    }

    accumulateCompanyIPCYear = (data) => {
        const accumulatedData = data.reduce((previous, item) => {
            const currentItem = { Company: item.Company, IPC: item.IPC, Year: item.Year, Count: item.Count, WorldDataCount: item.WorldDataCount, YourDataCount: item.YourDataCount };
            if (previous.length > 0) {
                var prevItem = previous.find(pItem => pItem.Company === item.Company && pItem.IPC === item.IPC && pItem.Year === item.Year);
                if (prevItem) {
                    prevItem.Count += item.Count;
                    prevItem.WorldDataCount += item.WorldDataCount;
                    prevItem.YourDataCount += item.YourDataCount;
                }
                else {
                    previous.push(currentItem);
                }
            }
            else
                previous.push(currentItem);

            return previous;
        }, []);
        return accumulatedData;
    }

    getAccumulatedYear(data) {
        const accumulatedData = this.accumulateYear(data);
        const topData = accumulatedData.sort((a, b) => (a.Year < b.Year) ? -1 : 1).slice(0, 20); //asc

        const chartData = topData.map(d => {
            const item = { yData: d.Count, xData: d.Year, category: "" };
            return item;
        });
        return chartData;
    }

    getAccumulatedYearGroup(data) {
        const accumulatedData = this.accumulateYear(data);
        const topData = accumulatedData.sort((a, b) => (a.Year < b.Year) ? -1 : 1).slice(0, 20); //asc
        const chartData = topData.map(d => {
            const item = {
                group: d.Year,
                count: d.WorldDataCount > d.YourDataCount ? d.WorldDataCount : d.YourDataCount,
                worldDataCount: d.WorldDataCount,
                yourDataCount: d.YourDataCount
            };
            return item;
        });
        return chartData;
    }

    getAccumulatedCountry(data) {
        const accumulatedData = this.accumulateCountry(data);
        const topData = accumulatedData.sort((a, b) => (a.Count < b.Count) ? 1 : -1).slice(0, 20); //desc

        const chartData = topData.map(d => {
            const item = { yData: d.Count, xData: d.Country, category: "" };
            return item;
        });
        return chartData;
    }

    getAccumulatedCountryGroup(data) {
        const accumulatedData = this.accumulateCountry(data);
        const topData = accumulatedData.sort((a, b) => (a.Count < b.Count) ? 1 : -1).slice(0, 20); //desc
        const chartData = topData.map(d => {
            const item = {
                group: d.Country,
                count: d.WorldDataCount > d.YourDataCount ? d.WorldDataCount : d.YourDataCount,
                worldDataCount: d.WorldDataCount,
                yourDataCount: d.YourDataCount
            };
            return item;
        });
        return chartData;
    }

    getTopApplicants(data) {
        const sortedData = this.accumulateCompany(data).sort((a, b) => (a.Count < b.Count) ? 1 : -1); //desc
        const topApplicants = sortedData.slice(0, 20);
        topApplicants.sort((a, b) => (a.Company < b.Company) ? -1 : 1); //alphabetical
        return topApplicants;
    }

    getTopIPCs(data) {
        const sortedData = this.accumulateIPC(data).sort((a, b) => (a.Count < b.Count) ? 1 : -1); //desc
        const topIPCs = sortedData.slice(0, 20);
        topIPCs.sort((a, b) => (a.IPC < b.IPC) ? -1 : 1);
        return topIPCs;
    }

    /* end of chart data accumulators */

    /* Chart options  */
    //chartOption1_init = () => {
    //    this.chartOption1_onSelect(null);
    //}

    chartOption1_onSelect = (e) => {
        const barContainer = $("#viz-opt-1-bar");
        const bubbleContainer = $("#viz-opt-1-bubble");
        const lineContainer = $("#viz-opt-1-line");

        if (e && e.dataItem.Value === "2") {
            barContainer.addClass("d-none");
            bubbleContainer.removeClass("d-none");
            lineContainer.addClass("d-none");
        }
        else if (e && e.dataItem.Value === "3") {
            barContainer.addClass("d-none");
            bubbleContainer.addClass("d-none");
            lineContainer.removeClass("d-none");
        }
        else {
            barContainer.removeClass("d-none");
            bubbleContainer.addClass("d-none");
            lineContainer.addClass("d-none");

            const topCountriesOpt = $("#viz-opt-1-bar-x-countries");
            const data = $(this.searchResultGrid).data("kendoGrid").dataSource.data();
            const countries = [... new Set(data.map(({ Country }) => Country))];
          
            if (countries.length > 1)
                topCountriesOpt.removeClass("d-none");
            else
                topCountriesOpt.addClass("d-none");

        }
    }

    chartOption1_draw_bar(data, options) {
        let chartData = [];
        //x-axis = year
        if (+options["viz-opt-1-bar-x"] === 1) {
            chartData = this.getAccumulatedYear(data);
        }

        //x-axis = countries top 20 based on Count
        else {
            chartData = this.getAccumulatedCountry(data);
        }
        this.displayBarChart(chartData, false);
    }

    chartOption1_draw_wyBar(data, options) {
        let chartData = [];
        //x-axis = year
        if (+options["viz-opt-1-bar-x"] === 1) {
            chartData = this.getAccumulatedYearGroup(data);
        }
        //x-axis = countries top 20 based on Count
        else {
            chartData = this.getAccumulatedCountryGroup(data);
        }
        this.displayBarChart_wy(chartData, false);
    }

    chartOption1_draw_wyLine(data, options) {
        let chartData = [];
        let accumulatedData = [];

        //x-axis = year
        if (+options["viz-opt-1-line-x"] === 1) {
            accumulatedData = this.getAccumulatedYearGroup(data);
        }
        //x-axis = countries top 20 based on Count
        else {
            accumulatedData = this.getAccumulatedCountryGroup(data);
        }

        const yourData = accumulatedData.map(d => {
            const item = { yData: d.yourDataCount, xData: d.group, category: "Your Data" };
            return item;
        });
        const worldData = accumulatedData.map(d => {
            const item = { yData: d.worldDataCount, xData: d.group, category: "World Data" };
            return item;
        });
        chartData = [...yourData, ...worldData];
        this.displayLineChart(chartData);
    }

    chartOption1_draw_line=(data, options)=> {
        let chartData = [];
        //x-axis = year
        if (+options["viz-opt-1-line-x"] === 1) {
            chartData = this.getAccumulatedYear(data);
        }

        //x-axis = countries top 20 based on Count
        else {
            chartData = this.getAccumulatedCountry(data);
        }
        //this.displayLineChart(chartData, this.basedOn === 1 ? this.colorList[1] : this.colorList[0]);
        this.displayLineChart(chartData);
    }

    chartOption1_draw_bubble(data, options) {
        const chartData = data.map(d => {
            const item = { yData: d.Country, xData: +d.Year, size: d.Count };
            return item;
        });
        chartData.sort((a, b) => (a.size < b.size) ? 1 : -1); //descending

        //top 20 
        const topCountries = [...(new Set(chartData.map(({ yData }) => yData)))].slice(0, 20);
        const years = [...(new Set(chartData.map(({ xData }) => xData)))].slice(0, 20);

        const topData = chartData.filter(d => topCountries.includes(d.yData));
        this.displayBubbleChart(topData, years.sort(),false, "Year","Country");
    }

    chartOption1_draw_wyBubble(data, options) {
        const worldData = data.map(d => {
            const item = { yData: d.Country, xData: d.Year + '-WD', size: d.WorldDataCount, source:'w' };
            return item;
        });
        worldData.sort((a, b) => (a.size < b.size) ? 1 : -1); //descending

        //top 20
        const topCountries = [...(new Set(worldData.map(({ yData }) => yData)))].slice(0, 20);

        const yourData = data.map(d => {
            const item = { yData: d.Country, xData: d.Year + '-YD', size: d.YourDataCount, source: 'y' };
            return item;
        });
        const chartData = [...worldData, ...yourData];
        const years = [...(new Set(chartData.map(({ xData }) => xData)))].slice(0, 20);
        const topData = chartData.filter(d => topCountries.includes(d.yData));
        this.displayBubbleChart(topData, years.sort(), false, "Year", "Country",true);
    }

    //chartOption3_init = () => {
    //    this.chartOption3_onSelect(null);
    //}

    chartOption3_onSelect = (e) => {
        const barContainer = $("#viz-opt-3-bar");
        const bubbleContainer = $("#viz-opt-3-bubble");
        const lineContainer = $("#viz-opt-3-line");

        if (e && e.dataItem.Value === "2") {
            barContainer.addClass("d-none");
            bubbleContainer.removeClass("d-none");
            lineContainer.addClass("d-none");
        }
        else if (e && e.dataItem.Value === "3") {
            barContainer.addClass("d-none");
            bubbleContainer.addClass("d-none");
            lineContainer.removeClass("d-none");
        }
        else {
            barContainer.removeClass("d-none");
            bubbleContainer.addClass("d-none");
            lineContainer.addClass("d-none");

            //const topCountriesOpt = $("#viz-opt-3-bar-x-countries");
            //const data = $(this.searchResultGrid).data("kendoGrid").dataSource.data();
            //const countries = [... new Set(data.map(({ Country }) => Country))];
            //const ipcs = [... new Set(data.map(({ IPC }) => IPC))];

            //if (countries.length > 1)
            //    topCountriesOpt.removeClass("d-none");
            //else
            //    topCountriesOpt.addClass("d-none");

            //const topIPCsOpt = $("#viz-opt-3-bar-x-ipcs");
            //if (ipcs.length > 1)
            //    topIPCsOpt.removeClass("d-none");
            //else
            //    topIPCsOpt.addClass("d-none");
        }
    }

    chartOption3_draw_bar(data, options) {
        let chartData = [];
        //x-axis = year
        if (+options["viz-opt-3-bar-x"] === 1) {
            chartData = this.getAccumulatedYear(data);
        }
        //x-axis = countries top 20 based on Count
        else if (+options["viz-opt-3-bar-x"] === 2) {
            chartData = this.getAccumulatedCountry(data);
        }
        //x-axis = CPCs
        else {
            const accumulatedData = this.accumulateIPC(data);
            const topData = accumulatedData.sort((a, b) => (a.IPC < b.IPC) ? 1 : -1).slice(0, 20); //desc

            chartData = topData.map(d => {
                const item = { yData: d.Count, xData: d.IPC };
                return item;
            });
        }

        this.displayBarChart(chartData, false);
    }

    chartOption3_draw_wyBar(data, options) {
        let chartData = [];
        //x-axis = year
        if (+options["viz-opt-3-bar-x"] === 1) {
            chartData = this.getAccumulatedYearGroup(data);
        }
        //x-axis = countries top 20 based on Count
        else if (+options["viz-opt-3-bar-x"] === 2) {
            chartData = this.getAccumulatedCountryGroup(data);
        }
        //x-axis = CPCs
        else {
            const accumulatedData = this.accumulateIPC(data);
            const topData = accumulatedData.sort((a, b) => (a.IPC < b.IPC) ? 1 : -1).slice(0, 20); //desc

            chartData = topData.map(d => {
                const item = {
                    group: d.IPC,
                    count: d.WorldDataCount > d.YourDataCount ? d.WorldDataCount : d.YourDataCount,
                    worldDataCount: d.WorldDataCount,
                    yourDataCount: d.YourDataCount
                };
                return item;
            });
        }
        this.displayBarChart_wy(chartData, false);
    }

    chartOption3_draw_wyLine(data, options) {
        let chartData = [];
        let accumulatedData = [];
        let yourData = [];
        let worldData = [];

        const yourDataLabel = "Your Data";
        const worldDataLabel = "World Data";

        //x-axis = year
        if (+options["viz-opt-3-line-x"] === 1) {
            accumulatedData = this.accumulateIPCYear(data);

            yourData = accumulatedData.map(d => {
                const item = { yData: d.YourDataCount, xData: d.Year, category: `${d.IPC}-${yourDataLabel}` };
                return item;
            });
            worldData = accumulatedData.map(d => {
                const item = { yData: d.WorldDataCount, xData: d.Year, category: `${d.IPC}-${worldDataLabel}`};
                return item;
            });
        }
        //x-axis = countries top 20 based on Count
        else if (+options["viz-opt-3-line-x"] === 2) {
            accumulatedData = this.accumulateIPCCountry(data);
            accumulatedData = accumulatedData.sort((a, b) => (a.Count < b.Count) ? 1 : -1); //desc
            const top20Countries = [...(new Set(accumulatedData.map(({ Country }) => Country)))].slice(0, 20);
            const topData = accumulatedData.filter(d => top20Countries.includes(d.Country));
        
            yourData = topData.map(d => {
                const item = { yData: d.YourDataCount, xData: d.Country, category: `${d.IPC}-${yourDataLabel}` };
                return item;
            });
            worldData = topData.map(d => {
                const item = { yData: d.WorldDataCount, xData: d.Country, category: `${d.IPC}-${worldDataLabel}` };
                return item;
            });
        }
        chartData = [...yourData, ...worldData];
        this.displayLineChart(chartData);
    }

    chartOption3_draw_bubble(data, options) {
        let chartData = [];
        let years = [...(new Set(data.map(({ Year }) => +Year)))].slice(0, 20);  //x-axis
        let xLabel, yLabel;

        //y-axis
        //top 20 of countries
        if (+options["viz-opt-3-bubble-y"] === 1) {
            const accumulatedData = this.accumulateCountryYear(data);
            chartData = accumulatedData.map(d => {
                const item = { yData: d.Country, xData: +d.Year, size: d.Count };
                return item;
            });
            xLabel = "Year";
            yLabel = "Country";
        }
        //CPC
        else {
            const accumulatedData = this.accumulateIPCYear(data);
            chartData = accumulatedData.map(d => {
                const item = { yData: d.IPC, xData: +d.Year, size: d.Count };
                return item;
            });
            xLabel = "Year";
            yLabel = "IPC";
        }

        chartData.sort((a, b) => (a.size < b.size) ? 1 : -1); //descending
        const top20Y = [...(new Set(chartData.map(({ yData }) => yData)))].slice(0, 20);
        const topData = chartData.filter(d => top20Y.includes(d.yData));
        topData.sort((a, b) => (a.yData < b.yData) ? -1 : 1);
        this.displayBubbleChart(topData, years.sort(),false,xLabel,yLabel);
    }

    chartOption3_draw_wyBubble(data, options) {
        let chartData = [];
        let xLabel, yLabel;

        //y-axis
        //top 20 of countries
        if (+options["viz-opt-3-bubble-y"] === 1) {
            const accumulatedData = this.accumulateCountryYear(data);
            const worldData = accumulatedData.map(d => {
                const item = { yData: d.Country, xData: d.Year + '-WD', size: d.WorldDataCount, source: 'w' };
                return item;
            });
            const yourData = accumulatedData.map(d => {
                const item = { yData: d.Country, xData: d.Year + '-YD', size: d.YourDataCount, source: 'y' };
                return item;
            });
            chartData = [...worldData, ...yourData];
            xLabel = "Year";
            yLabel = "Country";
        }
        //CPC
        else {
            const accumulatedData = this.accumulateIPCYear(data);
            const worldData = accumulatedData.map(d => {
                const item = { yData: d.IPC, xData: d.Year + '-WD', size: d.WorldDataCount, source: 'w' };
                return item;
            });
            const yourData = accumulatedData.map(d => {
                const item = { yData: d.IPC, xData: d.Year + '-YD', size: d.YourDataCount, source: 'y' };
                return item;
            });
            chartData = [...worldData, ...yourData];
            xLabel = "Year";
            yLabel = "IPC";
        }

        chartData.sort((a, b) => (a.size < b.size) ? 1 : -1); //descending
        const top20Y = [...(new Set(chartData.map(({ yData }) => yData)))].slice(0, 20);
        const topData = chartData.filter(d => top20Y.includes(d.yData));
        topData.sort((a, b) => (a.yData < b.yData) ? -1 : 1);
        let years = [...(new Set(chartData.map(({ xData }) => xData)))].slice(0, 20);  //x-axis
        this.displayBubbleChart(topData, years.sort(), false, xLabel, yLabel,true);
    }

    chartOption3_draw_line = (data, options) => {
        let chartData = [];
        let topData = [];
        
        //x-axis = year
        if (+options["viz-opt-3-line-x"] === 1) {
            const accumulatedData = this.accumulateIPCYear(data);
            chartData = accumulatedData.map(d => {
                const item = { yData: d.Count, xData: +d.Year, category: d.IPC };
                return item;
            });
            const years = [...(new Set(accumulatedData.map(({ Year }) => +Year)))].slice(0, 20); 
            topData = chartData.filter(d => years.includes(d.xData));
        }

        //x-axis = countries top 20 based on Count
        else {
            const accumulatedData = this.accumulateIPCCountry(data);
            chartData = accumulatedData.map(d => {
                const item = { yData: d.Count, xData: d.Country, category: d.IPC };
                return item;
            });

            accumulatedData.sort((a, b) => (a.Count < b.Count) ? 1 : -1);  //descending
            const countries = [...(new Set(accumulatedData.map(({ Country }) => Country)))].slice(0, 20);
            topData = chartData.filter(d => countries.includes(d.xData));
        }

        //this.displayLineChart(topData, this.basedOn === 1 ? this.colorList[1] : this.colorList[0]);
        this.displayLineChart(topData);
    }

    //chartOption4_init = () => {
    //    this.chartOption4_onSelect(null);
    //}

    chartOption4_onSelect = (e) => {
        const barContainer = $("#viz-opt-4-bar");
        const bubbleContainer = $("#viz-opt-4-bubble");
        const lineContainer = $("#viz-opt-4-line");
        const wyOption = $("#viz-opt-4-based-on");

        if (e && e.dataItem.Value === "2") {
            barContainer.addClass("d-none");
            bubbleContainer.removeClass("d-none");
            lineContainer.addClass("d-none");
            wyOption.closest("div").removeClass("d-none");
        }
        else if (e && e.dataItem.Value === "3") {
            barContainer.addClass("d-none");
            bubbleContainer.addClass("d-none");
            lineContainer.removeClass("d-none");
            wyOption.closest("div").addClass("d-none");
        }
        else {
            barContainer.removeClass("d-none");
            bubbleContainer.addClass("d-none");
            lineContainer.addClass("d-none");
            wyOption.closest("div").removeClass("d-none");
        }
    }

    chartOption4_draw_bar=(data, options)=> {
        let chartData = [];
        let filteredData = [];
        let barColor = null;

        if (this.basedOn === 3) {
            const display = +($("#viz-opt-4-based-on").val());

            if (display === 1) {
                filteredData = data.filter(d => d.YourDataCount > 0);
                filteredData.forEach(d => d.Count = d.YourDataCount);
                barColor = this.colorList[1];
            }
            else {
                filteredData = data.filter(d => d.WorldDataCount > 0);
                filteredData.forEach(d => d.Count = d.WorldDataCount);
                barColor = this.colorList[0];
            }
        }
        else 
            filteredData = data;

        //x-axis - Company
        const topApplicants = this.getTopApplicants(filteredData);
        chartData = topApplicants.map(d => {
            const item = { yData: d.Count, xData: d.Company };
            return item;
        });
        this.displayBarChart(chartData, false, true, barColor);
    }

    //chartOption4_draw_wyBar(data, options) {
    //    let chartData = [];

    //    //x-axis - Company
    //    const topApplicants = this.getTopApplicants(data);
    //    chartData = topApplicants.map(d => {
    //        const item = {
    //            group: d.Company,
    //            count: d.WorldDataCount > d.YourDataCount ? d.WorldDataCount : d.YourDataCount,
    //            worldDataCount: d.WorldDataCount,
    //            yourDataCount: d.YourDataCount
    //        };
    //        return item;
    //    });
    //    this.displayBarChart_wy(chartData, false, true);
    //}

    //chartOption4_draw_line = (data, options) => {
    //    let chartData = [];
    //    let topData = [];

    //    //x-axis = year
    //    if (+options["viz-opt-4-line-x"] === 1) {
    //        const accumulatedData = this.accumulateCompanyYear(data);
        
    //        chartData = accumulatedData.map(d => {
    //            const item = { yData: d.Count, xData: +d.Year, category: d.Company };
    //            return item;
    //        });
            
    //        const years = [...(new Set(accumulatedData.map(({ Year }) => +Year)))].sort().slice(0, 20);
    //        const sortedByCount = accumulatedData.sort((a, b) => (a.Count < b.Count) ? 1 : -1); //desc
    //        const topApplicants = [...(new Set(sortedByCount.map(({ Company }) => Company)))].slice(0, 20);
    //        topData = chartData.filter(d => years.includes(d.xData) && topApplicants.includes(d.category));
    //    }

    //    //x-axis = countries top 20 based on Count
    //    //else {
    //    //}
    //    this.displayLineChart(topData);
    //}

    chartOption4_draw_line = (data, options) => {
        let chartData = [];
        let topData = [];

        //x-axis = year
        if (+options["viz-opt-4-line-x"] === 1) {
            const accumulatedData = this.accumulateCompanyYear(data);

            chartData = accumulatedData.map(d => {
                const item = { yData: d.Count, xData: +d.Year, category: d.Company };
                return item;
            });

            const years = [...(new Set(accumulatedData.map(({ Year }) => +Year)))].sort().slice(0, 20);
            const sortedByCount = accumulatedData.sort((a, b) => (a.Count < b.Count) ? 1 : -1); //desc
            const topApplicants = [...(new Set(sortedByCount.map(({ Company }) => Company)))].slice(0, 20);
            topData = chartData.filter(d => years.includes(d.xData) && topApplicants.includes(d.category));
        }

        //x-axis = countries top 20 based on Count
        //else {
        //}
        this.displayLineChart(topData);
    }

    chartOption4_draw_wyLine = (data, options) => {
        let chartData = [];
        let topWData = [];
        let topYourData = [];

        //x-axis = year
        if (+options["viz-opt-4-line-x"] === 1) {
            const accumulatedData = this.accumulateCompanyYear(data);

            const worldChartData = accumulatedData.filter(d=> d.WorldDataCount > 0).map(d => {
                const item = { yData: d.WorldDataCount, xData: +d.Year, category: d.Company + '-WD'};
                return item;
            });

            const yourChartData = accumulatedData.filter(d => d.YourDataCount > 0).map(d => {
                const item = { yData: d.YourDataCount, xData: +d.Year, category: d.Company };
                return item;
            });

            const years = [...(new Set(accumulatedData.map(({ Year }) => +Year)))].sort().slice(0, 20);
            const sortedWCount = worldChartData.sort((a, b) => (a.yData < b.yData) ? 1 : -1); //desc
            const topWApplicants = [...(new Set(sortedWCount.map(({ category }) => category)))].slice(0, 20);
            topWData = worldChartData.filter(d => years.includes(d.xData) && topWApplicants.includes(d.category));

            const sortedYourCount = yourChartData.sort((a, b) => (a.yData < b.yData) ? 1 : -1); //desc
            const topYourApplicants = [...(new Set(sortedYourCount.map(({ category }) => category)))].slice(0, 20);
            topYourData = yourChartData.filter(d => years.includes(d.xData) && topYourApplicants.includes(d.category));

        }

        //x-axis = countries top 20 based on Count
        //else {
        //}
        chartData = [...topWData, ...topYourData];
        this.displayLineChart(chartData);
    }

    chartOption4_draw_bubble(data, options) {
        let chartData = [];
        let filteredData = [];
        let bubbleColor = null;

        if (this.basedOn === 3) {
            const display = +($("#viz-opt-4-based-on").val());

            if (display === 1) {
                filteredData = data.filter(d => d.YourDataCount > 0);
                filteredData.forEach(d => d.Count = d.YourDataCount);
                bubbleColor = this.colorList[1];
            }
            else {
                filteredData = data.filter(d => d.WorldDataCount > 0);
                filteredData.forEach(d => d.Count = d.WorldDataCount);
                bubbleColor = this.colorList[0];
            }
        }
        else
            filteredData = data;

        //x-axis - top 20 of companies
        if (+options["viz-opt-4-bubble-x"] === 1) {
            const sortedData = this.accumulateCompany(filteredData).sort((a, b) => (a.Count < b.Count) ? 1 : -1); //desc
            const topApplicants = sortedData.slice(0, 20).map(({ Company }) => Company);
            topApplicants.sort((a, b) => (a < b) ? -1 : 1);
            const applicantsData = filteredData.filter((item) => topApplicants.findIndex(ta => item.Company === ta) > -1);

            const accumulatedData = this.accumulateCompanyYear(applicantsData);
            chartData = accumulatedData.map(d => {
                const item = { yData: d.Year, xData: d.Company, size: d.Count };
                return item;
            });
            chartData.sort((a, b) => (a.yData < b.yData) ? 1 : -1); //descending
            this.displayBubbleChart(chartData, topApplicants, true, "Company", "Year", false, bubbleColor);
        }

    }

    //chartOption5_init = () => {
    //    this.chartOption5_onSelect(null);
    //}

    chartOption5_onSelect = (e) => {
        const barContainer = $("#viz-opt-5-bar");
        const bubbleContainer = $("#viz-opt-5-bubble");
        const lineContainer = $("#viz-opt-5-line");

        if (e && e.dataItem.Value === "2") {
            barContainer.addClass("d-none");
            bubbleContainer.removeClass("d-none");
            lineContainer.addClass("d-none");
        }
        else if (e && e.dataItem.Value === "3") {
            barContainer.addClass("d-none");
            bubbleContainer.addClass("d-none");
            lineContainer.removeClass("d-none");
        }
        else {
            barContainer.removeClass("d-none");
            bubbleContainer.addClass("d-none");
            lineContainer.addClass("d-none");
        }
    }

    chartOption5_draw_bar(data, options) {
        let chartData = [];
        let filteredData = [];
        let barColor = null;

        if (this.basedOn === 3) {
            const display = +($("#viz-opt-5-based-on").val());

            if (display === 1) {
                filteredData = data.filter(d => d.YourDataCount > 0);
                filteredData.forEach(d => d.Count = d.YourDataCount);
                barColor = this.colorList[1];
            }
            else {
                filteredData = data.filter(d => d.WorldDataCount > 0);
                filteredData.forEach(d => d.Count = d.WorldDataCount);
                barColor = this.colorList[0];
            }
        }
        else
            filteredData = data;

        //x-axis - Company
        const sortedData = this.accumulateCompany(filteredData).sort((a, b) => (a.Count < b.Count) ? 1 : -1); //desc
        const topApplicants = sortedData.slice(0, 20);
        topApplicants.sort((a, b) => (a.Company < b.Company) ? -1 : 1);
        chartData = topApplicants.map(d => {
            const item = { yData: d.Count, xData: d.Company };
            return item;
        });
        this.displayBarChart(chartData, false, true, barColor);
    }

    chartOption5_draw_bubble(data, options) {
        let chartData = [];
        let filteredData = [];
        let bubbleColor = null;

        if (this.basedOn === 3) {
            const display = +($("#viz-opt-5-based-on").val());

            if (display === 1) {
                filteredData = data.filter(d => d.YourDataCount > 0);
                filteredData.forEach(d => d.Count = d.YourDataCount);
                bubbleColor = this.colorList[1];
            }
            else {
                filteredData = data.filter(d => d.WorldDataCount > 0);
                filteredData.forEach(d => d.Count = d.WorldDataCount);
                bubbleColor = this.colorList[0];
            }
        }
        else
            filteredData = data;

        //x-axis - top 20 of companies
        if (+options["viz-opt-5-bubble-x"] === 1) {
            const sortedData = this.accumulateCompany(filteredData).sort((a, b) => (a.Count < b.Count) ? 1 : -1); //desc
            const topApplicants = sortedData.slice(0, 20).map(({ Company }) => Company);
            topApplicants.sort((a, b) => (a < b) ? -1 : 1);
            const applicantsData = filteredData.filter((item) => topApplicants.findIndex(ta => item.Company === ta) > -1);

            //y-axis = top 20 of countries
            if (+options["viz-opt-5-bubble-y"] === 1) {
                const accumulatedData = this.accumulateCompanyCountry(applicantsData).sort((a, b) => (a.Count < b.Count) ? 1 : -1).slice(0, 20);
                chartData = accumulatedData.map(d => {
                    const item = { yData: d.Country, xData: d.Company, size: d.Count };
                    return item;
                });
                this.displayBubbleChart(chartData, topApplicants, true, "Company", "Country");
            }
            //y-axis = CPC
            else if (+options["viz-opt-5-bubble-y"] === 2) {
                const accumulatedData = this.accumulateCompanyIPC(applicantsData).sort((a, b) => (a.Count < b.Count) ? 1 : -1).slice(0, 20); 
                chartData = accumulatedData.map(d => {
                    const item = { yData: d.IPC, xData: d.Company, size: d.Count };
                    return item;
                });
                this.displayBubbleChart(chartData, topApplicants, true, "Company", "CPC");
            }

            //y-axis = Year
            else  {
                const accumulatedData = this.accumulateCompanyYear(applicantsData);
                chartData = accumulatedData.map(d => {
                    const item = { yData: d.Year, xData: d.Company, size: d.Count };
                    return item;
                });
                chartData.sort((a, b) => (a.yData < b.yData) ? 1 : -1); //descending
                this.displayBubbleChart(chartData, topApplicants, true, "Company", "Year", false, bubbleColor);
            }
        }

    }

    chartOption5_draw_line = (data, options) => {
        let chartData = [];
        let topData = [];

        //x-axis = year
        if (+options["viz-opt-5-line-x"] === 1) {
            const accumulatedData = this.accumulateCompanyIPCYear(data);

            chartData = accumulatedData.map(d => {
                const item = { yData: d.Count, xData: +d.Year, category: d.IPC + '-' + d.Company };
                return item;
            });

            const years = [...(new Set(accumulatedData.map(({ Year }) => +Year)))].sort().slice(0, 20);
            const sortedByCount = chartData.sort((a, b) => (a.yData < b.yData) ? 1 : -1); //desc
            const topCategories = [...(new Set(sortedByCount.map(({ category }) => category)))].slice(0, 20);
            topData = chartData.filter(d => years.includes(d.xData) && topCategories.includes(d.category));
        }

        //x-axis = countries top 20 based on Count
        //else {
        //}
        this.displayLineChart(topData);
    }

    chartOption5_draw_wyLine = (data, options) => {
        let chartData = [];
        let topWData = [];
        let topYourData = [];

        //x-axis = year
        if (+options["viz-opt-5-line-x"] === 1) {
            const accumulatedData = this.accumulateCompanyIPCYear(data);

            const worldChartData = accumulatedData.filter(d => d.WorldDataCount > 0).map(d => {
                const item = { yData: d.WorldDataCount, xData: +d.Year, category: d.IPC + '-' + d.Company };
                return item;
            });

            const yourChartData = accumulatedData.filter(d => d.YourDataCount > 0).map(d => {
                const item = { yData: d.YourDataCount, xData: +d.Year, category: d.IPC + '-' + d.Company };
                return item;
            });

            const years = [...(new Set(accumulatedData.map(({ Year }) => +Year)))].sort().slice(0, 20);
            const sortedWCount = worldChartData.sort((a, b) => (a.yData < b.yData) ? 1 : -1); //desc
            const topWCategories = [...(new Set(sortedWCount.map(({ category }) => category)))].slice(0, 20);
            topWData = worldChartData.filter(d => years.includes(d.xData) && topWCategories.includes(d.category));

            const sortedYourCount = yourChartData.sort((a, b) => (a.yData < b.yData) ? 1 : -1); //desc
            const topYourCategories = [...(new Set(sortedYourCount.map(({ category }) => category)))].slice(0, 20);
            topYourData = yourChartData.filter(d => years.includes(d.xData) && topYourCategories.includes(d.category));

        }

        //x-axis = countries top 20 based on Count
        //else {
        //}
        chartData = [...topWData, ...topYourData];
        this.displayLineChart(chartData);
    }

    //chartOption6_init = () => {
    //    this.chartOption6_onSelect(null);
    //}

    chartOption6_onSelect = (e) => {
        const barContainer = $("#viz-opt-6-bar");
        const bubbleContainer = $("#viz-opt-6-bubble");

        if (e && e.dataItem.Value === "2") {
            barContainer.addClass("d-none");
            bubbleContainer.removeClass("d-none");
        }
        else {
            barContainer.removeClass("d-none");
            bubbleContainer.addClass("d-none");
        }
    }

    chartOption6_draw_bar(data, options) {
        let chartData = [];

        //x-axis - top 20 of IPCS
        const topIPCs = this.getTopIPCs(data);
        chartData = topIPCs.map(d => {
            const item = { yData: d.Count, xData: d.IPC };
            return item;
        });
        this.displayBarChart(chartData, false, false);
    }

    chartOption6_draw_line = (data, options) => {
        let chartData = [];

        //x-axis - top 20 of IPCS
        const topIPCs = this.getTopIPCs(data);
        chartData = topIPCs.map(d => {
            const item = { yData: d.Count, xData: d.IPC };
            return item;
        });
        this.displayLineChart(chartData);
    }

    chartOption6_draw_wyLine(data, options) {
        let chartData = [];

        const yourDataLabel = "Your Data";
        const worldDataLabel = "World Data";

        //x-axis - top 20 of IPCS
        const topIPCs = this.getTopIPCs(data);

        const yourData = topIPCs.map(d => {
            const item = { yData: d.YourDataCount, xData: d.IPC, category: yourDataLabel };
            return item;
        });
        const worldData = topIPCs.map(d => {
            const item = { yData: d.WorldDataCount, xData: d.IPC, category: worldDataLabel };
            return item;
        });

        chartData = [...yourData, ...worldData];
        console.log(chartData);
        this.displayLineChart(chartData);
    }

    chartOption6_draw_wyBar(data, options) {
        let chartData = [];

        //x-axis - top 20 of IPCS
        const topIPCs = this.getTopIPCs(data);
        chartData = topIPCs.map(d => {
            const item = {
                group: d.IPC,
                count: d.WorldDataCount > d.YourDataCount ? d.WorldDataCount : d.YourDataCount,
                worldDataCount: d.WorldDataCount,
                yourDataCount: d.YourDataCount
            };
            return item;
        });
        this.displayBarChart_wy(chartData, false, false);
    }

    chartOption6_draw_bubble(data, options) {
        let chartData = [];
        let xLabel, yLabel;

        //x-axis - top 20 of CPCs
        if (+options["viz-opt-6-bubble-x"] === 1) {
            const topIPCs = this.getTopIPCs(data).map(({ IPC }) => IPC);
            const ipcsData = data.filter((item) => topIPCs.findIndex(ta => item.IPC === ta) > -1);

            //y-axis = top 20 of countries
            if (+options["viz-opt-6-bubble-y"] === 1) {
                const accumulatedData = this.accumulateIPCCountry(ipcsData);
                chartData = accumulatedData.map(d => {
                    const item = { yData: d.Country, xData: d.IPC, size: d.Count };
                    return item;
                });
                xLabel = "CPC";
                yLabel = "Country"
            }

            //y-axis = Year
            else {
                const accumulatedData = this.accumulateIPCYear(ipcsData);
                chartData = accumulatedData.map(d => {
                    const item = { yData: d.Year, xData: d.IPC, size: d.Count };
                    return item;
                });
                xLabel = "CPC";
                yLabel = "Year"
            }

            chartData.sort((a, b) => (a.size < b.size) ? 1 : -1); //descending
            const top20Y = [...(new Set(chartData.map(({ yData }) => yData)))].slice(0, 20);
            const topData = chartData.filter(d => top20Y.includes(d.yData));

            if (+options["viz-opt-6-bubble-y"] === 2)
                topData.sort((a, b) => (a.yData < b.yData) ? 1 : -1); //descending

            topData.sort((a, b) => (a.yData < b.yData) ? -1 : 1);
            this.displayBubbleChart(topData, topIPCs.sort(),false,xLabel,yLabel);

        }

    }

    chartOption6_draw_wyBubble(data, options) {
        let chartData = [];
        let xLabel, yLabel;

        //x-axis - top 20 of CPCs
        if (+options["viz-opt-6-bubble-x"] === 1) {
            const topIPCs = this.getTopIPCs(data).map(({ IPC }) => IPC);
            const ipcsData = data.filter((item) => topIPCs.findIndex(ta => item.IPC === ta) > -1);

            //y-axis = top 20 of countries
            if (+options["viz-opt-6-bubble-y"] === 1) {
                const accumulatedData = this.accumulateIPCCountry(ipcsData);
                const worldData = accumulatedData.map(d => {
                    const item = { yData: d.Country, xData: d.IPC + '-WD', size: d.WorldDataCount, source: 'w' };
                    return item;
                });
                const yourData = accumulatedData.map(d => {
                    const item = { yData: d.Country, xData: d.IPC + '-YD', size: d.YourDataCount, source: 'y' };
                    return item;
                });
                chartData = [...worldData, ...yourData];
                xLabel = "CPC";
                yLabel = "Country";
            }

            //y-axis = Year
            else {
                const accumulatedData = this.accumulateIPCYear(ipcsData);
                const worldData = accumulatedData.map(d => {
                    const item = { yData: d.Year, xData: d.IPC + '-WD', size: d.WorldDataCount, source: 'w' };
                    return item;
                });
                const yourData = accumulatedData.map(d => {
                    const item = { yData: d.Year, xData: d.IPC + '-YD', size: d.YourDataCount, source: 'y' };
                    return item;
                });
                chartData = [...worldData, ...yourData];
                xLabel = "CPC";
                yLabel = "Year"
            }

            chartData.sort((a, b) => (a.size < b.size) ? 1 : -1); //descending

            const top20X = [...(new Set(chartData.map(({ xData }) => xData)))].slice(0, 20);
            const top20Y = [...(new Set(chartData.map(({ yData }) => yData)))].slice(0, 20);
            const topData = chartData.filter(d => top20Y.includes(d.yData));

            if (+options["viz-opt-6-bubble-y"] === 1)
                topData.sort((a, b) => (a.yData < b.yData) ? -1 : 1);
            else
                topData.sort((a, b) => (a.yData < b.yData) ? 1 : -1); //descending

            this.displayBubbleChart(topData, top20X, false, xLabel, yLabel, true);
        }

    }

    /* -- end of chart options  */

    drawChart=(options)=> {
        const data = $(this.searchResultGrid).data("kendoGrid").dataSource.data();

        if (data.length > 0) {

            switch (this.reportType) {
                case 1: //fall-through
                case 2:
                    if (+options["viz-opt-1-chart"] === 1)
                        this.basedOn === 3 ? this.chartOption1_draw_wyBar(data, options) : this.chartOption1_draw_bar(data, options);
                    else if (+options["viz-opt-1-chart"] === 2)
                        this.basedOn === 3 ? this.chartOption1_draw_wyBubble(data, options) : this.chartOption1_draw_bubble(data, options);
                    else 
                        this.basedOn === 3 ? this.chartOption1_draw_wyLine(data, options) : this.chartOption1_draw_line(data, options);
                    break;

                case 3:
                    if (+options["viz-opt-3-chart"] === 1)
                        this.basedOn === 3 ? this.chartOption3_draw_wyBar(data, options) : this.chartOption3_draw_bar(data, options);
                    else if (+options["viz-opt-3-chart"] === 2)
                        this.basedOn === 3 ? this.chartOption3_draw_wyBubble(data, options) : this.chartOption3_draw_bubble(data, options);
                    else
                        this.basedOn === 3 ? this.chartOption3_draw_wyLine(data, options) : this.chartOption3_draw_line(data, options);
                    break;
                
                case 4:
                    if (+options["viz-opt-4-chart"] === 1)
                        this.chartOption4_draw_bar(data, options);
                    else if (+options["viz-opt-4-chart"] === 2)
                        this.chartOption4_draw_bubble(data, options);
                    else 
                        this.basedOn === 3 ? this.chartOption4_draw_wyLine(data, options) : this.chartOption4_draw_line(data, options);
                    break;

                case 5:
                case 7:
                    if (+options["viz-opt-5-chart"] === 1)
                        this.chartOption5_draw_bar(data, options);
                    else if (+options["viz-opt-5-chart"] === 2)
                        this.chartOption5_draw_bubble(data, options);
                    else
                        this.basedOn === 3 ? this.chartOption5_draw_wyLine(data, options) : this.chartOption5_draw_line(data, options);
                    break;

                case 6:
                    if (+options["viz-opt-6-chart"] === 1)
                        this.basedOn === 3 ? this.chartOption6_draw_wyBar(data, options) : this.chartOption6_draw_bar(data, options);
                    else if (+options["viz-opt-6-chart"] === 2)
                        this.basedOn === 3 ? this.chartOption6_draw_wyBubble(data, options) : this.chartOption6_draw_bubble(data, options);
                    else
                        this.basedOn === 3 ? this.chartOption6_draw_wyLine(data, options) : this.chartOption6_draw_line(data, options);
                    break;
            }
        }
    }

    displayBarChart = (chartData, showValues, longXLabels,barColor) => {
        let margin = { top: 30, right: 50, bottom: longXLabels ? 250 : 30, left: 60 },
            width = 950 - margin.right - margin.left,
            height = 545 - margin.top - margin.bottom;

        let svg = d3.select("#viz")
            .append("svg")
            .attr("width", width + margin.right + margin.left)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", `translate(${margin.left},${margin.top})`);

        //scaleBand = categorical
        //domain = input, range = output (min,max)

        let x = d3.scaleBand()
            .domain(chartData.map(d => d.xData))
            .range([0, width])
            .padding(.2);

        let y = d3.scaleLinear()
            .domain([0, d3.max(chartData, d => d.yData)])
            .range([height, 0]) //top to bottom
            .nice();

        svg.append("g")
            .call(d3.axisLeft(y));

        const xAxis = svg.append("g")
            .attr("transform", `translate(0,${height})`)
            .call(d3.axisBottom(x).tickSize(0));

        //grids
        const xAxisGrid = d3.axisBottom(x).tickSize(-height).tickFormat('');
        const yAxisGrid = d3.axisLeft(y).tickSize(-width).tickFormat('');

        svg.append('g')
            .attr('class', 'x axis-grid')
            .attr('transform', 'translate(0,' + height + ')')
            .call(xAxisGrid)
            .call(g => g.select(".domain").remove());

        if (longXLabels) {
            xAxis.selectAll("text")
                .style("text-anchor", "end")
                .style("font-size", ".5rem")
                .attr("dx", "-.8em")
                .attr("dy", ".15em")
                .attr("transform", function (d) {
                    return "rotate(-65)"
                });
        }

        svg.append('g')
            .attr('class', 'y axis-grid')
            .call(yAxisGrid)
            .call(g => g.select(".domain").remove());

        let bars = svg.selectAll(".bar-group").data(chartData).enter().append("g").attr("class", "bar-group");
        bars.append("rect").attr("class", "bar")
            .attr("x", d => x(d.xData))
            .attr("y", d => y(d.yData))
            .attr("width", x.bandwidth())
            .attr("height", d => height - y(d.yData))
            .style("fill", barColor ? barColor : (this.basedOn === 1 ? this.colorList[1] : this.colorList[0]));
        // .style('fill', 'steelblue');

        if (showValues) {
            svg.selectAll(".text")
                .data(chartData)
                .enter()
                .append("text")
                .attr("class", "label")
                .attr("x", d => x(d.xData) + x.bandwidth() / 2)
                .attr("y", d => y(d.yData) + 1)
                .attr("dy", ".75em")
                .text(d => d3.format(",")(d.yData));
        }
    }

    displayBarChart_wy = (chartData, showValues, longXLabels) => {
        let margin = { top: 30, right: 0, bottom: longXLabels ? 250 : 30, left: 60 },
            width = 830 - margin.right - margin.left,
            height = 545 - margin.top - margin.bottom;

        let svg = d3.select("#viz")
            .append("svg")
            .attr("width", width + margin.right + margin.left)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", `translate(${margin.left},${margin.top})`);

        let subGroups = ["worldDataCount", "yourDataCount"];

        let x = d3.scaleBand()
            .domain(chartData.map(d => d.group))
            .range([0, width])
            .padding(.2);

        const maxCount = d3.max(chartData, d => d.count); 
        let y = d3.scaleLinear()
            .domain([0, maxCount])
            //.domain([-(maxCount *.02), maxCount]) //so very low values are still visible in the chart
            .range([height, 0]) //top to bottom
            .nice();

        svg.append("g")
            .call(d3.axisLeft(y));

        const xAxis = svg.append("g")
            .attr("transform", `translate(0,${height})`)
            .call(d3.axisBottom(x).tickSize(0));

        const xSubGroup = d3.scaleBand()
            .domain(subGroups)
            .range([0, x.bandwidth()])
            .padding(.05);
        
        const color = d3.scaleOrdinal()
            .domain(subGroups)
            .range(this.colorList);

        //grid lines
        const xAxisGrid = d3.axisBottom(x).tickSize(-height).tickFormat('');
        const yAxisGrid = d3.axisLeft(y).tickSize(-width).tickFormat('');

        svg.append('g')
            .attr('class', 'x axis-grid')
            .attr('transform', 'translate(0,' + height + ')')
            .call(xAxisGrid)
            .call(g => g.select(".domain").remove());

        if (longXLabels) {
            xAxis.selectAll("text")
                .style("text-anchor", "end")
                .style("font-size", ".5rem")
                .attr("dx", "-.8em")
                .attr("dy", ".15em")
                .attr("transform", function (d) {
                    return "rotate(-65)"
                });
        }

        svg.append('g')
            .attr('class', 'y axis-grid')
            .call(yAxisGrid)
            .call(g => g.select(".domain").remove());

        //
        svg.selectAll(".bar-group").data(chartData).enter().append("g")
            .attr("transform", function (d) { return "translate(" + x(d.group) + ",0)"; })
            .selectAll("rect")
            .data(function (d) { return subGroups.map(function (key) { return { key: key, value: d[key] }; }); })
            .enter().append("rect")
            .attr("x",  (d)=> xSubGroup(d.key))
            .attr("y", (d) => { const val = this.getMinimumValue(d.value, maxCount); return y(val); })
            .attr("width", xSubGroup.bandwidth())
            //.attr("height", (d)=> { return height - y(d.value); })
            .attr("height", (d) => { const val = this.getMinimumValue(d.value, maxCount); return height - y(val); })
            .attr("fill", function (d) { return color(d.key); });

        const legendLabels = ["Worldwide Data", "Your Data"];
        this.displayLegend(legendLabels, color);
    }

    //so it can be represented in the chart (visible)
    getMinimumValue = (val, max) => {
        if (val > 0 && max * 0.005 > val) {
            val = max * 0.005; //minimum is .5% 
        }
        return val;
    }

    displayLegend = (legendLabels, color) => {
        const svgLegend = d3.select("#viz-legend")
                .append("svg")
                .attr("width", 200)
                .attr("height", 500)
                .attr("class", "mt-5");

        const dotSize = 15;
        svgLegend.selectAll("legend-dot")
            .data(legendLabels)
            .enter()
            .append("rect")
            .attr("x", 25)
            .attr("y", function (d, i) { return 25 + i * (dotSize + 5) }) // 25 is where the first dot appears 
            .attr("width", dotSize)
            .attr("height", dotSize)
            .style("fill", function (d) { return color(d) });

        svgLegend.selectAll("legend-label")
            .data(legendLabels)
            .enter()
            .append("text")
            .attr("x", 25 + dotSize * 1.2)
            .attr("y", function (d, i) { return 25 + i * (dotSize + 5) + (dotSize / 2) }) // 25 is where the first dot appears 
            .style("fill", function (d) { return color(d) })
            .text(function (d) { return d })
            .attr("text-anchor", "left")
            .style("font-size", ".5rem")
            .style("alignment-baseline", "middle")
    }

    displayBubbleChart = (chartData, xData, longXLabels, xLabel, yLabel,wyCompare,bubbleColor) => {
        let margin = { top: 30, right: 50, bottom: longXLabels ? 250 : 30, left: 70 },
            width = (wyCompare ? 875 : 950) - margin.right - margin.left,
            height = 545 - margin.top - margin.bottom;

        //container
        let svg = d3.select("#viz")
            .append("svg")
            .attr("width", width + margin.right + margin.left)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", `translate(${margin.left},${margin.top})`);

        //svg.on("click", function () {
        //    var mouse = d3.mouse(this);
        //    console.log(mouse[0], mouse[1]);
        //});

        //x-axis
        let x = d3.scaleBand()
            .domain(xData)
            .range([0, width]);

        let xAxis = svg.append("g")
            .attr("transform", `translate(0,${height})`)
            .call(d3.axisBottom(x));

        if (longXLabels) {
            xAxis.selectAll("text")
                .style("text-anchor", "end")
                .style("font-size", ".5rem")
                .attr("dx", "-.8em")
                .attr("dy", ".15em")
                .attr("transform", function (d) {
                    return "rotate(-65)"
                });
        }

        //y-axis
        let y = d3.scaleBand()
            .domain(chartData.map(d => d.yData))
            .range([0, height]);
        svg.append("g")
            .call(d3.axisLeft(y));

        //grids
        const xAxisGrid = d3.axisBottom(x).tickSize(-height).tickFormat('');
        const yAxisGrid = d3.axisLeft(y).tickSize(-width).tickFormat('');
        svg.append('g')
            .attr('class', 'x axis-grid')
            .attr('transform', 'translate(0,' + height + ')')
            .call(xAxisGrid)
            .call(g => g.select(".domain").remove());
        svg.append('g')
            .attr('class', 'y axis-grid')
            .call(yAxisGrid)
            .call(g => g.select(".domain").remove());

        // Add a scale for bubble size
        var z = d3.scaleLinear()
            .domain([d3.min(chartData, d => d.size), d3.max(chartData, d => d.size)])
            .range([2, 30]);

        // create a tooltip div that is hidden by default:
        const tooltip = d3.select("#viz")
            .append("div")
            .style("opacity", 0)
            .attr("class", "tooltip")
            .style("background-color", "#efefef")
            .style("border-radius", "5px")
            .style("border", "1px solid #B9B9B9")
            .style("padding", "10px")
            .style("color", "#5e5656");

        // functions to show/update (when mouse move but stay on same circle) / hide the tooltip
        const showTooltip = function (d) {
            tooltip
                .transition()
                .duration(200)
            tooltip
                .style("opacity", 1)
                .html("<span>" + yLabel + ": " + d.yData + "</span><br/><span>" + xLabel + ": " + d.xData + "</span><br/><span>Count: " + d3.format(",")(d.size) + "</span>")
                .style("left", (d3.mouse(this)[0] + 250) + "px")
                .style("top", (d3.mouse(this)[1] + 100) + "px")
        };
        const moveTooltip = function (d) {
            tooltip
                .style("left", (d3.mouse(this)[0] + 250) + "px")
                .style("top", (d3.mouse(this)[1] + 100) + "px")
        };
        const hideTooltip = function (d) {
            tooltip
                .transition()
                .duration(200)
                .style("opacity", 0)
        };


        // Add dots
        svg.append('g')
            .selectAll("dot")
            .data(chartData)
            .enter()
            .append("circle")
            .attr("cx", d => x(d.xData) + (x.bandwidth() / 2))
            .attr("cy", d => y(d.yData) + (y.bandwidth() / 2))
            .attr("r", d => z(d.size))
            // .attr("class", "dot")
            .attr("fill", (d) => {
                if (this.basedOn === 1) 
                    return this.colorList[1]; //your

                else if (this.basedOn === 2) 
                    return this.colorList[0];  //world
                    
                else
                    return bubbleColor ? bubbleColor : (d.source === "w" || !wyCompare) ? this.colorList[0] : this.colorList[1];
            })
            .style("opacity", "0.7")
            .attr("stroke", "black")
            .on("mouseover", showTooltip)
            .on("mousemove", moveTooltip)
            .on("mouseleave", hideTooltip);

        if (wyCompare) {
            const legendLabels = ["Worldwide Data", "Your Data"];
            const color = d3.scaleOrdinal()
                .domain(legendLabels)
                .range(this.colorList);
            
            this.displayLegend(legendLabels, color);
        }
    }
        
    displayLineChart = (chartData) => {
        let margin = { top: 30, right: 50, bottom: 30, left: 60 },
            width = 800 - margin.right - margin.left,
            height = 545 - margin.top - margin.bottom;

        let svg = d3.select("#viz")
            .append("svg")
            .attr("width", width + margin.right + margin.left)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", `translate(${margin.left},${margin.top})`);

        let xScale = d3.scalePoint()
            .domain(chartData.map(d => d.xData))
            .range([50, width - 50])
            .padding(0.5);

        let yScale = d3.scaleLinear()
            .domain([0, d3.max(chartData, d => d.yData)])
            .range([height, 0]) //top to bottom
            .nice();

        let color = d3.scaleOrdinal()
            .domain(chartData.map(d => d.category))
            .range(d3.schemeCategory10);

        svg.append("g")
            .call(d3.axisLeft(yScale));

        const xAxis = svg.append("g")
            .attr("transform", `translate(0,${height})`)
            .call(d3.axisBottom(xScale).tickSize(0));

        //grids
        const xAxisGrid = d3.axisBottom(xScale).tickSize(-height).tickFormat('');
        const yAxisGrid = d3.axisLeft(yScale).tickSize(-width).tickFormat('');

        svg.append("g")
            .attr("class", "x axis-grid")
            .attr("transform", "translate(0," + height + ")")
            .call(xAxisGrid)
            .call(g => g.select(".domain").remove());

        svg.append("g")
            .attr("class", "y axis-grid")
            .call(yAxisGrid)
            .call(g => g.select(".domain").remove());

        const categories = [... new Set(chartData.map(({ category }) => category))];

        categories.forEach(category => {
            const categoryData = chartData.filter(d => d.category === category);
            const currentColor = color(category);

            //draw the line
            svg.append("path")
                .datum(categoryData)
                .attr("fill", "none")
                .attr("stroke", currentColor)
                .attr("stroke-width", 3)
                .attr("class", category)
                .attr("d", d3.line()
                    .x(function (d) { return xScale(d.xData) })
                    .y(function (d) { return yScale(d.yData) })
                );

            //circle on each datapoint
            //svg.selectAll(".dot")
            //    .data(chartData)
            //    .enter().append("circle")
            //    .attr("fill", currentColor)
            //    .attr("cx", function (d) { return xScale(d.xData) })
            //    .attr("cy", function (d) { return yScale(d.yData) })
            //    .attr("r", 3);
            //.on("mouseover", function (a, b, c) {
            //    console.log(a)
            //    this.attr('class', 'focus')
            //})
            //.on("mouseout", function () { })
        });

        const legendLabels = color.domain();
        if (legendLabels.length > 1)
           this.displayLegend(legendLabels, color);
    }


    symbolSearchParams = () => {
        var searchType = $("#SearchType").find(".btn.active input")[0].value;
        var searchMode = $("#SearchMode").find(".btn.active input")[0].value;
        return { searchText: $("#patStatSymbolSelectionDialog").find("#SymbolSearch").val(), symbolType: searchType, searchMode: searchMode };
    }

    symbolSearchSelectionChange(parent) {
        const applyButton = $(`#${parent}`).find("#apply");

        const grid = $("#pdtStatSymbolsGrid").data("kendoGrid");
        if (grid.selectedKeyNames().length > 0)
            applyButton.removeAttr("disabled");
        else
            applyButton.attr("disabled", "disabled");
    }
}





