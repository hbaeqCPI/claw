//Move tab-panes back to tab-content
(function ($) {
    if (typeof $.fn.resetTabContent === "undefined") {
        $.fn.resetTabContent = function (tabContent) {
            if ($(tabContent).find(".tab-pane").length === 0) {
                const tabs = $(this).find(".nav-item .nav-link");
                $(tabs).each(function () {
                    const tab = $(this);
                    const tabPane = $(tab).attr('href');

                    $(tabPane).removeClass("active");
                    if ($(tab).hasClass("active"))
                        $(tabPane).addClass("active");

                    $(tabPane).removeClass("form-sidebar");
                    $(tabPane).appendTo($(tabContent));
                });
            }

            return this;
        };
    }
}(jQuery));

(function ($) {
    if (typeof $.fn.resetFloatLabel === "undefined") {
        $.fn.resetFloatLabel = function() {

            let el = $(this);

            //multiselect has 2 input elements 
            if (el.closest("span.k-multiselect").length > 0)
                el = el.filter(":not(.k-input-inner)");

            const container = el.closest(".float-label");

            if (container.length === 0)
                return this;

            var isEmpty = !el.val() || el.val().length === 0;

            // Kendo NumericTextBox: display input is empty on blur but widget still has value
            if (isEmpty) {
                var numericInput = el.closest('.k-numeric-wrap').siblings('input[data-role="numerictextbox"]');
                if (numericInput.length === 0)
                    numericInput = container.find('input[data-role="numerictextbox"]');
                if (numericInput.length > 0) {
                    var ntb = numericInput.data('kendoNumericTextBox');
                    if (ntb && ntb.value() !== null && ntb.value() !== undefined) {
                        isEmpty = false;
                    }
                }
            }

            if (isEmpty) {
                container.removeClass('active').addClass('inactive');
            }
            else {
                container.removeClass('inactive').addClass('active');
            }
            el.removeAttr("placeholder");
            return this;
        };
    }
})(jQuery);

//reset comboboxes
(function ($) {
    if (typeof $.fn.resetComboBoxes === "undefined") {
        $.fn.resetComboBoxes = function() {
            const form = this;
            form.find("input[data-role='combobox']").each(function() {
                const el = $(this);
                //todo: clear value
                if (el.data("fetched") === 1) {
                    el.data("fetched", 0); //flag as not fetched so we can requery the latest data
                }
            });
        };
    }
})(jQuery);
