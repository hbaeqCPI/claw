
export default class DMSReviewPage {

    constructor() {
        this.IsDMSReviewer = false;
    }

    initPage(isReviewer) {
        const self = this;

        this.IsDMSReviewer = isReviewer === 1;

        const searchCriteriaContainer = $("#searchCriteriaContainer");
        searchCriteriaContainer.find("#search").click(function (e) {
            self.refreshSearchResultGrid();
        });


        self.refreshSearchResultGrid();
    }

    getCriteria() {
        return pageHelper.getFormCriteria();
    }

    refreshSearchResultGrid() {
        const self = this;
        const data = this.getCriteria();
        const grid = $("#reviewGrid").data("kendoGrid");
        grid.dataSource.read(data).done(function () {
            self.bindRating("#searchContainer", "rating-url", self.IsDMSReviewer, ".kv-fa-main");
            self.bindRatingEvents();
            //grid.showColumn("RatingValue");
        });
        
    }

   

    //---------- star rating bindings
    bindRating(parentContainer, urlData, allowEdit, ratingClass ) {
        // get rating settings and update screen view
        const self = this;
        const parent = $(parentContainer);
        const url = parent.data(urlData);

        $.ajax({
            url: url,
            type: "GET",
            success: function (result) {
                self.refreshRating(allowEdit, result, ratingClass);       // update rating inputs
            }
        });
    }

    refreshRating(allowEdit, ratingCaption, ratingClass) {
        // parse parameters
        const starCount = ratingCaption.RatingCount;
        const ratingStep = ratingCaption.RatingStep;
        const captions = ratingCaption.RatingCaptions;
        let starCaptions = {};
        for (var i = 0, item; i < captions.length; i++) {
            item = captions[i];
            starCaptions[item.RatingValue] = item.Rating;
        }
        let clearCaption = '';
        if (captions.length > 0) {
            clearCaption = captions[0].Rating;
        }

        // refresh rating input objects
        $(ratingClass).rating({
            theme: 'krajee-fa',
            filledStar: '<span class="fa fa-star"></span>',
            //emptyStar: '<i class="fa fa-star-o"></i>',            --- fa-star-o is nice but is not available in the local fontawesome css
            emptyStar: '<i class="fa fa-star"></i>',
            clearButton: '<i class="fa fa-lg fa-minus-circle"></i>',
            clearCaption: clearCaption,
            stars: starCount,
            step: ratingStep,
            showClear: allowEdit,
            displayOnly: !allowEdit,
            starCaptions: starCaptions
        });
        
    }

    // reviewer ratings popup
    openReviewerRatings(e, dataItem) {
        const selItem = dataItem.dataItem($(e.currentTarget).closest("tr"));
        const dmsId = selItem.DMSId;
        const parent = $("#reviewResultContainer");
        const url = parent.data("show-reviewer-ratings-url") + "/" + dmsId;
        const self = this;

        $.get(url).done(function (result) {
            const popupContainer = $(".cpiContainerPopup").last();
            popupContainer.html(result);
            const dialogContainer = popupContainer.find("#reviewerRatingsDialog");
            dialogContainer.modal("show");
            self.refreshReviewerRatingsGrid(self, dmsId);

        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
        });
    }
    // reviewer ratings popup grid
    refreshReviewerRatingsGrid(self, dmsId) {
        const grid = $("#reviewerRatingGrid").data("kendoGrid");
        const data = { dmsId: dmsId };
        grid.dataSource.read(data).done(function () {
            self.bindRating("#searchContainer", "rating-url", false, ".kv-fa-popup");
        });
    }

    // binds rating click event -> opens rating remarks pop-up -> save/cancel
    bindRatingEvents() {

        // bind rating change event
        $('.kv-fa-main').on('rating:change', function (event, value, caption) {
            dmsReviewPage.editReviewerRemarks(this, value);
        });

        // bind rating clear event
        $('.kv-fa-main').on('rating:clear', function () {
            dmsReviewPage.editReviewerRemarks(this, 0);
        });
    }

    editReviewerRemarks(self, value) {
        const grid = $("#reviewGrid").data("kendoGrid");
        const currentRow = self.closest("tr");
        const selItem = grid.dataItem(currentRow);

        const param = new Object();
        param.DMSReviewerId = selItem.DMSReviewerId;
        param.RatingValue = value;
        param.Remarks = selItem.ReviewerRemarks;
        param.tStamp = selItem.tStamp;

        const parent = $("#reviewResultContainer");
        const url = parent.data("show-reviewer-remarks-url");

        $.ajax({
            url: url,
            data: { model: param },
            type: "POST",
            headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
            success: function (result) {
                dmsReviewPage.showEditReviewerRemarks(result);
            },
            error: function (error) {
                pageHelper.showErrors(error.responseText);
            }
        });
    }

