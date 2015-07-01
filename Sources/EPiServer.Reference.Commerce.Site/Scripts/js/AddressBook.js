var AddressBook = {
    init: function () {
        $(document).on("change", ".jsChangeCountry", AddressBook.setRegion);
    },
    setRegion: function () {
        var $countryCode = $(this).val();
        var $addressRegionContainer = $(".address-region", $(this).parents().eq(1));
        var $region = $(".address-region-input", $addressRegionContainer).val();
        var $url = "/AddressBook/GetRegionsForCountry/";

        $.ajax({
            type: "POST",
            url: $url,
            data: { countryCode: $countryCode, region: $region },
            success: function (result) {
                $addressRegionContainer.replaceWith($(result));
            }
        });
    }
};