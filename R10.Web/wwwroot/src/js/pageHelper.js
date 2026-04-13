const verificationTokenFormData = "__RequestVerificationToken";

//new layout: move breadcrumb trail to given container
const moveBreadcrumbs = function (container) {
    //MOVE STATUS MESSAGE
    const pageStatus = $(`${container} .page-message`);
    if (pageStatus.length > 0)
        $("#page .page-status").appendTo(pageStatus);

    //MOVE BREAD CRUMBS
    const pageCrumbs = $(`${container} .page-crumbs`);
    if (pageCrumbs.length > 0)
        $(`#page .container-crumbs`).appendTo(pageCrumbs);
};

const showSuccess = function (message) {
    cpiStatusMessage.success(message, 3000);
}

const showErrors = function (response) {
    const message = getErrorMessage(response);
    if (message)
        cpiStatusMessage.error(message);
};

const hideErrors = function () {
    cpiStatusMessage.hide();
};

const onGridError = function (e) {
    showErrors(e);
};

const errorsArrayToString = function (e) {
    let message = "";
    $.each(e.errors, function (key, value) {
        if (Object.prototype.toString.call(value) == '[object String]') { //string value
            message += value + "<br>";
        }
        else if ("errors" in value) {
            $.each(value.errors, function () {
                message += this + "<br>";
            });
        }
        else {
            $.each(value.Value, function () {
                message += this + "<br>";
            });
        }
    });
    return message;
};

const postJson = function (url, json) {
    return $.ajax({
        type: "POST",
        url: url,
        contentType: "application/json; charset=utf-8",
        headers: { "RequestVerificationToken": json.verificationToken },
        data: JSON.stringify(json.payLoad)
    });
};

const postData = function (url, form) {
    if (form.find("input[type='file']").length > 0) {
        return postMultipart(url, form);
    } else {
        const json = formDataToJson(form);
        return postJson(url, json);
    }
};

const postMultipart = function (url, form) {
    const data = formToFormData(form);
    return $.ajax({
        type: "POST",
        url: url,
        headers: { "RequestVerificationToken": $(form.find(`input[name='${verificationTokenFormData}']`)[0]).val() },
        data: data,
        processData: false,
        contentType: false
    });
};

const formToFormData = function (form) {
    const json = formDataToJson(form);
    const items = json.payLoad;
    const formData = new FormData();

    for (const key in items) {
        if (items.hasOwnProperty(key)) {
            formData.append(key, items[key]);
        }
    }
    formData.append("verificationToken", json.verificationToken);

    const files = form.find("input[type='file']");
    $.each(files, function () {
        const fileInput = $(this);
        for (let i = 0; i < this.files.length; i++) {
            formData.append(fileInput.attr("name"), this.files[i]);
            i++;
        }
    });
    return formData;
};

//converts array of form data to json
const formDataToJson = function (form, includeEmpty) {
    const values = form.serializeArray();
    const formData = {};
    let verificationToken = "";

    $.each(values, function () {
        if (this.value)
            this.value = this.value.trim();

        if (this.name === verificationTokenFormData) {
            verificationToken = this.value;
        }
        else if ((this.value > "" || includeEmpty) && !this.name.endsWith("_input")) {
            let element = form.find("input[name='" + this.name + "']");

            if (element.data("role") === "datepicker") {
                if (element.data("kendoDatePicker")) {
                    let dateValue = element.data("kendoDatePicker").value();
                    if (dateValue) {
                        dateValue = cpiDateFormatToSave(dateValue);
                    }
                    formData[this.name] = dateValue;
                }
            }
            else if (element.data("role") === "datetimepicker") {
                if (element.data("kendoDateTimePicker")) {
                    let dateValue = element.data("kendoDateTimePicker").value();
                    if (dateValue) {
                        dateValue = cpiDateTimeFormatToSave(dateValue);
                    }
                    formData[this.name] = dateValue;
                }
            }
            else if (element.data("role") === "numerictextbox") {
                //asp.net core model binder uses invariant culture
                //always use "." decimal separator
                //kendo already strips out thousands separator
                formData[this.name] = this.value.replace(",", ".");
            }
            else if (element.length > 0 && element[0].type === "checkbox") {
                formData[this.name] = element[0].checked;
            }
            else if (element.length > 0 && $(element[0]).hasClass("boolean")) {
                formData[this.name] = this.value.toLowerCase() === "true";
            }
            else {

                if (element.length < 1) {
                    element = form.find("select[name='" + this.name + "']");
                    if (element.data("role") === "multiselect") {
                        let values = element.data("kendoMultiSelect").value();

                        if (values.length > 0)
                            formData[this.name] = values;

                    }
                    else formData[this.name] = this.value;
                }
                else {
                    formData[this.name] = this.value;
                }
            }

        }
    });

    //from copy/paste without selecting from the dropdown (only the _input is populated)
    $.each(values, function () {
        if ((this.value > "") && this.name.endsWith("_input")) {
            const name = this.name.replace("_input", "");

            //use the _input value
            if (!formData[name]) {
                formData[name] = this.value;
            }
        }
    });

    //from copy/paste
    $(form).find('select').filter(function () {
        if ($(this).data("role") == "multiselect") {
            const ms = $(this);
         
            let values = ms.data("kendoMultiSelect").value();
            if (values.length > 0) {
                const name = ms.attr('name').replace("_Options", "");
                //use the _input value
                if (!formData[name]) {
                    formData[name] = '|' + values.join("|") + '|';
                }
            }
       }
    });
    
    return { verificationToken: verificationToken, payLoad: formData };
};

//converts array of form data to criteria list
const formDataToCriteriaList = function (form) {
    const filters = [];
    let verificationToken = "";

    const formFields = $(form).serializeArray();
    const formId = $(form).attr("id");
    let combinedFields;
    if (formId !== undefined) {
        const extendedForm = $(`div`).filter(function () {
            return $(this).attr("form") === formId;
        }).find(':input');
        const extendedFields = extendedForm.serializeArray();
        combinedFields = formFields.concat(extendedFields);
    } else {
        combinedFields = formFields;
    }

    // Extract MultiSelect values explicitly (serializeArray may not capture them reliably)
    $(form).find('select[data-role="multiselect"]').each(function () {
        const ms = $(this).data("kendoMultiSelect");
        if (ms) {
            const name = $(this).attr("name");
            const selectedValues = ms.value();
            if (selectedValues && selectedValues.length > 0) {
                // Remove any existing entries for this field from combinedFields
                combinedFields = combinedFields.filter(function (f) { return f.name !== name; });
                // Add each selected value as a separate entry
                for (var i = 0; i < selectedValues.length; i++) {
                    combinedFields.push({ name: name, value: selectedValues[i] });
                }
            }
        }
    });
    // Also check multiselects in extended form containers
    if (formId !== undefined) {
        $('div[form="' + formId + '"]').find('select[data-role="multiselect"]').each(function () {
            const ms = $(this).data("kendoMultiSelect");
            if (ms) {
                const name = $(this).attr("name");
                const selectedValues = ms.value();
                if (selectedValues && selectedValues.length > 0) {
                    combinedFields = combinedFields.filter(function (f) { return f.name !== name; });
                    for (var i = 0; i < selectedValues.length; i++) {
                        combinedFields.push({ name: name, value: selectedValues[i] });
                    }
                }
            }
        });
    }

    $.each(combinedFields, function () {
        if (this.name === verificationTokenFormData) {
            verificationToken = this.value;
        }
        else if (this.value && !this.name.endsWith("_input")) {
            const name = this.name;
            let value = this.value;

            // Convert wildcard characters for search criteria
            if (typeof value === 'string') {
                value = value.replace(/\*/g, '%').replace(/\?/g, '_');
            }

            const element = form.is("form") ? form.find("input[name='" + this.name + "']") : form.filter("input[name='" + this.name + "']");
            if (element.data("role") === "datepicker") {
                let dateValue = element.data("kendoDatePicker").value();
                if (dateValue) {
                    //exclude timezone
                    //dateValue = `${dateValue.getFullYear()}-${dateValue.getMonth() + 1}-${dateValue.getDate()}T00:00:00`;
                    dateValue = cpiDateFormatToSave(dateValue);
                }
                value = dateValue;
            }
            else if (element.length > 0 && element[0].type === "checkbox") {
                value = element[0].checked;
            }

            //name may appear more than 1 (ex. country, countryName for country)
            const pos = filters.findIndex(function (e) {
                return e.property === name;
            });
            if (pos < 0) {
                const criteria = {
                    property: name,
                    operator: "",
                    value: value
                };
                filters.push(criteria);
            }
            else {
                //send multiple values as JSON array
                const val = filters[pos].value;

                if (val !== value) {
                    let values;
                    try {

                        if (typeof val === "string")
                            if (val.substring(0, 1) === "[") //string with array content
                                values = JSON.parse(val);
                            else
                                values = [val];
                        else
                            values = JSON.parse(val);
                    }
                    catch (err) {
                        values = [val];
                    }
                    values.push(value);
                    filters[pos].value = JSON.stringify(values);
                }

            }
        }
    });

    console.log("formDataToCriteriaList filters:", JSON.stringify(filters));
    return { verificationToken: verificationToken, payLoad: filters };
};


