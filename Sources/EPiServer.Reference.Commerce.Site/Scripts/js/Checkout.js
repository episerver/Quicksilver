var Checkout = {
    init: function () {
        $(document)
            .on('change', '.jsChangePayment', Checkout.changePayment)
            .on('change', '.jsChangeShipment', Checkout.changeShipment)
            .on('change', '.jsChangeAddress', Checkout.changeAddress)
            .on('click', '#AlternativeAddressButton', Checkout.enableShippingAddress)
            .on('click', '.remove-shipping-address', Checkout.removeShippingAddress);

        Checkout.initializeAddressAreas();
    },
    initializeAddressAreas: function () {

        if ($("#UseBillingAddressForShipment").val() == "False") {
            $("#AlternativeAddressButton").click();
        }
        else {
            $(".shipping-address").css("display", "none");
            $(".remove-shipping-address").click();
        }
    },
    changeAddress: function () {

        var form = $('.jsCheckoutForm');
        $("#ShippingAddressIndex").val($(".jsChangeAddress").index($(this)) - 1);

        $.ajax({
            type: "POST",
            chache: false,
            url: $(this).closest('.jsCheckoutAddress').data('url'),
            data: form.serialize(),
            success: function (result) {
                $("#AddressContainer").replaceWith($(result));
                Checkout.initializeAddressAreas();
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
    },
    enableShippingAddress: function (event) {

        event.preventDefault();

        var $billingShippingMethods = $(".billing-shipping-method");
        var $selectedShippingMethodId = $(".jsChangeShipment:checked", $billingShippingMethods).val();
        $("input[value='" + $selectedShippingMethodId + "']").prop('checked', true);
        $("#AlternativeAddressButton").hide();
        $(".billing-shipping-method").hide();
        $(".shipping-address:hidden").slideToggle(300);
        $("#UseBillingAddressForShipment").val("False");

    },
    removeShippingAddress: function (event) {

        event.preventDefault();

        var $billingShippingMethods = $(".billing-shipping-method");
        var $selectedShippingMethodId = $(".jsChangeShipment:checked", $(this).closest(".shipping-address")).val();
        $("#" + $selectedShippingMethodId).prop('checked', true);
        $("#AlternativeAddressButton").show();
        $(".billing-shipping-method").show();
        $(".shipping-address:visible").slideToggle(300);
        $("#UseBillingAddressForShipment").val("True");

    }
};