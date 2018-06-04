var Registration = {
    init: function () {
        $(document)
            .on('submit', '.jsOrderConfirmationRegistration', Registration.orderConfirmationRegistration)
            .on('submit', '.jsOrderConfirmationRegistrationAssign', Registration.orderConfirmationRegistrationAssign)
            .on('submit', '.jsRegistration', Registration.register);
    },
    register: function (e) {
        e.preventDefault();
        var form = $(this).closest(".jsRegistration");
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize().replace("&Address.Region=", "&Address.CountryRegion.Region="),
            success: function (result, status, xhr) {
                var ct = xhr.getResponseHeader("content-type") || "";
                if (ct.indexOf('json') > -1) {
                    document.location = result.ReturnUrl;
                }
                else
                {
                    $(form).parent().replaceWith($(result));
                }
            }
        });
    },
    orderConfirmationRegistration: function (e) {
        e.preventDefault();
        var form = $(this).closest(".jsOrderConfirmationRegistration");
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (result) {
                $(form).parent().replaceWith($(result));
            }
        });
    },
    orderConfirmationRegistrationAssign: function (e) {
        e.preventDefault();
        var form = $(this).closest(".jsOrderConfirmationRegistrationAssign");
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (result) {
                $(form).parent().replaceWith($(result));
            }
        });
    }
};