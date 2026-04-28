
export default class FileUtility {

    getFileStream(url, data) {
        // post a form, ajax posting is not able to stream file to client
        // reference: https://stackoverflow.com/questions/16086162/handle-file-download-from-ajax-post

        const html = '<form method="POST" action="' + url + '">' +
            '<input type="hidden" name="__RequestVerificationToken" value="' + $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val() + '">';

        // use pagehelper below
        //'<input type="hidden" name="__RequestVerificationToken" value="' + $("[name='__RequestVerificationToken']").val() + '">';

        let form = $('<form method="POST" action="' + url + '">');
        $.each(data, function (k, v) {
            form.append($('<input type="hidden" name="' + k + '" value="' + v + '">'));         // append data to post
        });
        $('body').append(form);
        form.submit();
        form.remove();
    }
}

