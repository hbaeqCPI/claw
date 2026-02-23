class IDSLib {

    caseNumberDetailValueMapper = (options) => {
        const url = $("#patIDSRelatedEntryDialog").data("case-number-mapper-url");

        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    openIDSRefEntry(grid, url, data, closeOnSave) {
        $.ajax({
            url: url,
            data: data,
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#patIDSRelatedEntryDialog");
                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
        
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: closeOnSave,
                        beforeSubmit: function () { idsLib.transferIDSRefKendoValues(entryForm); },
                        afterSubmit: function () {
                            grid.dataSource.read();
                            dialogContainer.modal("hide");
                        }
                    }
                );
                idsLib.rtsInpadocIDSSetListener(entryForm);
                idsLib.setIDSCountryCombo(entryForm);
            },
            error: function (error) {
                pageHelper.showErrors(error.responseText);
            }
        });
    }

    setIDSCountryCombo(entryForm) {
        const relatedAppId = $(entryForm.find("#RelatedAppId")).val();
        if (relatedAppId > 0) {
            idsLib.toggleIDSRelatedInfo(entryForm, false);
        }
    }

    toggleIDSRelatedInfo(entryForm, enable) {

        if (enable) {
            entryForm.find("#RelatedPubNumber,#RelatedPatNumber").removeAttr("readonly");
            $("#patIDS_RelatedCountryS").show();
            $("#patIDS_RelatedCountryR").hide();
        } else {
            entryForm.find("#RelatedPubNumber,#RelatedPatNumber").attr("readonly", "readonly");
            $("#patIDS_RelatedCountryS").hide();
            $("#patIDS_RelatedCountryR").show();

        }
        entryForm.find("input[name = 'RelatedPubDateR']").data("kendoDatePicker").enable(enable);
        entryForm.find("input[name = 'RelatedIssDateR']").data("kendoDatePicker").enable(enable);
        entryForm.find("input[name = 'RelatedFirstNamedInventorR']").data("kendoComboBox").enable(enable);
        
    }

    //disabled kendo control will not get posted
    transferIDSRefKendoValues(entryForm) {
        let pubDate = entryForm.find("input[name = 'RelatedPubDateR']").data("kendoDatePicker").value();
        let issDate = entryForm.find("input[name = 'RelatedIssDateR']").data("kendoDatePicker").value();
        const inventor = entryForm.find("input[name = 'RelatedFirstNamedInventorR']").data("kendoComboBox").text();

        if (pubDate !== null) {
            pubDate = (new Date(pubDate.getFullYear(), pubDate.getMonth(), pubDate.getDate())).toISOString().split("T")[0] + "T00:00:00";
        }
        if (issDate !== null) {
            issDate = (new Date(issDate.getFullYear(), issDate.getMonth(), issDate.getDate())).toISOString().split("T")[0] + "T00:00:00";
        }
        entryForm.find("#RelatedPubDate").val(pubDate);
        entryForm.find("#RelatedIssDate").val(issDate);
        entryForm.find("#RelatedFirstNamedInventor").val(inventor);
    }


    openIDSNonPatEntry(grid, url, data, closeOnSave) {
        $.ajax({
            url: url,
            data: data,
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#patIDSNonPatEntryDialog");
                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");

                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: closeOnSave,
                        afterSubmit: function () {
                            grid.dataSource.read();
                            dialogContainer.modal("hide");
                        }
                    }
                );
                let mainDragDropContainer = "#nonPatFileDropZone";
                pageHelper.setupDragDropFiles(mainDragDropContainer);

                mainDragDropContainer = $(mainDragDropContainer);
                mainDragDropContainer.on("filesDropped", function (e, droppedFiles) {
                    $("#nonPatFile").text(droppedFiles.files[0].name);
                    entryForm.find("#docFile")[0].files = droppedFiles.files; //only the 1st one is loaded
                });

                entryForm.on("click", ".dropZoneElement", function () {
                    entryForm.find("#docFile").trigger("click");
                });
                entryForm.find("#docFile").on("change", function (e) {
                    $("#nonPatFile").text(e.target.files[0].name);

                });

            },
            error: function (error) {
                pageHelper.showErrors(error.responseText);
            }
        });
    }

    onIDSRefCaseNumberChange() {
        const entryForm = $($(this.element).parents("form")[0]);

        if (this.text() === "") {
            idsLib.toggleIDSRelatedInfo(entryForm, true);
            const comboS = $(entryForm.find("input[name = 'RelatedCountryS']")).data("kendoComboBox");
            comboS.text("");
            comboS.value("");
        } else {
        
            idsLib.toggleIDSRelatedInfo(entryForm, false);
            const comboR = $(entryForm.find("input[name = 'RelatedCountryR']")).data("kendoComboBox");
            comboR.text("");
            comboR.value("");
            comboR.dataSource.read();
        }
        entryForm.find("#RelatedAppId").val(0);
        entryForm.find("#RelatedCountry").val("");
        entryForm.find("#RelatedSubCase").val("");
    }

    getIDSRefCaseNumber(id) {
        let entryForm;
        if (id) {
            entryForm = $("#idsEntry"); //from list view
        }
        else {
            const dialogContainer = $("#patIDSRelatedEntryDialog");
            entryForm = $(dialogContainer.find("form")[0]);
        }
       
        const appId = entryForm.find("input[name = 'AppId']").val();
        const caseNumber = entryForm.find("input[name = 'RelatedCaseNumber']").val();
       
        return { appId: appId, caseNumber: caseNumber };
    }

    onIDSRefCountryChange() {
        const entryForm = $($(this.element).parents("form")[0]);
        
        const country = this.text().split(" - ");
        let subCase = "";
        let relatedAppId = 0;

        entryForm.find("#RelatedCountry").val(country[0]);
        if ($.isNumeric(this.value())) {
            if (country.length > 1) subCase = country[1];
            relatedAppId = this.value();
            idsLib.getIDSRelatedInfo(relatedAppId, entryForm);

            //just to satisfy the required attribute
            $(entryForm.find("input[name = 'RelatedCountryS']")).data("kendoComboBox").value(country[0]);
        }
        entryForm.find("#RelatedSubCase").val(subCase);
        entryForm.find("#RelatedAppId").val(relatedAppId);
    }

    getIDSRelatedInfo(relatedAppId, entryForm) {
        const relatedInfoUrl = entryForm.data("related-url");
        $.get(relatedInfoUrl, { appId: relatedAppId })
            .done(function (result) {
                idsLib.setIDSRelatedInfo(result, entryForm);
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });
    }

    setIDSRelatedInfo(info, entryForm) {
        entryForm.find("#RelatedPubNumber").val(info.RelatedPubNumber);
        entryForm.find("#RelatedPatNumber").val(info.RelatedPatNumber);

        let pubDate = null;
        let issDate = null;
        if (info.RelatedPubDate) pubDate = new Date(info.RelatedPubDate);
        if (info.RelatedIssDate) issDate = new Date(info.RelatedIssDate);

        entryForm.find("input[name = 'RelatedPubDateR']").data("kendoDatePicker").value(pubDate);
        entryForm.find("input[name = 'RelatedIssDateR']").data("kendoDatePicker").value(issDate);
        entryForm.find("input[name = 'RelatedFirstNamedInventorR']").data("kendoComboBox").text(info.RelatedFirstNamedInventor);
    }

    editIDSRefRecord(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        const parent = grid.element.parent();
        const url = parent.data("url-edit");
        const data = { relatedCasesId: dataItem.RelatedCasesId, appId:dataItem.AppIDConnect};
        idsLib.openIDSRefEntry(grid, url, data, true);
    }

    editIDSNonPatRecord(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const parent = grid.element.parent();
        const url = parent.data("url-edit");
        const data = { nonPatLiteratureId: dataItem.NonPatLiteratureId };
        idsLib.openIDSNonPatEntry(grid, url, data, true);
    }

    rtsInpadocIDSSetListener(form) {
        form.find("#RelatedPubNumber,#RelatedPatNumber").siblings(".inp-search").click(function () {
            //const input = $(this);
            const input = $(this).siblings("input");
            const value = input.val();
    
            if (value.length >= 5) {
                let dateEl;
                const numType = input.data("num-type");

                if (numType === "U")
                    dateEl = "RelatedPubDateR";
                else
                    dateEl = "RelatedIssDateR";

                const date = form.find("input[name='" + dateEl + "']").data("kendoDatePicker").value();
                const searchInput = {
                    searchNo: value,
                    searchDate: date,
                    searchCaseType: "ORD",
                    searchCountry: form.find("input[name='RelatedCountryS_input']").val(),
                    searchNumberType: numType,
                    forIDS:true
                };
                input.siblings(".rtsInpSpinner").removeClass("d-none");
                input.siblings(".inp-search").addClass("d-none");
                rtsLib.rtsInpadocSearch(form, input, searchInput);
            }
        });

        form.on("onInpadocSelectedIDS", function (event, data, form) {
            idsLib.rtsInpadocIDSSelect(data, form);
        });
    }

    rtsInpadocIDSSelect(data, form) {
        if (data.NumberType === "U") {
            form.find("#RelatedPubNumber").val(data.PubNo);
            form.find("input[name='RelatedPubDateR']").data("kendoDatePicker").value(data.PubDateDisplay);
        }
        else if (data.NumberType === "P") {
            form.find("#RelatedPatNumber").val(data.PatNo);
            form.find("input[name='RelatedIssDateR']").data("kendoDatePicker").value(data.IssDateDisplay);
        }
        form.find("input[name='KindCode']").val(data.KD);

        if (data.Inventors) {
            const inventors = data.Inventors.split(";");
            let inventor = inventors[0];
            if (inventors.length > 1)
                inventor += " et al.";

            const inventorCombo = $(form.find("input[name='RelatedFirstNamedInventorR']"));
            inventorCombo.data("kendoComboBox").value(inventor);
        }
    }

    showEFSGenForm() {
        const dialog = $("#efsGenerationDialog");
        dialog.modal("show");

        dialog.find(".efs-generate, .efs-preview").on("click", function () {
            const button = $(this);
            const form = button.closest(".row");
            const params = form.data("params");

            const signatory = dialog.find("#Signatory").data("kendoComboBox").value();
            params.signatory = signatory;
            params.preview = button.hasClass("efs-preview");
            params.systemName = "Patent";
            params.systemType = "P";

            $("#efsFormParams").val(JSON.stringify(params));
            $("#efsForm").submit();
        });

        dialog.find(".efs-DOCX-generate, .efs-DOCX-preview").on("click", function () {
            const button = $(this);
            const form = button.closest(".row");
            const params = form.data("params");

            const signatory = dialog.find("#Signatory").data("kendoComboBox").value();
            params.signatory = signatory;
            //params.preview = button.hasClass("efs-preview");
            params.systemName = "Patent";
            params.systemType = "P";
            params.IsLog = button.hasClass("efs-DOCX-generate");;
            params.recordId = params.recId;
            params.docxScreenCode = "Country Application";
            params.screenSource = "genpopup";

            $("#docxFormParams").val(JSON.stringify(params));
            $("#docx").submit();
        });

    }

    stripeTableRows(tableId) {
        let highlightRow = 3;
        $(`#${tableId} tr`).each(function (index) {
            const row = index + 1;
            if (row === highlightRow || row === highlightRow + 1) {
                $(this).find("td").addClass("listView-alt-row");

                if (row === highlightRow + 1)
                    highlightRow = highlightRow + 4;
            }
            else {
                $(this).find("td").removeClass("listView-alt-row");
            }
        });
    }

}

const instance = new IDSLib();
//Object.freeze(instance);
export default instance;
