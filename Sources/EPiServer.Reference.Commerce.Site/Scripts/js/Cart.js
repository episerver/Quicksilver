var Cart = {
    init: function() {
        $(document)
            .on('focusout', '.jsQuantity', Cart.changeQuantityOnFocusout)
            .on('submit', '.jsQuantitySubmit', Cart.changeQuantityOnSubmit)
            .on('click', '.jsRemoveLineItem', Cart.removeLineItem)
            .on('change', '.jsMiniCartQuantity', Cart.changeQuantityMiniCart)
            .on('submit', '.jsAddToCart', Cart.addToCart)
            .on('submit', '.jsAddToWishList', Cart.addToWishList)
            .on('submit', '.jsRemoveFromWishList', Cart.removeFromWishlist)
            .on('click', '.jsCartDropdown', function(e) {
                return $(e.target).hasClass('btn');
            });
        $('#miniCartDropdown').on('show.bs.dropdown', Cart.showingMiniCartDropdown);
    },
    changeQuantityMiniCart: function(e) {
        e.preventDefault();
        var form = $(this).closest("form");
        if ($('.jsMiniCartQuantity').val() == '')
            $('.jsMiniCartQuantity').val('0');
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function(result) {

                $('.jsCartDropdown').replaceWith($(result).first());
                $('.jsCartItemCount').text($('.jsCartDropdown').data('item-count'));
            }
        });
    },
    changeQuantityOnFocusout: function (e) {
        e.preventDefault();
        var form = $(this).parent();
        Cart.changeQuantity(form);
    },
    changeQuantityOnSubmit: function (e) {
        e.preventDefault();
        var form = $(this);
        Cart.changeQuantity(form);
    },
    changeQuantity: function (form) {
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (result) {
                $('.jsLargeCart').replaceWith($(result).filter('.jsLargeCart'));
                Checkout.updateOrderSummary();
            }
        });
    },
    removeLineItem: function(e) {
        e.preventDefault();
        var form = $(this).closest('form');
        var lineItemRow = $(this).closest('.jsLineItemRow');
        lineItemRow.hide();
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function(result) {
                $('.jsLargeCart').replaceWith($(result).filter('.jsLargeCart'));
                Checkout.updateOrderSummary();
                Cart.updateMiniCartSummary();
            },
            error: function () {
                lineItemRow.show();
            }
        });
    },
    updateMiniCartSummary: function() {
        $.ajax({
            type: "GET",
            url: $('.jsCartToggle').data('url'),
            success: function (result) {
                $('.jsCartDropdown').replaceWith($(result).first());
                $('.jsCartItemCount').text($('.jsCartDropdown').data('item-count'));
            }
        });
    },
    addToCart: function(e) {
        e.preventDefault();
        var form = $(this);
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            context: this,
            success: function(result) {
                $('.jsCartDropdown').replaceWith($(result).first());
                $('.jsCartToggle').trigger("click");
                $('.jsCartItemCount').text($('.jsCartDropdown').data('item-count'));

                var isWishList = $(this).data('iswishlist');
                if (isWishList) {
                    $(this).closest('.jsProductTile').remove();
                }
            }
        });
    },
    addToWishList: function(e) {
        e.preventDefault();
        var form = $(this);
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (result) {
                $('.jsWishListResult').text(result.message);
                $('.jsWishListResult').show();

                if (result.added) {
                    $('.jsWishListResult').addClass("alert-info");
                } else {
                    $('.jsWishListResult').addClass("alert-warning");
                }
            }
        });
    },
    removeFromWishlist: function(e) {
        e.preventDefault();
        $.ajax({
            type: "POST",
            url: this.action,
            data: $(this).serialize(),
            context: this,
            success: function() {
                $(this).closest('.jsProductTile').remove();
            }
        });
    },
    showingMiniCartDropdown: function (e) {
        if ($('.jsCartDropdown').data('item-count') == 0) {
            //if there are no items in the cart, don't show the dropdown
            e.preventDefault();
        }
    }
};