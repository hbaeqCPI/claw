import * as dagreD3 from "dagre-d3";

import {
    createEdgeLabel, attachLinks, createInfoHeader, createInfo, createNodeDescriptions,
    createNodeHeaderText, formatList, formatDate, loadImage
} from "./familyTreeLinkPageHelper";

export default class FamilyTreeLink {
    constructor() {
        this.baseUrl = $("body").data("base-url");
        this.render = new dagreD3.render();
        this.fetchTreeArgument = {};
        this.nodeDisplayOptions = [];
        this.parentToChildren = new Map();      // for addtional logic
        this.childToParents = new Map();        // for addtional logic
        this.fullscreen = false;
        this.treeJson = {};         // for rendering
        this.isTD = false;
        this.treeViewName = "";
        this.stats = {};

        const self = this;

        // for non TD views
        this.render.shapes().treeNode = function (parent, bbox, node) {
            const { childToParents, parentToChildren } = self;

            let rectHeight = 25;
            const secondRectHeight = 35;
            const totalHeight = rectHeight + secondRectHeight;
            rectHeight = node.Type !== "F" ? rectHeight : totalHeight;

            const firstRectY = -totalHeight / 2;
            const secondRectY = firstRectY + rectHeight;
            const rectWidth = 175;
            const rectX = -rectWidth / 2;
            const baseFontSize = 12;

            let directParents = (childToParents.get(node.Id) || [])
                .map(parentId => ({ id: g.node(parentId).KeyId, name: g.node(parentId).Text }));
            let directChildren = (parentToChildren.get(node.Id) || [])
                .map(childId => ({ id: g.node(childId).KeyId, name: g.node(childId).Text }));

            const shapeSvg = parent.insert("g", ":first-child");

            const d1 = `
              M ${rectX}, ${firstRectY + rectHeight} 
              L ${rectX}, ${firstRectY} 
              Q ${rectX}, ${firstRectY} ${rectX}, ${firstRectY} 
              L ${rectX + rectWidth}, ${firstRectY} 
              Q ${rectX + rectWidth}, ${firstRectY} ${rectX + rectWidth}, ${firstRectY} 
              L ${rectX + rectWidth}, ${firstRectY + rectHeight} 
              Z
            `;

            const path = shapeSvg.append("path")
                .attr("d", d1)
                .attr("fill", "white")
                .attr("stroke", "black")
                .attr("stroke-width", "1");

            if (node.Type === "C" || node.Type === "T")
                path.attr("class", node.Active ? "activeNode" : "inactiveNode");


            const delimiter = node.Type === "F" ? " " : "/";
            const maxWidth = node.Type === "F" ? rectWidth + 5 : rectWidth - 15;
            const headerText = createNodeHeaderText(node.Text, maxWidth, delimiter);
            const xShift = rectX + 0.5 * baseFontSize;
            const yShift = headerText[1] == "" ? firstRectY + (rectHeight + baseFontSize) / 2 : firstRectY + baseFontSize - 1.5;

            const headerTextElement = shapeSvg.append("text")
                .attr("x", xShift)
                .attr("y", yShift)
                .attr("font-weight", "bold")
                .attr("font-size", baseFontSize)
                .attr("font-family", "Arial, sans-serif")
                .attr("xml:space", "preserve");

            $.each(headerText, function (i, text) {
                if (text === "") return false;
                headerTextElement.append("tspan")
                    .attr("x", xShift)
                    .attr("dy", i === 0 ? "0em" : "1em")
                    .text(i === 0 ? text : "\t".repeat(4) + text);
            })


            if (node.Type !== "F") {
                const d2 = `
                    M ${rectX}, ${secondRectY} 
                    L ${rectX + rectWidth}, ${secondRectY} 
                    L ${rectX + rectWidth}, ${secondRectY + secondRectHeight} 
                    Q ${rectX + rectWidth}, ${secondRectY + secondRectHeight} ${rectX + rectWidth}, ${secondRectY + secondRectHeight} 
                    L ${rectX}, ${secondRectY + secondRectHeight} 
                    Q ${rectX}, ${secondRectY + secondRectHeight} ${rectX}, ${secondRectY + secondRectHeight} 
                    L ${rectX}, ${secondRectY} 
                    Z
                `;

                shapeSvg.append("path")
                    .attr("d", d2)
                    .attr("fill", "white")
                    .attr("stroke", "black")
                    .attr("stroke-width", "1");


                attachLinks(shapeSvg, node, directParents, directChildren, rectWidth, firstRectY);
                const desText = createNodeDescriptions(node);

                const desTextElement = shapeSvg.append("text")
                    .attr("x", xShift)
                    .attr("y", secondRectY + baseFontSize + 3)
                    .attr("xml:space", "preserve");

                $.each(desText, function (i, text) {
                    desTextElement.append("tspan")
                        .attr("x", xShift)
                        .attr("dy", i === 0 ? "0em" : "1.2em")
                        .attr("font-size", baseFontSize)
                        .attr("font-family", "Arial, sans-serif")
                        .text(text);
                })
            }

            node.intersect = function (point) {

                const safeNode = {
                    x: node.x,
                    y: node.y,
                    width: rectWidth,
                    height: totalHeight,
                };

                return dagreD3.intersect.rect(safeNode, point);
            };

            return shapeSvg;
        };


        // TD only shape
        this.render.shapes().TDTreeNode = function (parent, bbox, node) {
            let baseFontSize = 14;
            const radius = 12;

            const shapeSvg = parent.insert("g", ":first-child");
            shapeSvg.append("circle")
                .attr("r", radius)
                .style("fill", "black");


            const desText = []
            desText.push(node.Text);

            if (!node.id.includes("PTA") && !node.Modified)
                desText.push(formatDate(node.Date) || "");

            if (node.Type == "TD" && node.id.includes("App")) {
                if (node.ExpDate != null) {
                    desText.push("Expires on " + formatDate(node.ExpDate));
                } else {
                    desText.push("No Expiration Date");
                }
            }

            //console.log(desText);

            if (node.id.includes("PTA")) {
                shapeSvg.append("text")
                    .attr("text-anchor", "start")
                    .attr("dominant-baseline", "middle")
                    .attr("x", radius * 1.5)
                    .attr("y", 0)
                    .attr("xml:space", "preserve")
                    .attr("font-weight", "bold")
                    .attr("font-size", baseFontSize)
                    .attr("font-family", "Arial, sans-serif")
                    .text(desText[0]);
            } else {
                const desTextElement = shapeSvg.append("text")
                    .attr("text-anchor", "middle")
                    .attr("x", 0)
                    .attr("y", radius * 2.5)
                    .attr("xml:space", "preserve");

                $.each(desText, function (i, text) {
                    desTextElement.append("tspan")
                        .attr("x", 0)
                        .attr("dy", i === 0 ? "0em" : "1.2em")
                        .attr("font-weight", i === 0 ? "bold" : "")
                        .attr("font-size", baseFontSize)
                        .attr("font-family", "Arial, sans-serif")
                        .text(text);
                });
            }

            node.intersect = function (point) {
                const dx = point.x - node.x;
                const dy = point.y - node.y;
                const angle = Math.atan2(dy, dx);

                let xOffset = Math.cos(angle) * (radius + 10);
                let yOffset = Math.sin(angle) * (radius + 10);

                return {
                    x: node.x + xOffset,
                    y: node.y + yOffset
                };
            };

            return shapeSvg;
        };
    }


