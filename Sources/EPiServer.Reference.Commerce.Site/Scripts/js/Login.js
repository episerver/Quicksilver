var login = {
    init: function () {
        $(document)
            .on('submit', '.jsLoginBlock', login.login);
    },
    login: function (e) {
        e.preventDefault();
        var form = $(this).closest(".jsLoginBlock");
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (response, status, xhr) {
                var ct = xhr.getResponseHeader("content-type") || "";
                if (ct.indexOf('html') > -1) {
                    $(form).closest('.jsLoginBlockWrapper').replaceWith($(response));
                }
                if (ct.indexOf('json') > -1) {
                    document.location = response.ReturnUrl;
                }
            }
        });
    }
}