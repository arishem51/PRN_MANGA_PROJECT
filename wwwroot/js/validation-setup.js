$(document).ready(function () {
    if ($.validator) {
        $.validator.setDefaults({
            onkeyup: false, // không validate khi gõ
            onclick: false, // không validate khi click
            onfocusout: function (element) {
                this.element(element); // validate khi rời input
            }
        });
    }
});