    async renderTree(data, refreshData = false) {
        this.fetchTreeArgument = data;

        try {
            if ((Object.keys(this.treeJson).length === 0) || refreshData) {
                this.treeJson = await this.fetchTreeData();
            }

            this.isTD = this.treeJson["Area"] === 1;
            if (!this.isTD) {
                let { properties, _ } = await this.fetchDisplayOptions();
                this.nodeDisplayOptions = properties;
                this.treeViewName = "Family_Tree_View"
                this.renderTreeInternal();
            } else {
                this.treeViewName = "Terminal_Disclaimer_Tree_View"
                this.renderTreeInternalTD();
            }

            this.attachEventListeners();
        } catch (error) {
            console.error(error);
            pageHelper.showErrors(`Error rendering family tree, please report this as a bug.`);
        }
    };


    centerGraph() {
        const d3svg = d3.select("#familyTreeSvg");
        const svgContainer = $("#d3-tree");
        const svgHeader = document.getElementById("svgHeader");
        const padding = 15;

        if (!g || !g.graph) return;

        const graphGroup = d3svg.select("g").node();
        if (!graphGroup) return;

        const graphBBox = graphGroup.getBBox();
        const graphWidth = graphBBox.width;
        const graphHeight = graphBBox.height;

        if (!graphWidth || !graphHeight) return;

        const headerHeight = svgHeader ? svgHeader.getBBox().height : 0;
        const availableHeight = svgContainer.height() - headerHeight - 2 * padding;

        const scaleX = svgContainer.width() / graphWidth;
        const scaleY = availableHeight / graphHeight;
        const zoomScale = Math.min(scaleX, scaleY, 1);

        const x0 = (svgContainer.width() - graphWidth * zoomScale) / 2 - graphBBox.x * zoomScale;
        const y0 = (availableHeight - graphHeight * zoomScale) / 2 - graphBBox.y * zoomScale + headerHeight + padding;

        d3svg.transition()
            .duration(1500)
            .call(zoom.transform, d3.zoomIdentity.translate(x0, y0).scale(zoomScale));

        return zoomScale;
    }


    centerAroundNode(node) {
        if (!node) {
            console.error(node);
            return;
        }

        const d3svg = d3.select("#familyTreeSvg");
        const svgContainer = $("#d3-tree");
        const ouputCanvas = $(".output");
        const selectedNode = $("#" + node.id);

        if (!selectedNode.length) return;

        const currentTransform = d3.zoomTransform(d3svg.node());
        const zoomedScale = 1;

        if (selectedNode[0].getBBox) {
            const bbox = selectedNode[0].getBBox();
            const selectedWidth = bbox.width * zoomedScale;
            const selectedHeight = bbox.height * zoomedScale;

            const relativeX = (selectedNode.offset().left - ouputCanvas.offset().left) / currentTransform.k * zoomedScale;
            const relativeY = (selectedNode.offset().top - ouputCanvas.offset().top) / currentTransform.k * zoomedScale;

            const graphWidth = g.graph().width * zoomedScale;
            const graphHeight = g.graph().height * zoomedScale;

            const xtrans = (svgContainer.width() - graphWidth) / 2 + (graphWidth / 2 - relativeX) - selectedWidth / 2;
            const ytrans = (svgContainer.height() - graphHeight) / 2 + (graphHeight / 2 - relativeY) - selectedHeight / 2;

            d3svg.transition().duration(1500).call(zoom.transform, d3.zoomIdentity.translate(xtrans, ytrans).scale(zoomedScale));
        }
    }


    applicationsGridDataBound() {
        const grid = $("#familyTreeInventionApplicationGrid").data("kendoGrid");
        const dataSource = grid.dataSource;
        const data = dataSource.view();

        if (data.length === 0) {
            console.warn("No data available yet.");
            return;
        }

        const sort = dataSource.sort() || [];

        if (sort.length > 0) {
            const query = new kendo.data.Query(data);
            try {
                const sortedData = query.sort(sort).data;
                this.applicationAppIds = sortedData.map(e => e.AppId);
            } catch (error) {
                console.error("Sorting error:", error, "Sort value:", sort);
            }
        }
    }


    getInventionApplicationGrid() {
        const countryApplicationSearchParam = () => {
            var InvId = 0;
            var foundNode = this.treeJson["Nodes"].find(x => x.Type === "I");
            if (foundNode) {
                InvId = foundNode.KeyId;
            }
            return {
                CountryApplicationInvId: InvId,
                CountryApplicationCaseNumber: "",
                CountryApplicationCountry: "",
                CountryApplicationSubCase: "",
                CountryApplicationCaseType: "",
                CountryApplicationStatus: "",
                CountryApplicationAppNumber: ""
            };
        };

        $("#familyTreeInventionApplicationGrid").kendoGrid({
            dataSource: {
                transport: {
                    read: function (options) {
                        $.ajax({
                            url: "/Patent/CountryApplication/InventionCountryApplicationGridRead",
                            dataType: "json",
                            type: "POST",
                            contentType: "application/x-www-form-urlencoded",
                            data: countryApplicationSearchParam(),
                            cache: true,
                            success: function (response) {
                                if (response.Data && Array.isArray(response.Data)) {
                                    response.Data.forEach(item => {
                                        if (item.IssDate)
                                            item.IssDate = formatDate(item.IssDate);

                                        if (item.FilDate)
                                            item.FilDate = formatDate(item.FilDate);
                                    });
                                }

                                options.success(response);
                            },
                            error: function (e) {
                                options.error(e);
                                // pageHelper.showErrors(e);
                            }
                        });
                    }
                },
                serverPaging: false,
                serverSorting: false,
                serverFiltering: false,
                schema: {
                    data: "Data",
                    total: "Total"
                },
                sort: { field: "Country", dir: "asc" },
                error: function (e) {
                    pageHelper.showErrors(e);
                }
            },
            autoBind: false,
            columns: [
                {
                    field: "CaseNumber",
                    title: "Case Number",
                    attributes: { class: "d-none d-md-table-cell name-field" },
                    headerAttributes: { class: "d-none d-md-table-cell" }
                },
                {
                    field: "Country",
                    title: "Country",
                },
                {
                    field: "SubCase",
                    title: "Sub Case",
                    attributes: { class: "d-none d-md-table-cell name-field" },
                    headerAttributes: { class: "d-none d-md-table-cell" }
                },
                {
                    field: "CaseType",
                    title: "Case Type",
                    attributes: { class: "d-none d-md-table-cell name-field" },
                    headerAttributes: { class: "d-none d-md-table-cell" }
                },
                {
                    field: "ApplicationStatus",
                    title: "Status",
                },
                {
                    field: "AppNumber",
                    title: "Application No.",
                },
                {
                    field: "FilDate",
                    title: "Filling Date",
                    attributes: { class: "d-none d-md-table-cell name-field" },
                    headerAttributes: { class: "d-none d-md-table-cell" }
                },
                {
                    field: "PatNumber",
                    title: "Patent Number",
                    attributes: { class: "d-none d-lg-table-cell name-field" },
                    headerAttributes: { class: "d-none d-lg-table-cell" }
                },
                {
                    field: "IssDate",
                    title: "Issue Date",
                    attributes: { class: "d-none d-lg-table-cell name-field" },
                    headerAttributes: { class: "d-none d-lg-table-cell" }
                }
            ],
            dataBound: this.applicationsGridDataBound.bind(this),
            sortable: true,
            scrollable: true,
            resizable: true,
            htmlAttributes: { class: "kendo-Grid" }
        });


        const grid = $("#familyTreeInventionApplicationGrid").data("kendoGrid");
        grid.dataSource.read();
    }


