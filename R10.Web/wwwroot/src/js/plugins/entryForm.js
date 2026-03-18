//manage submit/cancel of data entry screen (with dirty tracking, client side validation checking)
(function ($) {
    if (typeof $.fn.cpiEntryForm === "undefined") {
        $.fn.cpiEntryForm = function (options) {
            const plugin = this;

            let isParentDirty = false;
            const actionButtons = plugin.find("#editActionButtons");
            const detailContentContainer = $(`#${options.activePage.detailContentContainer}`);

            const getDirtyGridPosition = function () {
                const pos = options.activePage.editableGrids.findIndex(function (element) {
                    return element.isDirty;
                });
                return pos;
            };

            const refreshNodeDirtyFlag = function () {
                if (!isParentDirty) {
                    const pos = getDirtyGridPosition();
                    if (pos === -1) {
                        cpiBreadCrumbs.markLastNode({ dirty: false });
                        if (options.activePage.recordNavigator)
                            options.activePage.recordNavigator.show();
                    }
                }
            };

            //show save/cancel buttons, hide other buttons
            const setToSaveMode = function () {
                actionButtons.removeClass("d-none");
                options.activePage.mainControlButtons.hide();
            };

            const setToViewMode = function () {
                options.activePage.mainControlButtons.show();
                actionButtons.addClass("d-none");
                //pageHelper.hideErrors();
                refreshNodeDirtyFlag();
            };

            const markDirty = function () {
                if (isParentDirty)
                    return;

                isParentDirty = true;
                cpiBreadCrumbs.markLastNode({ dirty: true });
                options.activePage.recordNavigator.hide();
                setToSaveMode();
                
                detailContentContainer.addClass("dirty");
            };

            plugin.refreshGridDirtyStatus = function (grid, isDirty) {
                const tab = $(`#${$(grid).closest(".tab-pane").attr("aria-labelledby")}`);
                const tabContent = $(grid).closest(".tab-pane");
                console.log(isDirty, tab, tabContent);
                console.log(detailContentContainer);
                if (isDirty) {
                    cpiBreadCrumbs.markLastNode({ dirty: true });
                    options.activePage.recordNavigator.hide();
                    $(tab).addClass("dirty-grid");
                    $(tabContent).addClass("dirty-grid");
                }
                else {
                    refreshNodeDirtyFlag();

                    if ($(grid).closest(".tab-pane").find(".k-grid.dirty").length === 0) {
                        $(tab).removeClass("dirty-grid");
                        $(tabContent).removeClass("dirty-grid");
                    }
                }
                console.log(detailContentContainer.find(".k-grid.dirty"));

                if (detailContentContainer.find(".k-grid.dirty").length === 0) {
                    detailContentContainer.removeClass("dirty-grid");
                    detailContentContainer.disableControls(false);
                }
                else {
                    detailContentContainer.addClass("dirty-grid");
                    detailContentContainer.disableControls(true);
                }
            };

            $.validator.unobtrusive.parse(plugin); //attach jquery validator

            if (plugin.data("validator"))
                plugin.data("validator").settings.ignore = ""; //include hidden fields (kendo controls)

            pageHelper.addMaxLength(plugin);  //auto add maxlength to entry fields

            const onSubmit = function () {
                cpiLoadingSpinner.show();
                
                const json = pageHelper.formDataToJson(plugin);

                //workaround for error: Unable to perform operation because the record has been modified by another user.
                //one known cause is nested form inside detailForm
                //if tStamp is missing, it's possible that other fields may also be missing
                if (json.payLoad.tStamp == undefined) {
                    const recordStamp = plugin.closest(".cpiDataContainer").find(".content-stamp .record-stamp :input").serializeArray();
                    for (const stamp of recordStamp) {
                        json.payLoad[stamp.name] = stamp.value;
                    }
                }

                pageHelper.postJson(plugin.attr("action"), json)
                    .done(function (id) {
                        cpiLoadingSpinner.hide();
                        pageHelper.showSuccess(plugin.data("save-message"));

                        isParentDirty = false;
                        options.afterSubmit(id, options.afterSubmitOptions);
                        setToViewMode();

                        detailContentContainer.removeClass("dirty");
                    })
                    .fail(function (e) {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(e);

                        detailContentContainer.removeClass("dirty");
                    });
            };

            plugin.on("submit", function (e) {
                e.preventDefault();

                //client side validation (using jquery validation)
                if (plugin.valid()) {
                    if (options.beforeSubmit)
                        options.beforeSubmit(function () {
                            onSubmit();
                        });
                    else
                        onSubmit();
                }
                else {
                    cpiLoadingSpinner.hide();
                    plugin.wasValidated();
                }
            });

            

            //new layout: save, cancel, and delete click event handlers
            //save
            plugin.find(".save-changes").on("click", function () {
                plugin.submit();
            });
            //cancel
            plugin.find(".cancel-changes").on("click", function () {
                cpiConfirm.confirm(plugin.data("cancel-title"), plugin.data("cancel-message"), function () {
                    isParentDirty = false;

                    if (options.onCancel !== undefined) {
                        options.onCancel(options.onCancelOptions);
                    }
                    cpiStatusMessage.hide();

                    $(`#${options.activePage.detailContentContainer}`).removeClass("dirty");
                });
            });
            //delete confirmation
            const deleteConfirmation = function (title, content, url) {
                cpiConfirm.delete(title, content, function () {
                    const form = $(".modal-message .delete-confirm");
                    if (form.length > 0) {
                        $.validator.unobtrusive.parse(form);

                        if (!form.valid()) {
                            form.wasValidated();
                            throw "Delete confirmation failed.";
                        }
                    }

                    const activePage = options.activePage;
                    const data = {};
                    data.id = activePage.currentRecordId;
                    data.tStamp = $(`#${activePage.mainDetailContainer} #detailForm input[name=tStamp]`).val();
                    data.__RequestVerificationToken = $(`#${activePage.mainDetailContainer} #detailForm input[name=__RequestVerificationToken]`).val();

                    cpiLoadingSpinner.show();
                    $.post(url, data)
                        .done(function (response) {
                            cpiLoadingSpinner.hide();

                            if (response.emailWorkflows && response.emailWorkflows.length > 0) {
                                const promise = pageHelper.handleEmailWorkflow(response);
                                promise.then(() => {
                                    afterDelete();
                                });
                            }
                            else
                                afterDelete();

                            function afterDelete() {
                                const newIdToShow = activePage.recordNavigator.deleteRecordId(data.id);
                                if (newIdToShow !== null)
                                    activePage.showDetails(newIdToShow);
                                else {
                                    if (window.cpiBreadCrumbs.getNodes().length > 1)
                                        window.cpiBreadCrumbs.showPreviousNode();
                                    else
                                        pageHelper.showSearchScreen(activePage.searchUrl);
                                }
                                pageHelper.showSuccess(plugin.data("delete-success"));
                            }
                            
                        })
                        .fail(function (error) {
                            cpiLoadingSpinner.hide();
                            pageHelper.showErrors(error);
                        });

                });
            };
            //delete
            plugin.find(".delete-record").on("click", function () {
                const action = $(this);
                const title = plugin.data("delete-title");
                let content = plugin.data("delete-message");
                const url = action.data("url");

                const confirmationUrl = action.data("confirm-url") || "";

                if (confirmationUrl !== "") {
                    cpiLoadingSpinner.show();
                    $.get(confirmationUrl)
                        .done(function (result) {
                            cpiLoadingSpinner.hide();
                            content = `<div class="row message-wrap"><div class="col-2 text-center pl-md-4 pt-1"><i class="text-danger far fa-exclamation-triangle fa-2x"></i></div><div class="col-10"><p>${plugin.data("delete-message")}</p></div></div>${result}`;
                            deleteConfirmation(title, content, url);
                        })
                        .fail(function (e) {
                            cpiLoadingSpinner.hide();
                            pageHelper.showErrors(plugin.data("error-message") || "An error occurred. No updates were made.");
                        });
                }
                else {
                    deleteConfirmation(title, content, url);
                }
                
            });

            //print confirmation
            const printConfirmation = function (title, content, url) {
                cpiPrintConfirm.print(title, content, function () {
                    const printScreenOption = $("#PrintScreenOption");
                    const IDs = $("#IDs");
                    const reportFormatOption = $("#ReportFormatOption");
                    const reportFormat = $("#ReportFormat");
                    const downloadName = document.getElementById("DownloadName").value;

                    reportFormat[0].value = reportFormatOption[0].value;

                    const searchRecordIDs = options.activePage.mainSearchRecordIds;

                    if (printScreenOption[0].checked) {
                        var tempIDsList = "|";
                        for (var i = 0; i < searchRecordIDs.length; i++) {
                            tempIDsList += searchRecordIDs[i] + "|";
                        }
                        IDs[0].value = tempIDsList;
                    }
                    else {
                        IDs[0].value = "|" + options.activePage.currentRecordId+"|";
                    }

                    cpiLoadingSpinner.show();

                    const criteriaForm = $("#ReportCriteriaForm");

                    const url = criteriaForm.data("url")
                    const json = pageHelper.formDataToJson(criteriaForm);

                    //use fetch to get report and open new tab to display it
                    //displaying in new tab does not work when using $.ajax
                    //with fetch, get response as blob --> response.blob()
                    //then turn blob to object url --> URL.createObjectURL(blobData)
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
                            pageHelper.showSuccess("Printed successfully.");
                            if (reportFormat.value == 4) {
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
                                    pageHelper.showErrors(errorMessage);
                                })
                        });

                });
            };

            //print
            plugin.find(".print-record").on("click", function () {
                const action = $(this);
                const title = plugin.data("print-title");
                let content = plugin.data("print-message");
                const url = action.data("url");
                $("#PrintScreenOption").prop('checked', false);
                //$("#PrintScreenOption").val("false");
                if ($("#ReportFormatOption").data("kendoDropDownList")) {
                    $("#ReportFormatOption").data("kendoDropDownList").value(0);
                    $("#ReportFormatOption").data("kendoDropDownList").text("PDF");
                }
                    //$("#ReportFormatOption").data("kendoComboBox").refresh();
                //$("#ReportFormatOption").val(0);

                const confirmationUrl = action.data("confirm-url") || "";

                if (confirmationUrl !== "") {
                    cpiLoadingSpinner.show();
                    $.get(confirmationUrl)
                        .done(function (result) {
                            cpiLoadingSpinner.hide();
                            content = result;
                            printConfirmation(title, content, url);
                        })
                        .fail(function (e) {
                            cpiLoadingSpinner.hide();
                            pageHelper.showErrors(plugin.data("error-message") || "An error occurred. No updates were made.");
                        });
                }
                else {
                    printConfirmation(title, content, url);
                }

            });

            //mark form as dirty when user change something
            plugin.on("input", ".cpiMainEntry input, .cpiMainEntry textarea", function () {
                if ($(this).data("nosave") === undefined) {
                    markDirty();
                }
            });

            //track changes in tab content textareas (e.g., Law Highlights Remarks)
            plugin.on("input", ".tab-pane textarea", function () {
                if ($(this).data("nosave") === undefined) {
                    markDirty();
                }
            });

            //for checkboxes on ie/edge
            plugin.on("change", ".cpiMainEntry input[type='checkbox']", function () {
                markDirty();
            });

            //validate combobox text input when list is not loaded
            const onComboFocusOut = function (comboBox) {
                const el = $(comboBox.element);
                const text = el.parent().find("input.k-input-inner").val();

                if (text && comboBox.selectedIndex === -1) {
                    cpiLoadingSpinner.show();
                    comboBox.dataSource.read().always(() => {
                        cpiLoadingSpinner.hide();
                        comboBox.text(text);
                        comboBox.trigger("select");

                        if (el.data("limit-to-list") && comboBox.selectedIndex === -1) {
                            comboBox._clear.click();
                        }
                        //else {
                        //    el.trigger("change");
                        //    comboBox.trigger("change");
                        //}
                    });
                }
            };

            plugin.find(".cpiMainEntry .k-combobox > input").each(function () {
                const comboBox = $(this).data("kendoComboBox");
                if (comboBox) {
                    comboBox.bind("change", function () {
                        markDirty();
                    });

                    $(this).on("focusout", function (e) {
                        onComboFocusOut(comboBox);
                    });
                }
            });

            plugin.find(".cpiMainEntry .k-multiselect > select").each(function () {
                const multiSelect = $(this).data("kendoMultiSelect");
                if (multiSelect) {
                    multiSelect.bind("change", function (e) {
                        if (!e.sender._initialOpen || e.sender._initialValues != e.sender._old)
                           markDirty();
                    });
                }
            });

            plugin.find(".cpiMainEntry input[data-role='multicolumncombobox']").each(function () {
                const comboBox = $(this).data("kendoMultiColumnComboBox");
                if (comboBox) {
                    comboBox.bind("change", function () {
                        markDirty();
                    });

                    $(this).on("focusout", function (e) {
                        onComboFocusOut(comboBox);
                    });
                }
            });

            plugin.find(".cpiMainEntry .k-dropdown > input").each(function () {
                const dropdownList = $(this).data("kendoDropDownList");
                if (dropdownList) {
                    dropdownList.bind("change", function () {
                        markDirty();
                    });
                }
            });

            plugin.find(".cpiMainEntry .k-datepicker input").each(function () {
                const datePicker = $(this).data("kendoDatePicker");
                if (datePicker) {
                    datePicker.bind("change", function () {
                        markDirty();
                    });
                }
            });

            plugin.find(".cpiMainEntry .k-datetimepicker input").each(function () {
                const dateTimePicker = $(this).data("kendoDateTimePicker");
                if (dateTimePicker) {
                    dateTimePicker.bind("change", function () {
                        markDirty();
                    });
                }
            });

            plugin.find(".cpiMainEntry .k-numerictextbox input").each(function () {
                const numericTextBox = $(this).data("kendoNumericTextBox");
                if (numericTextBox) {
                    numericTextBox.bind("spin", function () {
                        markDirty();
                    });
                }
            });

            plugin.find(".cpiMainEntry .k-editable-area > textarea").each(function () {
                var editor = $(this).data("kendoEditor");
                if (editor) {
                    $(editor.body).bind("input", function (e) {
                        markDirty();
                    });
                    $(editor.body).bind("keyup", function (e) {
                        //delete key
                        if (e.keyCode === 46) {
                            markDirty();
                        }
                    });
                }
            });

            plugin.bind("markDirty", function () {
                markDirty();
            });

            options.activePage.editableGrids.forEach(function (el) {
                const grid = $(`#${el.name}`);
                const afterSave = () => {
                    plugin.refreshGridDirtyStatus(grid, false);
                    if (el.onSave)
                        el.onSave();
                };
                const afterCancel = () => {
                    plugin.refreshGridDirtyStatus(grid, false);
                    cpiStatusMessage.hide();
                    if (el.onCancel)
                        el.onCancel();
                };
                const onDirty = () => {
                    plugin.refreshGridDirtyStatus(grid, true);
                    if (el.onDirty)
                       el.onDirty();
                };
                pageHelper.kendoGridDirtyTracking(grid, el, afterSave, afterCancel, onDirty);
            });
        
            pageHelper.clearInvalidKendoDate(plugin);
            pageHelper.focusLabelControl(plugin);

            return this;

        };
    }
}
)(jQuery);