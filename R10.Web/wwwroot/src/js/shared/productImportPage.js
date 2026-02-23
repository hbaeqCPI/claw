export default class ProductImportPage {

    constructor() {
        this.loadedFromHistory = false;
    }

    initialize = () => {
        const self = this;
        $(document).ready(() => {
            this.importContainer = $("#importContent");
            if (this.importContainer.length > 0) {
                this.initializeStep_FileUpload();
            }
            const option = $("#import-menu");
            option.on("click", "a", function () {
                const selection = $(this).data("value");
                const system = $(this).data("system");

                if (selection === "new") {
                    const baseUrl = $("body").data("base-url");                    
                    const url = `${baseUrl}/Shared/ProductImport/${system === 'P' ? 'Patent' : (system === 'T' ? 'Trademark' : (system === 'G' ? 'GeneralMatter' : (system === 'A' ? 'AMS' : '')))}`;
                    window.location = url;
                }
                else {
                    const grid = $("#productImportHistoryGrid");
                    grid.data("kendoGrid").dataSource.read();
                }
            });

            $("#product-import-main").on("click", "#getStructure", function (e) {
                e.preventDefault();
                const url = $(this).attr("href");
                $.get(url).done(function (result) {
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(result);
                    const dialogContainer = popupContainer.find("#productImportTypeColumnsDialog");
                    dialogContainer.modal("show");
                });
            });

            $("#product-import").on("click", ".details-link", function (e) {
                e.preventDefault();
                const link = $(this);
                self.loadStepFromHistory(link);
            });
            
        });
    }

    loadStepFromHistory = (link) => {
        const status = link.data("status");
        const importId = link.data("id");
        this.loadedFromHistory = true;

        $("#importHistory").removeClass("active");
        $("#importContent").addClass("active");
        $("#import-menu .nav-link").removeClass("active");
        $("#di-link-import").addClass("active");
        $("#importContent .di-step").addClass("d-none");

        if (status === "Imported" || status === "Import Failed") {
            const next = this.importContainer.find("#di-step4");
            next.removeClass("d-none");
            const success = status === "Imported";
            this.initializeStep_ImportResult(next, importId, success);
        }
        else if (status === "For Mapping Review") {
            const next = this.importContainer.find("#di-step2");
            next.removeClass("d-none");
            this.initializeStep_Mapping(next, importId);
        }

    }

    gotoStep = (step, direction, reload, importId, success) => {
        const nextPos = `#di-step${step}`;
        const next = this.importContainer.find(nextPos)
        const self = this;

        if (next.length > 0) {
            reload = reload || this.loadedFromHistory;
            if (reload) {
                if (step === 1) {
                    swapScreen();
                    this.initializeStep_FileUpload_FromHistory(importId);
                }
                else if (step === 2) {
                    swapScreen();
                    this.initializeStep_Mapping(next, importId);
                }
                else if (step === 3) {
                    this.initializeStep_Review(next, importId, swapScreen);
                }
                else if (step === 4) {
                    swapScreen();
                    this.initializeStep_ImportResult(next, importId, success);
                }
            }
            else
                swapScreen();
        }

        function swapScreen() {
            next.removeClass("d-none");

            const currentStep = direction === "forward" ? step - 1 : step + 1;
            const current = self.importContainer.find(`#di-step${currentStep}`);
            current.addClass("d-none");
        }

    }

    //step 1 - select data to import and upload file
    initializeStep_FileUpload = () => {
        let mainDragDropContainer = "#importFileDropZone";
        const entryForm = $("#importFileForm");

        pageHelper.setupDragDropFiles(mainDragDropContainer);

        mainDragDropContainer = $(mainDragDropContainer);
        mainDragDropContainer.on("filesDropped", function (e, droppedFiles) {
            $("#importFileName").text(droppedFiles.files[0].name);
            entryForm.find("#importFile")[0].files = droppedFiles.files; //only the 1st one is loaded
            setFileToModified();
            validateForm();
        });
        entryForm.on("click", ".dropZoneElement", function () {
            setFileToModified();
            entryForm.find("#importFile").trigger("click");
        });
        entryForm.find("#importFile").on("change", (e) => {
            if (e.target.files.length > 0) {
                entryForm.find("#importFileName").text(e.target.files[0].name);
                validateForm();
            }
        });

        //entryForm.on("change", "input[name='dataType']", () => {
        //    validateForm();
        //});

        entryForm.on("submit", (e) => {
            e.preventDefault();
            const processForm = entryForm.find("#process");
            if (processForm.val() === "true") {
                cpiLoadingSpinner.show();
                pageHelper.postData(entryForm.attr("action"), entryForm)
                    .done((importId) => {
                        entryForm.find("#importId").val(importId);
                        processForm.val("false");
                        entryForm.find("#fileModified").val("false");
                        entryForm.find("#importFile").val("");
                        cpiLoadingSpinner.hide();

                        this.gotoStep(2, "forward", true, importId);
                    })
                    .fail(function (e) {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(e);
                    });
            }
            else {
                const importId=entryForm.find("#importId").val();
                this.gotoStep(2, "forward", false, importId);
            }
        });

        function validateForm() {
            const importFile = entryForm.find("#importFileName").text();
            //const valid = importFile.length > 0 && importFile.endsWith(".xlsx") && parseInt(entryForm.find("input[name='dataType']:checked").val()) > 0;
            const valid = importFile.length > 0 && importFile.endsWith(".xlsx");
            const button = $("#di-step1").find("button[type='submit']");
            if (valid) {
                button.removeAttr("disabled");
                entryForm.find("#process").val("true");
            }
        }

        function setFileToModified() {
            entryForm.find("#fileModified").val("true");
            entryForm.find("#process").val("true");
        }
    }

    initializeStep_FileUpload_FromHistory = (importId) => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/ProductImport/GetImportJob`;
        $.get(url, { importId: importId }).done((job) => {
            const entryForm = $("#importFileForm");
            entryForm.find("#importId").val(job.ImportId);
            entryForm.find("#importFileName").text(job.OrigFileName);
            //entryForm.find("[name='dataType']").val(job.DataTypeId);
            entryForm.find(`#im-dataType-${job.DataTypeId}_`).prop("checked", true);
            
            const button = $("#di-step1").find("button[type='submit']");
            button.removeAttr("disabled");
        });
    }

    //step 2 - change mappings
    initializeStep_Mapping = (next, importId) => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/ProductImport/LoadStep_FieldMapping`;
        $.get(url, { importId: importId })
            .done((result) => {
                next.html(result);

                this.mappingGrid = { name: 'dataImportMapping' };
                const grid = $(`#${this.mappingGrid.name}`);
                pageHelper.kendoGridDirtyTracking(grid, this.mappingGrid, this.afterMappingGridSaveOrCancel, this.afterMappingGridSaveOrCancel, this.onMappingGridDirty);

                const stepContainer = $("#di-step2");
                const nextButton = stepContainer.find(".next");
                nextButton.on("click", () => { this.gotoStep(3, "forward", true, importId) });

                const prevButton = stepContainer.find(".prev");
                prevButton.on("click", () => { this.gotoStep(1, "backward", false, importId) });
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });

    };

    //step 3 - review and upload options
    initializeStep_Review = (next, importId,onSuccess) => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/ProductImport/LoadStep_Review`;

        $.get(url, { importId: importId })
            .done((result) => {
                onSuccess();
                next.html(result);

                const stepContainer = $("#di-step3");
                const importButton = stepContainer.find("#import");
                importButton.on("click", () => {
                    var optionsForm = next.find("#optionsForm")[0];
                    var options = pageHelper.formDataToJson($(optionsForm), true);
                    
                    const url = `${baseUrl}/Shared/ProductImport/Import`;                    
                    cpiLoadingSpinner.show();
                    $.post(url, { importId: importId, options: JSON.stringify(options.payLoad) });
                    setTimeout(() => {
                        cpiLoadingSpinner.hide(); this.step3StatusCheck(baseUrl, importId, next);
                    }, 5000);
                });
                const prevButton = stepContainer.find(".prev");
                prevButton.on("click", () => { this.gotoStep(2, "backward", false, importId) });
             })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });
    };

    step3StatusCheck = (baseUrl, importId, next) => {
        const url = `${baseUrl}/Shared/ProductImport/GetImportStatus`;                    
        $.get(url, { importId: importId }).done((result) => {
            switch (result) {
                case "Import Failed":
                    this.gotoStep(4, "forward", true, importId, false);
                    break;

                case "Imported":
                    this.gotoStep(4, "forward", true, importId, true);
                    break;

                case "Processing":
                    this.stepImportResult_Processing(baseUrl, importId, next);
                    break;
            }
        });
    };

    //step 4 - import result
    initializeStep_ImportResult = (next, importId, success) => {
        const baseUrl = $("body").data("base-url");
        const status = success ? "Success" : "Fail";
        const url = `${baseUrl}/Shared/ProductImport/LoadStepImportResult_${status}`;
        $.get(url, { importId: importId }).done((result) => {
            next.html(result);
            if (!success) {
                next.on("click", "#downloadError", function (e) {
                    e.preventDefault();
                    const form = $(this).find("form")[0];
                    $(form).submit();
                });
                const prevButton = next.find(".prev");
                prevButton.on("click", () => { this.gotoStep(3, "backward", false, importId) });
            }
        });
    };

    stepImportResult_Processing = (baseUrl, importId, next) => {
        const url = `${baseUrl}/Shared/ProductImport/LoadStepImportResult_Processing`;
        $.get(url, { importId: importId }).done((result) => {
            next.html(result);
            const refreshButton = next.find("#refresh");
            refreshButton.on("click", () => {
                this.step3StatusCheck(baseUrl, importId, next);
            });
        });
    };

    afterMappingGridSaveOrCancel = () => {
        const nextButton = $("#di-step2").find(".next");
        nextButton.removeAttr("disabled");
    }

    onMappingGridDirty = () => {
        const nextButton = $("#di-step2").find(".next");
        nextButton.attr("disabled", "disabled");
    }

}




