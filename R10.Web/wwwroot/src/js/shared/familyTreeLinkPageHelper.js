export function createNodeHeaderText(headerText, maxWidth, delimiter = "/") {
    let tempContainer = d3.select("#familyTreeSvg").node();
    const svgNs = "http://www.w3.org/2000/svg";
    let output = [headerText, ''];

    let header = document.createElementNS(svgNs, "text");
    tempContainer.appendChild(header);
    header.textContent = output[0];

    while (header.getComputedTextLength() > maxWidth) {
        const parts = output[0].split(delimiter);
        const lastWord = parts.pop();
        output[1] = delimiter + lastWord + output[1];
        output[0] = parts.join(delimiter);

        header.textContent = output[0];
    }

    tempContainer.removeChild(header);

    return output;
}


export function createNodeDescriptions(node) {
    const descriptions = [];
    if (node.Type === "C" || node.Type === "T") {
        if (node.PatNumber) {
            descriptions.push(`Patent No.: ${node.PatNumber}`);
            descriptions.push(`Issue Date: ${formatDate(node.IssDate)}`);
        } else if (node.PubNumber) {
            descriptions.push(`Pub No.: ${node.PubNumber}`);
            descriptions.push(`Pub Date: ${formatDate(node.PubDate)}`);
        } else if (node.AppNumber) {
            descriptions.push(`App No.: ${node.AppNumber}`);
            descriptions.push(`File Date: ${formatDate(node.FilDate)}`);
        }
    }

    return descriptions;
}


export function attachLinks(shapeSvg, node, directParents, directChildren, rectWidth, yShift) {
    const iconSize = 20;
    const iconXStart = rectWidth / 2 - 2 * iconSize;

    let editNoteLink = shapeSvg.insert("a")
        .attr('href', '#')
        //todo
        .on("click", function () {

            const dialog = $("#RemarksDialog");
            if (dialog.length > 0 && node.Id === $('#familyTreeSelectedValue').val()) {
                dialog.modal("show");
            } else {
                d3.select(this).classed("disabled-link", true);
                $(".popup").empty();

                let data = {
                    Id: node.KeyId,
                    Type: node.Type,
                    ModalType: "Remarks",
                    RecordTitle: node.Text,
                };
                cpiLoadingSpinner.show();

                $.ajax({
                    url: $("#d3-tree").data('dialog-url'),
                    type: "POST", 
                    contentType: "application/json",
                    data: JSON.stringify(data),
                    success: function (response) {
                        $(".popup").html(response);
                        $("#RemarksDialog").modal("show");
                    },
                    error: function (xhr, status, error) {
                        console.error("AJAX Error:", status, error);
                        pageHelper.showErrors(xhr.responseText);
                    },
                    complete: () => {
                        cpiLoadingSpinner.hide();
                        d3.select(editNoteLink.node()).classed("disabled-link", false);
                    }
                });
            }
        });

    editNoteLink.insert("foreignObject")
        .attr("x", node.Type === "C" ? iconXStart : iconXStart + iconSize - 5)
        .attr("y", yShift)
        .attr("width", iconSize)
        .attr("height", iconSize)
        .html(`<i class="ForeignIconComment"></i>`);

    if (node.Type === "C") {
        let editRelationshipsLink = shapeSvg.insert("a")
            .attr('href', '#')
            .on("click", function () {
                const dialog = $("#RelationshipsDialog");
                if (dialog.length > 0 && node.Id === $('#familyTreeSelectedValue').val()) {
                    dialog.modal("show");
                } else {
                    d3.select(this).classed("disabled-link", true);

                    $(".popup").empty();

                    let data = {
                        Id: node.KeyId,
                        ModalType: "Relationships",
                        FamilyRef: node.Title,
                        Type: node.Type,
                        RecordTitle: node.Text,
                        DirectParents: directParents,
                        DirectChildren: directChildren,
                    };
                    cpiLoadingSpinner.show();

                    $.ajax({
                        url: $("#d3-tree").data('dialog-url'),
                        type: "POST",
                        contentType: "application/json",
                        data: JSON.stringify(data),
                        success: function (response) {
                            $(".popup").html(response);
                            $("#RelationshipsDialog").modal("show");
                        },
                        error: function (xhr, status, error) {
                            console.error("AJAX Error:", status, error);
                            pageHelper.showErrors(xhr.responseText);
                        },
                        complete: () => {
                            cpiLoadingSpinner.hide();
                            d3.select(editRelationshipsLink.node()).classed("disabled-link", false);
                        }
                    });
                }
            });
        editRelationshipsLink.insert("foreignObject")
            .attr("x", iconXStart + iconSize)
            .attr("y", yShift)
            .attr("width", iconSize)
            .attr("height", iconSize)
            .html(`<i class="fas fa-sitemap"></i>`);

    }

}


