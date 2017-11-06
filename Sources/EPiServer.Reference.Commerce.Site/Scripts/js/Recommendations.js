var Recommendations = {
    init: function () {
        this._bindEvents();
    },

    clickTracking: function (evt) { 
        var recommendationId = $(evt.delegateTarget).data("recommendation-id");
        Misc && Misc.setCookie("EPiServer_Commerce_RecommendationId", recommendationId, 60);//set lifetime of this cookie to only 1 minute.
    },

    keyboardHandle: function (evt) {
        if ((evt.type === "keydown" && evt.which === 13) || (evt.type === "contextmenu" && evt.which !== 3)) {
            Recommendations.clickTracking(evt); //handle keyup event of enter key, contextmenu event of menu key.
        }
    },

    // summary:
    //      Renders the recommended items received inside the given target container.
    // returnedData: [Object]
    //      The recommendations data.
    // contextSettings: [Object]
    //      The context settings used to render recommendations, that included:
    //      sectionMappings: [Object]
    //          area: [string]
    //              The area that recommendations belong to.
    //          selector: [string]
    //              The selector to target container (a HTML element).
    // tags:
    //      public, recommendations
    render: function (returnedData, contextSettings) {
        var data = null;
        try {
            data = JSON.parse(returnedData);
        } catch (e) {
            return;
        }

        var sectionMappings = contextSettings.sectionMappings;
        if (!(sectionMappings instanceof Array) || sectionMappings.length === 0) {
            return;
        }

        // Loads and parses templates.
        var listTemplate = $("#epiRecommendationListTemplate").html(),
            itemTemplate = $("#epiRecommendationItemTemplate").html();
        Mustache.parse(listTemplate);
        Mustache.parse(itemTemplate);

        // Gets selected currency.
        var selectedCurrency = Market.getSelectedCurrency(),
            setDisplayPrice = function (recs) {
                for (var i = 0, length = recs.length; i < length; i++) {
                    var selectedPrice = recs[i].prices[selectedCurrency];
                    if (!selectedPrice) {
                        continue;
                    }

                    recs[i].hasDiscount = selectedPrice.unitPrice > selectedPrice.salePrice;
                    recs[i].unitPrice = selectedPrice.unitPrice;
                    recs[i].salePrice = selectedPrice.salePrice;
                }

                return recs;
            };

        var renderRecommendations = function (data, targetArea, targetSelector, numberOfItemsToRender) {
            var $target = $(targetSelector);
            if ($target.length === 0) {
                return;
            }

            // Get indicated recommendations data.
            var recommendations = {
                recs: [],
                getRelativeUrl: function () {
                    return function (text, render) {
                        if (this.hasOwnProperty(text)) {
                            var a = document.createElement("a");
                            a.href = this[text];

                            return a.pathname;
                        } else {
                            return text;
                        }
                    }
                },
                formatPrice: function () {
                    return function (text, render) {
                        var localeSettings = contextSettings.localeSettings,
                            formattedPrice;
                        if (this.hasOwnProperty(text)) {
                            formattedPrice = this[text].toLocaleString(localeSettings.culture, {
                                style: "currency",
                                currency: localeSettings.currencyCode,
                                minimumFractionDigits: localeSettings.currencyDigits
                            });

                            formattedPrice = formattedPrice
                                .replace(localeSettings.currencyCode, localeSettings.currencySymbol)
                                .replace(localeSettings.currencySymbol, "<span class='product-price__currency-marker'>" + localeSettings.currencySymbol + "</span>");

                            // Special handling for Saudi Arabia
                            if (localeSettings.currencyCode === "SAR") {
                                var priceInSAR = this[text].toLocaleString(localeSettings.culture, { minimumFractionDigits: localeSettings.currencyDigits });
                                formattedPrice = formattedPrice.replace(priceInSAR, this[text].toFixed(localeSettings.currencyDigits));
                            }
                        } else {
                            formattedPrice = localeSettings.notAvailable;
                        }

                        return "<span>" + formattedPrice + "</span>";
                    }
                }
            };
            for (var i = 0, length = data.smartRecs.length; i < length; i++) {
                if (data.smartRecs[i].position === targetArea) {
                    recommendations.recs = recommendations.recs.concat(setDisplayPrice(data.smartRecs[i].recs));
                }
            }
            if (numberOfItemsToRender > 0) {
                recommendations.recs = recommendations.recs.slice(0, numberOfItemsToRender);
            }

            // Renders recommended data with loaded templates.
            var htmlContent = Mustache.render(listTemplate, recommendations, { epiRecommendationItemTemplate: itemTemplate });
            $target.html(htmlContent);
        };

        for (var i = 0, length = sectionMappings.length; i < length; i++) {
            var targetArea = sectionMappings[i].area,
                targetSelector = sectionMappings[i].selector,
                numberOfItemsToRender = sectionMappings[i].numberOfItemsToRender;
            renderRecommendations(data, targetArea, targetSelector, numberOfItemsToRender);
        }

        // Bind events for new rendered elements.
        Recommendations._bindEvents();
    },

    _bindEvents: function () {
        $(".recommendations").find("[data-recommendation-id]").on("click", ".quick-view-btn-container > button", Recommendations.clickTracking);
        $(".recommendations").find("[data-recommendation-id]").on("mouseup", ".product > a", Recommendations.clickTracking);
        $(".recommendations").find("[data-recommendation-id]").on("contextmenu", ".product > a", Recommendations.keyboardHandle);
        $(".recommendations").find("[data-recommendation-id]").on("keydown", ".product > a, .product img", Recommendations.keyboardHandle);
    }
};