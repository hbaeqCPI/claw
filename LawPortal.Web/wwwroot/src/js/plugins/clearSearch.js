//clear search controls
(function ($) {
    if (typeof $.fn.clearSearch === "undefined") {
        $.fn.clearSearch = function () {
            const form = this;

            var reportFormatDropdown = $("#ReportFormat").data("kendoDropDownList");
            if (reportFormatDropdown) {
                reportFormatDropdown.value(null);
            }

            form.find("input,textarea").each(function () {
                const el = $(this);

                //if (el.attr("name") !== page.verificationTokenFormData) {
                //do not clear hidden fields
                //removed && !el.is(":hidden") - this caused other non-active tabs to be not cleared
                //if clear is only for the active tab, then there's also a bug since all combobox are cleared
                if (el.attr('type') !== 'hidden') {
                    const type = el.attr("type") || "";

                    if (type === "radio") {
                        el.prop("checked", false);
                        const label = el.parent("label");
                        if (label) {
                            if (el.val() === "")
                                label.addClass("active");
                            else
                                label.removeClass("active");
                        }
                    }
                    else if (type === "checkbox")
                        el.prop("checked", false);

                    else {
                        let element = $("#" + el.attr("id"));
                        if (element.data("role") === "combobox") {
                            if (el.val().length > 0) {
                                element.data("kendoComboBox").value("");
                            }
                        }
                        else {
                            el.val("");
                        }
                    }
                        
                        
                }
                else {
                    if (el.data("role") === "numerictextbox") {
                        el.data("kendoNumericTextBox").value(null);
                    }
                }

                el.resetFloatLabel();
            });

            form.resetPickLists(true);
        };
    }
})(jQuery);
