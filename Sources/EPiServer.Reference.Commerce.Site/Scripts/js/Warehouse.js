﻿var Warehouse = {
    init: function () {
        $(document).on('click', '.warehouse-quickview-button', Warehouse.showQuickview);
        Warehouse.showWarehouseAvailability();
    },
    showWarehouseAvailability: function () {
        var container = $(".warehouse-availability-container");
        if (container.length) {
            var skuCode = container.data("sku-code");
            var url = container.data("url");
            $.ajax({
                type: "GET",
                cache: false,
                url: url,
                data: { skuCode: skuCode },
                success: function (result) {
                    container.html($(result));
                }
            });
        }
    },
    showQuickview: function (e) {
        e.preventDefault();
        var warehouseCode = $(this).data("warehouse-code");
        var url = $(this).data("url");
        $.ajax({
            type: "GET",
            cache: false,
            url: url,
            data: { warehouseCode: warehouseCode },
            success: function (result) {
                $(".modal-dialog", $("#ModalDialog")).html($(result));
            }
        });
    }
};