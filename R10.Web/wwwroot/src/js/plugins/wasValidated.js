(function ($) {
    window.addEventListener('load', function () {
        const forms = document.getElementsByClassName('needs-validation');
        const validation = Array.prototype.filter.call(forms, function (form) {
            form.addEventListener('submit', function (event) {
                if (form.checkValidity() === false) {
                    $(form).wasValidated();

                    event.preventDefault();
                    event.stopPropagation();
                }
            }, false);
        });
    }, false);

    if (typeof $.fn.wasValidated === "undefined") {
        $.fn.wasValidated = function () {
            if ($(this).is("form")) {
                const form = $(this);

                form.addClass('was-validated');
                form.find(".form-control").removeClass("cpi-is-valid");
                form.find(".valid").addClass("cpi-is-valid");

                //kendo combobox & dropdown
                //todo: test dropdown
                form.find(".k-dropdown-wrap, .k-picker-wrap").addClass("k-is-valid");
                form.find(".k-dropdown-wrap, .k-picker-wrap").removeClass("k-is-invalid");
                form.find(".k-combobox .input-validation-error, .k-dropdown .input-validation-error").each(function () {
                    const input = $(this);
                    const wrap = input.prev(".k-dropdown-wrap");

                    wrap.removeClass("k-is-valid");

                    //switch position so we can use input css selector
                    //instead of adding k-is-invalid on wrap
                    $(input).insertBefore($(wrap));
                });

                //kendo datepicker
                form.find(".k-datepicker .input-validation-error").each(function () {
                    const input = $(this);
                    const wrap = input.parent(".k-picker-wrap");

                    wrap.removeClass("k-is-valid");
                    wrap.addClass("k-is-invalid");

                    //input is inside wrap. css has no parent selector :(
                    //use on blur to check validation and assign appropriate class
                    input.blur(function () {
                        if ($(this).hasClass("valid")) {
                            wrap.removeClass("k-is-invalid");
                            wrap.addClass("k-is-valid");
                        }
                        else {
                            wrap.removeClass("k-is-valid");
                            wrap.addClass("k-is-invalid");
                        }
                    });
                });

                form.validate().errorList[0].element.focus();
            }

            return this;
        };
    }
}(jQuery));

