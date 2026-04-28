//Add input event handlers that trigger incremental search
(function ($) {
    if (typeof $.fn.liveSearch === "undefined") {
        $.fn.liveSearch = function(callback) {
            const filterContainer = $(this);
            if ($(filterContainer).length === 0) {
                console.error("liveSearch error: Filter Container element not found.");
                return this;
            }

            let delayTimer;
            $(filterContainer).find("input.form-control, textarea.form-control").on("input",
                function () {
                    const el = this;
                    clearTimeout(delayTimer);
                    delayTimer = setTimeout(function() {
                            callback(el);
                        },
                        500);

                });

            $(filterContainer).find("input:radio").on("change",
                function() {
                    callback(this);
                });
            
            $(filterContainer).find("input:checkbox").on("change",
                function() {
                    callback(this);
                });

            $(filterContainer).find("input[data-role='combobox']").each(function() {
                const el = this;
                const comboBox = $(el).data("kendoComboBox");

                comboBox.bind("change", function() {
                    callback(el);
                });

                $(el).parent().find("input.k-input-inner").on("focusout", function () {
                    onComboFocusOut(this, comboBox);
                })
            });

            $(filterContainer).find("input[data-role='multicolumncombobox']").each(function () {
                const el = this;
                const comboBox = $(el).data("kendoMultiColumnComboBox");

                comboBox.bind("change", function () {
                    callback(el);
                });

                $(el).parent().find("input.k-input-inner").on("focusout", function () {
                    onComboFocusOut(this, comboBox);
                })
            });

            //allow tab key from combobox to trigger search
            const onComboFocusOut = function (el, comboBox) {
                const text = $(el).val();
                const value = comboBox.value();

                if (text && !value) {
                    comboBox.value(text);
                    comboBox.trigger("change");
                }
            };

            $(filterContainer).find("input[data-role='datepicker']").each(function() {
                const el = this;
                
                $(this).data("kendoDatePicker").bind("change",
                    function() {
                        callback(el);
                    });
            });

            $(filterContainer).find("input[data-role='dropdownlist']").each(function () {
                const el = this;
            
                $(this).data("kendoDropDownList").bind("change", function () {
                    callback(el);
                });
            });

            $(filterContainer).find("input[data-role='numerictextbox']").each(function () {
                const el = this;

                $(this).data("kendoNumericTextBox").bind("change", function () {
                    callback(el);
                });
            });

            $(filterContainer).find("select[data-role='multiselect']").each(function () {
                const el = this;
                
                $(this).data("kendoMultiSelect").bind("change", function () {
                    //TODO: Change doesn't fire when using main clear "x" button then selecting same value
                    callback(el);
                });
            });

            return this;
        };
    }
}(jQuery));
