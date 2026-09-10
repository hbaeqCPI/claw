
//manage record navigator
(function ($) {
    if (typeof $.fn.cpiRecordNavigator === "undefined") {
        $.fn.cpiRecordNavigator = function (options) {
            const plugin = this;

            const updatePosition = function (index) {
                plugin.data("currentIndex", index);
                plugin.find(".currentRecPosition").val(index + 1);
                plugin.find(".navTotalRecs").html(options.recordIds.length);
            };

            const updateState = function (currentId) {
                if (options.recordIds.length > 0) {
                    let prevEnable = false;
                    let nextEnable = false;
                    const index = options.recordIds.indexOf(currentId);

                    if (index >= 0) {
                        prevEnable = index > 0;
                        nextEnable = index < options.recordIds.length - 1;
                    }
                    updatePosition(index);

                    plugin.find(".k-pager-nav").each(function () {
                        const button = $(this);
                        const direction = button.data("dir");

                        if (["top", "prev"].indexOf(direction) > -1) {
                            if (prevEnable)
                                button.removeClass("k-state-disabled");
                            else
                                button.addClass("k-state-disabled");
                        }
                        else {
                            if (nextEnable)
                                button.removeClass("k-state-disabled");
                            else
                                button.addClass("k-state-disabled");
                        }
                    });
                }
            };

            // A record is identified by an int id on most screens, but by a
            // RecordNavigationKey string on those keyed on a composite (Country Law,
            // Des Case Type, the Area Country pairs). Test for "has a value" rather
            // than "> 0" so keys are not rejected.
            const hasRecord = function (record) {
                return record !== undefined && record !== null && record !== "" && record !== 0;
            };

            const gotoPosition = function (pos) {
                if (pos > 0 && pos <= options.recordIds.length) {
                    const newCurrentId = options.recordIds[pos - 1];
                    if (hasRecord(newCurrentId)) {
                        options.navigateHandler(newCurrentId);
                        updateState(newCurrentId);
                    }
                }
            };

            plugin.data("currentIndex", 0);
            plugin.find(".k-pager-nav").on("click", function () {
                const index = plugin.data("currentIndex");
                const direction = $(this).data("dir");
                let newCurrentId = 0;

                switch (direction) {
                    case "top":
                        newCurrentId = options.recordIds[0];
                        break;
                    case "prev":
                        newCurrentId = options.recordIds[index - 1];
                        break;
                    case "next":
                        newCurrentId = options.recordIds[index + 1];
                        break;
                    case "last":
                        newCurrentId = options.recordIds[options.recordIds.length - 1];
                        break;
                }
                if (hasRecord(newCurrentId)) {
                    options.navigateHandler(newCurrentId);
                    updateState(newCurrentId);
                }

            });

            plugin.addRecordId = function (id) {
                options.recordIds.push(id);
                updateState(id);
            };

            plugin.deleteRecordId = function (id) {
                let index = options.recordIds.indexOf(id);

                if (index >= 0) {
                    options.recordIds.splice(index, 1); //delete the element
                    if (options.recordIds.length > 0) {
                        if (options.recordIds.length <= index)
                            index = options.recordIds.length - 1;

                        const newId = options.recordIds[index];
                        updateState(newId);
                        return newId;
                    }
                }
                return null;
            };


            plugin.on("keyup", ".currentRecPosition", function (e) {
                if (/\D/g.test(this.value)) {
                    this.value = this.value.replace(/\D/g, ''); //numeric only
                }

                if (e.keyCode === 13 && this.value.length > 0) {
                    gotoPosition(this.value);
                }
            });

            //init
            updateState(options.currentId);

            return this;
        };
    }
})(jQuery);