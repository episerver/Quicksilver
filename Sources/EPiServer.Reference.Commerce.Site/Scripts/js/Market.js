var Market = {
    init: function () {
        $(document)
            .on("change ", ".jsMarketSelector", Market.setMarket)
            .on("change ", ".jsLanguageSelector", Market.setLanguage)
            .on("change ", ".jsCurrencySelector", Market.setCurrency);
    },
    setMarket: function () {
        var form = $(this).closest("form");
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (response) {
                document.location = response.returnUrl;
            }
        });
    },
    setLanguage: function (e) {
        var form = $(this).closest("form");
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (response) {
                document.location = response.returnUrl;
            }
        });
    },
    setCurrency: function (e) {
        var form = $(this).closest("form");
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (response) {
                document.location = response.returnUrl;
            }
        });
    },
    getSelectedCurrency: function () {
        return $("#CurrencyCode").val() || "USD";
    },
    getSelectedMarketId: function () {
        return $("#MarketId").val() || "US";
    }
}