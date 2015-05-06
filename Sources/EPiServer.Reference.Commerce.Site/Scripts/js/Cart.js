var Cart = {
    init: function() {
        $(document)
            .on('focusout', '.jsQuantity', Cart.changeQuantity)
            .on('click', '.jsRemoveLineItem', Cart.removeLineItem)
            .on('click', '.jsCartToggle', Cart.details)
            .on('change', '.jsMiniCartQuantity', Cart.changeQuantityMiniCart)
            .on('submit', '.jsAddToCart', Cart.addToCart)
            .on('submit', '.jsAddToWishList', Cart.addToWishList)
            .on('submit', '.jsRemoveFromWishList', Cart.removeFromWishlist)
            .on('click', '.jsCartDropdown', function(e) {
                return $(e.target).hasClass('btn');
            });
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
                $('.cart-items').text($('.jsCartDropdown').data('item-count'));
            }
        });
    },
    changeQuantity: function(e) {
        e.preventDefault();
        var form = $(this).parent();
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function(result) {
                $('.jsLargeCart').replaceWith($(result).filter('.jsLargeCart'));
                Checkout.updateOrderSummary();
            }
        });
    },
    removeLineItem: function(e) {
        e.preventDefault();
        var form = $(this).closest('form');
        var lineItemRow = $(this).closest('tr');
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
                $('.cart-items').text($('.jsCartDropdown').data('item-count'));
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
            success: function(result) {
                $('.jsCartDropdown').replaceWith($(result).first());
                $(".btn.btn-cart.dropdown-toggle").trigger("click");
                $('.cart-items').text($('.jsCartDropdown').data('item-count'));
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
            success: function(result) {
                $('.wishListResult').show();
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
    details : function(e) {
        e.preventDefault();
        if ($('.jsCartDropdown').children().length == 0) {
            $.ajax({
                type: "GET",
                url: $(this).data('url'),
                success: function(result) {
                    $('.jsCartDropdown').replaceWith($(result).first());
                }
            });
        }
    }
};