const populateForm = function (form, values) {
    $.each(values, function (key, value) {

        let input = form.find(`input[name='${key}']`)[0];
        if (!input)
            input = form.find(`select[name='${key}']`)[0];
        if (!input)
            input = form.find(`textarea[name='${key}']`)[0];

        if (input) {
            const element = $(input);
            if (element.data("role") === "datepicker") {
                if (typeof value === "string")
                    value = new Date(value);
                element.data("kendoDatePicker").value(value);
            }
            else if (element.data("role") === "combobox") {
                if (value > "")
                    element.data("kendoComboBox").value(value);
                else
                    element.data("kendoComboBox").text(value);
            }
            else if (element.data("role") === "multicolumncombobox") {
                if (value > "")
                    element.data("kendoMultiColumnComboBox").value(value);
                else
                    element.data("kendoMultiColumnComboBox").text(value);
            }
            else if (element.data("role") === "dropdownlist") {
                if (value > "")
                    element.data("kendoDropDownList").value(value);
                else
                    element.data("kendoDropDownList").text(value);
            }
            else if (element.data("role") === "numerictextbox") {
                element.data("kendoNumericTextBox").value(value);
            }
            else if (element.data("role") === "multiselect") {
                let values = [value];
                if (value.startsWith("["))
                    values = JSON.parse(value);

                //to handle saved criteria with wildcard (not selectable)
                if (value.includes('*')) {
                    $(element.siblings('div')[0]).find("input").val(value);
                    element.closest(".form-group.float-label").removeClass("inactive").addClass("active");
                }
                else {
                    element.data("kendoMultiSelect").value(values);
                }
            }
            else if (input.type === "checkbox") {
                input.checked = value;
            }
            else if (input.type === "radio") {
                input = form.find(`input[name='${key}'][value='${value}']`)[0];
                $(input).prop("checked", true);

                if ($(input).data("change-on-load"))
                    $(input).trigger("change");

                const radio = form.find(`input[name='${key}']`)
                radio.parent("label.btn-outline").removeClass("active");
                $(input).parent("label.btn-outline").addClass("active");
            } else {
                element.val(value);
            }
            element.parents(".float-label").removeClass("inactive").addClass("active");

        }
        //else {
        //    const textArea = form.find(`textarea[name='${key}']`)[0];
        //    if (textArea) {
        //        $(textArea).val(value);
        //    }
        //}
    });

};

const searchFormSubmit = function (formInfo) {
    const json = formDataToCriteriaList(formInfo.form);

    cpiLoadingSpinner.show($(formInfo.form).data("loading"), 1);

    postJson(formInfo.url, json)
        .done(function (html) {
            appendPage(html);
            cpiLoadingSpinner.hide();
        })
        .fail(function (e) {
            showErrors(e.responseText);
            cpiLoadingSpinner.hide();
        });
};


//restores active tab (like after loading record info)
const restoreActiveTab = function (activePage, activeTabId, activeContentPaneId) {

    if (activeTabId) {
        activePage.infoContainer.find(".cpiDetailInfoNav .nav-link").removeClass("active");
        $(`#${activeTabId}`).addClass("active");

        if (activeContentPaneId) {
            activePage.infoContainer.find(".tab-content .tab-pane").removeClass("active show");
            $(`#${activeContentPaneId}`).addClass("active show");
        }
    }
};

//detail screen tabs
const initializeDetailTabs = function (activePage) {
    const horizontalTabs = $(`#${activePage.mainDetailContainer} .nav-tabs-horizontal.detail`);

    //initialize scrolling+sticky tabs
    if (horizontalTabs.length > 0) {
        horizontalTabs.scrollingTabs();
        horizontalTabs.stickyTabs();
    }

    //back to search results
    const searchResultsUrl = activePage.searchUrl + '/search';
    const backToSearch = $(`#${activePage.mainDetailContainer} .back-to-results`);
    backToSearch.off("click");

    const hasSearchResultsUrl = cpiBreadCrumbs.hasNodeByUrl(searchResultsUrl);
    if (hasSearchResultsUrl || cpiBreadCrumbs.hasNodeByUrl(activePage.searchUrl)) {
        backToSearch.on("click", function () {
            cpiStatusMessage.hide();
            $(activePage.searchResultContainer).resetComboBoxes();
            window.cpiBreadCrumbs.showNodeByUrl(hasSearchResultsUrl ? searchResultsUrl : activePage.searchUrl);
        });
    }
    else {
        backToSearch.on("click", function () {
            cpiStatusMessage.hide();
            window.location.href = activePage.searchUrl;
        });
    }

    //load active tab
    const activeTab = $(`#${activePage.detailContentContainer}`).find(".nav-tabs.detail .nav-link.active");
    if (activeTab)
        activeTab.click();
};


const showDetails = function (activePage, id, afterShowHandler) {
    window.kendo.destroy(activePage.infoContainer);

    //get active tab info before loading the next record
    const activeTabId = activePage.infoContainer.find(".cpiDetailInfoNav .nav-link.active").attr("id");

    const contentTabContainer = activePage.infoContainer.find(".page-content").siblings(".tab-content");
    const activeContentPane = contentTabContainer.children(".tab-pane.active");
    const activeContentPaneId = $(activeContentPane[0]).attr("id");

    //$(document).ready(function () {
    if (isNaN(id))
        id = id.id;

    return getDetails(activePage, id, function () {
        if (activeTabId) {
            restoreActiveTab(activePage, activeTabId, activeContentPaneId);
        }
        if (afterShowHandler)
            afterShowHandler();
    });
    //});
};

const getDetails = function (activePage, id, afterGetHandler) {
    const deferred = $.Deferred();
    const detailUrl = activePage.detailUrl.replace("recid", id);

    cpiLoadingSpinner.show();

    $.get(detailUrl)
        .done(function (result) {
            activePage.infoContainer.html(result);
            //hideErrors();

            if (afterGetHandler)
                afterGetHandler();

            cpiLoadingSpinner.hide();

            deferred.resolve();
        })
        .fail(function (error) {
            cpiLoadingSpinner.hide();

            cpiAlert.warning(error.responseText, function () {
                window.cpiBreadCrumbs.markLastNode({ dirty: false });

                const searchResultsUrl = activePage.searchUrl + '/search';
                if (window.cpiBreadCrumbs.hasNodeByUrl(searchResultsUrl))
                    window.cpiBreadCrumbs.showNodeByUrl(searchResultsUrl);
                else
                    openLink(searchResultsUrl, true);
            });

            deferred.reject(error);
        });

    return deferred.promise();
};

const manageDetailPage = function (options) {
    const activePage = options.activePage;
    const mainContainer = $(`#${activePage.mainDetailContainer}`);

    if (mainContainer.length > 0) {
        manageDetailPageMainButtons(mainContainer, options);  //attach handler to main buttons

        //entry form preparation
        const entryForm = $(mainContainer).find("form")[0];
        if (entryForm && entryForm.length > 0) {

            if (options.addMode) {
                if (activePage.recordNavigator)
                    activePage.recordNavigator.hide();

                activePage.entryFormInstance = $(entryForm).cpiEntryForm(setupAddModeOptions(options));
            }

            //edit mode
            else {
                activePage.currentRecordId = options.id;
                if (activePage.recordNavigator)
                    activePage.recordNavigator.show();

                activePage.entryFormInstance = $(entryForm).cpiEntryForm(setupEditModeOptions(options));
            }

            //attach handler for link buttons
            $(entryForm).on("click", ".cpiButtonLink, .cpiDetailsLink", function () {
                let url = $(this).data("url");
                if (url.length > 0) {
                    cpiStatusMessage.hide();

                    let queryString = "";
                    const urlData = $(this).data("url-data");
                    for (var key in urlData) {
                        if (urlData.hasOwnProperty(key)) {
                            //queryString = queryString + `&${encodeURI(key)}=${encodeURI(urlData[key])}`;
                            queryString = queryString + `&${encodeURIComponent(key)}=${encodeURIComponent(urlData[key])}`;
                        }
                    }
                    if (queryString.length > 0 && url.indexOf("?") < 0)
                        queryString = queryString.replace("&", "?");

                    const source = $(this).data("source");
                    if (source) {
                        const el = $(`#${source}`);
                        let comboBox = el.data("kendoMultiColumnComboBox");
                        if (!comboBox)
                            comboBox = el.data("kendoComboBox");

                        if (comboBox) {
                            el.data("fetched", 0); //flag as not fetched
                            url = url.replace("actualValue", encodeURIComponent(comboBox.value()));
                        }
                    }
                    if ($(this).data("new-tab"))
                        window.open(url + queryString, "_blank");
                    else
                        openLink(url + queryString, false);
                }
            });
        }

        //update breadcrumbs
        window.history.replaceState("", "", options.requestedUrl);
        window.cpiBreadCrumbs.updateNodeInfo({ name: activePage.mainDetailContainer, url: options.requestedUrl });

        // show active tab
        showActiveTab(options);


    }
};

const setupAddModeOptions = function (options) {
    const activePage = options.activePage;

    const defaults = {
        afterSubmit: activePage.afterInsert,
        afterSubmitOptions: {},
        activePage: activePage,
        addMode: true
    };

    if (options.addFromSearch) {
        defaults.afterSubmitOptions = { url: options.detailUrl };

        if (options.addFromOtherScreen) {
            defaults.onCancel = activePage.cancelAddFromOtherScreen;
            defaults.onCancelOptions = activePage.searchUrl;
        }
        else {
            //defaults.onCancel = pageHelper.showSearchScreen;
            //defaults.onCancelOptions = options.searchUrl;

            //new layout: showPreviousNode when adding record is cancelled
            //todo: issue when cancelling add from combo link
            defaults.onCancel = window.cpiBreadCrumbs.showPreviousNode;
        }
    }
    else {
        defaults.onCancel = activePage.afterCancelledInsert;
        defaults.onCancelOptions = activePage.currentRecordId;
    }
    const settings = $.extend({}, defaults, options.addModeOptions); //override the default if supplied
    return settings;
};