    attachEventListeners() {
        const isTD = this.isTD;

        if (!isTD) {
            d3.select("#familyTreeSvg g").selectAll("g.node").on("click", this.highlightNode.bind(this));

            $('#graphDirection').on('change', () => this.renderTreeInternal());
            $('#graphLayout').on('change', () => this.renderTreeInternal());

            $('[data-toggle="tooltip"]').tooltip();

            $('#FTViewToggleFull').on('click', () => {
                if (!this.fullscreen) {
                    $('#InfoSideBar').hide();
                    this.fullscreen = true;
                } else {
                    $('#InfoSideBar').show();
                    this.fullscreen = false;
                }
            });
        }

        function autoResize(svgElement, parentDiv) {
            const gElement = svgElement.querySelector("g");
            if (!gElement) return;

            const bbox = gElement.getBBox();
            const parentRect = parentDiv.getBoundingClientRect();
            const padding = 30;

            let availableHeight;
            let translateY;
            if (!isTD) {
                const headerElement = svgElement.querySelector("#svgHeader");
                const headerHeight = headerElement.getBBox().height;
                availableHeight = parentRect.height - headerHeight - padding;
                translateY = headerHeight + padding / 2 + (availableHeight / 2);
            } else {
                availableHeight = parentRect.height - padding;
                translateY = padding / 2 + (availableHeight / 2);
            }

            const computedScaleFactor = Math.min(
                parentRect.width / bbox.width,
                availableHeight / bbox.height
            );

            const scaleFactor = computedScaleFactor < 1 ? computedScaleFactor : 0.9;

            const xMid = bbox.x + bbox.width / 2;
            const yMid = bbox.y + bbox.height / 2;

            const translateX = parentRect.width / 2;

            gElement.setAttribute(
                "transform",
                `translate(${translateX}, ${translateY}) scale(${scaleFactor}) translate(${-xMid}, ${-yMid})`
            );
        }

        $(".exportPDF").click(() => {
            // console.log("Exporting PDF");
            const { jsPDF } = window.jspdf;

            cpiLoadingSpinner.show();

            this.clearInfo();
            this.centerGraph();

            try {
                if ($("#familyTreeSystem").val() === "patent")
                    this.getInventionApplicationGrid();

                const svgElement = document.querySelector("#familyTreeSvg");
                const foreignObjects = svgElement.querySelectorAll("foreignObject");
                const originalDisplay = [];

                foreignObjects.forEach((element, index) => {
                    originalDisplay[index] = element.style.display;
                    element.style.display = "none"; // Temporarily hide it
                });

                const parentDiv = document.querySelector("#d3-tree");
                autoResize(svgElement, parentDiv);

                const exportScale = 2 * (window.devicePixelRatio || 1);

                html2canvas(parentDiv, {
                    useCORS: true,
                    allowTaint: false,
                    scale: exportScale
                }).then(async canvas => {
                    const imgData = canvas.toDataURL("image/jpeg", 1.0);
                    const margin = 40; // Margin around the image

                    const canvasWidthPt = canvas.width * 0.75;  //in points; todo
                    const canvasHeightPt = canvas.height * 0.75;

                    const pdf = new jsPDF({
                        orientation: canvasWidthPt > canvasHeightPt ? 'landscape' : 'portrait',
                        unit: 'pt',
                        format: [canvasWidthPt + 2 * margin, canvasHeightPt + 2 * margin]
                    });


                    const pdfWidth = pdf.internal.pageSize.getWidth();
                    const pdfHeight = pdf.internal.pageSize.getHeight();

                    const availableWidth = pdfWidth - 2 * margin;
                    const availableHeight = pdfHeight - 2 * margin;
                    // console.log(availableWidth, availableHeight);
                    // console.log(canvas.width, canvas.height);
                    const imgRatio = canvas.width / canvas.height;
                    const pdfRatio = availableWidth / availableHeight;
                    // console.log(imgRatio, pdfRatio);

                    let imgWidth, imgHeight;
                    if (imgRatio > pdfRatio) {
                        imgWidth = availableWidth;
                        imgHeight = (canvas.height * imgWidth) / canvas.width;
                    } else {
                        imgHeight = availableHeight;
                        imgWidth = (canvas.width * imgHeight) / canvas.height;
                    }

                    const imgX = (pdfWidth - imgWidth) / 2; // Centering the image
                    const imgY = (pdfHeight - imgHeight) / 2;

                    pdf.addImage(imgData, 'JPEG', imgX, imgY, imgWidth, imgHeight);

                    const logoUrl = `${window.location.origin}/images/site_report_logo.png`;
                    let logoWidth, logoHeight;


                    async function loadImageAndAddToPDF(logoUrl, pdf, margin) {
                        try {
                            const { width, height } = await loadImage(logoUrl);
                            logoWidth = width;
                            logoHeight = height;
                            pdf.addImage(logoUrl, 'PNG', margin, margin, logoWidth, logoHeight);
                        } catch (error) {
                            throw ('Error loading image:', error);
                        }
                    }

                    await loadImageAndAddToPDF(logoUrl, pdf, margin);

                    if ($("#familyTreeSystem").val() === "patent") {
                        pdf.addPage();

                        const headerElement = svgElement.querySelector("#svgHeader");
                        const svgBox = svgElement.getBBox();
                        const headerBox = headerElement.getBBox();

                        const cropRatio = headerBox.height / svgBox.height;

                        const cropHeight = Math.floor(canvas.height * cropRatio);

                        const croppedCanvas = document.createElement("canvas");
                        const ctx = croppedCanvas.getContext("2d");

                        croppedCanvas.width = canvas.width;
                        croppedCanvas.height = cropHeight;

                        // Crop only the top portion of the imgData
                        ctx.drawImage(
                            canvas,
                            0, 0,                      // Source X, Y
                            canvas.width, cropHeight,  // Source Width & Height
                            0, 0,                      // Destination X, Y
                            canvas.width, cropHeight   // Destination Width & Height
                        );

                        const croppedImgData = croppedCanvas.toDataURL("image/png");
                        pdf.addImage(croppedImgData, 'PNG', margin, margin, imgWidth, cropHeight * 0.75);

                        //console.log(logoWidth, logoHeight);
                        pdf.addImage(logoUrl, 'PNG', margin, margin, logoWidth, logoHeight);


                        const grid = $("#familyTreeInventionApplicationGrid").data("kendoGrid");
                        const dataSource = grid.dataSource.view();
                        const columns = grid.columns.map(col => col.title || col.field);

                        const headers = [columns];
                        const data = dataSource.map(item => grid.columns.map(col => item[col.field]));

                        const estimatedColWidth = availableWidth / columns.length;
                        const minFontSize = 14;
                        const maxFontSize = 30;
                        const calculatedFontSize = Math.max(
                            minFontSize,
                            Math.min(maxFontSize, (estimatedColWidth / 10)) // Adjust based on column width
                        );

                        pdf.autoTable({
                            head: headers,
                            body: data,
                            startY: Math.max(logoHeight, cropHeight) + margin + 10,
                            theme: "grid",
                            headStyles: {
                                fillColor: [224, 224, 224],
                                textColor: [0, 0, 0],
                                fontStyle: "bold"
                            },
                            styles: {
                                fontSize: calculatedFontSize,
                                cellPadding: 5,
                                overflow: 'linebreak', // Allow text to break across lines
                                halign: 'center',
                                // valign: 'middle',
                            },
                            tableWidth: 'auto', // Dynamically adjust table width
                        });
                    }

                    pdf.save(this.treeViewName + ".pdf");
                }).catch(error => {
                    pageHelper.showErrors(`Error from generating report, please report this as a bug: ${error}`, 15);
                    console.error("html2canvas error:", error);
                }).finally(() => {
                    foreignObjects.forEach((element, index) => {
                        element.style.display = originalDisplay[index];
                    });
                    $("#familyTreeInventionApplicationGrid").empty();
                    const gElement = svgElement.querySelector("g");
                    if (gElement) gElement.removeAttribute("transform");

                    // this.highlightNode($("#familyTreeSelectedValue").val());
                    this.centerGraph();
                    cpiLoadingSpinner.hide();
                });
            } catch (error) {
                pageHelper.showErrors(`Error from generating report, please report this as a bug: ${error}`, 15);
                console.error("Please report this error:", error);
            }
        });


        $(".exportPPT").click(() => {
            cpiLoadingSpinner.show();

            this.clearInfo();
            this.centerGraph();

            if ($("#familyTreeSystem").val() === "patent")
                this.getInventionApplicationGrid();

            const svgElement = document.querySelector("#familyTreeSvg");
            const foreignObjects = svgElement.querySelectorAll("foreignObject");
            const originalDisplay = [];

            foreignObjects.forEach((element, index) => {
                originalDisplay[index] = element.style.display;
                element.style.display = "none"; // Temporarily hide it
            });

            const parentDiv = document.querySelector("#d3-tree");
            autoResize(svgElement, parentDiv);

            const exportScale = 2 * (window.devicePixelRatio || 1);

            html2canvas(parentDiv, {
                useCORS: true,
                allowTaint: false,
                scale: exportScale
            }).then(async (canvas) => {
                const imgData = canvas.toDataURL("image/jpeg", 1.0);
                const pptx = new PptxGenJS();

                const pptWidth = 10; // Default width in inches
                const pptHeight = 5.625; // Default height in inches
                pptx.defineLayout({ name: "MSFT PPT", width: pptWidth, height: pptHeight });

                const slide = pptx.addSlide({ layout: "MSFT PPT" });

                const imgRatio = canvas.width / canvas.height;
                let imgWidth = pptWidth - 1; // Slight margin from edges
                let imgHeight = imgWidth / imgRatio;

                slide.addImage({
                    data: imgData,
                    x: (pptWidth - imgWidth) / 2, // Centering the image
                    y: (pptHeight - imgHeight) / 2,
                    w: imgWidth,
                    h: imgHeight
                });


                const logoUrl = `${window.location.origin}/images/site_report_logo.png`;
                let logoWidth, logoHeight;
                const margin = 0.2;

                const logoBase64 = await fetch(logoUrl)
                    .then(res => res.blob())
                    .then(blob => new Promise((resolve) => {
                        const reader = new FileReader();
                        reader.onloadend = () => resolve(reader.result.split(",")[1]); // Get base64 without prefix
                        reader.readAsDataURL(blob);
                    })); // Adjusted for pptx units
                async function loadImageAndAddToSlide(logoUrl, slide, margin) {
                    try {
                        const { width, height } = await loadImage(logoUrl);
                        logoWidth = width / 96 * 0.75;
                        logoHeight = height / 96 * 0.75;
                        //console.log(logoWidth, logoHeight);
                        slide.addImage({
                            data: `data:image/png;base64,${logoBase64}`,
                            x: margin,
                            y: margin,
                            w: logoWidth,
                            h: logoHeight
                        });
                    } catch (error) {
                        throw ('Error loading image:', error);
                    }
                }

                await loadImageAndAddToSlide(logoUrl, slide, margin);

                if ($("#familyTreeSystem").val() === "patent") {

                    const grid = $("#familyTreeInventionApplicationGrid").data("kendoGrid");
                    const dataSource = grid.dataSource.view();
                    const columns = grid.columns.map(col => col.title || col.field);

                    const headers = columns.map(title => ({ text: title, options: { bold: true, fill: 'E0E0E0' } }));
                    const data = dataSource.map(item => grid.columns.map(col => ({ text: item[col.field] || "", options: {} })));

                    const totalCols = columns.length;
                    const estimatedColWidth = (pptWidth - 1) / totalCols;

                    // Need a better way to calculate font
                    //const calculatedFontSize = Math.max(
                    //    14, // minFontSize
                    //    Math.min(20, (estimatedColWidth / 10)) // maxFontSize; adjust based on column width
                    //);

                    const calculatedFontSize = 10;
                    const maxRowsPerSlide = 12;

                    function addSlideWithTable(rows) {
                        const slide = pptx.addSlide({ layout: "MSFT PPT" });

                        // Add logo
                        slide.addImage({
                            data: `data:image/png;base64,${logoBase64}`,
                            x: margin,
                            y: margin,
                            w: logoWidth,
                            h: logoHeight
                        });

                        // Add table
                        slide.addTable(
                            [headers, ...rows],
                            {
                                x: 0.5,
                                y: Math.max(1, margin + logoHeight),
                                w: pptWidth - 1,
                                colW: estimatedColWidth,
                                fontSize: calculatedFontSize,
                                color: "000000",
                                border: { pt: "1", color: "C0C0C0" },
                                align: "center",
                            }
                        );
                    }

                    // Split data into multiple slides
                    for (let i = 0; i < data.length; i += maxRowsPerSlide) {
                        const chunk = data.slice(i, i + maxRowsPerSlide);
                        addSlideWithTable(chunk);
                    }
                }

                pptx.writeFile({ fileName: "Family_Tree_View.pptx" });
            }).catch((error) => {
                pageHelper.showErrors(error);
                console.error("html2canvas error:", error);
            }).finally(() => {
                foreignObjects.forEach((element, index) => {
                    element.style.display = originalDisplay[index];
                });

                $("#familyTreeInventionApplicationGrid").empty();
                const gElement = svgElement.querySelector("g");
                if (gElement) gElement.removeAttribute("transform");

                this.centerGraph();
                cpiLoadingSpinner.hide();
            });
        });

    }