    // show reviewer rating and remarks; if "save" clicked, save the rating & remarks
    showEditReviewerRemarks(result) {
        const popupContainer = $(".cpiContainerPopup").last();
        popupContainer.html(result);
        const dialogContainer = popupContainer.find("#editReviewerRemarksDialog");
        dialogContainer.modal("show");
        dmsReviewPage.bindRating("#searchContainer", "rating-url", false, ".kv-fa-edit");

        // attach save event
        $(dialogContainer.find("#saveEditReviewerRemarks").click(function (e) {
            const formBody = $(dialogContainer.find("#editReviewerRemarksBody"));
            const url = formBody.data("reviewer-remarks-save-url");
            const param = new Object();
            param.DMSReviewerId = $(formBody.find("#DMSReviewerId")).val();
            param.RatingValue = $(formBody.find("#RatingValue")).val();
            param.Remarks = $(formBody.find("#Remarks")).val();
            param.tStamp = $(formBody.find("#tStamp")).val();

            $.ajax({
                url: url,
                data: param,
                type: "POST",
                headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                success: function (result) {
                    cpiStatusMessage.success(result.success, 1500);
                    dmsReviewPage.refreshSearchResultGrid();
                },
                error: function (error) {
                    pageHelper.showErrors(error.responseText);
                }
            });
            dialogContainer.modal("hide");

        }));

        // attach cancel event
        $(dialogContainer.find("#cancelEditReviewerRemarks").click(function (e) {
            dmsReviewPage.refreshSearchResultGrid();
        }));
    }


    // main review screen - recommendation save
    isRecommendationEditable (dataItem) {
        return dataItem.CanEditRecommendation;
    }

    recommendationSave(e) {
        if (e.item) {
            const grid = $("#reviewGrid").data("kendoGrid");
            const selItem = grid.dataItem(grid.select());
            const param = new Object();
            param.DMSId = selItem.DMSId;
            param.DMSReviewerId = selItem.DMSReviewerId;
            //param.DMSReviewerId = selItem.DMSReviewerId;
            param.Recommendation = e.dataItem["Recommendation"];
            param.tStamp = selItem.tStamp;

            const parent = $("#reviewResultContainer");
            const url = parent.data("recom-save-url");

            
            $.ajax({
                url: url,
                data: param,
                type: "POST", 
                headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                success: function (result) {
                    dmsReviewPage.refreshSearchResultGrid();
                    cpiStatusMessage.success(result.success, 1500);
                    // send email, if required
                    if (result.sendEmail) {
                        dmsReviewPage.sendRecommendationEmail(param.DMSId, param.DMSReviewerId, result.isAutoEmail);
                    }
                },
                error: function (error) {
                    pageHelper.showErrors(error.responseText);
                }
            });
            
        }
    }

    sendRecommendationEmail(dmsId, dmsReviewerId, isAutoEmail) {
        const parent = $("#reviewResultContainer");
        const url = parent.data("review-email-url");
        const data = { dmsId: dmsId, dmsReviewerId: dmsReviewerId };
        $.ajax({
            url: url,
            type: "GET", 
            data: data,
            success: function (result) {

                if (!isAutoEmail) {
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(result);
                    const dialog = $("#quickEmailDialog");
                    dialog.modal("show");
                }
                else {
                    cpiStatusMessage.success(result.success, 1500);
                }
            },
            error: function (error) {
                cpiStatusMessage.error(e.responseText);
            }
        });
    }


    openRecommendationHistory(e, dataItem) {
        const selItem = dataItem.dataItem($(e.currentTarget).closest("tr"));
        const dmsId = selItem.DMSId;
        const parent = $("#reviewResultContainer");
        const url = parent.data("show-recom-history-url") + "/" + dmsId;

        $.get(url).done(function (result) {
            const popupContainer = $(".cpiContainerPopup").last();
            popupContainer.html(result);
            const dialog = $("#recommendationHistoryDialog");
            dialog.modal("show");
        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
            });
    }
   
    openStatusHistory(e, dataItem) {
        const selItem = dataItem.dataItem($(e.currentTarget).closest("tr"));
        const dmsId = selItem.DMSId;
        const parent = $("#reviewResultContainer");
        const url = parent.data("status-history-url") + "/" + dmsId;

        $.get(url).done(function (result) {
            const popupContainer = $(".cpiContainerPopup").last();
            popupContainer.html(result);
            const dialog = $("#statusHistoryDialog");
            dialog.modal("show");
        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
        });
    }