const setupEditModeOptions = function (options) {
    const activePage = options.activePage;
    const defaults = {
        //new layout: showDetails afterSubmit and onCancel
        afterSubmit: activePage.showDetails,
        //afterSubmit: activePage.updateRecordStamps,
        afterSubmitOptions: {},
        onCancel: activePage.showDetails,
        onCancelOptions: activePage.currentRecordId,
        activePage: activePage,
        addMode: false
    };
    const settings = $.extend({}, defaults, options.editModeOptions); //override the default if supplied
    return settings;
};



const showSearchScreen = function (url) {
    //remove search screen
    //use search results url
    var searchUrl = url + "/search";

    if (window.cpiBreadCrumbs.hasNodeByUrl(searchUrl)) {
        window.cpiBreadCrumbs.showNodeByUrl(searchUrl);
    }
    else if (window.cpiBreadCrumbs.hasNodeByUrl(url)) {
        window.cpiBreadCrumbs.showNodeByUrl(url);
    }
    else {
        window.location.href = url;
    }
};

//handles get request and appends the result to the screen
const openLink = function (url, clearLastNode, errorRedirectUrl) {
    const deferred = $.Deferred();
    cpiLoadingSpinner.show();

    const loadPage = !isPageInTheDom(url);
    if (!loadPage) {
        cpiLoadingSpinner.hide();
        window.open(url, '_blank');
        return;
    }

    $.get(url)
        .done(function (html) {
            if (clearLastNode)
                window.cpiBreadCrumbs.deleteLastNode();
            //else {
            //    const mainUrl = url.toLowerCase().substring(0, url.lastIndexOf("/")).replace("detaillink", "detail");
            //    window.cpiBreadCrumbs.deleteNodeByUrl(mainUrl); 
            //}
            setTimeout(() => { appendPage(html); cpiLoadingSpinner.hide(); deferred.resolve(); }, 1000); //hopefully the timeout can help with kendo issue (box)

        })
        .fail(function (error) {
            cpiLoadingSpinner.hide();

            if (errorRedirectUrl) {
                cpiAlert.warning(error.responseText, function () {
                    window.cpiBreadCrumbs.markLastNode({ dirty: false });

                    if (window.cpiBreadCrumbs.hasNodeByUrl(errorRedirectUrl))
                        window.cpiBreadCrumbs.showNodeByUrl(errorRedirectUrl);
                    else
                        openLink(errorRedirectUrl, true);
                });
            }
            else {
                showErrors(error);
            }
            deferred.reject(error);
        });

    return deferred.promise();
};

const appendPage = function (html) {
    hideErrors();
    window.cpiBreadCrumbs.hideContainers();
    $("#page").append(html);
};

const openDetailsLink = function (link) {
    const url = link.attr("href");
    openLink(url);
};

const isPageInTheDom = function (url) {
    let page = "";
    if (url.indexOf("?") > -1) {
        page = url.substring(0, url.indexOf("?"));
    }
    else {
        const pos = url.lastIndexOf("/");
        page = url.substring(0, pos);
    }
    const nodes = cpiBreadCrumbs.getNodes();

    const node = nodes.find(el => {

        if (el.url.toLowerCase() === page.toLowerCase()) {
            return true;
        }
        else {
            const nodeUrlItems = el.url.split('/');
            const newUrlItems = page.split('/');

            //only exlude part after action
            //if (nodeUrlItems.length > 1)
            if (nodeUrlItems.length > 5) //assuming we always use virtual path
                nodeUrlItems.pop(); //exclude last element
            else if (nodeUrlItems.length === 5 && nodeUrlItems[4] && nodeUrlItems[4].includes("?"))  // not virtual path
                nodeUrlItems.pop(); //exclude last element

            if (nodeUrlItems.length !== newUrlItems.length) {
                return false;
            }

            let match = true;
            for (let i = 0; i <= nodeUrlItems.length - 1; i++) {
                const nodeItem = nodeUrlItems[i].toLowerCase().startsWith('detail') ? 'detail' : nodeUrlItems[i].toLowerCase();
                const newItem = newUrlItems[i].toLowerCase().startsWith('detail') ? 'detail' : newUrlItems[i].toLowerCase();

                //allow if last element
                if ((nodeItem !== newItem) || (el.url === nodes[nodes.length - 1].url)) {
                    match = false;
                    break;
                }
            }
            return match;
        }
    });
    return node;
}


const getDataItemValues = function (dataItem) {
    const data = {};
    for (const key in dataItem) {
        if (dataItem.hasOwnProperty(key)) {
            const value = dataItem[key];
            if (typeof value !== "object" && typeof value !== "function")
                data[key] = value;
        }
    }
    return data;
};

const deleteGridRow = function (e, dataItem, afterDelete) {
    e.preventDefault();
    const grid = $("#" + e.delegateTarget.id).data("kendoGrid");
    const form = $(e.currentTarget).closest("form");

    const deletePrompt = grid.options.editable.confirmDelete;
    const title = form.data("delete-title");

    cpiConfirm.delete(title, deletePrompt, function () {

        if (dataItem.id === 0) {
            grid.removeRow($(e.currentTarget).closest("tr"));
            grid.dataSource._destroyed = [];
        }
        else {
            cpiLoadingSpinner.show();
            $.ajax({
                url: grid.dataSource.transport.options.destroy.url,
                data: { deleted: getDataItemValues(dataItem) },
                type: "POST",
                success: function (result) {
                    cpiLoadingSpinner.hide();
                    showSuccess(result.success);

                    grid.removeRow($(e.currentTarget).closest("tr"));
                    grid.dataSource._destroyed = [];

                    //do not refresh grid to keep new and modified rows
                    //grid.dataSource.read();
                    //grid.refresh();
                    if (afterDelete)
                        afterDelete(result);
                },
                error: function (e) {
                    cpiLoadingSpinner.hide();
                    showErrors(e);
                }
            });
        }

    });
};

const emailGridRow = function (e, dataItem, afterEmail) {
    e.preventDefault();
    const parent = $("#" + e.delegateTarget.id).parent();
    const url = parent.data("url-email");

    $.ajax({
        url: url,
        data: { data: getDataItemValues(dataItem) },
        type: "Get",
        success: function (result) {
            const popupContainer = $(".cpiContainerPopup").last();
            popupContainer.html(result);

            if (afterEmail)
                afterEmail(result);
        },
        error: function (e) {
            showErrors(e);
        }
    });
};

//sortable grid
const placeholder = function (element) {
    return element.clone().addClass("placeholder");
};

const hint = function (element) {
    return element.clone().addClass("hint")
        .height(element.height())
        .width(element.width());
};

//clear invalid date entry
const clearInvalidKendoDate = function (parent) {
    parent.find("input[data-role='datepicker']").blur(function () {
        const el = $(this);

        const date = window.kendo.parseDate(el.val(), ['dd-MMM-yyyy']);
        if (date === null) {
            el.val("");
        }
    });
};

//handle label click to focus control
const focusLabelControl = function (parent) {
    parent.find("label").click(function () {
        const label = $(this);
        focusKendoWidget(label);
    });
};

const focusKendoWidget = function (label) {
    const forEl = label.attr("for");

    if (forEl) {
        var form = label.closest("form");
        if (form.length > 0) {
            var widget = $(form).find("input[name='" + forEl + "'],textarea[name='" + forEl + "'],select[name='" + forEl + "']");
            if (widget) {
                switch (widget.data("role")) {
                    case "combobox":
                        widget.data("kendoComboBox").focus();
                        break;
                    case "numerictextbox":
                        widget.data("kendoNumericTextBox").focus();
                        break;
                    case "multiselect":
                        widget.data("kendoMultiSelect").focus();
                        break;
                    default:
                        widget.focus();
                }
            }
        }
    }
};

const resetComboBoxes = function (form) {
    form.find("input[data-role='combobox']").each(function () {
        var el = $(this);
        //todo: clear value
        if (el.data('fetched') === 1) {
            el.data('fetched', 0); //flag as not fetched so we can requery the latest data
        }
    });
};

const initializeMainSearchTabs = function (searchContainer) {
    const tabHorizontal = $(`${searchContainer} .nav-tabs-horizontal`);

    tabHorizontal.scrollingTabs();
    tabHorizontal.stickyTabs();
    tabHorizontal.resetTabContent(`${searchContainer} .tab-content.search`);

    const vtabFilterCount = $(`${searchContainer} .nav-tabs-vertical`).filterCount();
    const htabFilterCount = tabHorizontal.filterCount();
    vtabFilterCount.refreshAll();
    htabFilterCount.refreshAll();

    $(`${searchContainer} .tab-content.search`).liveSearch(function (el) {
        vtabFilterCount.refresh(el);
        htabFilterCount.refresh(el);
    });

    //clear filter count
    //delegate so would fire last, after inputs are cleared
    $("body").on("click", `${searchContainer} .search-clear`, function () {
        vtabFilterCount.refreshAll();
        htabFilterCount.refreshAll();
    });

    //sync vertical and horizontal tabs
    $(`${searchContainer} .nav-tabs.search li a`).on("click", function () {
        const href = $(this).attr("href");

        $(`${searchContainer} .nav-tabs.search li a`).removeClass("active");
        $(`${searchContainer} .nav-tabs.search li a[href="${href}"]`).addClass("active");

        $(`${searchContainer} .tab-content.search .tab-pane`).removeClass("active show");
        $(`${searchContainer} .tab-content.search`).find(href).addClass("active show");
    });

    //move breadcrumbs
    moveBreadcrumbs(searchContainer);
};