    highlightNode(nodeId) {
        this.clearInfo();
        if (nodeId !== $("#familyTreeSelectedValue").val()) {
            const node = g.node(nodeId);
            this.displayInfo(node);
            this.centerAroundNode(node);
        } else {
            $("#familyTreeSelectedValue").val("");
        }
    }


    clearInfo() {
        $("#info").empty();
        g.nodes().forEach(id => {
            $(`#${id}`).removeClass('childPat parentPat selectedNode');
        });

        // $("#familyTreeSelectedValue").val("");

        g.edges().forEach(edgeId => {
            const edge = g.edge(edgeId);
            if (edge && edge.elem) {
                $(edge.elem).removeClass('childEdge parentEdge');
            }
        });
    }


    displayInfo(node) {
        let isTradeSecret = this.treeJson["Area"] === 2;
        let info = document.createElement("div");
        info.classList.add("infoDetails");

        info.appendChild(createInfoHeader(node));

        if (isTradeSecret) {
            const div = document.createElement("div");
            div.classList.add("infoDetailsContent");

            const span = document.createElement("span");
            span.classList.add("dymo-alert");
            span.textContent = "Trade Secret";
            info.appendChild(div.appendChild(span));
        }

        if (node.Type !== "F") {
            info.appendChild(createInfo((node.Type !== "T" ? "Title" : "Trademark"), (node["Title"] === null ? "" : node["Title"])));
            this.nodeDisplayOptions.forEach(option => {
                if (!option.Include) return;
                if (option.PropertyName !== "Client" && node["Type"] !== "C" && node["Type"] !== "T")
                    return;

                const value = node[option.PropertyName] || "";
                info.appendChild(createInfo(option.Label, value));
            });
        }

        const parentHeight = $("#InfoSideBar").parent().height();

        $("#InfoSideBar").css({
            height: parentHeight + "px",
            overflowY: "auto"
        });

        $("#info").append(info);

        $(node.elem).addClass('selectedNode');
        $("#familyTreeSelectedValue").val(node.Id);

        this.highlightForwards(node.Id);
        this.highlightBackwards(node.Id);
    }


