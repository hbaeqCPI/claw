import * as pageHelper from "../pageHelper";

class RTSLib {
    templateUrl = "";
    forIDS = false;

    rtsInpadocSearch(form, input, searchInput) {
        const json = {
            payLoad: searchInput,
            verificationToken: form.find("input[name='" + pageHelper.verificationTokenFormData + "']").val()
        };
        this.templateUrl = form.data("pfs-template");
        this.forIDS = searchInput.forIDS;
        if (this.forIDS) {
            this.selectLabel = form.data("pfs-select-label");
        }
        this.rtsInpadocQuery(json, form.data("pfs-search"), input);
    }

    rtsInpadocQuery = (data, url, input) => {
        const self = this;
        pageHelper.postJson(url, data)
            .done(function (result) {
                if (result.length > 0) {
                    self.rtsInpadocBuildPicklist(result, input);
                }
            })
            .fail(function (e) {
                if (e.responseJSON !== undefined)
                    pageHelper.showErrors(e.responseJSON);
                else
                    pageHelper.showErrors(e.responseText);
            })
            .always(function () {
                $(".rtsInpSpinner").addClass("d-none");
                $(".rtsInpTrigger .inp-search").removeClass("d-none");
            });
    }

    rtsInpadocBuildPicklist(result, input) {
        const inpContainer = "rtsInpContainer";
        $(`.${inpContainer}`).remove();

        if (result.length === 0)
            return;

        const self = this;
        const numType = input.data("num-type");
        $.get(this.templateUrl+`?numType=${numType}`).done(function (response) {
            let options = "";
            for (let i = 0; i < result.length; i++) {
                const displayKey = result[i].DisplayKey.split("|");
                let option;
                if (self.forIDS) {
                    option = `<li class="k-item" data-key="${result[i].Id}">
                                    <div class="row combobox-item">
                                        <div style="width:120px"><span>${displayKey[0]}</span></div>
                                        <div style="width:120px"><span>${displayKey[1]}</span></div>
                                        <div style="width:30px"><span>${displayKey[2]}</span></div>
                                        <div style="width:140px"><span>${displayKey[3]}</span></div>
                                        <div class="pl-2 row-select" title="${self.selectLabel}"><i class="fal"></i></div>
                                    </div>
                                </li>`;
                }
                else {
                    option = `<li class="k-item" data-key="${result[i].Id}">
                                    <div class="row combobox-item">
                                        <div style="min-width:200px"><span>${displayKey[0]}</span></div>
                                        <div><span>${displayKey[1]}</span></div>
                                    </div>
                                </li>`;
                }
                options += option;
            }
            const html = response.replace("<li></li>", options);
            const form = $(input.closest('form'));
            input.closest('.rtsInpTrigger').append(html);

            //if (input.parents("#patIDSRelatedEntryDialog").length > 0) {
              //$(".rtsInpContainer").addClass("rtsInpSingle");
            //}

            const container = $(".rtsInpContainer");
            $(".rtsInpContainer").addClass("rtsInpMultiple");
            const offset = input.offset();
            container.offset({ top: offset.top - 300, left: offset.left });

            if (self.forIDS) {
                const selectElement = $(".rtsInpIDSList li");
                selectElement.on("click", ".row-select", function () {
                    const key = $(this).closest("li").data("key");
                    self.rtsInpadocIDSSelect(result, key, form);
                });
                selectElement.hover(function () { $(this).find("i.fal").addClass("fa-check-circle"); },
                    function () { $(this).find("i.fal").removeClass("fa-check-circle"); },    
                );
            }
            else {
                const selectElement = $(".rtsInpList li");
                selectElement.on("click", function () {
                    const key = $(this).data("key");
                    $(".rtsInpPreview").removeClass("d-none");
                    self.rtsInpadocPreview(result, key, form);
                });
            }

            $(document).click(function (event) {
                const target = $(event.target);
                const container = $(`.${inpContainer}`);
                if (!target.closest(container).length && container.is(":visible")) {
                    container.remove();
                }
            });


        });

        

       

        

    }

    rtsInpadocPreview(list, id, form) {
        const pos = list.findIndex(function (element) { return element.Id === id.toString(); });
        if (pos > -1) {
            let url = form.data("pfs-format");

            const data = list[pos];
            const json = {
                payLoad: data,
                verificationToken: form.find("input[name='" + pageHelper.verificationTokenFormData + "']").val()
            };
            pageHelper.postJson(url, json)
                .then(function (result) {
                    json.payLoad = result;
                    url = form.data("pfs-preview");
                    return pageHelper.postJson(url, json);
                })
                .done(function (result) {
                    const container = form.find(".rtsInpPreview");
                    $(container).html(result);
                    $(container).find("#PFSSelect").click(function () {
                        form.trigger("onInpadocSelected", [json.payLoad, form]);
                        $(".rtsInpContainer").remove();
                    });
                    $(container).find("#PFSClose").click(function () {
                        $(".rtsInpContainer").remove();
                    });
                })
                .fail(function (e) {
                    if (e.responseJSON !== undefined)
                        pageHelper.showErrors(e.responseJSON);
                    else
                        pageHelper.showErrors(e.responseText);
                });

        }
    }

    rtsInpadocIDSSelect(list, id, form) {
        const pos = list.findIndex(function (element) { return element.Id === id.toString(); });
        if (pos > -1) {
            let url = form.data("pfs-format");

            const data = list[pos];
            const json = {
                payLoad: data,
                verificationToken: form.find("input[name='" + pageHelper.verificationTokenFormData + "']").val()
            };
            pageHelper.postJson(url, json)
                .done(function (result) {
                    form.trigger("onInpadocSelectedIDS", [result, form]);
                    $(".rtsInpContainer").remove();
                })
                .fail(function (e) {
                    if (e.responseJSON !== undefined)
                        pageHelper.showErrors(e.responseJSON);
                    else
                        pageHelper.showErrors(e.responseText);
                });

        }
    }


}

const instance = new RTSLib();
//Object.freeze(instance);
export default instance;
