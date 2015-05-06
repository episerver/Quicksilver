var AddressBook = {
    init: function () {
        $(document)
            .on('change', '.jsCountry', AddressBook.setRegion);
    },
    setRegion: function () {
        var form = $(this).closest('form');
        $.ajax({
            type: "POST",
            url: "/my-pages/address-book/EditForm/",
            data: form.serialize(),
            context: form,
            success: function (result) {
                $('.jsAddressBook').replaceWith(result);
            }
        });
    }
};