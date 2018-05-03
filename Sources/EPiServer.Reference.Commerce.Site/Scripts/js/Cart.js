var Cart = {
    init: function () {

        $(document)
            .on('keypress', '.jsChangeCartItem', Cart.preventSubmit)
            .on('click', '.jsRemoveCartItem', Cart.removeCartItem)
            .on('change', '.jsChangeCartItem', Cart.changeCartItem)
            .on('change', '.jsChangeShipment', Cart.changeShipment)
            .on('click', '.jsAddToCart', Cart.addCartItem)
            .on('change', '#MiniCart', function () { $("#MiniCartResponsive > div:first").html($("div:first", this).html()); })
            .on('change', '#WishListMiniCart', function () { $("#WishListMiniCartResponsive > div:first").html($("div:first", this).html()); })
            .on('click', '.jsCartContinueShopping', function () {
                if ($(this).closest('#cart-dropdown')) {
                    $(this).closest('#cart-dropdown').collapse('hide');
                }                 
            })
            .on('click', '.jsWishListContinueShopping', function () {
                if ($(this).closest('#wishlist-dropdown')) {
                    $(this).closest('#wishlist-dropdown').collapse('hide');
                }                
            })
            .on('click', '.jsCartDropdown', function (e) {
                return ($(e.target).hasClass('btn') || $(e.target).parent().is('a'));
            });

        $('.cart-dropdown').on('show.bs.dropdown', function (e) {
            if ($('#CartItemCount', $(this)).val() === 0) {
                e.preventDefault();
            }
        });

    },
    changeShipment: function () {
        var container = $(this).closest('.shipping-method');
        var url = container.data('url');
        $.ajax({
            type: "POST",
            url: url,
            cache: false,
            success: function (result) {
                Checkout.updateOrderSummary();
            }
        });
    },
    changeCartItem: function (e) {

        e.preventDefault();
        var form = $(this).closest("form");
        var quantity = $("#quantity", form).val();

        if (parseInt(quantity, 10) < 0) {
            return;
        }

        var formContainer = $("#" + form.data("container"));
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (result) {

                formContainer.html($(result));
                $('.cartItemCountLabel', formContainer.parent()).text($('#CartItemCount', formContainer).val());
                $('.cartTotalAmountLabel', formContainer.parent()).text($('#CartTotalAmount', formContainer).val());

                if (formContainer.is($('#WishListMiniCart'))) {
                    if (result.indexOf('list-group-item') === -1) {
                        $('.delete-wishlist').hide();
                    }
                    // If items where removed from the wishlist cart from the wishlist view, they should be removed from the view.
                    var wishListAction = form.closest(".wishlist-actions");
                    if (wishListAction.length > 0) {
                        wishListAction.parent().remove();
                    }
                }
            }
        });

    },
    removeCartItem: function (e) {

        e.preventDefault();
        var form = $(this).closest('form');
        $("#quantity", form).val(0).change();
        $(this).closest(".jsProductTile").remove();
        if (!$(".jsProductTile").length) {
            $(".wishlist-noitem").show();
        }
    },
    addCartItem: function (e) {

        e.preventDefault();
        var form = $(this).closest("form");
        var formContainer = $("#" + form.data("container"));
        var skuCode = $("#code", form).val();

        $("#CartWarningMessage").hide()
        $(".warning-message", $("#CartWarningMessage")).html("");

        $.ajax({
            type: "POST",
            url: form[0].action,
            data: { code: skuCode },
            success: function (result) {

                formContainer.html($(result));
                $('.cartItemCountLabel', formContainer.parent()).text($('#CartItemCount', formContainer).val());
                $('.cartTotalAmountLabel', formContainer.parent()).text($('#CartTotalAmount', formContainer).val());

                formContainer.change();
            },
            error: function (xhr, status, error) {
                $(".warning-message", $("#CartWarningMessage")).html(xhr.statusText);
                $("#CartWarningMessage").show();
            }
        });
    },
    preventSubmit: function (e) {
        if (e.keyCode === 13) {
            e.preventDefault();
        }
    }
};