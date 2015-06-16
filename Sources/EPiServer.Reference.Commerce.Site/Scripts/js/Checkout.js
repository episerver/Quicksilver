var Checkout = {
    init: function () {
        $(document)
            .on('change', '.jsChangePayment', Checkout.changePayment)
            .on('change', '.jsChangeShipment', Checkout.changeShipment)
            .on('change', '.jsChangeAddress', Checkout.changeAddress)
            .on('change', '.jsChangeCountry', Checkout.changeCountry);
    },
    changeCountry: function () {
       
        var $countryCode = $(this).val();
        var $region = $(".address-region").val();

        $.ajax({
            type: "POST",
            url: $(this).closest(".jsCheckoutAddress").data("url").replace("ChangeAddress", "GetRegionsForCountry"),
            data: { countryCode: $countryCode, region: $region },
            success: function (result) {
                $("#AddressRegion").replaceWith($(result));
            }
        });
    },
    changeAddress: function () {
        $.ajax({
            type: "POST",
            url: $(this).closest('.jsCheckoutAddress').data('url'),
            data: $(this).serialize(),
            success: function (result) {
                $('.jsCheckoutAddress').replaceWith($(result).filter('.jsCheckoutAddress'));
                Misc.updateValidation('jsCheckoutForm');
            }
        });
    },
    changePayment: function () {
        var form = $('.jsCheckoutForm');
        $.ajax({
            type: "POST",
            url: form.data("updateurl"),
            data: form.serialize(),
            success: function (result) {
                $('.jsPaymentMethod').replaceWith($(result).find('.jsPaymentMethod'));
                Checkout.updateOrderSummary();
                Misc.updateValidation('jsCheckoutForm');
            }
        });
    },
    changeShipment: function () {
        var form = $('.jsCheckoutForm');
        $.ajax({
            type: "POST",
            url: form.data("updateurl"),
            data: form.serialize(),
            success: function (result) {
                $('.jsPaymentMethod').replaceWith($(result).find('.jsPaymentMethod'));
                Checkout.updateOrderSummary();
            }
        });
    },
    updateOrderSummary: function () {
        $.ajax({
            cache: false,
            type: "GET",
            url: $('.jsOrderSummary').data('url'),
            success: function (result) {
                $('.jsOrderSummary').replaceWith($(result).filter('.jsOrderSummary'));
            }
        });
    }
};