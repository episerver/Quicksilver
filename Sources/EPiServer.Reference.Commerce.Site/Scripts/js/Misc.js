
var Misc = {
    init: function () {
        if (Misc.getCookie('AcceptedCookies') != 1) {
            $(document).on('click', '.jsCookies', Misc.acceptCookies);
            $('.jsCookies').show();
        }
    },
    acceptCookies: function () {
        var date = new Date();
        date.setTime(date.getTime() + (365 * 24 * 60 * 60 * 1000));
        document.cookie = 'AcceptedCookies=1; expires=' + date.toUTCString() + '; path=/';
        $('.jsCookies').hide();
    },
    getCookie: function (cname) {
        var name = cname + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1);
            if (c.indexOf(name) != -1) return c.substring(name.length, c.length);
        }
        return "";
    },
    updateValidation: function (formClass) {
        var currForm = $('.' + formClass);
        currForm.removeData("validator");
        currForm.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(currForm);
        currForm.validate();
    }
};