    openEditRemarks(e, dataItem) {
        const selItem = dataItem.dataItem($(e.currentTarget).closest("tr"));
        const dmsId = selItem.DMSId;
        const parent = $("#reviewResultContainer");
        const url = parent.data("show-remarks-url") + "/" + dmsId;
        const self = this;

        $.get(url).done(function (result) {
            self.showEditRemarks(result);

        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
        });
    }

    showEditRemarks(result) {
        const popupContainer = $(".cpiContainerPopup").last();
        popupContainer.html(result);
        const dialogContainer = popupContainer.find("#editRemarksDialog");
        dialogContainer.modal("show");

        $(dialogContainer.find("#saveEditRemarks").click(function (e) {
            const formBody = $(dialogContainer.find("#editRemarksBody"));
            const url = formBody.data("remarks-save-url");
            const param = new Object();
            param.dmsId = $(formBody.find("#DMSId")).val();
            param.tStamp = $(formBody.find("#tStamp")).val();
            param.remarks = $(formBody.find("#Remarks")).val();
            
            $.ajax({
                url: url,
                data: param,
                type: "POST",
                headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                success: function (result) {
                    cpiStatusMessage.success(result.success, 1500);
                },
                error: function (error) {
                    pageHelper.showErrors(error.responseText);
                }
            });
            dialogContainer.modal("hide");

        }));
    }
}

/*
    ----- RatingStar plugin reference
    references: https://www.jqueryscript.net/other/Simple-jQuery-Star-Rating-System-For-Bootstrap-3.html
                https://plugins.krajee.com/star-rating#usage

    // assign a CSS class with the rating-<theme-name> to the rating container
    // The default (blank) theme (for displaying bootstrap glyphicons)
    // krajee-svg (for displaying svg icons)
    // krajee-uni (for displaying unicode symbols as stars)
    // krajee-fa (for displaying font awesome icons)
    theme:'',

    // enable the plugin to display messages for your locale (you must set the ISO code for the language).
    language:'en',

    // number of stars to display
    stars: 5,

    // the symbol markup to display for a filled / highlighted star
    filledStar:'<i class="glyphicon glyphicon-star"></i>',

    // the symbol markup to display for an empty star
    emptyStar:'<i class="glyphicon glyphicon-star-empty"></i>',

    // the CSS class to be appended to the star rating container.
    containerClass:'',

    // whether the input is read only
    displayOnly:false,

    // whether the rating input is to be oriented RIGHT TO LEFT.
    rtl:false,

    // size of the rating control
    // xl, lg, md, sm, and xs
    size:'md',

    // whether the clear button is to be displayed
    showClear:true,

    // whether the rating caption is to be displayed
    showCaption:true,

    // the caption titles corresponding to each of the star rating selected
    starCaptionClasses: {
      0.5:'label label-danger',
      1:'label label-danger',
      1.5:'label label-warning',
      2:'label label-warning',
      2.5:'label label-info',
      3:'label label-info',
      3.5:'label label-primary',
      4:'label label-primary',
      4.5:'label label-success',
      5:'label label-success'
    },

    // the markup for displaying the clear button
    clearButton:'<i class="glyphicon glyphicon-minus-sign"></i>',

    // the base CSS class for the clear button
    clearButtonBaseClass:'clear-rating',

    // the CSS class for the clear button that will be appended to the base class above when button is hovered/activated.
    clearButtonActiveClass:'clear-rating-active',

    // the caption displayed when clear button is clicked
    clearCaption:'Not Rated',

    // the CSS Class to apply to the caption displayed, when clear button is clicked
    clearCaptionClass:'label label-default',

    // the value to clear the input to, when the clear button is clicked
    clearValue: 0,

    // the identifier for the container element selector for displaying the caption.
    captionElement:null,

    // the identifier for the container element selector for displaying the clear button.
    clearElement:null,

    // whether hover functionality is enabled.
    hoverEnabled:true,

    // control whether the caption should dynamically change on mouse hover
    hoverChangeCaption:true,

    // control whether the stars should dynamically change on mouse hover
    hoverChangeStars:true,

    // whether to dynamically clear the rating on hovering the clear button
    hoverOnClear:true<br>


    ----- Methods -----
    // The method accepts a rating value as a parameter.
    $('#input-id').rating('update', 3);

    // Example: Call the method below in rating.change event to disable the rating and
    // hide the clear button.
    $('#input-id').rating('refresh', {disabled:true, showClear:false, showCaption:true});

    // Reset the rating.
    $('#input-id').rating('reset');

    // Clear the rating.
    $('#input-id').rating('clear');

    // Destroy the rating.
    $('#input-id').rating('destroy');

    // Re-creates the rating (after a destroy).
    $('#input-id').rating('create');


*/