    highlightForwards(nodeId) {
        const visited = new Set();
        const stack = [nodeId];

        while (stack.length > 0) {
            const currentId = stack.pop();
            visited.add(currentId);

            //if (currentId !== startId) {
            //    $(document.getElementById(currentId)).addClass('childNode');
            //}

            const children = this.parentToChildren.get(currentId);
            if (!children) continue;

            for (const childId of children) {
                const edge = g.edge(currentId, childId);
                if (edge && edge.elem) {
                    $(edge.elem).addClass('childEdge');
                }

                if (!visited.has(childId)) {
                    stack.push(childId);
                }
            }
        }
    }


    highlightBackwards(nodeId) {
        const visited = new Set();
        const stack = [nodeId];

        while (stack.length) {
            const currentId = stack.pop();
            visited.add(currentId);

            //if (nodeName !== anId) {
            //    $(`#${nodeName}`).addClass('parentNode');
            //}

            const parents = this.childToParents.get(currentId);
            if (!parents) continue;

            for (const parentId of parents) {
                const edge = g.edge(parentId, currentId);
                if (!edge) continue;

                $(edge.elem).addClass('parentEdge');

                if (!visited.has(parentId)) {
                    stack.push(parentId);
                }
            }
        }
    }



    attachViewHeader() {
        let svg = d3.select("#familyTreeSvg");
        let header = this.treeJson['Header'];

        const viewTitle = [
            { bold: `Family Tree for ${header.LabelCaseNumber}: `, normal: ` ${header.CaseNumber}` },
            { bold: `${header.LabelTitle}: `, normal: ` ${formatList(header.Title)}` },
            { bold: `${header.LabelClient}: `, normal: ` ${formatList(header.Client)}` }
        ];

        $("#svgHeader").remove();

        let textElement = svg.append("text")
            .attr("id", "svgHeader")
            .attr("x", "50%")
            .attr("y", 15)
            .attr("text-anchor", "middle")
            .attr("font-size", "16px")
            .attr("fill", "rgb(47, 72, 88)");

        //let viewBoxWidth = svg.attr("viewBox")?.split(" ")[2]; 
        let maxWidth = $("#d3-tree").width() / 3;

        function wrapText(text, width) {
            let words = text.split(/\s+/);
            let lines = [];
            let currentLine = words.shift();

            words.forEach(word => {
                if ((currentLine + " " + word).length > width / 6) { // Rough character width calculation
                    lines.push(currentLine);
                    currentLine = word;
                } else {
                    currentLine += " " + word;
                }
            });

            lines.push(currentLine);
            return lines;
        }


        viewTitle.forEach((line, i) => {
            let wrappedLines = wrapText(line.normal.trim(), maxWidth);
            let tspan = textElement.append("tspan")
                .attr("x", "50%")
                .attr("dy", i === 0 ? "0em" : `1.2em`);

            tspan.append("tspan")
                .attr("font-weight", "bold")
                .text(line.bold);

            wrappedLines.forEach((wrappedLine, j) => {
                if (j === 0) {
                    tspan.append("tspan")
                        .text(j === 0 ? wrappedLine : " " + wrappedLine);
                } else {
                    textElement.append("tspan")
                        .attr("x", "50%")
                        .attr("dy", `1.2em`)
                        .text(" " + wrappedLine);
                }
            });

        });
    }


    attachViewStatistics() {
        let svg = d3.select("#familyTreeSvg");
        let stats = this.treeJson['Stats'];
        var viewStats;
        if ($("#familyTreeSystem").val() === "patent") {
            let priority = ["Priority: "]
            for (let i = 0; i < stats.PriDate.length; i++) {
                const priorityInfo = (stats.PriCountry[i] || "") + " " + (stats.PriNumber[i] || "") + " " + (stats.PriDate[i] ? formatDate(stats.PriDate[i]) : "");
                priority[priority.length - 1] += priorityInfo;
                if (i < stats.PriDate.length - 1) {
                    if (priorityInfo.trim() !== "") {
                        priority.push("; ");
                    } else {
                        priority[priority.length - 1] += ", ";
                    }
                }
            }

            let appStats = [
                `Number of Active Applications: ${stats.ActiveCount}`,
                `Number of Inactive Applications: ${stats.InactiveCount}`,
                `Number of Expired Patents: ${stats.ExpiredPatents}`,
                `Number of Validated Applications: ${stats.ValidatedApps}`
            ]

            viewStats = priority.concat(appStats);
        } else {
            viewStats = [
                `Mark Type: ${stats.MarkType || ""}`,
                `Classes: ${stats.Classes || ""}`,
                `Number of Active Trademarks: ${stats.ActiveCount}`,
                `Number of Inactive Applications: ${stats.InactiveCount}`,
                `Madrid Protocol: ${stats.MadridProtocol ? "Yes" : "No"}`,
                `European Union (Community): ${stats.EuropeanUnion ? "Yes" : "No"}`,
            ]
        }


        let bbox = svg.node().getBoundingClientRect();
        let svgHeight = bbox.height;

        $("#svgFooter").remove();

        let textElement = svg.append("text")
            .attr("id", "svgFooter")
            .attr("x", 10)
            .attr("y", svgHeight - viewStats.length * 16)
            .attr("text-anchor", "start")
            .attr("font-size", "14px")
            .attr("fill", "rgb(85, 113, 124)");

        viewStats.forEach((line, i, arr) => {
            textElement.append("tspan")
                .attr("x", 10)
                .attr("dy", i === 0 ? "0em" : "1.2em")
                .text(line);
        });
    }


    attachTDViewHeader() {
        let svg = d3.select("#familyTreeSvg");
        let header = this.treeJson['Header'];

        const viewTitle = [
            { bold: `${header.LabelCaseNumber}: `, normal: ` ${header.CaseNumber}` },
            { bold: `${header.LabelTitle}: `, normal: ` ${formatList(header.Title)}` },
            { bold: `${header.LabelExpDate}: `, normal: ` ${formatDate(header.ExpDate)}` }
        ];

        $("#svgHeader").remove();

        let textElement = svg.append("text")
            .attr("id", "svgHeader")
            .attr("x", "50%")
            .attr("y", 15)
            .attr("text-anchor", "middle")
            .attr("font-size", "16px")
            .attr("fill", "rgb(47, 72, 88)");

        //let viewBoxWidth = svg.attr("viewBox")?.split(" ")[2]; 
        let maxWidth = $("#d3-tree").width() / 3;

        function wrapText(text, width) {
            let words = text.split(/\s+/);
            let lines = [];
            let currentLine = words.shift();

            words.forEach(word => {
                if ((currentLine + " " + word).length > width / 6) { // Rough character width calculation
                    lines.push(currentLine);
                    currentLine = word;
                } else {
                    currentLine += " " + word;
                }
            });

            lines.push(currentLine);
            return lines;
        }


        viewTitle.forEach((line, i) => {
            let wrappedLines = wrapText(line.normal.trim(), maxWidth);
            let tspan = textElement.append("tspan")
                .attr("x", "50%")
                .attr("dy", i === 0 ? "0em" : `1.2em`);

            tspan.append("tspan")
                .attr("font-weight", "bold")
                .text(line.bold);

            wrappedLines.forEach((wrappedLine, j) => {
                if (j === 0) {
                    tspan.append("tspan")
                        .text(j === 0 ? wrappedLine : " " + wrappedLine);
                } else {
                    textElement.append("tspan")
                        .attr("x", "50%")
                        .attr("dy", `1.2em`)
                        .text(" " + wrappedLine);
                }
            });
        });
    }