const initializeSidebarPage = function (sidebar, content) {
    if ($(sidebar).hasClass("collapsible"))
        return;

    const showFilterButtonContainer = $(`${content} .sidebar-link`);
    const collapsibleSidebar = $(sidebar).collapsibleSidebar(showFilterButtonContainer);

    if (showFilterButtonContainer.length > 1)
        $(`${content} .sidebar-link.default`).hide();

    return collapsibleSidebar;
}

//search results sidebar
const initializeSidebar = function (activePage) {
    //$(".kendo-Grid .k-grid-toolbar").addClass("sidebar-link");
    const searchResultsContainer = activePage.searchResultContainer;
    const sidebar = initializeSidebarPage(`${searchResultsContainer} .page-sidebar`, `${searchResultsContainer} .page-main`);

    sidebar.openButton.append("<span class='total-filter-count'></span>");

    activePage.sidebar = sidebar;

    const formId = $(activePage.searchResultGrid).attr("id").replace("Grid", "RefineSearch");
    const extendedForm = $(`div`).filter(function () {
        return $(this).attr("form") === formId;
    });

    const filterTabs = $(`${searchResultsContainer} .nav-tabs.refine-search`);
    $(filterTabs).accordionTabs();

    const filterCount = $(filterTabs).filterCount();
    filterCount.refreshAll();
    filterCount.openDefault();

    const refreshTotalFilterCount = function () {
        let extendedCount = 0;
        if (extendedForm.length > 0)
            extendedCount = $(extendedForm).find(`input:checked`).val() !== "" ? 1 : 0;

        const totalFilterCount = filterCount.total();
        const currentTotalCount = totalFilterCount + extendedCount;

        sidebar.openButton.find(".total-filter-count").html(currentTotalCount > 0 ? currentTotalCount : "");
    };
    refreshTotalFilterCount();

    $(activePage.searchResultGrid).refineSearch(activePage.refineSearchContainer, function (el) {
        filterCount.refresh($(el));
        refreshTotalFilterCount();

        //auto close floating sidebar
        //if (sidebar.isFloating()) {
        //    sidebar.close();
        //}
    }, activePage.validate);


    if (extendedForm.length > 0) {
        $(activePage.searchResultGrid).refineSearch(extendedForm, function (el) {
            refreshTotalFilterCount();
        }, activePage.validate);
    }

    const clearExtendedForm = function () {
        if (extendedForm.length > 0) {
            $(extendedForm).find('label').removeClass('active').first().addClass('active');
            $(extendedForm).find(`input`).first().prop('checked', true);
        }
    }

    //clear filter count
    //delegate so would fire last, after inputs are cleared
    $("body").on("click", `${searchResultsContainer} .search-clear`, function () {
        filterCount.refreshAll();
        clearExtendedForm();

        //show initial result if refine search is on
        if ($(activePage.searchResultGrid).hasClass("no-refine-search"))
            clear();
        else
            search();
    });

    const getSearchResultsGrid = function () {
        let grid = $(activePage.searchResultGrid).data("kendoGrid");

        if (grid === undefined)
            grid = $(activePage.searchResultGrid).data("kendoListView");

        return grid;
    }

    const search = function () {
        const grid = getSearchResultsGrid();

        //use .query() to reset to page 1
        //grid.dataSource.read();
        const dataSource = grid.dataSource;
        dataSource.query({
            sort: dataSource.sort(),
            page: 1,
            pageSize: dataSource.pageSize()
        });
    }

    const clear = function () {
        const grid = getSearchResultsGrid();
        grid.dataSource.data([]);
    }

    $(`${searchResultsContainer} .save-filters`).on("click", function () {
        const form = activePage.refineSearchContainer;
        let screen = form.replace("#", '');
        if (activePage.systemTypeCode)
            screen = activePage.systemTypeCode + "-" + screen; 
        getSearchCriteriaScreen(form, screen, true);
    });

    $(`${searchResultsContainer} .load-filters`).on("click", function () {
        const form = activePage.refineSearchContainer;
        let screen = form.replace("#", '');
        if (activePage.systemTypeCode)
            screen = activePage.systemTypeCode + "-" + screen; 

        getSearchCriteriaScreen(form, screen, false, function (response) {
            $(form).clearSearch();
            loadSearchCriteria(response, activePage);
        });
    });

    //back to search screen
    const backToSearch = $(`${searchResultsContainer} .back-to-search`);
    backToSearch.off("click");
    backToSearch.on("click", function () {
        //make sure sidebar open button is hidden
        sidebar.open();
        cpiStatusMessage.hide();
        window.cpiBreadCrumbs.showNodeByUrl(activePage.searchUrl);
    });

    const searchClear = $(`${searchResultsContainer} .search-clear`);
    searchClear.off("click");
    searchClear.on("click", function () {
        $(activePage.refineSearchContainer).clearSearch();
        sidebar.openButton.find(".total-filter-count").html("");
    });

    if ($(activePage.searchResultGrid).hasClass("no-refine-search")) {
        const searchSubmit = $(`${searchResultsContainer} .search-submit`);
        searchSubmit.show();
        searchSubmit.off("click");
        searchSubmit.on("click", function () {
            search();
        });
    }

    let screen = activePage.refineSearchContainer.replace("#", '');
    if (activePage.systemTypeCode)
        screen = activePage.systemTypeCode + "-" + screen; 

    //load default search criteria
    loadDefaultSearchCriteria(activePage.refineSearchContainer, screen, activePage);

    return filterCount;
};

//search criteria save/load functions; added customExtract for custom extraction of criteria (used by globalSearch)
const getSearchCriteriaScreen = function (form, screen, save, afterSubmit, customExtract, screenUrl) {
    const baseUrl = $("body").data("base-url");
    let url = screenUrl;
    if (!url)
        url = `${baseUrl}/Shared/SearchCriteria/CriteriaScreen`;

    let criteria = null;
    let jsonCriteria = null;

    if (save) {
        if (customExtract) {
            jsonCriteria = customExtract();
            criteria = JSON.stringify(jsonCriteria);
        } else {
            jsonCriteria = pageHelper.gridMainSearchFilters($(form));
            criteria = JSON.stringify(jsonCriteria.mainSearchFilters);
        }
    }

    $.post(url, { screen: screen ? screen : form.replace("#", ''), criteria: criteria, save: save })
        .done((result) => {
            const container = $("body").find(".cpiContainerPopup");
            if (container.length === 0) {
                $("body").append("<div class='cpiContainerPopup'></div>");
            }
            let popupContainer = $(".cpiContainerPopup").last();

            popupContainer.html(result);
            const dialogContainer = $("#saveSearchCriteriaDialog");
            //load
            if (!save) {
                dialogContainer.on("click", ".btn-action", function () {
                    const classes = $(this).attr("class");
                    if (classes.includes("btn-action-delete")) {
                        dialogContainer.find("#LoadType").val("delete");
                        afterSubmit = null;
                    }
                    else if (classes.includes("btn-action-update")) {
                        dialogContainer.find("#LoadType").val("update");
                        afterSubmit = null;
                    }
                    else if (classes.includes("btn-action-load"))
                        dialogContainer.find("#LoadType").val("load");
                });
            }

            let entryForm = dialogContainer.find("form")[0];
            dialogContainer.modal("show");
            entryForm = $(entryForm);
            entryForm.cpiPopupEntryForm(
                {
                    dialogContainer: dialogContainer,
                    afterSubmit: function (response) {
                        if (afterSubmit)
                            afterSubmit(response);

                        dialogContainer.modal("hide");
                    }
                }
            );
        })
        .fail((e => {
            console.log(e);
            pageHelper.showErrors(e);
        }));
}

const onSearchCriteriaNameChange = function (e) {
    const comboBox = e.item;

    if (comboBox) {
        const name = comboBox.text();
        const dialog = $("#saveSearchCriteriaDialog").last();
        dialog.find("#OldCriteriaName").val(name);
        dialog.find("#IsDefault").prop("checked", e.dataItem.IsDefault);
    }
}

//loadDefaultSearchCriteria - added customLoad for custom extraction of criteria, afterLoad for event after load (used by globalSearch)
const loadDefaultSearchCriteria = function (form, screen, activePage, customLoad, afterLoad) {
    const baseUrl = $("body").data("base-url");
    const url = `${baseUrl}/Shared/SearchCriteria/GetDefault`;

    $.get(url, { screen: screen ? screen : form.replace("#", '') })
        .done((response) => {
            if (response.length > 0) {
                if (customLoad) {
                    customLoad(response);
                }
                else {
                    $(form).clearSearch();
                    loadSearchCriteria(response, activePage);
                }
            }
        })
        .then(() => {
            if (afterLoad) {
                afterLoad();
            }
        })

        .fail((e => {
            pageHelper.showErrors(e);
        }));
}

