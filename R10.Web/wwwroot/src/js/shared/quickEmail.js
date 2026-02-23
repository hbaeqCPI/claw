
export default class QuickEmail {

    initialize = () => {
        this.selectedImagePages = [];
        this.setupMoreInfo();
        this.setupEntryForm();
        this.setupMailTo();
        this.setupImages();
        this.templateName = "";
        this.setDefaultTemplateName = false;

        //const dialogContainer = $("#quickEmailDialog");
        //dialogContainer.on('click', ".qe-search-submit", function () {
        //    const gridId = "#qeListGrid";
        //    const gridHandle = $(gridId);
        //    const grid = gridHandle.data("kendoGrid");
        //    grid.dataSource.read();
        //})
    }

    initializeLogViewer = () => {
        const dialogContainer = $("#quickEmailLogDialog");
        dialogContainer.modal("show");

        const listView = $("#qeAttachmentListView").data("kendoListView");

        if (listView) {
            listView.dataSource.read();
        }

        $("#qeAttachmentsGalleryContainer").show();
        $("#qeAttachmentsGridContainer").hide();

        const qeAttachmentsViewOptionContainer = $("#qeAttachmentsViewOptionContainer");
        qeAttachmentsViewOptionContainer.find("input:radio").on("click", function (e) {
            if (e.currentTarget.id === "listView") {
                const listView = $("#qeAttachmentListView").data("kendoListView");
                listView.dataSource.read();

                $("#qeAttachmentsGalleryContainer").show();
                $("#qeAttachmentsGridContainer").hide();
            }
            else {
                const grid = $("#qeAttachmentsGrid").data("kendoGrid");
                grid.dataSource.read();

                $("#qeAttachmentsGridContainer").show();
                $("#qeAttachmentsGalleryContainer").hide();
            }
        });


    }

    setupMoreInfo() {
        this.quickEmailForm = $("#quickEmailForm");
        this.quickEmailForm.find("#downIcon").show();
        this.quickEmailForm.find("#upIcon").hide();
        this.quickEmailForm.find("#moreContainer").hide();

        this.quickEmailForm.find("#moreButton").on("click",
            () => {
                this.quickEmailForm.find("#downIcon").toggle();
                this.quickEmailForm.find("#upIcon").toggle();
                this.quickEmailForm.find("#moreContainer").toggle();
            });
    }

    setupImages() {
        const grid = this.quickEmailForm.find("#qeImageLinkGrid").data("kendoGrid");
        if (grid) {
            this.quickEmailForm.find("#qeImageLinkGrid").on("change", "input.chkbox", function (e) {
                const dataItem = grid.dataItem($(e.target).closest("tr"));
                dataItem.set("Send", this.checked);
                $(e.target).closest("td").removeClass("k-dirty-cell");
            });
        }
    }