    attachTDViewLegend() {
        const svg = d3.select("#familyTreeSvg");
        let bbox = svg.node().getBoundingClientRect();

        const defs = svg.append("defs");

        defs.append("marker")
            .attr("id", "arrowhead")
            .attr("markerWidth", 10)
            .attr("markerHeight", 7)
            .attr("refX", 10)
            .attr("refY", 3.5)
            .attr("orient", "auto")
            .append("polygon")
            .attr("points", "0 0, 10 3.5, 0 7")
            .attr("fill", "black");


        const legends = [
            ["Continuation", "", true],
            ["Continuation-in-part", "7,2", true],
            ["Divisional", "2,4", true],
        ];

        let rectHeight = 70;
        let legendY = bbox.height - (rectHeight + 10);

        const legendGroup = svg.append("g")
            .attr("id", "legend")
            .attr("transform", `translate(20, ${legendY})`);

        legendGroup.append("rect")
            .attr("width", 220)
            .attr("height", rectHeight)
            .attr("fill", "white")
            .attr("stroke", "black")
            .attr("stroke-width", 2);

        const x1 = 10;
        const x2 = 100;
        const textOffsetX = 110;
        const startY = 15;
        const lineSpacing = 20;

        legends.forEach((legend, index) => {
            const [label, dasharray, arrowHead] = legend;
            const y = startY + index * lineSpacing;

            const line = legendGroup.append("line")
                .attr("x1", x1)
                .attr("y1", y)
                .attr("x2", x2)
                .attr("y2", y)
                .attr("stroke", "black")

            if (dasharray) {
                line.attr("stroke-dasharray", dasharray);
            }

            if (arrowHead) {
                line.attr("marker-end", "url(#arrowhead)");
            }

            // Draw the label
            legendGroup.append("text")
                .attr("x", textOffsetX)
                .attr("y", y + 4)
                .attr("font-size", "12px")
                .attr("font-family", "serif")
                .text(label);
        });
    }


    getTdMetaData(sortedDates) {
        if (sortedDates.length == 0) return null;
        const maxDate = sortedDates.at(-1).Date;
        const minDate = sortedDates.at(0).Date;

        const nodes = d3.selectAll("g.node").nodes();
        const maxNodeWidth = Math.max(...nodes.map(node => node.getBBox().width));
        const maxNodeHeight = Math.max(...nodes.map(node => node.getBBox().height)) * 1.3;

        const totalPixelWidth = $("#d3-tree").width() * 0.7;
        const c = (1000 * 60 * 60 * 24);
        const totalDays = (maxDate - minDate) / c;
        const pixelsPerDay = totalPixelWidth / totalDays;
        const offsetDays = (maxNodeWidth + 20) / pixelsPerDay;
        const radius = 12;
        const ptaNodes = this.treeJson['Nodes'].filter(node => node.id.includes("PTA"));
        const tdcNodes = [];

        let shortestExpDate = null;
        let tempShortestExpDate = null;

        const eligibleTDNodes = this.treeJson['Nodes'].filter(
            node => node.Type === "TD" && node.ExpDate != null
        );

        const hasEligibleTD = eligibleTDNodes.length > 0;

        if (hasEligibleTD) {
            shortestExpDate = eligibleTDNodes.reduce((minNode, currNode) => {
                const minDate = new Date(minNode.ExpDate);
                const currDate = new Date(currNode.ExpDate);
                return currDate < minDate ? currNode : minNode;
            }).ExpDate;

            shortestExpDate = new Date(shortestExpDate);
        }

        if (!shortestExpDate) {
            tempShortestExpDate = new Date(maxDate);
            tempShortestExpDate.setDate(tempShortestExpDate.getDate() + offsetDays);
            shortestExpDate = new Date(tempShortestExpDate);
        } else {
            tempShortestExpDate = new Date(maxDate);
            tempShortestExpDate.setDate(tempShortestExpDate.getDate() + offsetDays);
        }

        ptaNodes.forEach((node, index) => {
            let newDate = new Date(maxDate); // clone maxDate
            const expDate = node.ExpDate ? new Date(node.ExpDate) : null;
            const tempDate = node.IssDate ? new Date(node.IssDate) : (node.FilDate ? new Date(node.FilDate) : null);
            //console.log(node.IssDate, node.FilDate);
            //console.log(newDate, expDate, tempDate, shortestExpDate, tempShortestExpDate);

            if (expDate && tempDate && expDate <= maxDate && expDate >= tempDate) {
                newDate = new Date(expDate);
            }
            else if (expDate && tempDate && expDate < tempDate && node.Type !== "TDC") {
                newDate = new Date(tempDate);
                newDate.setDate(newDate.getDate() + offsetDays);
            }
            else {
                newDate = new Date(tempShortestExpDate);
                if (node.PatentTermAdj) {
                    newDate.setDate(newDate.getDate() + node.PatentTermAdj);

                }
            }

            // spacing adjustment to avoid visual overlap
            //if (node.Type !== "TDC" && expDate) {
            //    const diffInPixels = Math.abs(expDate - shortestExpDate) / c * pixelsPerDay;
            //    const minPixelThreshold = (radius * 2) * 15;

            //    if (diffInPixels < minPixelThreshold) {
            //        newDate.setDate(newDate.getDate() + minPixelThreshold);
            //    }
            //}

            node.Date = newDate;
            if (node.Type === "TDC") tdcNodes.push(node);
            sortedDates.push({ Id: node.id, Date: node.Date });
        });



        const tdMetaData = {
            maxDate: maxDate,
            minDate: minDate,
            c: c,
            pixelsPerDay: pixelsPerDay,
            minPixelGap: { width: maxNodeWidth, height: maxNodeHeight },
            maxPixelGapInWidth: maxNodeWidth * 3,
            shortestExpDate: hasEligibleTD ? shortestExpDate : null,
            ptaNodes: tdcNodes
        }

        return tdMetaData;
    }



