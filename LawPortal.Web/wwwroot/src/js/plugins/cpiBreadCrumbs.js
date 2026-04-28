
(function ($) {
    
    if (typeof $.fn.cpiBreadCrumbs === "undefined") {
        $.fn.cpiBreadCrumbs = function () {
            const plugin = this;
            const breadCrumbs = [];
            const maxCountAllowed = 30;
       
            const removeContainer = function (pos) {
                const container = $("#" + breadCrumbs[pos].name);
                kendo.destroy(container); //removes auto-generated HTML content, which is outside the widget
                container.remove();
            };

            const removeContainers = function (startPos) {
                for (let i = 0; i < breadCrumbs.length; i++) {
                    if (i >= startPos) {
                        removeContainer(i);
                    }
                }
            };

            //bootstrap css based
            const refreshBreadCrumbs = function () {
                //pageHelper.hideErrors();

                if (breadCrumbs.length >= 1) {
                    let crumbsNodes = "";
                    let node = "";

                    for (let i = 0; i < breadCrumbs.length; i++) {
                        const classNames = breadCrumbs[i].classNames || "";

                        if (breadCrumbs.length - 1 === i) {
                            node = `<li class="breadcrumb-item active ${classNames}" aria-current="page">${breadCrumbs[i].label}</a></li>`;
                        }
                        else {
                            node = `<li class="breadcrumb-item ${classNames}"><a data-url="${breadCrumbs[i].url}" href="#${breadCrumbs[i].name}">${breadCrumbs[i].label}</a></li>`;
                        }
                        crumbsNodes = crumbsNodes + node;
                    }

                    plugin.empty().append(`<ol class='breadcrumb bg-transparent'>${crumbsNodes}</ol>`);
                   
                }
                else {
                    plugin.empty();
                }
            };

            //use pageHelper.moveBreadcrumbs
            //const moveBreadCrumbs = function (container) {
            //    //MOVE STATUS MESSAGE
            //    const pageStatus = $(`${container} .page-message`);
            //    if (pageStatus.length > 0)
            //        $("#page .page-status").appendTo(pageStatus);

            //    //MOVE BREAD CRUMBS
            //    const pageCrumbs = $(`${container} .page-crumbs`);
            //    if (pageCrumbs.length > 0)
            //        $(`#page .container-crumbs`).appendTo(pageCrumbs);
            //};

            const showNode = function (pos) {
                if (pos > -1) {
                    const breadCrumb = breadCrumbs[pos];
                    const name = breadCrumb.name;

                    //new layout: run refresh before removeContainers
                    if (breadCrumb.refresh && breadCrumb.refreshHandler !== undefined) {
                        //refresh only when screen is not in dirty mode
                        if (!breadCrumb.isDirty) {
                            breadCrumb.refreshHandler(breadCrumb);
                        }
                        
                    }

                    //new layout: move breadcrumbs
                    pageHelper.moveBreadcrumbs(`#${breadCrumb.name}`);
                    removeContainers(pos + 1);

                    breadCrumbs.splice(pos + 1, breadCrumbs.length - pos - 1); //delete elements after
                    $("#" + name).show(); //show the container
                    refreshBreadCrumbs();

                    if (breadCrumb.updateHistory) {
                        window.history.replaceState("", "", breadCrumb.url);
                    }
                  
                }
            };

            const showNodeByName = function (name) {
                const pos = breadCrumbs.findIndex(function (element) {
                    return element.name === name.substr(1);
                });

                //todo: move to showNode() ??
                //const dirtyPagePrompt = plugin.data("dirty-page-msg");

                //check for dirty forms, prompt before leaving
                for (let i = breadCrumbs.length - 1; i > pos; i--) {
                    if (breadCrumbs[i].isDirty) {
                        cpiConfirm.confirm(plugin.data("dirty-page-title"), plugin.data("dirty-page-msg"), function () {
                            showNode(pos);
                        });

                        return;
                    }
                }
                showNode(pos);

            };

            plugin.deleteNodeByUrl = function (url) {
                const pos = plugin.getNodePosition(url);
                if (pos > -1) {
                    removeContainer(pos);
                    breadCrumbs.splice(pos, 1); 
                }
            };

            plugin.addNode = function (node) {
                const pos = breadCrumbs.findIndex(function (element) {
                    return element.name === node.name;
                });

                //remove if existing
                if (pos > -1) {
                    removeContainer(pos);
                    breadCrumbs.splice(pos, 1); //remove the existing element
                }

                if (breadCrumbs.length >= maxCountAllowed) {
                    breadCrumbs.shift(); //remove 1st item
                }
                node.url = node.url.toLowerCase();
                breadCrumbs.push(node);

                refreshBreadCrumbs();
            };

            plugin.hideContainers = function () {
                breadCrumbs.forEach(function (item) {
                    $("#" + item.name).hide();
                });
            };

            plugin.hasNodeByUrl = function (url) {
                const pos = breadCrumbs.findIndex(function (element) {
                    return element.url.toLowerCase() === url.toLowerCase();
                });
                return pos > -1;
            };

            plugin.getNodePosition = function (url) {
                const pos = breadCrumbs.findIndex(function (element) {
                    return element.url.toLowerCase().startsWith(url.toLowerCase());
                });
                return pos;
            };

            plugin.showNodeByUrl = function (url) {
                const pos = breadCrumbs.findIndex(function (element) {
                    return element.url.toLowerCase() === url.toLowerCase();
                });

                //todo: move to showNode() ??
                //check for dirty forms, prompt before leaving
                if (plugin.hasDirtyNode()) {
                    cpiConfirm.confirm(plugin.data("dirty-page-title"), plugin.data("dirty-page-msg"), function () {
                        showNode(pos);
                    });
                }
                else
                    showNode(pos);
            };

            plugin.showPreviousNode = function () {
                const pos = breadCrumbs.length - 2;
                if (pos > -1) {
                    showNode(pos);
                }
            };

            plugin.markLastNode = function (options) {
                const pos = breadCrumbs.length - 1; //always the last
                if (pos > -1) {
                    breadCrumbs[pos].isDirty = options.dirty;
                }
            };

            plugin.deleteLastNode = function () {
                const pos = breadCrumbs.length - 1; //always the last
                if (pos > -1) {
                    //move status message before removing container
                    $("#page .page-status").appendTo($(".site-content"));

                    //move breadcrumbs before removing container
                    $("#page .container-crumbs").appendTo($(".site-content"));

                    removeContainers(pos);
                    breadCrumbs.splice(pos, breadCrumbs.length - pos); //delete elements 
                }
            };

            plugin.hasDirtyNode = function () {
                const posDirty = breadCrumbs.findIndex(function (element) {
                    return element.isDirty === true;
                });
                return posDirty > -1;
            };

            plugin.showNodeByPosition = function (pos) {
                if (breadCrumbs.length >= pos) {
                    showNode(pos);
                }
            };

            plugin.updateNodeInfo = function (node) {
                const pos = breadCrumbs.findIndex(function (element) {
                    return element.name === node.name;
                });
                if (pos > -1) {
                    breadCrumbs[pos].url = node.url;
                }
            };

            plugin.updateLastNode = function (node) {
                const pos = breadCrumbs.length - 1;

                if (pos > -1) {
                    breadCrumbs[pos] = node;
                    refreshBreadCrumbs();
                }
            };

            plugin.on("click", "a", function (e) {
                e.preventDefault();
                const link = $(this);
                const node = link.attr("href");
                const url = link.data("url");
                showNodeByName(node);
            });

            plugin.getTitle = function () {
                const pos = breadCrumbs.length - 1;
                if (pos > -1) {
                    return breadCrumbs[breadCrumbs.length - 1].label;
                }
                return "";
            };

            plugin.getNodes = function () {
                return breadCrumbs;
            };

            //plugin.appendNodes = function (nodes) {
            //    nodes.forEach((item) => {
            //        breadCrumbs.push(item);
            //    });
            //    refreshBreadCrumbs();
            //};

            return plugin;


        };
    }
}(jQuery));

