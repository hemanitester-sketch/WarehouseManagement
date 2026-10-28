$(function () {
    $('#LoginInput_UserNameOrEmailAddress, input[name="LoginInput.UserNameOrEmailAddress"]').attr('placeholder', 'Enter your email address');
    $('#LoginInput_Password, input[name="LoginInput.Password"]').attr('placeholder', 'Enter your password');

    $("#PasswordVisibilityButton").click(function (e) {
        let button = $(this);
        let passwordInput = button.parent().find("input");
        if (!passwordInput) {
            return;
        }

        if (passwordInput.attr("type") === "password") {
            passwordInput.attr("type", "text");
        }
        else {
            passwordInput.attr("type", "password");
        }

        let icon = button.find("i");
        if (icon) {
            icon.toggleClass("fa-eye-slash").toggleClass("fa-eye");
        }
    });
});
