var AddressBook = {
    init: function () {
        $(document)
            .on("change", ".jsCountry", AddressBook.setRegion);
    },
    setRegion: function () {
        var $countryCode = $(this).val();
        var $region = $("#Region").val();
        var $url = $("#AddressForm").attr("action").replace("/Save/", "/GetRegionsForCountry/");
        $.ajax({
            type: "POST",
            url: $url,
            data: { countryCode: $countryCode, region: $region },
            success: function (result) {
                $("#AddressRegion").replaceWith($(result));
            }
        });
    }
};