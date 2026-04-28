
export default class DynamicGrid {

    // generates grid dynamically based on columns in the gridModelData; gridModelData previously retrieved via ajax call
    // reference: https://docs.telerik.com/kendo-ui/knowledge-base/grid-generate-model-from-data?_ga=2.122400450.852910390.1587046227-957280252.1583866590
    // with modifications
    // note: datetime2 returns "object" property, not "date"
    createGrid(gridId, gridModelData, url, data, pageSize = 20, filterable = false) {

        // sort fields work-around; for some reason, sort info is sent to server, but server is not processing it and so it is always null in DataSourceRequest
        let sortField = "";
        let sortDir = "";

        //const dateFields = [];

        removeGrid();
        generateGrid();

        // clear previous grid object
        function removeGrid() {
            var grid = $(gridId).data("kendoGrid");
            if (grid) {
                grid.destroy();
                $(gridId).empty();
            }
        }

        // generate grid
        function generateGrid() {
            // generate the model from the passed gridModelData
            const model = generateModel(gridModelData[0]);
            var columns = generateColumns(gridModelData);

            let exists = columns.some(item => item.field === "RecId_Select");
            if (exists && filterable == true) {
                columns = columns.filter(item => item.field !== "RecId_Select");
                model.id = "RecId_Select";
            }

            function getParam() {
                return $.extend(true, data, { sortField: sortField, sortDir: sortDir });
            }

            var dataSource = new kendo.data.DataSource({
                transport: {
                    read: { "url": url, "data": getParam },
                    dataType: "json",
                    type: "GET"
                },
                schema: {
                    model: model,
                    data: "Data",
                    total: "Total",
                    errors: "Error"
                },
                pageSize: pageSize,
                serverPaging: true,
                serverSorting: true
            })



            $(gridId).kendoGrid({
                dataSource: dataSource,
                editable: false,
                filterable: filterable,
                pageable: {
                    alwaysVisible: false,
                    buttonCount: 5,
                    pageSizes: [5, 10, 20, 100]
                },
                //sortable: true,
                sortable: {
                    mode: "single",
                    initialDirection: "asc",
                    allowUnsort: false
                },
                resizable: true,
                scrollable: true,
            });

            const grid = $(gridId).data("kendoGrid");

            if (filterable == true && columns.length > 0 && columns[0].field != "NoData") {
                var selectable = {
                    selectable: true,
                    width: 35
                };

                columns.splice(0, 0, selectable);
                grid.setOptions({
                    columns: columns
                });
            }

            grid.bind("sort", function (e) {
                sortField = e.sort.field;
                sortDir = e.sort.dir;
            });

        }

        // infers the fields from a single model data
        function generateModel(modelData) {
            const model = {};
            //model.id = "ID";
            const fields = {};

            for (const property in modelData) {
                const propType = typeof modelData[property];
                //alert("property: " + property + ".... propType: " + propType)
                if (propType === "number") {
                    fields[property] = { type: propType };
                }
                else if (propType === "boolean") {
                    fields[property] = { type: propType };
                }
                else if (propType === "string") {
                    const parsedDate = kendo.parseDate(modelData[property]);
                    if (parsedDate) {
                        fields[property] = { type: "string" };
                    } else {
                        fields[property] = {};
                    }
                }
                else {
                    fields[property] = {};
                }
            }
            model.fields = fields;

            return model;
        }

        function generateColumns(response) {
            var columnNames = Object.keys(response[0]);
            //var columnNames = response["columns"];
            return columnNames.map(function (name) {
                return { field: name };
            })
        }

        //let parseFunction;
        //if (dateFields.length > 0) {
        //    parseFunction = function (response) {
        //        for (let i = 0; i < response.length; i++) {
        //            for (let fieldIndex = 0; fieldIndex < dateFields.length; fieldIndex++) {
        //                const record = response[i];
        //                const dt = new Date(record[dateFields[fieldIndex]]);
        //                //record[dateFields[fieldIndex]] = kendo.parseDate(record[dateFields[fieldIndex]]);             // original: renders entire date/time data
        //                record[dateFields[fieldIndex]] = dt.toLocaleDateString();                                       // replacement (fsn) - localize date to render properly
        //            }
        //        }
        //        return response;
        //    };
        //}
        //const grid = $(gridId).kendoGrid({
        //    dataSource: {
        //        data: gridModelData,
        //        schema: {
        //            model: model
        //            //, parse: parseFunction
        //        },
        //        pageSize: 20
        //    },
        //    editable: false,
        //    pageable: {
        //        alwaysVisible: false,
        //        buttonCount: 5,
        //        pageSizes: [5, 10, 20, 100]
        //    },
        //    sortable: {
        //        mode: "single",
        //        allowUnsort: false
        //    },
        //    scrollable: true

        //});
    }
}

