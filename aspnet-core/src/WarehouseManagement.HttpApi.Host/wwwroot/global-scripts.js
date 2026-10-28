(function () {
    function enhanceLoginForm() {
        var userField = document.getElementById('LoginInput_UserNameOrEmailAddress') ||
                        document.querySelector('input[name="LoginInput.UserNameOrEmailAddress"]') ||
                        document.querySelector('input[type="text"]');
        if (userField && !userField.getAttribute('placeholder')) {
            userField.setAttribute('placeholder', 'Enter your email address');
        }

        var passField = document.getElementById('LoginInput_Password') ||
                        document.querySelector('input[name="LoginInput.Password"]') ||
                        document.querySelector('input[type="password"]');
        if (passField && !passField.getAttribute('placeholder')) {
            passField.setAttribute('placeholder', 'Enter your password');
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', enhanceLoginForm);
    } else {
        enhanceLoginForm();
    }
    setTimeout(enhanceLoginForm, 150);
    setTimeout(enhanceLoginForm, 500);
})();
