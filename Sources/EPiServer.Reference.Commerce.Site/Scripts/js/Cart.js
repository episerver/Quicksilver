var Cart = {
    init: function () {

        $(document)
            .on('click', '.jsRemoveCartItem', Cart.removeCartItem)
            .on('change', '.jsChangeCartItem', Cart.changeCartItem)
            .on('click', '.jsAddToCart', Cart.addCartItem)
            .on('change', '#MiniCart', function () { $("#MiniCartResponsive").html($(this).html()); })
            .on('change', '#WishListMiniCart', function () { $("#WishListMiniCartResponsive").html($(this).html()); })
            .on('click', '.jsCartDropdown', function (e) {
                return ($(e.target).hasClass('btn') || $(e.target).parent().is('a'));
            });

        $('.cart-dropdown').on('show.bs.dropdown', function (e) {
            if ($('#CartItemCount', $(this)).val() == 0) {
                e.preventDefault();
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

                formContainer.change();

                if (formContainer.is($('#WishListMiniCart'))) {
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

                if (formContainer.is($('#MiniCart'))) {
                    // If items where added to the cart from the wishlist view, they should be removed from the view.
                    var wishListAction = form.closest(".wishlist-actions");
                    if (wishListAction.length > 0) {
                        wishListAction.closest(".product").remove();
                    }

                    // If items were added to the cart while the same item exists in the wishlist, they should be removed
                    // from the wishlist.
                    var wishListItems = $("." + skuCode, $('#WishListMiniCart'));
                    if (wishListItems.length > 0) {
                        Cart.refreshWishList();
                    }
                }
            },
            error: function (xhr, status, error) {
                $(".warning-message", $("#CartWarningMessage")).html(xhr.statusText);
                $("#CartWarningMessage").show();
            }
        });
    },
    refreshWishList: function () {

        $.ajax({
            type: "GET",
            url: "/WishList/WishListMiniCartDetails",
            cache: false,
            success: function (result) {
                var formContainer = $('#WishListMiniCart');
                formContainer.html($(result));
                $('.cartItemCountLabel', formContainer.parent()).text($('#CartItemCount', formContainer).val());
            }
        });

    },
    removeFromWishlist: function (e) {

        e.preventDefault();
        $.ajax({
            type: "POST",
            url: this.action,
            data: $(this).serialize(),
            context: this,
            success: function () {
                $(this).closest('.jsProductTile').remove();
            }
        });
    }
};