const loadSearchCriteria = function (response, activePage) {
    const criteria = JSON.parse(response);
    if (criteria.length > 0) {
        const form = activePage.refineSearchContainer;
        const keyValues = {};
        for (const item of criteria) {
            keyValues[item.property] = item.value;
        }
        populateForm($(form), keyValues);

        //multi select combo issue (slow in selecting values)
        setTimeout(function () {
            if (activePage.searchResultGrid) {
                let resultsGrid = activePage.searchResultGrid.data("kendoGrid");
                if (!resultsGrid)
                    resultsGrid = activePage.searchResultGrid.data("kendoListView");
                resultsGrid.dataSource.read();
            }

        }, 600);
    }
}
// -------------------


//attach handler to detail page main buttons
const manageDetailPageMainButtons = function (mainContainer, options) {
    const activePage = options.activePage;
    activePage.mainControlButtons = $(mainContainer).find(".cpiButtonsDetail");
    if (activePage.mainControlButtons.length > 0) {
        const defaultMainButtonsSettings = {
            onRefresh: function (id) {
                activePage.showDetails(id);
            },
            onRefreshOptions: options.id
        };
        const settingsMainButtons = $.extend({}, defaultMainButtonsSettings, options.settingsMainButtons); //override the default if supplied
        activePage.mainControlButtons.cpiMainButtons(settingsMainButtons);
    }
};

//auto add maxlength to form inputs
const addMaxLength = function (form) {
    form.find("input[data-val-length-max]").each(function () {
        const length = parseInt($(this).data("val-length-max"));
        $(this).prop("maxlength", length);
    });
};

const kendoGridSave = function (params) {
    const grid = $("#" + params.name).data("kendoGrid"),
        parameterMap = grid.dataSource.transport.parameterMap;

    const changes = kendoGridGetChanges(grid);
    const data = {};
    $.extend(data,
        typeof (params.filter) === "function" ? params.filter() : params.filter,
        parameterMap({ updated: changes.updatedRecords }),
        parameterMap({ deleted: changes.deletedRecords }),
        parameterMap({ new: changes.newRecords }));

    const defer = $.Deferred();

    if (params.beforeSubmit) {
        params.beforeSubmit().then(
            () => save(),
            () => defer.reject(),
        );
    }
    else {
        save();
    }

    function save() {
        $.ajax({
            url: grid.dataSource.transport.options.update.url,
            data: data,
            type: "POST",
            success: function (e) {
                grid.dataSource._destroyed = [];
                grid.dataSource.read();
                params.isDirty = false;
                hideErrors();
                if (params.afterSubmit) {
                    params.afterSubmit(e);
                }

                let successMsg = "";
                if (e.success)
                    successMsg = e.success;
                else {
                    const parentForm = $("#" + params.name).parents("#detailForm");
                    if (parentForm.length > 0) {
                        successMsg = parentForm.data("save-message");
                    }
                }
                if (successMsg)
                    showSuccess(successMsg);

                defer.resolve(successMsg);

            },
            error: function (e) {
                const error = getErrorMessage(e);
                showErrors(error);

                defer.reject(error);
            }
        });
    }

    return defer.promise();
};

const kendoGridIsDirty = function (name) {
    const grid = $("#" + name).data("kendoGrid");
    const changes = kendoGridGetChanges(grid);
    return changes.newRecords.length > 0 || changes.updatedRecords.length > 0;
};

const kendoGridGetChanges = function (grid) {

    //get the new and the updated records
    const updatedRecords = [];
    const newRecords = [];
    const deletedRecords = [];

    if (grid && grid.dataSource) {
        const currentData = grid.dataSource.data();
        for (let i = 0; i < currentData.length; i++) {

            if (currentData[i].isNew() && currentData[i].dirty) {
                newRecords.push(currentData[i].toJSON());
            } else if (currentData[i].dirty) {
                updatedRecords.push(currentData[i].toJSON());
            }
        }
        for (let d = 0; d < grid.dataSource._destroyed.length; d++) {
            deletedRecords.push(grid.dataSource._destroyed[d].toJSON());
        }
    }
    return { updatedRecords: updatedRecords, newRecords: newRecords, deletedRecords: deletedRecords };
};

const kendoGridDirtyTracking = function (grid, el, afterSave, afterCancel, onDirty) {
    if (grid.length > 0) {
        if (grid.data().kendoGrid) {

            grid.find(".k-grid-toolbar").on("click", ".k-grid-Cancel", function () {
                grid.data("kendoGrid").dataSource.read();
                el.isDirty = false;
                kendoGridAfterSaveHandler(grid, afterCancel);
            });

            grid.find(".k-grid-toolbar").on("click", ".k-grid-Save", function () {
                pageHelper.kendoGridSave(el).then(function () {
                    kendoGridAfterSaveHandler(grid, afterSave);
                });
            });

            grid.data().kendoGrid.dataSource.bind('change', function (e) {
                //el.isDirty = e.action === "itemchange" || e.action === "add" || e.action === "remove";
                //el.isDirty = e.action === "itemchange" || e.action === "add"; 

                //include e.action === "add" to show save/cancel button right after clicking add button
                //todo: can't we just use line commented above? --> el.isDirty = e.action === "itemchange" || e.action === "add"; 
                //el.isDirty = pageHelper.kendoGridIsDirty(el.name);
                el.isDirty = e.action === "add" || pageHelper.kendoGridIsDirty(el.name);
                if (el.isDirty) {
                    kendoGridDirtyHandler(grid, onDirty);
                }
            });
            grid.on("input", "input,textarea", function (e) {
                const target = $(e.target);
                if (!(target.hasClass("k-checkbox") && (target.closest("td").hasClass("selector-only") || target.closest("th").hasClass("k-header")))) {
                    kendoGridDirtyHandler(grid, onDirty);
                }
            });
        }
    }

};

const kendoGridDirtyHandler = function (grid, onDirty) {
    grid.find(".k-grid-Save,.k-grid-Cancel").removeClass("d-none");
    $(grid).addClass("dirty");

    if (onDirty) {
        onDirty();
    }
};

const kendoGridAfterSaveHandler = function (grid, afterSave) {
    grid.find(".k-grid-Save,.k-grid-Cancel").addClass("d-none");
    $(grid).removeClass("dirty");

    if (afterSave) {
        afterSave();
    }
};

const showActiveTab = function (options) {
    if (options.activeTab) {
        const tabId = $(`#${options.activePage.detailContentContainer}`).find(".nav-tabs.cpiDetailInfoNav").attr("id");
        const selector = `#${tabId} #${options.activeTab}`;
        $(selector).tab("show");
    }
};

const cpiDateFormatToDisplay = function (date) {
    if (date) {
        return window.kendo.toString(date, "dd-MMM-yyyy");
    } else {
        return "";
    }
};

//todo: use in formDataToJson and formDataToCriteriaList
const cpiDateFormatToSave = function (dateValue) {
    if (dateValue) {
        //return (new Date(dateValue.getFullYear(), dateValue.getMonth(), dateValue.getDate())).toISOString().split("T")[0] + "T00:00:00";
        //return `${dateValue.getFullYear()}-${dateValue.getMonth() + 1}-${dateValue.getDate()}T00:00:00`;
        return `${dateValue.getFullYear()}-${("0" + (dateValue.getMonth() + 1)).slice(-2)}-${("0" + dateValue.getDate()).slice(-2)}T00:00:00`;



    } else {
        return "";
    }
};

const cpiDateTimeFormatToSave = function (dateValue) {
    if (dateValue) {
        return `${dateValue.getFullYear()}-${("0" + (dateValue.getMonth() + 1)).slice(-2)}-${("0" + dateValue.getDate()).slice(-2)}T${("0" + dateValue.getHours()).slice(-2)}:${("0" + dateValue.getMinutes()).slice(-2)}:00`;
    } else {
        return "";
    }
};

//refreshes record detail display after add, update the record navigator 
const afterInsert = function (activePage, id) {
    if (activePage.recordNavigator && activePage.recordNavigator.length > 0) {
        if (isNaN(id))
            id = id.id;

        activePage.recordNavigator.addRecordId(id);
    }
    return activePage.showDetails(id);
};

const setDelay = function (callback, ms) {
    let timer = 0;
    return function () {
        const context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () {
            callback.apply(context, args);
        },
            ms || 0);
    };
};



const getPagedComboBoxValue = function (combo) {
    return {
        text: $(`#${combo}`).data("kendoComboBox").input.val()
    };
};

const handleComboBoxInvalidEntry = function () {
    //if (this.value() && this.selectedIndex === -1) {
    //    this.text("");
    //}
    //if (this.selectedIndex === -1) {
    //    this.value("");
    //}
    if (this.value() && this.selectedIndex === -1) {
        this._clear.click();
    }
};

const refreshGridNameField = function (e, nameField) {
    if (e.item) {
        $(`#${e.sender.element[0].id}`).closest("tr").find(".name-field").html(e.dataItem[nameField]);
    }
};

const onComboBoxSelect = function (e, name) {
    if (e.item) {
        displayNameFromComboBox(e.sender.element[0].id, e.dataItem[name]);
    }
};

const displayNameFromComboBox = function (comboBox, name) {
    var nameElement = $(`#${comboBox}_Name`);

    if (nameElement.length > 0) {
        if (nameElement.is("input"))
            nameElement.val(name).trigger("change");
        else
            nameElement.html(kendo.htmlEncode(name));
    }
};

