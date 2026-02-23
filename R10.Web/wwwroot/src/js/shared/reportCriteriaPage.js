import ActivePage from "../activePage";

export default class ReportCriteriaPage extends ActivePage {

    constructor() {
        super();
        this.image = window.image;
    }

    //init() {
        //this.tabsLoaded = [];
        //this.tabChangeSetListener();
        //actionAddSetUp();

        //if (addMode) {
        //    const scheduleName = this.getKendoComboBox("ScheduleName");
        //    scheduleName.focus();

        //}
        //this.initialize();
    //}

    customReportSetListener = () => {
        $(document).ready(() => {

            const scheduleActionGrid = $("#customReportGrid");
            scheduleActionGrid.find(".k-grid-toolbar").on("click",
                ".k-grid-AddCustomReport",
                () => {
                    const parent = $("#customReportGrid").parent();
                    const url = parent.data("url-add");
                    const grid = scheduleActionGrid.data("kendoGrid");
                    this.openCustomReportUpload(grid, url, true);

                });
        });
    }

    openCustomReportUpload(grid, url, closeOnSave) {
        const self = this;
        $.ajax({
            url: url,
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#customReportUploadDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: closeOnSave,
                        beforeSubmit: function () {
                            cpiLoadingSpinner.show();
                        },
                        afterSubmit: function () {
                            //combo box datasource
                            grid.dataSource.read();
                            dialogContainer.modal("hide");
                            cpiLoadingSpinner.hide();
                        }
                    }
                );
            },
            error: function (error) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(error.Name);
            }
        });
    }

    initialize(formName) {
        $(`${formName}`).floatLabels();
        const form = $(formName);
        form.find(".print-report").on("click", function (e) { printReport(form) })
        this.customReportSetListener();

        $("#ReportCriteriaForm").on("click", ".k-grid-Add", function (e) {
            const a = document.createElement("a");
            a.href = e.currentTarget.getAttribute("href");
            a.hidden = "hidden";
            document.body.appendChild(a);
            
            a.click();
            //e.preventDefault();
            //const link = $(this);

            //if (link.attr("target") === "_blank") {
            //    window.open(link.attr("href"), "_blank");
            //}
            //else
            //    pageHelper.openDetailsLink(link);
        });
    }

    ReportFormatChange() {
        if (document.getElementById("ReportFormat") != null) {
            var reportFormat = document.getElementById("ReportFormat").value;
            document.querySelectorAll('.email-report').forEach(element => {
                if (reportFormat == 0 || reportFormat == 1 || reportFormat == 2) {
                    element.removeAttribute("hidden");
                } else {
                    element.setAttribute("hidden", "hidden");
                }
            });
        }
    }

    submitForm(form, name, pageName) {
        if (this.CheckInvalidKendoDate(form)) {
            cpiLoadingSpinner.hide();
            return;
        }
        const url = form.data("url")
        const json = pageHelper.formDataToJson(form);

        //use fetch to get report and open new tab to display it
        //displaying in new tab does not work when using $.ajax
        //with fetch, get response as blob --> response.blob()
        //then turn blob to object url --> URL.createObjectURL(blobData)
        if (document.getElementById("ReportFormat").value != 99) {
            fetch(url, {
                method: "POST",
                headers: {
                    Accept: "arraybuffer",
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(json.payLoad)
            })
                //if response ok, get response as blob
                //if not ok, throw response as error
                .then(response => {
                    if (!response.ok)
                        throw response;

                    return response.blob();
                })
                .then(data => {
                    cpiLoadingSpinner.hide();

                    if (document.getElementById("ReportFormat").value == 4 || document.getElementById("ReportFormat").value == 99) {
                        const a = document.createElement("a");
                        document.body.appendChild(a);
                        const blobUrl = window.URL.createObjectURL(data);
                        a.href = blobUrl;
                        a.target = "_blank"
                        a.click();
                        setTimeout(() => {
                            window.URL.revokeObjectURL(blobUrl);
                            document.body.removeChild(a);
                        }, 0);
                    } else {
                        const a = document.createElement("a");
                        document.body.appendChild(a);
                        const blobUrl = window.URL.createObjectURL(data);
                        a.href = blobUrl;
                        var downloadName = this.htmlDecode(name);
                        a.download = downloadName;
                        a.click();
                        setTimeout(() => {
                            window.URL.revokeObjectURL(blobUrl);
                            document.body.removeChild(a);
                        }, 0);
                    }

                })
                .catch(error => {
                    cpiLoadingSpinner.hide();
                    error.text().then(errorMessage => {
                        if (errorMessage === "Email was successfully sent.") {
                            pageHelper.showSuccess(errorMessage)
                        }
                        pageHelper.showErrors(errorMessage);
                    })
                });
        } else {
            if (this.CheckInvalidKendoDate(form)) {
                cpiLoadingSpinner.hide();
                return;
            }
            const json = pageHelper.formDataToJson(form);
            for (var i in json.payLoad) {
                if (form[0].elements[i].type == "text")
                    form[0].elements[i].value = json.payLoad[i];
            }
            form.submit();
            cpiLoadingSpinner.hide();
        }
    }

    htmlDecode(input) {
    let doc = new DOMParser().parseFromString(input, "text/html");
    return doc.documentElement.textContent;
    }

    submitFormWithToken(form, name) {
        if (this.CheckInvalidKendoDate(form)) {
            cpiLoadingSpinner.hide();
            return;
        }

        const url = form.data("url")
        const json = pageHelper.formDataToJson(form);

        cpiLoadingSpinner.show();
        //use fetch to get report and open new tab to display it
        //displaying in new tab does not work when using $.ajax
        //with fetch, get response as blob --> response.blob()
        //then turn blob to object url --> URL.createObjectURL(blobData)
        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
                "RequestVerificationToken": json.verificationToken
            },
            body: JSON.stringify(json.payLoad)
        })
            //if response ok, get response as blob
            //if not ok, throw response as error
            .then(response => {
                if (!response.ok)
                    throw response;

                return response.blob();
            })
            .then(data => {
                cpiLoadingSpinner.hide();

                if (document.getElementById("ReportFormat").value == 4) {
                    const a = document.createElement("a");
                    document.body.appendChild(a);
                    const blobUrl = window.URL.createObjectURL(data);
                    a.href = blobUrl;
                    a.target = "_blank"
                    a.click();
                    setTimeout(() => {
                        window.URL.revokeObjectURL(blobUrl);
                        document.body.removeChild(a);
                    }, 0);
                }
                else {
                    const a = document.createElement("a");
                    document.body.appendChild(a);
                    const blobUrl = window.URL.createObjectURL(data);
                    a.href = blobUrl;
                    var downloadName = this.htmlDecode(name);
                    a.download = downloadName;
                    a.click();
                    setTimeout(() => {
                        window.URL.revokeObjectURL(blobUrl);
                        document.body.removeChild(a);
                    }, 0);
                }

            })
            .catch(error => {
                cpiLoadingSpinner.hide();
                //if (error.status >= 500)
                //    pageHelper.showErrors("Unhandled error.");
                //else
                    error.text().then(errorMessage => {
                        pageHelper.showErrors(errorMessage);
                    })
            });
    }

    rebindStatus (statusDataSourceUrl, activeSwitch, multiSelect) {
    var dataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: statusDataSourceUrl + "&activeSwitch=" + activeSwitch,
            }
        }
    });
    multiSelect.data("kendoMultiSelect").setDataSource(dataSource);
    }

    CheckInvalidKendoDate (form) {
        for (var i = 0; i < form[0].elements.length; i++) {
            if (form[0].elements[i].getAttribute("data-role") === "datepicker") {
                var currentValue = form[0].elements[i].value;
                if (currentValue != null && currentValue != "") {
                    const date = window.kendo.parseDate(currentValue, ['dd-MMM-yyyy']);
                    if (date === null) {
                        pageHelper.showErrors(currentValue + " is not a valid date.");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    caseNumberSearchValueMapper = (options) => {
        var result = { CaseNumber: options.value }
        setTimeout(function () { options.success(result); }, 300);
    }

    disclosureNumberSearchValueMapper = (options) => {
        var result = { DisclosureNumber: options.value }
        setTimeout(function () { options.success(result); }, 300);
    }

    TrademarkNameSearchValueMapper = (options) => {
        var result = { TrademarkName: options.value }
        setTimeout(function () { options.success(result); }, 300);
    }

    ClientCodeSearchValueMapper = (options) => {
        var result = { ClientCode: options.value, ClientName: "" }
        setTimeout(function () { options.success(result); }, 300);
    }

    ClientNameSearchValueMapper = (options) => {
        var result = { ClientName: options.value }
        setTimeout(function () { options.success(result); }, 300);
    }

    
    switchMultiAndSingle(multiClass, singleClass) {
    var multi = document.getElementsByClassName(multiClass);
    var single = document.getElementsByClassName(singleClass);

    if (multi.length != 1 || single.length != 1) {
        //pageHelper.
            return;
    }

    if (multi[0].getAttributeNames().includes("hidden")) {
        single[0].setAttribute("hidden", "hidden");
        var Clear = single[0].getElementsByClassName("k-clear-value");
        if (Clear.length != 1)
            return;
        Clear[0].click();
        multi[0].removeAttribute("hidden");
        var focused = multi[0].getElementsByClassName("k-state-focused");
        //focused[0].classList.remove("k-state-focused");
        if (focused.length==1)
        focused[0].className = focused[0].className.replace(/\bk-state-focused\b/g, "");
    }
    else {
        multi[0].setAttribute("hidden", "hidden");
        var Clear = multi[0].getElementsByClassName("k-clear-value");
        if (Clear.length != 1)
            return;
        Clear[0].click();
        single[0].removeAttribute("hidden");
        var focused = single[0].getElementsByClassName("k-state-focused");
        //focused[0].classList.remove("k-state-focused");
        if (focused.length == 1)
        focused[0].className = focused[0].className.replace(/\bk-state-focused\b/g, "");
    }
}

    showMultiWhenNotEmpty() {
        CombineFields();
        document.querySelectorAll('.k-multiselect').forEach(element => {
            element.querySelectorAll("SELECT").forEach(childrenElement => {
                if (childrenElement.nodeName == "SELECT") {
                    var parentNode = element.parentElement.parentElement;
                    var currentValue = document.getElementById(childrenElement.name.substring(0, childrenElement.name.length - 8)).value;
                    if (currentValue != "" && parentNode.className.indexOf("MultiSelectClass") > 0) {
                        parentNode.removeAttribute("hidden");
                        if (parentNode.nextElementSibling != null && parentNode.nextElementSibling.className.indexOf("ComboBoxClass") > 0)
                            parentNode.nextElementSibling.setAttribute("hidden", "hidden");
                    }
                    else {
                        if (parentNode.className.indexOf("MultiSelectClass") > 0) {
                            parentNode.setAttribute("hidden", "hidden");
                            if (parentNode.nextElementSibling != null && parentNode.nextElementSibling.className.indexOf("ComboBoxClass") > 0)
                                parentNode.nextElementSibling.removeAttribute("hidden");
                        }
                    }
                }
            })
        });
    }
    combineMultiSelect() {
    document.querySelectorAll('.k-multiselect').forEach(element => {
        element.querySelectorAll("SELECT").forEach(childrenElement => {
            if (childrenElement.nodeName == "SELECT") {
                var result = "|"
                var elementName = childrenElement.name;
                var target = document.getElementById(elementName.substring(0, elementName.length - 8));

                for (var i = 0; i < childrenElement.selectedOptions.length; i++) {
                    result += childrenElement.selectedOptions[i].value + "|";
                }

                if (result == "|")
                    result = "";

                target.value = result;
            }
        })
    });
    }

    emailReport(form) {
        if (this.CheckInvalidKendoDate(form)) {
            cpiLoadingSpinner.hide();
            return;
        }  

        var reportNameControl = document.getElementById("ReportName_customReport")
        if (reportNameControl != null) {
            var reportName = reportNameControl.value;
            if (reportName == null || reportName == "") {
                pageHelper.showErrors("Report Name is Required.");
                return;
            }
        }

        cpiLoadingSpinner.show();

        if (document.getElementById("token") == null) {
            CombineFields();
            $.validator.unobtrusive.parse(form);
            form.data("validator").settings.ignore = "";
            const criteriaData = new FormData(form[0]);

            $.ajax({
                type: "POST",
                url: form.data("email-report-url"),
                data: criteriaData,
                contentType: false,
                processData: false,
                success: function (result) {
                    const popupContainer = $(".popup").last();
                    popupContainer.html(result);
                    cpiLoadingSpinner.hide();
                },
                error: function (e) {
                    pageHelper.showErrors(e);
                    cpiLoadingSpinner.hide();
                }
            });
        } else {
            const tokenUrl = form.data("token-url");
            const username = form.data("username");

            //get token
            $.ajax({
                url: tokenUrl,
                data: { username: username, password: "_", grant_type: "password" },
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                success: function (result) {
                    form.find("#token").val(result.access_token);
                    CombineFields();
                    $.validator.unobtrusive.parse(form);
                    form.data("validator").settings.ignore = "";
                    const criteriaData = new FormData(form[0]);
                    $.ajax({
                        type: "POST",
                        url: form.data("email-report-url"),
                        headers: {
                            "RequestVerificationToken": result.access_token
                        },
                        data: criteriaData,
                        contentType: false,
                        processData: false,
                        success: function (result) {
                            const popupContainer = $(".popup").last();
                            popupContainer.html(result);
                            cpiLoadingSpinner.hide();
                        },
                        error: function (e) {
                            pageHelper.showErrors(e);
                            cpiLoadingSpinner.hide();
                        }
                    });
                },
                error: function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                }
            });
        }
    };

    initializeEmailReport() {
        this.setupMoreInfo();
        this.setupEntryForm();
    }

    setupMoreInfo() {
        this.emailReportForm = $("#emailReportForm");
        this.emailReportForm.find("#downIcon").show();
        this.emailReportForm.find("#upIcon").hide();
        this.emailReportForm.find("#moreContainer").hide();

        this.emailReportForm.find("#moreButton").on("click",
            () => {
                this.emailReportForm.find("#downIcon").toggle();
                this.emailReportForm.find("#upIcon").toggle();
                this.emailReportForm.find("#moreContainer").toggle();
            });
    }

    setupEntryForm() {
        const dialogContainer = $("#emailReportDialog");
        dialogContainer.modal("show");

        let entryForm = dialogContainer.find("form")[0];
        entryForm = $(entryForm);

        $.validator.unobtrusive.parse(entryForm);
        entryForm.data("validator").settings.ignore = "";

        pageHelper.addMaxLength(entryForm);
        pageHelper.clearInvalidKendoDate(entryForm);
        pageHelper.focusLabelControl(entryForm);

        const editor = $("#Body").data("kendoEditor");
        editor.refresh();



        entryForm.on("submit",
            (e) => {
                e.preventDefault();
                
                const formData = new FormData(entryForm[0]);

                // replace Body key with the content of Body Editor
                const body = entryForm.find("#Body").data("kendoEditor").value();
                formData.set("Body", body);

                if (entryForm.valid()) {
                    cpiLoadingSpinner.show();
                    $.ajax({
                        type: "POST",
                        url: entryForm.attr("action"),
                        data: formData,
                        contentType: false, // needed for file upload
                        processData: false, // needed for file upload
                        success: (result) => {
                            if (!result.Success) {
                                pageHelper.showErrors(result.ErrorMessage);
                            }
                            else {
                                pageHelper.showSuccess(entryForm.data("success"));
                                dialogContainer.modal("hide");
                            }
                            cpiLoadingSpinner.hide();
                        },
                        error: function (e) {
                            pageHelper.showErrors(e);
                            cpiLoadingSpinner.hide();
                        }
                    });

                } else {
                    entryForm.wasValidated();
                    $("#erMainTabs").find(".nav-link").removeClass("active");
                    $("#erMainTabsContent").find(".tab-pane").removeClass("active");
                    $("#erRecipientTab").addClass("active");
                    $("#erRecipientTabContent").addClass("active");
                    $("#erRecipientTabContent").addClass("show");

                }
            });
    }

    setToken(form) {
        if (document.getElementById("token") == null)
            return;
    const tokenUrl = form.data("token-url");
    const username = form.data("username");

    cpiLoadingSpinner.show();

    //get token
    $.ajax({
        url: tokenUrl,
        data: { username: username, password: "_", grant_type: "password" },
        type: "POST",
        contentType: "application/x-www-form-urlencoded",
        success: function (result) {
            cpiLoadingSpinner.hide();

            //set token value
            form.find("#token").val(result.access_token);
        },
        error: function (error) {
            cpiLoadingSpinner.hide();
            pageHelper.showErrors(error);
        }
    });

    }

    openAwardReportUpdate(url, AwardData) {
        const awardDataString = AwardData.join(',');
        $.ajax({
            url: url,
            data: { awardData: awardDataString },
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#inventorAwardReportMassUpdateEntryDialog");
                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: true,
                        beforeSubmit: function () {
                        },
                        afterSubmit: function (e) {
                            dialogContainer.modal("hide");

                            if (e.emailWorkflows) {
                                const promise = pageHelper.handleEmailWorkflow(e);
                                promise.then(() => {
                                });
                            }
                            location.reload();
                        }
                    }
                );
            },
        });
    }
}