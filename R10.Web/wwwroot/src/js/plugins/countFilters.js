//count search filters
(function ($) {
    if (typeof $.fn.countFilters === "undefined") {
        $.fn.countFilters = function () {
            const filters = this;
            const countKendo = $(filters).find("input.k-input:not(.k-formatted-value),select[data-role='multiselect'],input[data-role='dropdownlist'],input.k-input-inner:not(.k-formatted-value)").filter(function () {
                const equalityOperator = $(this).attr("data-role") === "dropdownlist" && $(this).attr("name").endsWith("Op");
                return this.value !== "" && !equalityOperator;
            }).length;

            const countBootstrap = $(filters).find("input.form-control,input.toggle-option,input.form-check-input,textarea.form-control").filter(function () {
                const type = $(this).attr("type") || "";
                if (type === "radio")
                    return $(this).prop("checked") && this.value !== "";
                if (type === "checkbox")
                    return $(this).prop("checked");
                else
                    return this.value !== "";
            }).length;

            return countKendo + countBootstrap;
        };
    }
})(jQuery);