const onComboBoxChangeDisplayName = function (e, nameProperty, limitToList = true) {
    var comboBox = e.sender;
    var name = ""

    if (limitToList && comboBox.value() && comboBox.selectedIndex === -1)
        comboBox.value("");

    //selectedIndex = 0 could be from Enter key pressed
    if (comboBox.selectedIndex > 0) {
        //name = comboBox.dataSource._data[comboBox.selectedIndex][nameProperty];
        name = comboBox.dataItem()[nameProperty];
    }
    else if (comboBox.selectedIndex == 0 && comboBox.value() != "") {
        var dataValueField = comboBox.options.dataValueField;
        var dataValue = comboBox.value();
        var len = comboBox.dataSource._data.length;
        if (len > 0) {
            var i;
            for (i = 0; i < len; i++) {
                if (comboBox.dataSource._data[i][dataValueField] == dataValue) {
                    name = comboBox.dataSource._data[i][nameProperty];
                }
            }
        }
    }

    displayNameFromComboBox(comboBox.element[0].id, name);
};

//const setWebLinksDefaultOption = function (container, e) {
//    const optionsContainer = $(`.${container}`);
//    if (optionsContainer.length > 0) {
//        if (optionsContainer.data("init") === 1) {
//            const option = optionsContainer.find('input[name="webLinksDisplayOption"]');
//            const links = e.items;
//            const pos = links.find(el => el.RecordLink === true);
//            option.val(pos !== undefined ? [0] : [1]);
//        }
//    }
//};

const setWebLinksDefaultOption = function (e) {
    const container = e.sender.element.closest(".weblinks");
    if (container.length > 0 && container.data("init") === 1) {
        const options = container.find("input.weblinks-option").data("kendoDropDownList");
        if (options) {
            const links = e.items;
            const pos = links.find(el => el.RecordLink === true);
            options.value(pos ? [0] : [1]);
        }
        container.data("init", 0);
    }
};

const setWebLinksOption = function (e) {
    const grid = e.sender.element.closest(".weblinks").find(".webLinksGrid").data("kendoGrid");
    grid.dataSource.read();
};

const getWebLinksOption = function (e, optionsName, params) {
    const options = $(`#${optionsName}`);
    const container = options.closest(".weblinks");
    if (container.length > 0) {
        let displayChoice = 2;
        if (container.data("init") !== 1)
            displayChoice = options.data("kendoDropDownList").value();

        return $.extend(params, { displayChoice: displayChoice });
    }

    return params;
};

const doNothing = function () {
};

const getFormCriteria = function () {
    const form = $("#searchCriteriaForm").serializeArray();
    const data = {};
    $.each(form,
        function () {
            data[this.name] = this.value;
        });
    return data;
};

const kendoGridDeleteRecord = function (e, dataItem, afterDelete) {
    e.preventDefault();
    const grid = $("#" + e.delegateTarget.id).data("kendoGrid");

    const deletePrompt = grid.options.messages.editable.confirmation;
    if (confirm(deletePrompt)) {
        $.ajax({
            url: grid.dataSource.transport.options.destroy.url,
            data: { deleted: pageHelper.getKendoDataItemProperties(dataItem) },
            type: "POST",
            success: function () {
                grid.removeRow($(e.currentTarget).closest("tr"));
                grid.dataSource._destroyed = [];
                //pageHelper.hideErrors();
                if (afterDelete)
                    afterDelete();
            },
            error: function (e) {
                showErrors(e);
            }
        });

    }
};

const getKendoDataItemProperties = function (dataItem) {
    const data = {};
    for (const key in dataItem) {
        const value = dataItem[key];
        if (typeof (value) !== "object" && typeof (value) !== "function")
            data[key] = value;
    }
    return data;
};

const setKendoListWidth = function (e, width) {
    if (width > 0)
        e.sender.list.width(width);
};

const updateRecordStamps = function (activePage) {
    $.get(activePage.recordStampsUrl).done(function (response) {
        // $("#" + activePage.recordStampsContainer).html(response);

        const recordStampsContainer = activePage.infoContainer.find(".content-stamp");
        if ($(recordStampsContainer).length > 0)
            $(recordStampsContainer).html(response);
    });
};

const gridMainSearchFilters = function (form) {
    const data = formDataToCriteriaList($(form));
    return {
        mainSearchFilters: data.payLoad,
        __RequestVerificationToken: data.verificationToken
    };
};

const getErrorMessage = function (e) {

    if (typeof e === "string")
        return e;

    if (e.responseJSON)
        if (typeof e.responseJSON === "string")
            return e.responseJSON;
        else
            return errorsArrayToString(e.responseJSON);

    if (e.responseText)
        return e.responseText;

    if (e.xhr)
        return e.xhr.responseText;

    //console.log(e);
    //let errorMessage = "";
    //for (const key in e) {
    //    if (e.hasOwnProperty(key)) {
    //        errorMessage = errorMessage + e[key] + "<br>";
    //    }
    //}
    //if (errorMessage)
    //    return errorMessage;

    switch (e.status) {
        case 401:
            return "Unauthorized request.";
        case 403:
            return "Access denied.";
        case 404:
            return "The requested resource was not found.";
        default:
            return "Unhandled error.";
    }
};

const addBreadCrumbsRefreshHandler = function (container, callBack) {
    container.on("click", ".details-link", function () {
        const breadCrumbs = cpiBreadCrumbs.getNodes();
        const lastNode = breadCrumbs[breadCrumbs.length - 1];
        lastNode.refreshHandler = function () {
            callBack();
        }
    });
}

const setupDragDropFiles = function (mainContainer) {
    $(document).ready(function () {
        const container = mainContainer + " .dropZoneElement";

        //browser's default behavior for files dropped on the document itself is to open it,
        //to avoid that, we need to handle the drop events on the document
        $(document).on("dragenter", function (e) {
            e.stopPropagation();
            e.preventDefault();
        });
        //while you are holding the mouse
        $(document).on("dragover", function (e) {
            e.stopPropagation();
            e.preventDefault();
        });
        $(document).on("drop", function (e) {
            e.stopPropagation();
            e.preventDefault();
        });

        //handle only if in the correct container
        $(container).on("dragenter", function (e) {
            e.stopPropagation();
            e.preventDefault();
        });
        $(container).on("dragover", function (e) {
            e.stopPropagation();
            e.preventDefault();
        });

        $(container).on("drop", function (e) {
            e.preventDefault();
            const files = e.originalEvent.dataTransfer.files;
            const droppedFiles = { files: files };
            $(mainContainer).trigger("filesDropped", droppedFiles);
        });
    });
};

//NumericTextBox does not work on grid attached to a sortable control, this function will fix it
const sortableGridOnEdit = function (e) {
    var input = e.container.find("[data-role=numerictextbox]");
    var widget = input.data("kendoNumericTextBox");
    //var model = e.model;

    if (widget) {
        widget.bind("spin", function (e) {
            e.sender.trigger("change");
        });
    }
    input.on("keyup", function (e) {
        if (e.key === kendo.culture().numberFormat["."]) {
            // for the Kendo UI NumericTextBox only.
            return;
        }
        widget.value(input.val());
        widget.trigger("change");
    });
}

//validation function for /Views/EntityFilter/_MultiSelect.chtml 
//and activePage.requiredEntityFilter
const validateRequiredEntityFilterList = function (e) {
    var form = $("#RequiredEntityFilterList");
    var list = form.find("#EntityFilter").data("kendoMultiSelect");
    var error = form.find(".error-message");

    if (list.value().length > 0) {
        form.find(".k-multiselect").addClass("k-is-valid");
        form.find(".k-multiselect").removeClass("k-is-invalid");
        error.hide();
    }
    else {
        form.find(".k-multiselect").removeClass("k-is-valid");
        form.find(".k-multiselect").addClass("k-is-invalid");
        error.show();
    }
};

//dummy valueMapper to avoid an error when setting the value dynamically of a paged kendoComboBox
const kendoValueMapper = function (options) {
    const baseUrl = $("body").data("base-url");
    const url = `${baseUrl}/Shared/SearchCriteria/GenericValueMapper`;
    $.ajax({
        url: url,
        data: { value: options.value },
        success: function (data) {
            options.success(data);
        }
    });
}


const resetFormValidator = function (form) {
    $(form).removeData('validator');
    $(form).removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse(form);
}

const initKendoEditor = function (name) {
    const el = $(`#${name}`);
    const elWrap = el.closest(".html-editor");
    const editor = el.data("kendoEditor")
    const editorBody = $(editor.body);

    editorBody.css("color", elWrap.css("color"));
    editorBody.css("background-color", elWrap.css("background-color"));

    if (elWrap.hasClass("disabled")) {
        editorBody.removeAttr("contenteditable").find("a").on("click.readonly", false);
        elWrap.find(".k-editor-toolbar").hide();
    }
}

const refreshMessageCount = function () {
    const baseUrl = $("body").data("base-url");
    const url = `${baseUrl}/Account/GetMessageCount`;
    $.post(url)
        .done((result) => {
            $(".global-message-count").html(result.count > 0 ? result.count : "");
        })
        .fail((e => {
            pageHelper.showErrors(e);
        }));
}

const initializePage = function () {
    $(document).ready(() => {
        $(".banner #cpi-cultures").on("click", function (e) {
            $(this).siblings(".cultures").toggleClass("d-none");
        });
        $(".banner .cultures .culture").on("click", function () {
            const locale = $(this).data("locale");
            $(".banner .cultures").addClass("d-none");

            const baseUrl = $("body").data("base-url");
            let url = `${baseUrl}/Manage/UpdateCulture`;
            $.post(url, { locale: locale })
                .done(() => {
                    location.reload();
                })
                .fail(function (error) {
                    pageHelper.showErrors();
                    cpiLoadingSpinner.hide();
                });

        });
    });

}

