(function ($) {
    if (typeof $.fn.disableControls === "undefined") {
        $.fn.disableControls = function(disable) {
            const container = $(this);

            container.find(".cpiMainEntry input, .cpiMainEntry textarea, .cpiMainEntry input[type='checkbox']").each(function () {
                if (disable && !$(this).prop('disabled')) {
                    $(this).addClass("disabled");
                    $(this).attr("disabled", true);
                }

                if (!disable && $(this).hasClass("disabled")) {
                    $(this).removeClass("disabled");
                    $(this).attr("disabled", false);
                }
            });

            container.find(".cpiMainEntry .k-combobox > input").each(function() {
                const comboBox = $(this).data("kendoComboBox");
                if (comboBox) {
                    comboBox.enable(!disable);
                }
            });

            container.find(".cpiMainEntry .k-dropdownlist > input").each(function() {
                const dropdownList = $(this).data("kendoDropDownList");
                if (dropdownList) {
                    dropdownList.enable(!disable);
                }
            });

            container.find(".cpiMainEntry .k-datepicker input").each(function() {
                const datePicker = $(this).data("kendoDatePicker");
                if (datePicker) {
                    datePicker.enable(!disable);
                }
            });

            container.find(".cpiMainEntry .k-numerictextbox input").each(function() {
                const numericTextBox = $(this).data("kendoNumericTextBox");
                if (numericTextBox) {
                    numericTextBox.enable(!disable);
                }
            });
        };
    }
})(jQuery);
