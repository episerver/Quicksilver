
var Misc = {
    init: function () {
        if (Misc.getCookie("AcceptedCookies") != 1) {
            $(document).on("click", ".jsCookies", Misc.acceptCookies);
            $(".jsCookies").show();
        }
    },
    acceptCookies: function () {
        Misc.setCookie("AcceptedCookies", "1", (365 * 24 * 60 * 60));
        $(".jsCookies").hide();
    },
    setCookie: function (cname, cvalue, exSeconds) {
        //  Session cookie
        if (!exSeconds) {
            document.cookie = cname + "=" + cvalue + "; path=/";
            return;
        }

        var date = new Date();
        date.setTime(date.getTime() + (exSeconds * 1000));
        document.cookie = cname + "=" + cvalue + "; expires=" + date.toUTCString() + "; path=/";
    },
    getCookie: function (cname) {
        var name = cname + "=";
        var ca = document.cookie.split(";");
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == " ") c = c.substring(1);
            if (c.indexOf(name) != -1) return c.substring(name.length, c.length);
        }
        return "";
    },
    updateValidation: function (formClass) {
        var currForm = $("." + formClass);
        currForm.removeData("validator");
        currForm.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(currForm);
        currForm.validate();
    }
};