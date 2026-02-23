import ActivePage from "../activePage";

export default class TaskSchedulerPage extends ActivePage {
    constructor() {
        super();
    }

    initializeDetailContentPage = (detailContentPage) => {
        const detailContentContainer = $(`#${this.detailContentContainer}`);

        //override before submit callback
        detailContentPage.editModeOptions.beforeSubmit = (onSubmit) => {
            //validate password
            const username = detailContentContainer.find("#UserName");
            const pwd = detailContentContainer.find("#Password");

            if (username.val() && !pwd.val()) {
                pwd.addClass("input-validation-error");
                pwd.trigger("focus");
            }

            onSubmit();
        };

        super.initializeDetailContentPage(detailContentPage);

        const freqEl = $(`#${this.mainDetailContainer}_Frequency`);
        if (freqEl.length > 0) {
            const freq = freqEl.data("kendoDropDownList").value();
            this.setRecurFactorLabel(freq);
            this.setDayOption(freq);
        }

        const pwd = detailContentContainer.find(".pwd");
        this.togglePassword(pwd);

        detailContentContainer.find(".btn-toggle-pwd").on("click", (e) => {
            this.togglePassword($(e.currentTarget));
        });

        detailContentContainer.find(".run-task").on("click", (e) => {
            this.runTask($(e.currentTarget));
        });


        const useServiceAccount = detailContentContainer.find("#UseServiceAccount");
        const authentication = detailContentContainer.find(".authentication")
        this.toggleAuthentication(useServiceAccount, authentication);

        useServiceAccount.on("click", (e) => {
            this.toggleAuthentication(useServiceAccount, authentication);
        });
    }

    toggleAuthentication(useServiceAccount, authentication) {
        if (useServiceAccount.is(":checked"))
            authentication.hide();
        else
            authentication.show();
    }

    runTask(el) {
        const id = el.data("id");
        const url = el.data("url");
        const form = $(`#${this.mainDetailContainer}`).find("form");

        if (url) {
            cpiConfirm.confirm(el.data("title"), el.data("message"), () => {
                cpiLoadingSpinner.show();
                $.post(url, { id: id, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                    .done(function (response) {
                        cpiLoadingSpinner.hide();
                        form.find("a.refresh-record").trigger("click");
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        cpiAlert.warning(pageHelper.getErrorMessage(error), () => {
                            form.find("a.refresh-record").trigger("click");
                        }, el.data("title"));
                    });
            });
        }
    }

    togglePassword(el) {
        const form = el.closest("form");
        const pwd = form.find(".pwd");
        const toggleIcon = form.find(".toggle-pwd");

        if (pwd.attr('type') === "password") {
            pwd.attr('type', 'text');
            toggleIcon.removeClass("fa-eye-slash")
            toggleIcon.addClass("fa-eye")
        } else {
            pwd.attr('type', 'password');
            toggleIcon.addClass("fa-eye-slash")
            toggleIcon.removeClass("fa-eye")
        }
    }

    onFrequencyChange = (e) => {
        const freq = e.sender.value();
        this.setRecurFactorLabel(freq, true);
        this.setDayOption(freq);
    }

    setRecurFactorLabel = (freq, resetValue = false) => {
        const recurFactor = $(`.task-scheduler #${this.mainDetailContainer}_RecurFactor`).data("kendoNumericTextBox");
        const recurFactorlabels = [ "", "minutes", "days", "weeks", "months" ];
        
        if (freq == 0) {
            recurFactor.readonly();
            recurFactor.setOptions({format: `#`});
            recurFactor.value(null);
        }
        else {
            recurFactor.enable();
            recurFactor.setOptions({format: `# (${recurFactorlabels[freq]})`});
            recurFactor.value(resetValue ? 1 : recurFactor.value());
        }

        recurFactor.trigger('change');

        if (resetValue)
            recurFactor.focus();
    }

    setDayOption = (freq) => {
        const dayOfWeek = $(".task-scheduler .day-of-week");
        const dayOfMonth = $(".task-scheduler .day-of-month");

        dayOfWeek.hide();
        dayOfMonth.hide();

        if (freq == 3)
            dayOfWeek.show();
        else if (freq == 4)
            dayOfMonth.show();
    }
}