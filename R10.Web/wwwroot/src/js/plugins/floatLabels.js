//floating form labels
(function ($) {
    if (typeof $.fn.floatLabels === "undefined") {
        $.fn.floatLabels = function () {
            $(this).find(".float-label").each(function () {
                const container = $(this);
                
                const input = container.find('input, textarea, select');
                input.resetFloatLabel();

                var activate = function (el) {
                    el.closest(".float-label").addClass("active").removeClass("inactive");

                    if (el.attr("data-placeholder") !== null)
                        el.attr("placeholder", el.attr("data-placeholder"));

                    if (!el.is(":focus"))
                        el.resetFloatLabel();
                };

                input.on("focus input change", function (e) {
                    activate($(this));
                });
                

                //workaround for clear button (x) on kendo combobox
                //which fires combobox change but not input change
                //and does not set focus on combobox
                container.find("input[data-role='combobox']").each(function () {
                    const comboBox = $(this).data("kendoComboBox");
                    if (comboBox) {
                        comboBox.bind("change", function () {
                            //TODO: fix issue --> have to click "x" twice if combo has default value (detail screen)
                            const el = this.input;
                            if (this.text() === "" && !el.is(":focus")) {
                                el.focus();
                            }
                            else
                                activate(el);
                        });
                    }
                });

                //workaround for clear button (x) on kendo multiselect
                //which fires multiselect change but not input change
                container.find("select[data-role='multiselect']").each(function () {
                    $(this).data("kendoMultiSelect").bind("change", function () {
                        this.element.resetFloatLabel();
                    });
                });

                input.on("blur", function () {
                    $(this).resetFloatLabel();
                });

                if ($(input).attr("autofocus") !== undefined)
                    $(input).focus();
            });

            return this;
        };
    }
})(jQuery);