    setupEntryForm() {
        const dialogContainer = $("#quickEmailDialog");
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

                const images = this.getSelectedImages();
                formData.append("Images", images);

                const attachments = this.getAttachments();  //qe log
                formData.append("Attachments", attachments);

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
                            cpiLoadingSpinner.hide();
                            if (!result.Success) {
                                pageHelper.showErrors(result.ErrorMessage);
                            }
                            else {
                                pageHelper.showSuccess(entryForm.data("success"));
                                dialogContainer.modal("hide");
                                this.refreshDocsOutGrid(entryForm);
                            }
                        },
                        error: function (e) {
                            cpiLoadingSpinner.hide();
                            pageHelper.showErrors(e);
                        }
                    });

                } else {
                    entryForm.wasValidated();
                    $("#qeMainTabs").find(".nav-link").removeClass("active");
                    $("#qeMainTabsContent").find(".tab-pane").removeClass("active");
                    $("#eqRecipientTab").addClass("active");
                    $("#eqRecipientTabContent").addClass("active");
                    $("#eqRecipientTabContent").addClass("show");

                }
            });
    }

    editorModified = (e) => {
        let editorText = e.sender.value();
        editorText = editorText.replace(/href="(javascript.*?)"/, 'href=#');

        e.sender.value(editorText);
    }

    setupMailTo() {
        const dialogContainer = $("#quickEmailDialog");
        let entryForm = dialogContainer.find("form")[0];
        entryForm = $(entryForm);

        //entryForm.find("#open").on("click", function () {
        //    let subject = entryForm.find("#Subject").val();
        //    subject = subject.replaceAll("&", " ");

        //    const to = entryForm.find("#To").val();
        //    let cc = entryForm.find("#CopyTo").val();
        //    //let body = entryForm.find("#Body").data("kendoEditor").value();
        //    let body = entryForm.find("#BodyMailTo").val();

        //    console.log(body);

        //    const hasNonAscii = entryForm.find("#HasNonAscii").val().toLowerCase()==="true";
        //    if (hasNonAscii) {
        //        body = body.replaceAll("<br />", encodeURI("\r\n"));
        //        body = body.replaceAll("</tr>", "</tr>" + encodeURI("\r\n"));
        //    }
        //    else {
        //        body = body.replaceAll("<br />", "\r\n");
        //        body = body.replaceAll("</tr>", "</tr>\r\n");
        //    }

        //    body = body.replaceAll("</td>", "</td>  ");
        //    body = body.replace(/(<br([^>]+)>)/gi, "\r\n");
        //    body = body.replace(/(<([^>]+)>)/gi, "");
        //    body = body.replaceAll("&nbsp;", " ");
        //    body = body.replaceAll("&amp;", " ");

        //    console.log(body);

        //    if (cc === null || cc === "")
        //        cc = ";";

        //    let mailTo = "";
        //    if (hasNonAscii) {
        //        mailTo = `${to}?cc=${cc}&subject=${subject}`;
        //        mailTo = encodeURI(mailTo);
        //        mailTo = `${mailTo}&body=${body}`;
        //        console.log("nonascii");
        //    }
        //    else {
        //        mailTo = `${to}?cc=${cc}&subject=${subject}&body=${body}`;
        //        mailTo = encodeURI(mailTo);
        //    }
        //    mailTo = mailTo.substring(0, 2000);

        //    window.location.href = `mailto:${mailTo}`;
        //});

        entryForm.find("#open").on("click", function () {
            let subject = entryForm.find("#Subject").val();
            subject = subject.replaceAll("&", " ");

            const to = entryForm.find("#To").val();
            let cc = entryForm.find("#CopyTo").val();
            let body = entryForm.find("#Body").data("kendoEditor").value();

            console.log(body);
            body = body.replaceAll("<br />", "\r\n");
            body = body.replaceAll("</tr>", "</tr>\r\n");
            body = body.replaceAll("</td>", "</td>  ");

            body = body.replace(/(<([^>]+)>)/gi, "");
            body = body.replaceAll("&nbsp;", " ");
            body = body.replaceAll("&amp;", "and"); //& will break the mailto url

            //body = body.replaceAll("&Aring;", "Å");
            //body = body.replaceAll("&AElig;", "Æ");

            console.log(body);

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Shared/QuickEmail/DecodeHtml`;

            $.post(url, { html: body })
                .done(function (result) {
                    body = result;

                    if (cc === null || cc === "")
                        cc = ";";

                    let mailTo = `${to}?cc=${cc}&subject=${subject}&body=${body}`;
                    mailTo = encodeURI(mailTo);
                    mailTo = mailTo.substring(0, 2000);
                    window.location.href = `mailto:${mailTo}`;
                })
                .fail(function (e) {
                    pageHelper.showErrors(e.responseText);
                });

        });

        entryForm.find("#download").on("click", function () {
            const baseUrl = $("body").data("base-url");
            const form = dialogContainer.find("form#quickEmailForm")[0];
            const formData = new FormData(form);

            pageHelper.fetchReport(`${baseUrl}/Shared/QuickEmail/Download`, formData, "", "");
        });

    }

    refreshDocsOutGrid = (entryForm) => {
        //const container = entryForm.closest(".cpiDataContainer");
        const container = $("div").find(".cpiDataContainer");
        if (container) {
            const id = container.attr("id").split("-")[0];
            const grid = $(`#docsOutGrid_${id}`);
            if (grid.length > 0) {
                grid.data("kendoGrid").dataSource.read();
            }

        }
    }

    getSelectedImages() {
        const grid = this.quickEmailForm.find("#qeImageLinkGrid").data("kendoGrid");
        if (grid) {
            const selectedImages = grid.dataSource.data().filter(i => i.Send);
            const images = selectedImages.map(i => {
                const image = { FileId: i.FileId, FileName: i.FilePath, Thumbnail: i.ThumbnailFile, FileTitle: i.ImageTitle, SharePointDocLibrary: i.SharePointDocLibrary, ItemId: i.ItemId };
                return image;
            });
            return JSON.stringify(images);
        }
        else
            return JSON.stringify([]);
    }

    getAttachments() {
        const grid = this.quickEmailForm.find("#qeAttachmentsGrid").data("kendoGrid");
        if (grid) {
            const selectedImages = grid.dataSource.data().filter(i => i.Send);
            const images = selectedImages.map(i => {
                const image = { FileId: i.FileId, FileName: i.FileName, Thumbnail: i.Thumbnail, FileTitle: i.FileTitle, ItemId: i.ItemId, SharePointDocLibrary: i.SharePointDocLibrary };
                return image;
            });
            return JSON.stringify(images);
        }
        else
            return JSON.stringify([]);
    }

    getKendoComboBox(name) {
        const selector = "input[name=" + name + "]";
        const el = $("#quickEmailForm").find(selector);
        const comboBox = el.data('kendoComboBox');
        return comboBox;
    }

    getElement(name) {
        const selector = "input[name=" + name + "]";
        const el = $("#quickEmailForm").find(selector);
        return el;
    }

    getTextArea(name) {
        const selector = "textarea[name=" + name + "]";
        const el = $("#quickEmailForm").find(selector);
        return el;
    }

    refreshGrid(name, param) {
        const el = $("#" + name);
        const grid = el.data("kendoGrid");
        if (grid) {
            grid.dataSource.read(param);
        }
    }

    refreshGridbyId(gridId) {
        const gridHandle = $(gridId);
        const grid = gridHandle.data("kendoGrid");
        grid.dataSource.read();

    }

    refreshAddresseeGrid(name, template) {
        const param = { qeSetupId: template.QESetupID, parentId: template.ParentId, parentTable: template.ParentTable };
        this.refreshGrid(name, param);
    }

    refreshImageGrid(name, template) {
        const param = { dataSourceId: template.DataSourceID, parentId: template.ParentId };
        this.refreshGrid(name, param);
    }

    populateForm(form, values) {
        const unEscapeHtml = function (escapedHtml) {
            if (escapedHtml !== null)
                return escapedHtml.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&amp;/g, '&');
        };

        $.each(values, function (key, value) {
            const input = form.find("input[name='" + key + "']");
            if (input.length > 0) {
                const element = $(input);

                if (element.data("role") === "datepicker") {
                    element.data("kendoDatePicker").value(value);
                }
                else if (element.data("role") === "combobox") {
                    if (value > "")
                        element.data("kendoComboBox").value(value);
                    else
                        element.data("kendoComboBox").text(value);
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
                else if (input[0].type === "checkbox") {
                    input[0].checked = value;

                }
                else {
                    if (input[0].readOnly === false)
                        element.val(value);
                }
            }
            else {
                const textArea = form.find("textarea[name='" + key + "']")[0];
                if (textArea) {
                    if (textArea.getAttribute("data-role") === "editor") {
                        if (value !== null) {
                            const newValue = unEscapeHtml(value);
                            $(textArea).data("kendoEditor").value(newValue);
                        }
                        else
                            $(textArea).data("kendoEditor").value(value);
                    } else
                        $(textArea).val(value);
                }
            }
        });

    }

    onChange_Template = (e) => {
        //const comboBox = this.getKendoComboBox("TemplateName");

        //if (comboBox) {
        //    if (this.templateName !== comboBox.value()) {
        //        this.templateName = comboBox.value();
        //        this.change(comboBox.value());
        //    }

        //}

        var grid = $("#qeListGrid").data("kendoGrid");
        var selectedItem = grid.dataItem(grid.select());

        if (selectedItem) {
            this.change(selectedItem.QESetupId);
        }

    }

    change(id) {
        const form = $("#quickEmailDialog");
        const url = form.find("#QESetupID").attr("data-url");

        const param = {
            qeSetupId: id,
            systemType: form.find("#SystemType").val(),
            parentKey: form.find("#ParentKey").val(),
            parentId: form.find("#ParentId").val(),
            parentTable: form.find("#ParentTable").val(),
            screenName: form.find("#ScreenName").val(),
            roleLink: form.find("#RoleLink").val(),
            docLibrary: form.find("#SharePointDocLibrary").val(),
            docLibraryFolder: form.find("#SharePointDocLibraryFolder").val(),
            recKey: form.find("#SharePointRecKey").val(),
            screenCode: form.find("#ScreenCode").val()
        };

        $.get(url, $.param(param))
            .done((result) => {
                this.populateForm($("#quickEmailForm"), result);
                this.refreshAddresseeGrid("qeRecipientGrid", result);
                this.refreshImageGrid("qeImageLinkGrid", result);
            })
            .fail(function (jqXHR, textStatus, error) {
                if (jqXHR.responseText > "") {
                    error = jqXHR.responseText;
                }

                if (error !== "Your search did not match any records.") {
                    pageHelper.showErrors(error);
                }
            });

    }

    onDataBound_Addressee() {
        const grid = $("#qeRecipientGrid").data("kendoGrid");
        const data = grid.dataSource.data();
        $.each(data,
            function (i, row) {
                if (row.IsDefault && row.EntityEmail !== null) {
                    $('tr[data-uid="' + row.uid + '"] ').addClass("text-info");
                }
            });
    }

    onChange_Addressee = () => {

        const highlightRow = function (email) {
            const grid = $("#qeRecipientGrid").data("kendoGrid");
            const data = grid.dataSource.data();
            $.each(data,
                function (i, row) {
                    if (row.EntityEmail === email) {
                        $('tr[data-uid="' + row.uid + '"] ').addClass("text-info");
                    }
                });
        };

        const removeHighlight = function (email) {
            const grid = $("#qeRecipientGrid").data("kendoGrid");
            const data = grid.dataSource.data();
            $.each(data,
                function (i, row) {
                    if (row.EntityEmail === email) {
                        $('tr[data-uid="' + row.uid + '"] ').removeClass("text-info");
                    }
                });
        };

        const addRecipient = (name, email) => {

            const separator = ";";
            const el = this.getTextArea(name);
            if (el) {
                const recipient = el.val().trim();
                const emailAndSeparator = email + separator;

                if (recipient === "") {
                    el.val(emailAndSeparator);
                    highlightRow(email);
                }
                else {
                    // add email address if not exist yet
                    if (!recipient.includes(email)) {
                        const lastChar = recipient.charAt(recipient.length - 1);
                        if (lastChar === separator) {
                            el.val(recipient + " " + emailAndSeparator);
                        } else {
                            el.val(recipient + separator + " " + emailAndSeparator);
                        }
                        highlightRow(email);
                    } else {
                        el.val(recipient.replace(emailAndSeparator, "").trim());
                        removeHighlight(email);
                    }
                }
            }
        };

        const grid = $("#qeRecipientGrid").data("kendoGrid");
        if (grid) {
            const selectedItem = grid.dataItem(grid.select());
            if (selectedItem) {
                if (selectedItem.EntityEmail !== null) {
                    const sendAs = selectedItem.SendAs.replace("Copy to", "CopyTo");
                    addRecipient(sendAs, selectedItem.EntityEmail);
                }
            }
        }
    }

    initializePopup = () => {
        this.selectedImagePages = [];
        this.templateName = "";
        this.setDefaultTemplateName = false;

        //setupMoreInfo
        this.quickEmailForm = $("#quickEmailForm");
        this.quickEmailForm.find("#downIcon").show();
        this.quickEmailForm.find("#upIcon").hide();
        this.quickEmailForm.find("#moreContainer").hide();

        this.quickEmailForm.find("#moreButton").on("click", () => {
            this.quickEmailForm.find("#downIcon").toggle();
            this.quickEmailForm.find("#upIcon").toggle();
            this.quickEmailForm.find("#moreContainer").toggle();
        });

        //setupEntryForm
        let entryForm = $("#quickEmailForm");
        $.validator.unobtrusive.parse(entryForm);
        entryForm.data("validator").settings.ignore = "";

        pageHelper.addMaxLength(entryForm);
        pageHelper.clearInvalidKendoDate(entryForm);
        pageHelper.focusLabelControl(entryForm);

        const editor = $("#Body").data("kendoEditor");
        editor.refresh();

        var qePopupSummary = entryForm.find("#qePopupSummary");
        qePopupSummary.find("button.close").on("click", function () {
            $(qePopupSummary).slideUp();
        });

        entryForm.on("submit",
            (e) => {
                e.preventDefault();

                const formData = new FormData(entryForm[0]);

                const images = this.getSelectedImages();
                formData.append("Images", images);

                const attachments = this.getAttachments();  //qe log
                formData.append("Attachments", attachments);

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
                            cpiLoadingSpinner.hide();
                            if (!result.Success) {
                                //pageHelper.showErrors(result.ErrorMessage);
                                $(qePopupSummary).addClass("alert-danger");
                                $(qePopupSummary).removeClass("alert-success");
                                qePopupSummary.find("span.message").html(result.ErrorMessage);
                                $(qePopupSummary).show();
                            }
                            else {
                                //pageHelper.showSuccess(entryForm.data("success"));
                                $(qePopupSummary).removeClass("alert-danger");
                                $(qePopupSummary).addClass("alert-success");
                                qePopupSummary.find("span.message").html(entryForm.data("success"));
                                $(qePopupSummary).show(function () {
                                    $(qePopupSummary).delay(5000).slideUp();
                                });
                            }
                        },
                        error: function (e) {
                            cpiLoadingSpinner.hide();
                            //pageHelper.showErrors(e);
                            $(qePopupSummary).addClass("alert-danger");
                            $(qePopupSummary).removeClass("alert-success");
                            qePopupSummary.find("span.message").html(pageHelper.getErrorMessage(e));
                            $(qePopupSummary).show();
                        }
                    });

                } else {
                    entryForm.wasValidated();
                    $("#qeMainTabs").find(".nav-link").removeClass("active");
                    $("#qeMainTabsContent").find(".tab-pane").removeClass("active");
                    $("#eqRecipientTab").addClass("active");
                    $("#eqRecipientTabContent").addClass("active");
                    $("#eqRecipientTabContent").addClass("show");

                }
            });

        //setupMailTo        
        entryForm.find("#open").on("click", function () {
            let subject = entryForm.find("#Subject").val();
            subject = subject.replaceAll("&", " ");

            const to = entryForm.find("#To").val();
            let cc = entryForm.find("#CopyTo").val();
            let body = entryForm.find("#Body").data("kendoEditor").value();

            console.log(body);
            body = body.replaceAll("<br />", "\r\n");
            body = body.replaceAll("</tr>", "</tr>\r\n");
            body = body.replaceAll("</td>", "</td>  ");

            body = body.replace(/(<([^>]+)>)/gi, "");
            body = body.replaceAll("&nbsp;", " ");
            body = body.replaceAll("&amp;", "and"); //& will break the mailto url

            //body = body.replaceAll("&Aring;", "Å");
            //body = body.replaceAll("&AElig;", "Æ");

            console.log(body);

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Shared/QuickEmail/DecodeHtml`;

            $.post(url, { html: body })
                .done(function (result) {
                    body = result;

                    if (cc === null || cc === "")
                        cc = ";";

                    let mailTo = `${to}?cc=${cc}&subject=${subject}&body=${body}`;
                    mailTo = encodeURI(mailTo);
                    mailTo = mailTo.substring(0, 2000);
                    window.location.href = `mailto:${mailTo}`;
                })
                .fail(function (e) {
                    pageHelper.showErrors(e.responseText);
                });

        });

        entryForm.find("#download").on("click", function () {
            const baseUrl = $("body").data("base-url");
            const formData = new FormData(entryForm[0]);
            pageHelper.fetchReport(`${baseUrl}/Shared/QuickEmail/Download`, formData, "", "");
        });

        //setupImages
        const grid = this.quickEmailForm.find("#qeImageLinkGrid").data("kendoGrid");
        if (grid) {
            this.quickEmailForm.find("#qeImageLinkGrid").on("change", "input.chkbox", function (e) {
                const dataItem = grid.dataItem($(e.target).closest("tr"));
                dataItem.set("Send", this.checked);
                $(e.target).closest("td").removeClass("k-dirty-cell");
            });
        }
    }

    searchTemplate = () => this.refreshGridbyId("#qeListGrid");

    getSearchedQE = () => {
        const form = $("#quickEmailDialog");
        return {
            systemType: form.find("#SystemType").val(),
            screenCode: form.find("#ScreenCode").val(),
            templateName: form.find("input[name = 'TemplateName_Search']").data("kendoComboBox").text(),
            qeCatId: $("#quickEmailDialog [name='QECat']").data("kendoComboBox").value(),
            tags: $("#quickEmailDialog [name='Tag']").val()
        }
    };

    qeListGridDataBound = (templateName, e) => {
        if (this.setDefaultTemplateName != true) {
            const grid = $("#qeListGrid").data("kendoGrid");

            var selectedItem = grid.dataItem(grid.select());
            if (selectedItem) {
                this.setDefaultTemplateName = true;
                return;
            }

            var rows = grid.items();
            if (rows) {
                $(rows).each(function (e) {
                    var row = this;
                    var dataItem = grid.dataItem(row);

                    if (dataItem.TemplateName == templateName) {
                        grid.select(row);
                        this.setDefaultTemplateName = true;
                        return;
                    }

                });
            }
        }
    };

    divFadeOut = () => {
        $(".search").slideUp(300, function () {
            $("#expandSearch").fadeIn(200);
        });
    };
}