const getSortDescriptor = function (dataSource) {
    const sort = dataSource.sort();
    const sortDescriptor = {};

    if (sort.length > 0) {
        sortDescriptor.Member = sort[0].field;
        sortDescriptor.SortDirection = sort[0].dir == "asc" ? 0 : 1;
    }

    return sortDescriptor;
}

const fetchReport = function (url, payload, verificationToken, downloadName) {
    const isFormData = payload instanceof FormData;
    const headers = {
        "Accept": "arraybuffer",
        "RequestVerificationToken": verificationToken
    }

    //do not set content-type header if payload is FormData
    if (!isFormData)
        headers["Content-Type"] = "application/json";

    cpiLoadingSpinner.show();
    
    //use fetch to get report and open new tab to display it
    //displaying in new tab does not work when using $.ajax
    //with fetch, get response as blob --> response.blob()
    //then turn blob to object url --> URL.createObjectURL(blobData)
    fetch(url, {
        method: "POST",
        headers: headers,
        body: isFormData ? payload : JSON.stringify(payload)
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

            if (downloadName) {
                const blobUrl = window.URL.createObjectURL(data);
                const a = document.createElement("a");
                document.body.appendChild(a);
                a.href = blobUrl;
                a.download = downloadName;
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            }
            else
                window.open(URL.createObjectURL(data));
        })
        .catch(error => {
            cpiLoadingSpinner.hide();
            if (error.status >= 500)
                pageHelper.showErrors("Unhandled error.");
            else
                error.text().then(errorMessage => {
                    pageHelper.showErrors(errorMessage);
                })
        });
}

//with date formatting
const gridExcelExport = (e) => {
    let sheet = e.workbook.sheets[0];
    let grid = e.sender;
    let fields = grid.dataSource.options.fields;
    let fieldsModels = grid.dataSource.options.schema.model.fields;
    //let columns = grid.columns;
    let dateCells = [];

    for (let i = 0; i < fields.length; i++) {
        let currentField = fields[i].field;
        let currentModel = fieldsModels[currentField];
        if (!fields[i].hidden && currentModel.type === "date") {
            var visibleColumns = fields.filter(f => !f.hidden);
            for (let j = 0; j < visibleColumns.length; j++) {
                if (currentField === visibleColumns[j].field) {
                    dateCells.push(j);
                    break;
                };
            };
        };
    };

    for (let rowIndex = 1; rowIndex < sheet.rows.length; rowIndex++) {
        let row = sheet.rows[rowIndex];
        for (let q = 0; q < dateCells.length; q++) {
            let cellIndex = dateCells[q];
            let value = row.cells[cellIndex].value;
            row.cells[cellIndex].value = value;
            row.cells[cellIndex].format = "dd-MMM-yyyy";
        };
    };

}

//get authentication token and pass it to callBack function
const callWithAuthToken = function (tokenUrl, username, callBack) {
    cpiLoadingSpinner.show();

    $.ajax({
        url: tokenUrl,
        data: { username: username, password: "_", grant_type: "password" },
        type: "POST",
        contentType: "application/x-www-form-urlencoded",
        success: function (result) {
            cpiLoadingSpinner.hide();
            callBack(result.access_token);
        },
        error: function (error) {
            cpiLoadingSpinner.hide();
            pageHelper.showErrors(error);
        }
    });
}

const comboBoxFiltering = function (ev, combo, columns) {
    const filterValue = ev.filter !== undefined ? ev.filter.value : "";
    ev.preventDefault();

    var filterColumns = columns.split(",").map((column) => {
        var filter = {
            "field": column,
            "operator": "contains",
            "value": filterValue
        };
        return filter;
    });

    combo.dataSource.filter({
        logic: "or",
        filters: filterColumns
    });
};

const onChange_Owner = (gridName, e) => {
    if (e.item) {

        const row = $(`#${e.sender.element[0].id}`).closest("tr");
        const ownerName = e.dataItem["OwnerName"];
        const grid = $(`#${gridName}`).data("kendoGrid");
        const dataItem = grid.dataItem(row);
        dataItem.OwnerID = e.dataItem["OwnerID"];
        dataItem.OwnerName = ownerName;
        $(row).find(".owner-name-field").html(ownerName);
    }
}

const onChange_Client = (gridName, e) => {
    if (e.item) {

        const row = $(`#${e.sender.element[0].id}`).closest("tr");
        const clientName = e.dataItem["ClientName"];
        const grid = $(`#${gridName}`).data("kendoGrid");
        const dataItem = grid.dataItem(row);
        dataItem.ClientID = e.dataItem["ClientID"];
        dataItem.ClientName = clientName;
        $(row).find(".client-name-field").html(clientName);
    }
}

const handleEmailWorkflow = (result) => {
    if (result.emailWorkflows && result.emailWorkflows.length > 0) {
        let id = result.id;
        if (result.emailWorkflows[0].id)
            id = result.emailWorkflows[0].id;

        let url = result.emailUrl;
        if (result.emailWorkflows[0].emailUrl)
            url = result.emailWorkflows[0].emailUrl;

        let promise = processEmailWorkflow(url, id, result.emailWorkflows[0].isAutoEmail, result.emailWorkflows[0].qeSetupId, result.emailWorkflows[0].autoAttachImages, result.emailWorkflows[0].fileNames, result.emailWorkflows[0].parentId, result.emailWorkflows[0].emailTo, result.emailWorkflows[0].strId,result.emailWorkflows[0].attachmentFilter);
        for (let i = 1; i < result.emailWorkflows.length; i++) {

            promise = promise.then(() => {
                const workflow = result.emailWorkflows[i];

                if (workflow.id)
                    id = workflow.id;

                if (workflow.emailUrl)
                    url = workflow.emailUrl;

                return processEmailWorkflow(url, id, workflow.isAutoEmail, workflow.qeSetupId, workflow.autoAttachImages, workflow.fileNames, workflow.parentId, workflow.emailTo, workflow.strId,workflow.attachmentFilter);
            });
        }
        return promise;
    }

}

const processEmailWorkflow = (url, id, isAutoEmail, qeSetupId, autoAttachImages, fileNames, imageParent, emailTo, strId, attachmentFilter) => {
    const deferred = $.Deferred();

    $.get(url, { id: id, sendImmediately: isAutoEmail, qeSetupId: qeSetupId, autoAttachImages: autoAttachImages, fileNames: fileNames.join('~'), imageParent: imageParent, emailTo: emailTo, strId: strId, attachmentFilter: attachmentFilter  })
        .done((emailResult) => {
            if (!isAutoEmail) {
                const popupContainer = $(".site-content .popup");
                popupContainer.html(emailResult);
                const dialog = $("#quickEmailDialog");
                dialog.modal("show");
                dialog.find("#ok, #cancel,.close").on("click", () => {
                    $(".modal-backdrop").remove();
                    deferred.resolve();
                });
            }
            else deferred.resolve();
        })
        .fail(function (error) {
            pageHelper.showErrors(error.responseText);
            deferred.resolve();
        });
    return deferred.promise();
}

const imageLoadRetry = function (img) {
    const image = $(img);
    const retryAttr = image.data("src-retry");
    let retry = +retryAttr;
    retry = retry - 1;

    if (retry > 0) {
        image.data("src-retry", retry);
        setTimeout(() => {
            const src = image.attr("src");
            image.attr("src", src);
        }, 1000);
    }
}

const reconnect = function () {
    const baseUrl = $("body").data("base-url");
    let url = `${baseUrl}/Home/Reconnect`;
    setInterval(() => {
        $.get(url);
    }, 1000 * 60 * 4); //reconnect every 4 mins.
}


const stopTimeTrack = function (e) {
    let url = $("#StopTimeTrack").data("url");
    $.ajax({
        type: "POST",
        url: url,
        contentType: false, // needed for file upload
        processData: false, // needed for file upload
        success: (result) => {
            pageHelper.showSuccess(result.success);
            document.getElementById("StopTimeTrack").setAttribute("hidden", "hidden");
            if (document.getElementById("StartTimeTrack") != null)
                document.getElementById("StartTimeTrack").removeAttribute("hidden");
        },
        error: function (e) {
            pageHelper.showErrors(e);
        }
    });
}

const showCountryAppLink = function (screen, title, isReadOnly) {
    const container = $(`#${screen}`).find(".cpiButtonsDetail");
    const pageNav = container.find(".nav");
    pageNav.prepend(`<a class="nav-link country-app-link" href="#" target="_self" title="${title}" role="button"><i class="fal fa-external-link pr-2"></i>${title}</a>`);
    container.find(".country-app-link").on("click", function () {
        if (isReadOnly) {
            $(`#${screen}`).find(".country-link").trigger("click");
        }
        else
            $(`#Country_${screen}_cpiButtonLink`).trigger("click");
    });
}

const showInventionLink = (screen, title, isReadOnly) => {
    const container = $(`#${screen}`).find(".cpiButtonsDetail");
    const pageNav = container.find(".nav");
    pageNav.prepend(`<a class="nav-link invention-link" href="#" target="_self" title="${title}" role="button"><i class="fal fa-external-link pr-2"></i>${title}</a>`);
    container.find(".invention-link").on("click", function () {
        if (isReadOnly) {
            $(`#${screen}`).find(".case-number-link").trigger("click");
        }
        else
            $(`#CaseNumber_${screen}_cpiButtonLink`).trigger("click");
    });
}