export function isTradeSecret(text) {
    if (/^█+$/.test(text.trim())) {
        return '█';
    } else {
        return text;
    }
}


export function formatList(list) {
    return list.map(item => {
        if (item === null || item === "") return "";
        const tryDate = new Date(item);

        if (!isNaN(tryDate.getTime())) {
            return tryDate.toISOString().split('T')[0];
        }
        else {
            return isTradeSecret(item);
        }
    }).join(" / ");
}

export function formatDate(date) {
    if (!date) return "";
    const d = new Date(date);
    const day = String(d.getDate()).padStart(2, '0');
    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    const month = monthNames[d.getMonth()];
    const year = d.getFullYear();
    return `${day}-${month}-${year}`;
}


export function createInfoHeader(node) {
    const span = document.createElement("span");
    span.className = "infoDetailsTitle";

    if (node.Type === "F") {
        span.textContent = "Family Number: " + node.Text;
        span.classList.add("font-weight-bold");
        return span;
    }

    let linkoutUrl;
    if (node.Type === "C") {
        linkoutUrl = `/Patent/CountryApplication/Detail/${node.KeyId}`;
    } else if (node.Type === "I") {
        linkoutUrl = `/Patent/Invention/Detail/${node.KeyId}`;
    } else if (node.Type === "T") {
        linkoutUrl = `/Trademark/TmkTrademark/Detail/${node.KeyId}`;
    }

    const link = document.createElement("a");
    link.href = linkoutUrl;
    link.setAttribute("data-toggle", "tooltip");
    link.title = "Open in new tab";
    link.target = "_blank";
    link.textContent = `${node.Text} `;

    const linkIcon = document.createElement("span");
    linkIcon.className = "fal fa-external-link";
    link.appendChild(linkIcon);

    span.appendChild(link);
    span.appendChild(document.createElement("br"));

    return span;
}


export function createInfo(heading, content) {
    const span = document.createElement("span");
    span.setAttribute("class", "infoDetailsContent");
    const header = document.createElement("b");
    header.appendChild(document.createTextNode(" " + heading + ": "));
    span.appendChild(header);

    if (content instanceof Element) {
        span.appendChild(content);
    } else if (typeof content === "boolean") {
        span.appendChild(document.createTextNode(content ? "Yes" : "No"));
    } else if (typeof content === "number" && Number.isInteger(content)) {
        span.appendChild(document.createTextNode(content + " day(s)"));
    } else {
        var tryDate = new Date(content);
        if (!isNaN(tryDate.getTime())) {
            span.appendChild(document.createTextNode(formatDate(tryDate)));
        }
        else {
            span.appendChild(document.createTextNode(isTradeSecret(content)));
        }
    }

    return span;
}



export function createEdgeLabel(labelText) {
    const svgNs = "http://www.w3.org/2000/svg";
    const xmlNs = "http://www.w3.org/XML/1998/namespace";
    const text = document.createElementNS(svgNs, "text");
    const tspan = document.createElementNS(svgNs, "tspan");
    tspan.setAttributeNS(xmlNs, "xml:space", "preserve");
    tspan.setAttribute("font-size", "12");
    tspan.setAttribute("font-weight", "bold");
    tspan.setAttribute("font-family", "Arial, sans-serif");
    tspan.setAttribute("dy", "1em");
    tspan.setAttribute("x", "1");
    tspan.textContent = labelText;
    text.appendChild(tspan);
    return text;
}



export function loadImage(src) {
    return new Promise((resolve, reject) => {
        const img = new Image();

        img.onload = () => {
            resolve({
                width: img.width,
                height: img.height
            });
        };

        img.onerror = reject;
        img.src = src;
    });
}