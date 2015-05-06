var Market = {
    init: function () {
        $(document)
            .on('click', '.jsMarketSelector .dropdown-menu a', Market.setMarket)
            .on('click', '.jsLanguageSelector .dropdown-menu a', Market.setLanguage)
            .on('click', '.jsCurrencySelector .dropdown-menu a', Market.setCurrency);
    },
    setMarket: function (e) {
        e.preventDefault();
        $.ajax({
            type: "POST",
            url: $(this).data('url'),
            data: 'marketId=' + $(this).data('marketid'),
            success: function (response) {
                document.location = response.returnUrl;
            }
        });
    },
    setLanguage: function (e) {
        e.preventDefault();
        $.ajax({
            type: "POST",
            url: $(this).data('url'),
            data: 'language=' + $(this).data('language'),
            success: function (response) {
                document.location = response.returnUrl;
            }
        });
    },
    setCurrency: function (e) {
        e.preventDefault();
        $.ajax({
            type: "POST",
            url: $(this).data('url'),
            data: 'currencyCode=' + $(this).data('currency-code'),
            success: function (response) {
                document.location = response.returnUrl;
            }
        });
    }
}