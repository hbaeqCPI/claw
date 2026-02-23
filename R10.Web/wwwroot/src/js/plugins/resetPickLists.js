(function ($) {
    if (typeof $.fn.resetPickLists === "undefined") {
        $.fn.resetPickLists = function (clearValue = false) {
            this.find("input[data-role='combobox'], input[data-role='dropdownlist'], select[data-role='multiselect']").each(function () {
                const el = $(this);
                el.data("fetched", 0);

                if (clearValue) {
                    switch (el.data("role")) {
                    case "combobox":
                        el.data("kendoComboBox").text(""); //.value("") will refresh datasource!
                        break;
                    case "dropdownlist":
                        var widget = el.data("kendoDropDownList");
                        widget.value(widget.dataItem(0).Value);
                        break;
                    case "multiselect":
                        el.data("kendoMultiSelect").value([]);
                        el.resetFloatLabel();
                        break;
                    }
                }
            });
            
        };
    }
}(jQuery));