    adjustTDEdges(sortedDates, tdMetaData) {
        let svg = d3.select("#familyTreeSvg");
        //console.log(tdMetaData);
        let globalYCollision = new Map();

        const distanceBetweenNodes = (nodeA, nodeB) => {
            const dx = nodeA.x - nodeB.x;
            const dy = nodeA.y - nodeB.y;
            const signX = Math.sign(dx) || 1;
            const signY = Math.sign(dy) || 1;
            return { dx: dx, dy: dy, signX: signX, signY: signY };
        }

        const findClosestThresholds = (arr, targetDate) => {
            if (!arr.length) return [];

            let minDiff = Math.abs(targetDate - arr[0].date);

            for (let i = 1; i < arr.length; i++) {
                const diff = Math.abs(targetDate - arr[i].date);
                if (diff < minDiff) {
                    minDiff = diff;
                }
            }

            const closest = [];
            for (const item of arr) {
                if (Math.abs(targetDate - item.date) === minDiff) {
                    closest.push(item);
                }
            }
            return closest;
        };

        const insertThresholdSorted = (arr, item) => {
            let i = arr.length - 1;
            while (i >= 0 && arr[i].date > item.date) i--;
            arr.splice(i + 1, 0, item);
        };

        const applyOffset = (node) => {
            const closestThresholds = findClosestThresholds(thresholds, node.Date);
            const daysSinceBase = (node.Date - closestThresholds[0].date) / tdMetaData.c;
            node.x = closestThresholds[0].offset + daysSinceBase * tdMetaData.pixelsPerDay;
            //console.log(`%cOffset for ${node.Text}: ${node.x}`, "color: green;");
            //console.log(closestThresholds);
            return closestThresholds;
        };


        const checkCollision = (endNode, startNode, yCollision) => {
            if (!startNode) return;

            const minPixelGap = tdMetaData.minPixelGap;
            const nodePrefix = id => id.substring(0, id.indexOf('-'));

            if (startNode.yModified) {
                endNode.y = startNode.y;
                endNode.yModified = true;
                //console.log("%cy position compromised", "color: red");
            }
            else if (nodePrefix(startNode.Id) === nodePrefix(endNode.Id)) {
                endNode.y = startNode.y;
            }

            const dist = distanceBetweenNodes(endNode, startNode);

            if (Math.abs(dist.dx) < minPixelGap.width && Math.abs(dist.dy) < minPixelGap.height) {
                endNode.x = startNode.x + dist.signX * minPixelGap.width;
            }

            if (dist.signX === -1 && Math.abs(dist.dy) < minPixelGap.height) {
                endNode.y = startNode.y + dist.signY * minPixelGap.height;
                //endNode.yModified = true;
            }

            if (yCollision.has(startNode.y)) {
                startNode.y = endNode.y + dist.signY * minPixelGap.height;
                startNode.yModified = true;
                svg.select(`g.node[id='${startNode.Id}']`).attr("transform", `translate(${startNode.x},${startNode.y})`);
            }

            yCollision.add(startNode.y);
        };

        // Find parent nodes
        const findParentNodes = (node) => {
            let nodeId = node.Id;
            let parentIds = this.childToParents.get(nodeId) || [];

            if (!parentIds.length) {
                let currentForwardEdges = this.parentToChildren.get(nodeId);
                if (currentForwardEdges) {
                    currentForwardEdges.forEach(startNodeId => {
                        const reverseStartNode = g.node(startNodeId);
                        if (node.Date > reverseStartNode.Date) parentIds.push(startNodeId);
                    });
                }
            }

            return parentIds;
        };

        // Recursive node processor
        const processNode = (node) => {
            if (processed.has(node.Id)) return;
            processed.add(node.Id);


            const nodeGroup = svg.select(`g.node[id='${node.Id}']`);
            const parentIds = findParentNodes(node) || [];

            const thresholdResults = applyOffset(node);

            thresholdResults.forEach(threshold => {
                const thresholdId = threshold.Id;
                if (!parentIds.includes(thresholdId) && node.Id != thresholdId) {
                    parentIds.push(thresholdId);
                }
            });

            let yCollision = new Set();

            if (parentIds.length > 0) {
                parentIds.forEach(parentId => {
                    const parentNode = g.node(parentId);
                    if (!parentNode) return;
                    //console.log(`%cProcessing ${node.Text} ${node.Id}, parents: ${parentNode.Text}`, "color: navy");
                    checkCollision(node, parentNode, yCollision);
                });
            } else {
                //console.log(`%cProcessing ${node.Text} ${node.Id}, parents: null`, "color: navy");
                checkCollision(node, null, yCollision);
            }

            nodeGroup.attr("transform", `translate(${node.x},${node.y})`);
            insertThresholdSorted(thresholds, { date: node.Date, offset: node.x, id: node.Id });
            //console.log("adjusted offset:", node.x);
            //console.table(thresholds.map(t => ({
            //    date: t.date?.toISOString?.() ?? t.date,
            //    offset: t.offset
            //})));
            //console.log("---------------------------------");
        };



        const thresholds = [{ date: tdMetaData.minDate, offset: g.node(sortedDates[0].Id).x, id: sortedDates[0].Id }];
        const processed = new Set();
        let maxY = -Infinity;

        // use a sorted list and search backwards
        sortedDates.forEach(({ Id, Date }) => {
            const node = g.node(Id);
            if (!node) return;
            processNode(node);

            // for the legend
            maxY = Math.max(maxY, node.y);
        });


        // update edge paths
        svg.selectAll("g.edgePath").each(function (e) {
            const sourceNode = g.node(e.v);
            const targetNode = g.node(e.w);

            const sourcePoint = { x: sourceNode.x, y: sourceNode.y };
            const targetPoint = { x: targetNode.x, y: targetNode.y };

            const sourceIntersect = sourceNode.intersect
                ? sourceNode.intersect(targetPoint)
                : sourcePoint;

            const targetIntersect = targetNode.intersect
                ? targetNode.intersect(sourcePoint)
                : targetPoint;

            const points = [sourceIntersect, targetIntersect];

            const line = d3.line().x(d => d.x).y(d => d.y).curve(d3.curveBasis);
            const pathStr = line(points);

            d3.select(this).select("path").attr("d", pathStr);
        });

        ////**************************************************************************/
        if (tdMetaData.ptaNodes.length == 0) return

        const minY = Math.min(...tdMetaData.ptaNodes.map(node => node.y))
        const rectHeight = 60;
        const rectWidth = 220;
        const rectPadding = 100;
        const legendX = tdMetaData.ptaNodes[0].x;
        const legendY = maxY + rectPadding;
        const lineSpacing = 20;

        const inner = svg.select("g.output");
        const legendGroup = inner.append("g")
            .attr("id", "innerLegend")
            .attr("transform", `translate(${legendX - rectWidth / 2}, ${legendY})`);

        legendGroup.append("rect")
            .attr("width", rectWidth)
            .attr("height", rectHeight)
            .attr("fill", "white")
            .attr("stroke", "black")
            .attr("stroke-width", 2);

        legendGroup.append("line")
            .attr("x1", rectWidth / 2)
            .attr("y1", 0)
            .attr("x2", rectWidth / 2)
            .attr("y2", -rectPadding - (maxY - minY))
            .attr("stroke", "black")
            .attr("stroke-width", 1.5);

        legendGroup.append("text")
            .attr("x", rectWidth / 2)
            .attr("y", 5 + lineSpacing)
            .attr("text-anchor", "middle")
            .attr("font-weight", "bold")
            .attr("font-size", "14px")
            .text("Expiration without PTA");

        legendGroup.append("text")
            .attr("x", rectWidth / 2)
            .attr("y", 5 + lineSpacing + lineSpacing)
            .attr("text-anchor", "middle")
            .attr("font-weight", "bold")
            .attr("font-size", "14px")
            .text(tdMetaData.shortestExpDate ? formatDate(tdMetaData.shortestExpDate) : "No Expiration Date");
    }



    renderTreeInternalTD() {
        g = new dagreD3.graphlib.Graph().setGraph({
            rankdir: "LR",
            nodesep: 100,
        });

        this.childToParents = new Map();
        this.parentToChildren = new Map();
        const treeNodes = this.treeJson['Nodes'];
        const relationships = this.treeJson['Edges'];

        let sortedDates = [];

        for (let i = 0; i < treeNodes.length; i++) {
            let node = treeNodes[i];
            node.label = "";
            node.id = node.Id;
            node.shape = "TDTreeNode";
            node.Date = new Date(node.Date);

            if (!node.id.includes("PTA"))
                sortedDates.push({ Id: node.id, Date: new Date(node.Date) });

            g.setNode(node.id, node);
        }

        // set edges
        for (let i = 0, len = relationships.length; i < len; i++) {
            let edge = relationships[i];
            let start = edge.StartId;
            const startNode = g.node(start);
            let end = edge.EndId;
            const endNode = g.node(end);

            let reverse = endNode.Date < startNode.Date && !endNode.id.includes("PTA");
            if (reverse)
                [start, end] = [end, start];

            let currentForwardEdges = this.parentToChildren.get(start);
            if (currentForwardEdges === undefined) {
                currentForwardEdges = [];
                this.parentToChildren.set(start, currentForwardEdges);
            }
            if (!currentForwardEdges.includes(end))
                currentForwardEdges.push(end);

            let currentBackwardEdges = this.childToParents.get(end);
            if (currentBackwardEdges === undefined) {
                currentBackwardEdges = [];
                this.childToParents.set(end, currentBackwardEdges);
            }
            if (!currentBackwardEdges.includes(start))
                currentBackwardEdges.push(start);

            if (reverse)
                [start, end] = [end, start];

            switch (edge.Label) {
                case "CIP":
                    g.setEdge(start, end, { style: "stroke-dasharray: 7, 2;" });
                    break;
                case "CON":
                    g.setEdge(start, end, { style: "" });
                    break;
                case "DIV":
                    g.setEdge(start, end, { style: "stroke-dasharray: 2, 4;" });
                    break;
                default:
                    g.setEdge(start, end, { arrowhead: "undirected" });
            }
        }


        for (const list of this.childToParents.values()) {
            list.sort((a, b) => g.node(a).Date - g.node(b).Date);
        }

        sortedDates.sort((a, b) => {
            const dateDiff = a.Date - b.Date;
            if (dateDiff !== 0) return dateDiff;

            const aParents = this.childToParents.get(a.Id) || [];
            const aLastParent = aParents.at(0);
            let aParentDate = 0;
            if (aLastParent) {
                aParentDate = g.node(aLastParent).Date;
            }

            const bParents = this.childToParents.get(b.Id) || [];
            const bLastParent = bParents.at(0);
            let bParentDate = 0;
            if (bLastParent) {
                bParentDate = g.node(bLastParent).Date;
            }
            return aParentDate - bParentDate;
        });


        let svg = d3.select("#familyTreeSvg");
        if (svg.select("g").empty())
            svg.append("g");

        let inner = svg.select("g");
        zoom = d3.zoom()
            .scaleExtent([0.05, 10])
            .on("zoom", function () {
                if (isFinite(d3.event.transform.x) && isFinite(d3.event.transform.y)) {
                    inner.attr("transform", d3.event.transform);
                }
            });
        svg.call(zoom);
        this.render(inner, g);

        try {
            const tdMetaData = this.getTdMetaData(sortedDates);
            if (tdMetaData) {
                this.attachTDViewLegend();
                this.adjustTDEdges(sortedDates, tdMetaData);
            }
        } catch (error) {
            console.log(error);
            pageHelper.showErrors(`Error rendering Terminal Disclaimer Diagram.`);
        }

        this.attachTDViewHeader();
        this.centerGraph();
    }

