import { searchFormSubmit, focusLabelControl, clearInvalidKendoDate } from "../pageHelper";

(function ($) {
    if (typeof $.fn.cpiSearchForm === "undefined") {
        $.fn.cpiSearchForm = function (options) {
            const plugin = this;

            const defaults = {onSubmit: searchFormSubmit};
            const settings = $.extend({}, defaults, options);

            plugin.on("submit", function (e) {
                e.preventDefault();

                const form = plugin;
                const formInfo = {
                    url: form.attr("action"),
                    form: form
                };
                plugin.data("cpiSearchForm", formInfo);
                settings.onSubmit(formInfo);
            });

            plugin.on("click", ".search-submit", function (e) {
                plugin.trigger("submit");
            });

            focusLabelControl(plugin);
            clearInvalidKendoDate(plugin);
            return plugin;
        };
    }

}
)(jQuery);