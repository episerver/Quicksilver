var ProductPage = {
    init: function () {
        $(document).on('change', '.jsVariationSwitch', ProductPage.switchVariant);
        $(document).on('click', '.quickview-button', ProductPage.showQuickview);

        ProductPage.resetCarousel();
        $('#product-carousel').carousel({
            interval: 6000
        });
        $('#carousel-thumbs').on("click", "a", function () {
            var thumbId = $(this).attr("id");
            var thumbId = parseInt(thumbId);
            $('#product-carousel').carousel(thumbId);

            return false;
        });
    },
    resetCarousel: function () {
        $('.carousel-inner .item:first-child()').addClass("active");
    },
    switchVariant: function () {
        var form = $(this).closest('form');
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (result) {
                $('.jsProductDetails').replaceWith($(result));
                ProductPage.resetCarousel();
            },
            error: function () {
                $('.jsAddToCart button').addClass('disabled');
                alert('The variant is not available.');
            }
        });
    },
    showQuickview: function (e) {
        e.preventDefault();
        var skuCode = $(this).data("code");
        var url = $(this).data("url");
        $.ajax({
            type: "GET",
            cache: false,
            url: url,
            data: { variationCode: skuCode, quickview: true },
            success: function (result) {
                $(".modal-dialog", $("#Quickview")).html($(result));
            }
        });
    }
};