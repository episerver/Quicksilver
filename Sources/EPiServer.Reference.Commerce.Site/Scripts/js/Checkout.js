var Checkout = {
    init: function () {
        $(document)
            .on('change', '.jsChangePayment', Checkout.changePayment)
            .on('change', '.jsChangeAddress', Checkout.changeAddress)
            .on('change', '.jsChangeTaxAddress', Checkout.changeTaxAddress)
            .on('change', '#MiniCart', Checkout.refreshView)
            .on('change', '#MiniCartResponsive', Checkout.refreshView)
            .on('change', '#CheckoutView .jsChangeCartItem', Checkout.refreshView)
            .on('click', '.jsNewAddress', Checkout.newAddress)
            .on('click', '#AlternativeAddressButton', Checkout.enableShippingAddress)
            .on('click', '.remove-shipping-address', Checkout.removeShippingAddress)
            .on('click', '.js-add-couponcode', Checkout.addCouponCode)
            .on('click', '.js-remove-couponcode', Checkout.removeCouponCode);

        Checkout.initializeAddressAreas();
    },
    initializeAddressAreas: function () {

        if ($("#UseBillingAddressForShipment").val() == "False") {
            Checkout.doEnableShippingAddress();
        }
        else {
            Checkout.doRemoveShippingAddress();
        }
    },
    addCouponCode: function (e) {
        e.preventDefault();
        var couponCode = $(inputCouponCode).val();
        if (couponCode.trim()) {
            $.ajax({
                type: "POST",
                url: $(this).data("url"),
                data: { couponCode: couponCode },
                success: function (result) {
                    if (!result) {
                        $('.couponcode-errormessage').show();
                        return;
                    }
                    $('.couponcode-errormessage').hide();
                    $("#CheckoutView").replaceWith($(result));
                    Checkout.initializeAddressAreas();
                }
            });
        }
    },
    removeCouponCode: function (e) {
        e.preventDefault();
        $.ajax({
            type: "POST",
            url: $(this).attr("href"),
            data: { couponCode: $(this).siblings().text() },
            success: function (result) {
                $("#CheckoutView").replaceWith($(result));
                Checkout.initializeAddressAreas();
            }
        });
    },
    refreshView: function () {

        var view = $("#CheckoutView");

        if (view.length == 0) {
            return;
        }
        var url = view.data('url');
        $.ajax({
            cache: false,
            type: "GET",
            url: view.data('url'),
            success: function (result) {
                view.replaceWith($(result));
                Checkout.initializeAddressAreas();
            }
        });
    },
    newAddress: function (e) {
        e.preventDefault();
        AddressBook.showNewAddressDialog($(this));
    },
    changeAddress: function () {
        var form = $('.jsCheckoutForm');
        var id = $(this).attr("id");
        var isBilling = id.indexOf("Billing") > -1;
        if (isBilling) {
            $("#ShippingAddressIndex").val(-1);
        } else {
            $("#ShippingAddressIndex").val($(".jsChangeAddress").index($(this)) - 1);
        }

        $.ajax({
            type: "POST",
            cache: false,
            url: $(this).closest('.jsCheckoutAddress').data('url'),
            data: form.serialize(),
            success: function (result) {
                if (isBilling) {
                    $("#billingAddressContainer").html($(result));
                } else {
                    $("#AddressContainer").html($(result));
                }
                Checkout.initializeAddressAreas();
                Checkout.updateOrderSummary();
            }
        });
    },
    changeTaxAddress: function (sender) {
        if (sender.originalEvent instanceof Event) {
            sender = $(this);
        }
        var id = $(sender).attr("id");
        if ((id.indexOf("Billing") > -1) && $("#UseBillingAddressForShipment").val() == "False") {
            return;
        }
        var form = $('.jsCheckoutForm');
        if (form.length == 0)
        {
            return;
        }
        $.ajax({
            type: "POST",
            cache: false,
            url: $(sender).closest('.jsCheckoutAddress').data('url'),
            data: form.serialize(),
            success: function (result) {
                Checkout.updateOrderSummary();
            }
        });
    },
    changePayment: function () {
        $.ajax({
            type: "POST",
            url: $(this).data('url'),
            success: function (result) {
                $('.jsPaymentMethod').replaceWith($(result).find('.jsPaymentMethod'));
                Checkout.updateOrderSummary();
                Misc.updateValidation('jsCheckoutForm');
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
    doEnableShippingAddress: function () {
        $("#AlternativeAddressButton").hide();
        $(".shipping-address:hidden").slideToggle(300);
        $(".shipping-address").css("display", "block");
        $("#UseBillingAddressForShipment").val("False");        
    },
    enableShippingAddress: function (event) {

        event.preventDefault();

        Checkout.doEnableShippingAddress();
        
        var form = $('.jsCheckoutForm');
        $("#ShippingAddressIndex").val(0);

        $.ajax({
            type: "POST",
            cache: false,
            url: $('.jsCheckoutAddress').data('url'),
            data: form.serialize(),
            success: function (result) {
                $("#AddressContainer").html($(result)); 
                Checkout.initializeAddressAreas();
                Checkout.updateOrderSummary();
            }
        });
    },
    doRemoveShippingAddress: function() {
        $("#AlternativeAddressButton").show();
        $(".shipping-address:visible").slideToggle(300);
        $(".shipping-address").css("display", "none");
        $("#UseBillingAddressForShipment").val("True");        
    },
    removeShippingAddress: function (event) {

        event.preventDefault();

        Checkout.doRemoveShippingAddress();

        var form = $('.jsCheckoutForm');
        $("#ShippingAddressIndex").val(-1);

        $.ajax({
            type: "POST",
            cache: false,
            url: $('.jsCheckoutAddress').data('url'),
            data: form.serialize(),
            success: function (result) {
                Checkout.initializeAddressAreas();
                Checkout.updateOrderSummary();
            }
        });
    }
};