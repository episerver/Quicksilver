var addressDropDown;

var AddressBook = {
    init: function () {
        $(document).on("change", ".jsChangeCountry", AddressBook.setRegion);
        $(document).on("click", ".save-address-ajax-button", AddressBook.saveNewAddress);
    },
    setRegion: function () {
        var $countryCode = $(this).val();
        var $addressRegionContainer = $(".address-region", $(this).parents().eq(1));
        var $region = $(".address-region-input", $addressRegionContainer).val();
        var $htmlPrefix = $("input[name='address-htmlfieldprefix']", $(this).parent()).val();
        var $url = "/AddressBook/GetRegionsForCountry/";

        $.ajax({
            type: "POST",
            url: $url,
            data: { countryCode: $countryCode, region: $region, htmlPrefix: $htmlPrefix },
            success: function (result) {
                $addressRegionContainer.replaceWith($(result));
            }
        });
    },
    showNewAddressDialog: function (sender) {

        addressDropDown = $(sender).siblings(".address-dropdown");

        var url = $(sender).data("url");

        $.ajax({
            type: "GET",
            cache: false,
            url: url,
            success: function (result) {
                AddressBook.setAddressFormAsModalContent(result);
            }
        });
    },
    setAddressFormAsModalContent: function(view)
    {
        $("#AddressDialogContent").html($(view));
        $('button[type="submit"]', $("#AddressDialog")).addClass("save-address-ajax-button");
    },
    saveNewAddress: function (e) {

        e.preventDefault();

        var form = $(this).closest("form");

        $.ajax({
            type: "POST",
            cache: false,
            url: form[0].action,
            data: form.serialize(),
            success: function (result) {

                if (result.AddressId) {
                    $('.address-dropdown').each(function () {
                        $('option[value=""]', this).remove();
                    });

                    var option = $('<option>');
                    option.attr('value', result.AddressId).text(result.Name);                    
                    $('.address-dropdown').append(option);
                    $("#AddressDialog").modal("hide");

                    if (addressDropDown) {
                        addressDropDown.val(result.AddressId);
                    }

                    $('.address-dropdown').each(function () {
                        Checkout.changeTaxAddress($(this));
                    });
                    return;
                }

                AddressBook.setAddressFormAsModalContent(result);
            }
        });
    }
};