var Recommendations = {
    init: function () {
        $(".recommendations").find("[data-recommendation-id]").on("click", ".quick-view-btn-container > button", Recommendations.clickTracking);
        $(".recommendations").find("[data-recommendation-id]").on("mouseup", ".product > a", Recommendations.clickTracking);
        $(".recommendations").find("[data-recommendation-id]").on("contextmenu", ".product > a", Recommendations.keyboardHandle);
        $(".recommendations").find("[data-recommendation-id]").on("keydown", ".product > a, .product img", Recommendations.keyboardHandle);
    },

    clickTracking: function (evt) { 
        var recommendationId = $(evt.delegateTarget).data("recommendation-id");
        Misc && Misc.setCookie("EPiServer_Commerce_RecommendationId", recommendationId, 60);//set lifetime of this cookie to only 1 minute.
    },

    keyboardHandle: function (evt) {
        if ((evt.type == "keydown" && evt.which == 13) || (evt.type == "contextmenu" && evt.which != 3)) {
            Recommendations.clickTracking(evt); //handle keyup event of enter key, contextmenu event of menu key.
        }
    }
};