const showTmkLink = function (screen, title, isReadOnly) {
    const container = $(`#${screen}`).find(".cpiButtonsDetail");
    const pageNav = container.find(".nav");
    pageNav.prepend(`<a class="nav-link tmk-link" href="#" target="_self" title="${title}" role="button"><i class="fal fa-external-link pr-2"></i>${title}</a>`);
    container.find(".tmk-link").on("click", function () {
        if (isReadOnly) {
            $(`#${screen}`).find(".case-number-link").trigger("click");
        }
        else
            $(`#CaseNumber_${screen}_cpiButtonLink`).trigger("click");
    });
}

const showGMLink = function (screen, title, isReadOnly) {
    const container = $(`#${screen}`).find(".cpiButtonsDetail");
    const pageNav = container.find(".nav");
    pageNav.prepend(`<a class="nav-link gm-link" href="#" target="_self" title="${title}" role="button"><i class="fal fa-external-link pr-2"></i>${title}</a>`);
    container.find(".gm-link").on("click", function () {
        if (isReadOnly) {
            $(`#${screen}`).find(".case-number-link").trigger("click");
        }
        else
            $(`#CaseNumber_${screen}_cpiButtonLink`).trigger("click");
    });
}

const showFavorite = (screen, title, id, count, isFavorite, systemType, dataKey) => {
    const container = $(`#${screen}`).find(".cpiButtonsDetail");
    const pageNav = container.find(".nav");
    pageNav.append(`<a class="nav-link rec-favorite-link" href="#" title="${title}" role="button"><i class="${isFavorite ? "fad" : "fal"} fa-heart"></i><span class="pl-1 rec-count" style="font-size:10px;font-weight:bold">${count > 0 ? count : ''}</span></a>`);

    container.find(".rec-favorite-link").on("click", () => {

        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/MyFavorite/UpdateFavoriteCount`;
        $.get(url, { systemType, dataKey, id })
            .done((result) => {
                if (result.added)
                    container.find(".rec-favorite-link i").removeClass("fal").addClass("fad");
                else
                    container.find(".rec-favorite-link i").removeClass("fad").addClass("fal");
                container.find(".rec-favorite-link .rec-count").html(result.count > 0 ? result.count : '');

            })
            .fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });
    });
}

const handleSignatureWorkflow = (result,callBack) => {
    if (result.eSignatureWorkflows && result.eSignatureWorkflows.length > 0) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/DocuSign/SendEnvelopeFromFileUploadWorkflow`;

        cpiLoadingSpinner.show();
        $.post(url, { workflows: result.eSignatureWorkflows })
            .done((result) => {
                pageHelper.showSuccess(result.success);
                cpiLoadingSpinner.hide();
                if (callBack) callBack();
            })
            .fail(function (error) {
                if (callBack) callBack();
                cpiLoadingSpinner.hide();
                if (error.responseJSON) {
                    const jsonError = error.responseJSON;
                    pageHelper.showErrors(jsonError.errorMessage);
                    if (jsonError.consentRequired) {
                        console.log(jsonError.url);
                        window.open(jsonError.url);
                    }
                }
                else
                    pageHelper.showErrors(error);
            });
    }
    else {
        if (callBack) callBack();
    }
}

const pullDocuSignDoc = (e, grid,callBack) => {
    const row = $(e.target).closest("tr");
    const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);

    if (dataItem && dataItem.SentToDocuSign) {
        if (dataItem.SignatureCompleted) {
            const repullPrompt = $("#" + grid).closest(".image-container").data("repull-prompt");
            const title = $("#" + grid).closest(".image-container").data("confirm");

            cpiConfirm.confirm(title, repullPrompt, function () {
                pullCompletedDoc();
            });
        }
        else {
            pullCompletedDoc();
        }

        function pullCompletedDoc() {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Shared/DocuSign/GetSignedDocumentsAndSave`;

            cpiLoadingSpinner.show();
            $.post(url, {
                viewModelParam: {
                    DocName: dataItem.DocName,
                    SystemType: dataItem.SystemType,
                    ScreenCode: dataItem.ScreenCode,
                    ParentId: dataItem.ParentId,
                    FileId: dataItem.FileId,
                    EnvelopeId: dataItem.EnvelopeId,
                    DataKey: dataItem.DataKey,
                    SignedDoc: dataItem.SignedDoc
                }
            })
                .done((result) => {
                    $("#" + grid).data("kendoGrid").dataSource.read();
                    if (callBack) callBack();
                    cpiLoadingSpinner.hide();
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    if (error.responseJSON) {
                        const jsonError = error.responseJSON;
                        pageHelper.showErrors(jsonError.errorMessage);
                        if (jsonError.consentRequired) {
                            console.log(jsonError.url);
                            window.open(jsonError.url);
                        }
                    }
                    else
                        pageHelper.showErrors(error);
                });
        }
    }
}

const pushDocuSignDoc = (e, grid,callBack) => {
    const row = $(e.target).closest("tr");
    const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
    if (dataItem) {
        const title = $("#" + grid).closest(".image-container").data("confirm");
        if (dataItem.SentToDocuSign) {
            const resendPrompt = $("#" + grid).closest(".image-container").data("resend-prompt");
            cpiConfirm.confirm(title, resendPrompt, function () {
                pushDoc();
            });
        }
        else {
            const sendPrompt = $("#" + grid).closest(".image-container").data("send-prompt");
            cpiConfirm.confirm(title, sendPrompt, function () {
                pushDoc();
            });
        }

        function pushDoc() {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Shared/DocuSign/ResendEnvelopeFromFileUpload`;
            const workflow = {
                UserFile: {
                    FileName: dataItem.DocFileName,
                    FileId: dataItem.FileId,
                    Name: dataItem.DocName
                },
                QESetupId: dataItem.QESetupId,
                ParentId: dataItem.ParentId,
                ScreenCode: dataItem.ScreenCode,
                RoleLink: dataItem.RoleLink
            };

            cpiLoadingSpinner.show();
            $.post(url, {workflow})
                .done((result) => {
                    cpiLoadingSpinner.hide();
                    if (callBack) callBack();
                    $("#" + grid).data("kendoGrid").dataSource.read();
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    if (error.responseJSON) {
                        const jsonError = error.responseJSON;
                        pageHelper.showErrors(jsonError.errorMessage);
                        if (jsonError.consentRequired) {
                            console.log(jsonError.url);
                            window.open(jsonError.url);
                        }
                    }
                    else
                        pageHelper.showErrors(error);
                });
        }

    }
}

const formatFileSize = (bytes) => {
    const sizes = ["Bytes", "KB", "MB", "GB", "TB"];
    if (bytes == 0) return "0 Byte";
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${(bytes / Math.pow(1024, i)).toFixed(2)} ${sizes[i]}`;
}

const formatString = function() {
    let str = arguments[0];
    for (var i = 1; i < arguments.length; i++) {
        var reg = new RegExp("\\{" + (i - 1) + "\\}", "gm");
        str = str.replace(reg, arguments[i]);
    }
    return str;
}

const editorModified = function(e) {
    let editorText = e.sender.value();
    editorText = editorText.replace(/href="(javascript.*?)"/, 'href=#');

    e.sender.value(editorText);
}

export {
    moveBreadcrumbs, showSearchScreen, searchFormSubmit, manageDetailPage, showDetails, getDetails, openLink,
    initializeDetailTabs, deleteGridRow, emailGridRow, focusLabelControl, clearInvalidKendoDate, resetComboBoxes, initializeMainSearchTabs,
    initializeSidebar, initializeSidebarPage, manageDetailPageMainButtons, formDataToJson, hideErrors, addMaxLength, showSuccess, showErrors,
    populateForm, kendoGridSave, kendoGridIsDirty, kendoGridDirtyTracking, showActiveTab, cpiDateFormatToDisplay, cpiDateFormatToSave, errorsArrayToString,
    formDataToCriteriaList, appendPage, openDetailsLink, postJson, afterInsert, postData, setDelay,
    getPagedComboBoxValue, handleComboBoxInvalidEntry, refreshGridNameField, onComboBoxSelect, kendoGridDeleteRecord,
    displayNameFromComboBox, setWebLinksDefaultOption, setWebLinksOption, getWebLinksOption, getFormCriteria, onComboBoxChangeDisplayName, getKendoDataItemProperties,
    verificationTokenFormData, updateRecordStamps, setKendoListWidth, gridMainSearchFilters, placeholder,
    hint, onGridError, getErrorMessage, setupDragDropFiles, addBreadCrumbsRefreshHandler, sortableGridOnEdit, validateRequiredEntityFilterList,
    getSearchCriteriaScreen, loadSearchCriteria, loadDefaultSearchCriteria, onSearchCriteriaNameChange, kendoValueMapper, resetFormValidator,
    initKendoEditor, refreshMessageCount, getSortDescriptor, fetchReport, gridExcelExport, callWithAuthToken, initializePage, comboBoxFiltering,
    cpiDateTimeFormatToSave, onChange_Owner, onChange_Client, handleEmailWorkflow, imageLoadRetry, reconnect, stopTimeTrack, showCountryAppLink, showInventionLink, showTmkLink, showGMLink,
    isPageInTheDom, showFavorite, handleSignatureWorkflow, pullDocuSignDoc, pushDocuSignDoc, kendoGridGetChanges, getDataItemValues, kendoGridDirtyHandler,
    formatFileSize, formatString, editorModified

};


