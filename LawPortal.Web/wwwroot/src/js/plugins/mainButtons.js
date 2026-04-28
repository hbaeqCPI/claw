
//manage main buttons (add, delete, search, print, etc..)
(function ($) {
    if (typeof $.fn.cpiMainButtons === "undefined") {
        $.fn.cpiMainButtons = function(options) {
            const plugin = this;

            const defaults = {
                onRefresh: function() {},
                onRefreshOptions: {}
            };
            const settings = $.extend({}, defaults, options);

            //new layout: include page-navs
            plugin.on("click",
                "button, .page-nav",
                function() {
                    const link = $(this);
                    let url = link.data("url");

                    //new layout: hide status message
                    cpiStatusMessage.hide();
                    if (url) {
                        cpiLoadingSpinner.show();
                        $.get(url).done(function (result) {
                            cpiLoadingSpinner.hide();
                            const parentContainer = link.data("container");

                            //if response should be placed in a popup container
                            const isPopup = link.data("popup");
                            if (isPopup !== undefined && isPopup) {
                                const loading = link.data("loading");
                                if (!loading) {
                                    link.data("loading", "true");

                                    //use one container to avoid duplicate element ids
                                    const popupContainer = $(".site-content .popup");
                                    popupContainer.empty();
                                    popupContainer.html(result);
                                    setTimeout(() => { link.removeData("loading"); }, 1000);
                                }

                            }
                            else {
                                //if container has been defined, empty content and add the response
                                //otherwise, just append the response to the DOM
                                if (parentContainer !== undefined && parentContainer !== null && parentContainer.length > 0) {
                                    const dataContainer = $('#' + parentContainer).find(".cpiDataContainer");

                                    if (link.attr("id") && link.attr("id").endsWith("-link")) {
                                        window.open(result, '_blank');
                                    }
                                    else {
                                        if (dataContainer.length > 0) {
                                            dataContainer.empty();
                                            dataContainer.html(result);
                                        }
                                    }
                                }
                                else
                                    pageHelper.appendPage(result);
                            }
                        }).fail(function (e) {
                            cpiLoadingSpinner.hide();
                            if (e.status == 401 || e.responseText.toLowerCase().includes("access token is empty")) {
                                const baseUrl = $("body").data("base-url");
                                const url = `${baseUrl}/Graph/SharePoint`;

                                sharePointGraphHelper.getGraphToken(url, () => {
                                    const retryMsg = "Please try it again after the SharePoint authentication.";
                                    pageHelper.showErrors(retryMsg);
                                });
                            }
                            else
                               pageHelper.showErrors(e);
                        });
                        
                    } else if (link.hasClass("refresh-record")) { //record refresh
                        settings.onRefresh(settings.onRefreshOptions);
                    }
                });
            return this;
        };
    }
})(jQuery);