    renderTreeInternal() {
        g = new dagreD3.graphlib.Graph()
            .setGraph({
                ranksep: 100,
                rankdir: $('#graphDirection').val() || "LR",
                ranker: $('#graphLayout').val() || "network-simplex",
            });

        this.parentToChildren = new Map();
        this.childToParents = new Map();
        const treeNodes = this.treeJson['Nodes'];
        const relationships = this.treeJson['Edges'];

        // set nodes
        for (let i = 0; i < treeNodes.length; i++) {
            const node = treeNodes[i];
            node.label = "";
            node.id = node.Id;
            node.shape = "treeNode";

            g.setNode(node.id, node);
        }

        // set edges
        for (let i = 0, len = relationships.length; i < len; i++) {
            const edge = relationships[i];
            const start = edge.StartId;
            const end = edge.EndId;

            let currentForwardEdges = this.parentToChildren.get(start);
            if (currentForwardEdges === undefined) {
                currentForwardEdges = [];
                this.parentToChildren.set(start, currentForwardEdges);
            }
            currentForwardEdges.push(end);

            let currentBackwardEdges = this.childToParents.get(end);
            if (currentBackwardEdges === undefined) {
                currentBackwardEdges = [];
                this.childToParents.set(end, currentBackwardEdges);
            }
            currentBackwardEdges.push(start);

            const svgLabel = createEdgeLabel(edge.Label);
            g.setEdge(start, end, { labelType: "svg", label: svgLabel, curve: d3.curveBasis });
        }

        let svg = d3.select("#familyTreeSvg");
        if (svg.select("g").empty()) {
            svg.append("g");
        }
        let inner = svg.select("g");

        zoom = d3.zoom()
            .scaleExtent([0.05, 10])
            .on("zoom", function () {
                if (isFinite(d3.event.transform.x) && isFinite(d3.event.transform.y)) {
                    inner.attr("transform", d3.event.transform);
                }
            });
        svg.call(zoom);

        this.render(inner, g);

        this.clearInfo();
        this.attachViewHeader();
        this.attachViewStatistics();
        this.centerGraph();

        let selectedNode = g.node($("#familyTreeParamValue").val());
        if (selectedNode !== undefined) {
            this.displayInfo(selectedNode);
        }

        if ($("#familyTreeParamValue").val() != "") {
            $("#" + $("#familyTreeParamValue").val()).addClass("mainNode");
        }
    }


    async reRender(toFetchTreeData, toFetchDisplayOptions) {
        if (toFetchTreeData) {
            this.treeJson = await this.fetchTreeData();
            d3.select("#familyTreeSvg").empty();
            this.renderTreeInternal();
        }
        if (toFetchDisplayOptions) {
            let { properties, _ } = await this.fetchDisplayOptions();
            this.nodeDisplayOptions = properties;
            let selectedNode = g.node($("#familyTreeSelectedValue").val());
            if (selectedNode !== undefined) {
                $("#info").empty();
                this.displayInfo(selectedNode);
            }
        }
    }


    async fetchTreeData() {
        cpiLoadingSpinner.show();
        return $.ajax({
            url: $("#d3-tree").data('data-url'),
            type: "GET",
            dataType: "json",
            data: this.fetchTreeArgument,
            success: (response) => {
                console.log(response);  // debuging purpose
            },
            error: (xhr, status, error) => {
                console.error(error);
                if (this.isTD) {
                    pageHelper.showErrors(`Error rendering Terminal Disclaimer Diagram.`);

                } else {
                    pageHelper.showErrors(`Error rendering family tree, please report this as a bug.`);
                }
            },
            complete: () => {
                cpiLoadingSpinner.hide();
            }
        });
    }


    async fetchDisplayOptions() {
        cpiLoadingSpinner.show();
        return $.ajax({
            url: $("#d3-tree").data('settings-url'),
            type: "GET",
            dataType: "json",
            success: (response) => {
                const properties = response.properties;
                let checkboxHtml = "";

                $.each(properties, function (index, item) {
                    const checkedAttr = item.Include ? 'checked' : '';
                    checkboxHtml += `
                        <div class="col-md-4">
                            <input type="checkbox" class="k-checkbox" id="${item.PropertyName}" name="${item.PropertyName}" ${checkedAttr}>
                            <label class="k-checkbox-label" for="${item.PropertyName}">
                                ${item.Label}
                            </label>
                        </div>`;
                });

                const modal = $("#familyTreeDataSettingDialog");
                if (modal.length > 0) {
                    const settingsContainer = modal.find('.settings-fields');
                    settingsContainer.empty().append(checkboxHtml);
                    $("#tempSettingsContainer, #tempIsSettingDefault").remove();
                    modal.find('#DefaultSetting').prop('checked', response.isSettingDefault);
                } else {
                    let $tempSettingsContainer = $("#tempSettingsContainer");
                    if ($tempSettingsContainer.length > 0) {
                        $tempSettingsContainer.empty();
                        $tempSettingsContainer.append(checkboxHtml);
                    } else {
                        $tempSettingsContainer = $(`<div id="tempSettingsContainer" style="display:none;"></div>`).append(checkboxHtml);
                        $("body").append($tempSettingsContainer);
                    }
                    let $tempIsSettingDefault = $("#tempIsSettingDefault");
                    if ($tempIsSettingDefault.length > 0) {
                        $tempIsSettingDefault.val(response.isSettingDefault);
                    } else {
                        $tempIsSettingDefault = $(`<input type="hidden" id="tempIsSettingDefault" value="${response.isSettingDefault}" />`);
                        $("body").append($tempIsSettingDefault);
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                if (this.isTD) {
                    pageHelper.showErrors(`Error rendering Terminal Disclaimer Diagram.`);

                } else {
                    pageHelper.showErrors(`Error rendering family tree, please report this as a bug.`);
                }
            },
            complete: function () {
                cpiLoadingSpinner.hide();
            }
        });
    }
}


