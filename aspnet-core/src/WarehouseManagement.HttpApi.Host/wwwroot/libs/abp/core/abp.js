(function() {
    function setPlaceholders() {
        var u = document.getElementById('LoginInput_UserNameOrEmailAddress') || document.querySelector('input[name*="UserName"]');
        if (u) {
            u.placeholder = 'Enter your email address';
            u.setAttribute('placeholder', 'Enter your email address');
        }
        var p = document.getElementById('LoginInput_Password') || document.querySelector('input[name*="Password"]');
        if (p) {
            p.placeholder = 'Enter your password';
            p.setAttribute('placeholder', 'Enter your password');
        }
    }
    setPlaceholders();
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', setPlaceholders);
    }
    window.addEventListener('load', setPlaceholders);
    var t = setInterval(setPlaceholders, 50);
    setTimeout(function() { clearInterval(t); }, 4000);
})();

var abp = abp || {};
(function () {

    /* Application paths *****************************************/

    //Current application root path (including virtual directory if exists).
    var baseElement = document.querySelector('base');
    var baseHref = baseElement ? baseElement.getAttribute('href') : null;
    abp.appPath = baseHref || abp.appPath || '/';

    abp.pageLoadTime = new Date();

    //Converts given path to absolute path using abp.appPath variable.
    abp.toAbsAppPath = function (path) {
        if (path.indexOf('/') == 0) {
            path = path.substring(1);
        }

        return abp.appPath + path;
    };

    /* LOGGING ***************************************************/
    //Implements Logging API that provides secure & controlled usage of console.log

    abp.log = abp.log || {};

    abp.log.levels = {
        DEBUG: 1,
        INFO: 2,
        WARN: 3,
        ERROR: 4,
        FATAL: 5
    };

    abp.log.level = abp.log.levels.DEBUG;

    abp.log.log = function (logObject, logLevel) {
        if (!window.console || !window.console.log) {
            return;
        }

        if (logLevel != undefined && logLevel < abp.log.level) {
            return;
        }

        console.log(logObject);
    };

    abp.log.debug = function (logObject) {
        abp.log.log("DEBUG: ", abp.log.levels.DEBUG);
        abp.log.log(logObject, abp.log.levels.DEBUG);
    };

    abp.log.info = function (logObject) {
        abp.log.log("INFO: ", abp.log.levels.INFO);
        abp.log.log(logObject, abp.log.levels.INFO);
    };

    abp.log.warn = function (logObject) {
        abp.log.log("WARN: ", abp.log.levels.WARN);
        abp.log.log(logObject, abp.log.levels.WARN);
    };

    abp.log.error = function (logObject) {
        abp.log.log("ERROR: ", abp.log.levels.ERROR);
        abp.log.log(logObject, abp.log.levels.ERROR);
    };

    abp.log.fatal = function (logObject) {
        abp.log.log("FATAL: ", abp.log.levels.FATAL);
        abp.log.log(logObject, abp.log.levels.FATAL);
    };

    /* LOCALIZATION ***********************************************/

    abp.localization = abp.localization || {};
    abp.localization.internal = abp.localization.internal || {};
    abp.localization.values =  abp.localization.values || {};
    abp.localization.resources =  abp.localization.resources || {};

    abp.localization.internal.getResource = function (resourceName) {
        var resource = abp.localization.resources[resourceName];
        if (resource) {
            return resource;
        }

        var legacySource = abp.localization.values[resourceName];
        if (legacySource) {
            return {
                texts: abp.localization.values[resourceName],
                baseResources: []
            };
        }

        abp.log.warn('Could not find localization source: ' + resourceName);
        return null;
    };

    abp.localization.internal.localize = function (key, sourceName) {
        var resource = abp.localization.internal.getResource(sourceName);
        if (!resource){
            return {
                value: key,
                found: false
            };
        }

        var value = resource.texts[key];
        if (value === undefined) {
            for (var i = 0; i < resource.baseResources.length; i++){
                var basedArguments = Array.prototype.slice.call(arguments, 0);
                basedArguments[1] = resource.baseResources[i];

                var result = abp.localization.internal.localize.apply(this, basedArguments);
                if (result.found){
                    return result;
                }
            }

            return {
                value: key,
                found: false
            };
        }

        var copiedArguments = Array.prototype.slice.call(arguments, 0);
        copiedArguments.splice(1, 1);
        copiedArguments[0] = value;

        return {
            value: abp.utils.formatString.apply(this, copiedArguments),
            found: true
        };
    };

    abp.localization.localize = function (key, sourceName) {
        if (sourceName === '_') { //A convention to suppress the localization
            return key;
        }

        if (sourceName) {
            return abp.localization.internal.localize.apply(this, arguments).value;
        }

        if (!abp.localization.defaultResourceName) {
            abp.log.warn('Localization source name is not specified and the defaultResourceName was not defined!');
            return key;
        }

        var copiedArguments = Array.prototype.slice.call(arguments, 0);
        copiedArguments.splice(1, 1, abp.localization.defaultResourceName);

        return abp.localization.internal.localize.apply(this, copiedArguments).value;
    };

    abp.localization.isLocalized = function (key, sourceName) {
        if (sourceName === '_') { //A convention to suppress the localization
            return true;
        }

        sourceName = sourceName || abp.localization.defaultResourceName;
        if (!sourceName) {
            return false;
        }

        return abp.localization.internal.localize(key, sourceName).found;
    };

    abp.localization.getResource = function (name) {
        return function () {
            var copiedArguments = Array.prototype.slice.call(arguments, 0);
            copiedArguments.splice(1, 0, name);
            return abp.localization.localize.apply(this, copiedArguments);
        };
    };

    abp.localization.defaultResourceName = undefined;
    abp.localization.currentCulture = {
        cultureName: undefined
    };

    var getMapValue = function (packageMaps, packageName, language) {
        language = language || abp.localization.currentCulture.name;
        if (!packageMaps || !packageName || !language) {
            return language;
        }

        var packageMap = packageMaps[packageName];
        if (!packageMap) {
            return language;
        }

        for (var i = 0; i < packageMap.length; i++) {
            var map = packageMap[i];
            if (map.name === language){
                return map.value;
            }
        }

        return language;
    };

    abp.localization.getLanguagesMap = function (packageName, language) {
        return getMapValue(abp.localization.languagesMap, packageName, language);
    };

    abp.localization.getLanguageFilesMap = function (packageName, language) {
        return getMapValue(abp.localization.languageFilesMap, packageName, language);
    };

    /* AUTHORIZATION **********************************************/

    abp.auth = abp.auth || {};

    abp.auth.grantedPolicies = abp.auth.grantedPolicies || {};

    abp.auth.isGranted = function (policyName) {
        return abp.auth.grantedPolicies[policyName] != undefined;
    };

    abp.auth.isAnyGranted = function () {
        if (!arguments || arguments.length <= 0) {
            return true;
        }

        for (var i = 0; i < arguments.length; i++) {
            if (abp.auth.isGranted(arguments[i])) {
                return true;
            }
        }

        return false;
    };

    abp.auth.areAllGranted = function () {
        if (!arguments || arguments.length <= 0) {
            return true;
        }

        for (var i = 0; i < arguments.length; i++) {
            if (!abp.auth.isGranted(arguments[i])) {
                return false;
            }
        }

        return true;
    };

    abp.auth.tokenCookieName = 'Abp.AuthToken';

    abp.auth.setToken = function (authToken, expireDate) {
        abp.utils.setCookieValue(abp.auth.tokenCookieName, authToken, expireDate, abp.appPath, abp.domain);
    };

    abp.auth.getToken = function () {
        return abp.utils.getCookieValue(abp.auth.tokenCookieName);
    }

    abp.auth.clearToken = function () {
        abp.auth.setToken();
    }

    /* SETTINGS *************************************************/

    abp.setting = abp.setting || {};

    abp.setting.values = abp.setting.values || {};

    abp.setting.get = function (name) {
        return abp.setting.values[name];
    };

    abp.setting.getBoolean = function (name) {
        var value = abp.setting.get(name);
        return value == 'true' || value == 'True';
    };

    abp.setting.getInt = function (name) {
        return parseInt(abp.setting.values[name]);
    };

    /* NOTIFICATION *********************************************/
    //Defines Notification API, not implements it

    abp.notify = abp.notify || {};

    abp.notify.success = function (message, title, options) {
        abp.log.warn('abp.notify.success is not implemented!');
    };

    abp.notify.info = function (message, title, options) {
        abp.log.warn('abp.notify.info is not implemented!');
    };

    abp.notify.warn = function (message, title, options) {
        abp.log.warn('abp.notify.warn is not implemented!');
    };

    abp.notify.error = function (message, title, options) {
        abp.log.warn('abp.notify.error is not implemented!');
    };

    /* MESSAGE **************************************************/
    //Defines Message API, not implements it

    abp.message = abp.message || {};

    abp.message._showMessage = function (message, title) {
        alert((title || '') + ' ' + message);
    };

    abp.message.info = function (message, title) {
        abp.log.warn('abp.message.info is not implemented!');
        return abp.message._showMessage(message, title);
    };

    abp.message.success = function (message, title) {
        abp.log.warn('abp.message.success is not implemented!');
        return abp.message._showMessage(message, title);
    };

    abp.message.warn = function (message, title) {
        abp.log.warn('abp.message.warn is not implemented!');
        return abp.message._showMessage(message, title);
    };

    abp.message.error = function (message, title) {
        abp.log.warn('abp.message.error is not implemented!');
        return abp.message._showMessage(message, title);
    };

    abp.message.confirm = function (message, titleOrCallback, callback) {
        abp.log.warn('abp.message.confirm is not properly implemented!');

        if (titleOrCallback && !(typeof titleOrCallback == 'string')) {
            callback = titleOrCallback;
        }

        var result = confirm(message);
        callback && callback(result);
    };

    abp.message.prompt = function (message, titleOrOptionsOrCallback, callback) {
        abp.log.warn('abp.message.prompt is not properly implemented!');

        if (titleOrOptionsOrCallback && !(typeof titleOrOptionsOrCallback == 'string')) {
            callback = titleOrOptionsOrCallback;
        }

        var result = prompt(message);
        callback && callback(result);
    };

    /* UI *******************************************************/

    abp.ui = abp.ui || {};

    /* UI BLOCK */
    //Defines UI Block API and implements basically

    var $abpBlockArea = document.createElement('div');
    $abpBlockArea.classList.add('abp-block-area');

    /* opts: { //Can be an object with options or a string for query a selector
     *  elm: a query selector (optional - default: document.body)
     *  busy: boolean (optional - default: false)
     *  promise: A promise with always or finally handler (optional - auto unblocks the ui if provided)
     * }
     */
    abp.ui.block = function (opts) {
        if (!opts) {
            opts = {};
        } else if (typeof opts == 'string') {
            opts = {
                elm: opts
            };
        }

        var $elm = document.querySelector(opts.elm) || document.body;

        if (opts.busy) {
            $abpBlockArea.classList.add('abp-block-area-busy');
        } else {
            $abpBlockArea.classList.remove('abp-block-area-busy');
        }

        if (document.querySelector(opts.elm)) {
            $abpBlockArea.style.position = 'absolute';
        } else {
            $abpBlockArea.style.position = 'fixed';
        }

        $elm.appendChild($abpBlockArea);

        if (opts.promise) {
            if (opts.promise.always) { //jQuery.Deferred style
                opts.promise.always(function () {
                    abp.ui.unblock({
                        $elm: opts.elm
                    });
                });
            } else if (opts.promise['finally']) { //Q style
                opts.promise['finally'](function () {
                    abp.ui.unblock({
                        $elm: opts.elm
                    });
                });
            }
        }
    };

    /* opts: {
     *
     * }
     */
    abp.ui.unblock = function (opts) {
        var element = document.querySelector('.abp-block-area');
        if (element) {
            element.classList.add('abp-block-area-disappearing');
            setTimeout(function () {
                if (element) {
                    element.classList.remove('abp-block-area-disappearing');
                    if (element.parentElement) {
                        element.parentElement.removeChild(element);
                    }
                }
            }, 250);
        }
    };

    /* UI BUSY */
    //Defines UI Busy API, not implements it

    abp.ui.setBusy = function (opts) {
        if (!opts) {
            opts = {
                busy: true
            };
        } else if (typeof opts == 'string') {
            opts = {
                elm: opts,
                busy: true
            };
        }

        abp.ui.block(opts);
    };

    abp.ui.clearBusy = function (opts) {
        abp.ui.unblock(opts);
    };

    /* SIMPLE EVENT BUS *****************************************/

    abp.event = (function () {

        var _callbacks = {};

        var on = function (eventName, callback) {
            if (!_callbacks[eventName]) {
                _callbacks[eventName] = [];
            }

            _callbacks[eventName].push(callback);
        };

        var off = function (eventName, callback) {
            var callbacks = _callbacks[eventName];
            if (!callbacks) {
                return;
            }

            var index = -1;
            for (var i = 0; i < callbacks.length; i++) {
                if (callbacks[i] === callback) {
                    index = i;
                    break;
                }
            }

            if (index < 0) {
                return;
            }

            _callbacks[eventName].splice(index, 1);
        };

        var trigger = function (eventName) {
            var callbacks = _callbacks[eventName];
            if (!callbacks || !callbacks.length) {
                return;
            }

            var args = Array.prototype.slice.call(arguments, 1);
            for (var i = 0; i < callbacks.length; i++) {
                try {
                    callbacks[i].apply(this, args);
                } catch(e) {
                    console.error(e);
                }
            }
        };

        // Public interface ///////////////////////////////////////////////////

        return {
            on: on,
            off: off,
            trigger: trigger
        };
    })();


    /* UTILS ***************************************************/

    abp.utils = abp.utils || {};

    /* Creates a name namespace.
    *  Example:
    *  var taskService = abp.utils.createNamespace(abp, 'services.task');
    *  taskService will be equal to abp.services.task
    *  first argument (root) must be defined first
    ************************************************************/
    abp.utils.createNamespace = function (root, ns) {
        var parts = ns.split('.');
        for (var i = 0; i < parts.length; i++) {
            if (typeof root[parts[i]] == 'undefined') {
                root[parts[i]] = {};
            }

            root = root[parts[i]];
        }

        return root;
    };

    /* Find and replaces a string (search) to another string (replacement) in
    *  given string (str).
    *  Example:
    *  abp.utils.replaceAll('This is a test string', 'is', 'X') = 'ThX X a test string'
    ************************************************************/
    abp.utils.replaceAll = function (str, search, replacement) {
        var fix = search.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
        return str.replace(new RegExp(fix, 'g'), replacement);
    };

    /* Formats a string just like string.format in C#.
    *  Example:
    *  abp.utils.formatString('Hello {0}','Tuana') = 'Hello Tuana'
    ************************************************************/
    abp.utils.formatString = function () {
        if (arguments.length < 1) {
            return null;
        }

        var str = arguments[0];

        for (var i = 1; i < arguments.length; i++) {
            var placeHolder = '{' + (i - 1) + '}';
            str = abp.utils.replaceAll(str, placeHolder, arguments[i]);
        }

        return str;
    };

    abp.utils.toPascalCase = function (str) {
        if (!str || !str.length) {
            return str;
        }

        if (str.length === 1) {
            return str.charAt(0).toUpperCase();
        }

        return str.charAt(0).toUpperCase() + str.substr(1);
    }

    abp.utils.toCamelCase = function (str) {
        if (!str || !str.length) {
            return str;
        }

        if (str.length === 1) {
            return str.charAt(0).toLowerCase();
        }

        return str.charAt(0).toLowerCase() + str.substr(1);
    }

    abp.utils.truncateString = function (str, maxLength) {
        if (!str || !str.length || str.length <= maxLength) {
            return str;
        }

        return str.substr(0, maxLength);
    };

    abp.utils.truncateStringWithPostfix = function (str, maxLength, postfix) {
        postfix = postfix || '...';

        if (!str || !str.length || str.length <= maxLength) {
            return str;
        }

        if (maxLength <= postfix.length) {
            return postfix.substr(0, maxLength);
        }

        return str.substr(0, maxLength - postfix.length) + postfix;
    };

    abp.utils.isFunction = function (obj) {
        return !!(obj && obj.constructor && obj.call && obj.apply);
    };

    /**
     * parameterInfos should be an array of { name, value } objects
     * where name is query string parameter name and value is it's value.
     * includeQuestionMark is true by default.
     */
    abp.utils.buildQueryString = function (parameterInfos, includeQuestionMark) {
        if (includeQuestionMark === undefined) {
            includeQuestionMark = true;
        }

        var qs = '';

        function addSeperator() {
            if (!qs.length) {
                if (includeQuestionMark) {
                    qs = qs + '?';
                }
            } else {
                qs = qs + '&';
            }
        }

        for (var i = 0; i < parameterInfos.length; ++i) {
            var parameterInfo = parameterInfos[i];
            if (parameterInfo.value === undefined) {
                continue;
            }

            if (parameterInfo.value === null) {
                parameterInfo.value = '';
            }

            addSeperator();

            if (parameterInfo.value.toJSON && typeof parameterInfo.value.toJSON === "function") {
                qs = qs + parameterInfo.name + '=' + encodeURIComponent(parameterInfo.value.toJSON());
            } else if (Array.isArray(parameterInfo.value) && parameterInfo.value.length) {
                for (var j = 0; j < parameterInfo.value.length; j++) {
                    if (j > 0) {
                        addSeperator();
                    }

                    qs = qs + parameterInfo.name + '[' + j + ']=' + encodeURIComponent(parameterInfo.value[j]);
                }
            } else {
                qs = qs + parameterInfo.name + '=' + encodeURIComponent(parameterInfo.value);
            }
        }

        return qs;
    }

    /**
     * Sets a cookie value for given key.
     * This is a simple implementation created to be used by ABP.
     * Please use a complete cookie library if you need.
     * @param {string} key
     * @param {string} value
     * @param {Date} expireDate (optional). If not specified the cookie will expire at the end of session.
     * @param {string} path (optional)
     */
    abp.utils.setCookieValue = function (key, value, expireDate, path) {
        var cookieValue = encodeURIComponent(key) + '=';

        if (value) {
            cookieValue = cookieValue + encodeURIComponent(value);
        }

        if (expireDate) {
            cookieValue = cookieValue + "; expires=" + expireDate.toUTCString();
        }

        if (path) {
            cookieValue = cookieValue + "; path=" + path;
        }

        document.cookie = cookieValue;
    };

    /**
     * Gets a cookie with given key.
     * This is a simple implementation created to be used by ABP.
     * Please use a complete cookie library if you need.
     * @param {string} key
     * @returns {string} Cookie value or null
     */
    abp.utils.getCookieValue = function (key) {
        var equalities = document.cookie.split('; ');
        for (var i = 0; i < equalities.length; i++) {
            if (!equalities[i]) {
                continue;
            }

            var splitted = equalities[i].split('=');
            if (splitted.length != 2) {
                continue;
            }

            if (decodeURIComponent(splitted[0]) === key) {
                return decodeURIComponent(splitted[1] || '');
            }
        }

        return null;
    };

    /**
     * Deletes cookie for given key.
     * This is a simple implementation created to be used by ABP.
     * Please use a complete cookie library if you need.
     * @param {string} key
     * @param {string} path (optional)
     */
    abp.utils.deleteCookie = function (key, path) {
        var cookieValue = encodeURIComponent(key) + '=';

        cookieValue = cookieValue + "; expires=" + (new Date(new Date().getTime() - 86400000)).toUTCString();

        if (path) {
            cookieValue = cookieValue + "; path=" + path;
        }

        document.cookie = cookieValue;
    }

    /**
     * Escape HTML to help prevent XSS attacks.
     */
    abp.utils.htmlEscape = function (html) {
        return typeof html === 'string' ? html.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;') : html;
    }

    /* SECURITY ***************************************/
    abp.security = abp.security || {};
    abp.security.antiForgery = abp.security.antiForgery || {};

    abp.security.antiForgery.tokenCookieName = 'XSRF-TOKEN';
    abp.security.antiForgery.tokenHeaderName = 'RequestVerificationToken';

    abp.security.antiForgery.getToken = function () {
        return abp.utils.getCookieValue(abp.security.antiForgery.tokenCookieName);
    };

    /* CLOCK *****************************************/
    abp.clock = abp.clock || {};

    abp.clock.kind = 'Unspecified';

    abp.clock.supportsMultipleTimezone = function () {
        return abp.clock.kind === 'Utc';
    }

    abp.clock.timeZone = function () {
        return abp.setting.get('Abp.Timing.TimeZone') || abp.clock.browserTimeZone();
    }

    // Normalize Date object or date string to standard string format that will be sent to server
    abp.clock.normalizeToString = function (date) {
        if (!date) {
            return date;
        }

        var dateObj = date instanceof Date ? date : new Date(date);
        if (isNaN(dateObj)) {
            return date;
        }

        function padZero(num) {
            return num < 10 ? '0' + num : num;
        }

        var addZulu = false;
        if (abp.clock.supportsMultipleTimezone()) {
            var timeZone = abp.clock.timeZone();
            var now = new Date();
            var formattedDate = now.toLocaleString('en-US', { timeZone: timeZone, timeZoneName: 'longOffset' });
            var match = formattedDate.match(/GMT([+-]\d+)/);
            var targetOffsetHours = match ? parseInt(match[1], 10) : 0;
            dateObj = new Date(dateObj.getTime() - (targetOffsetHours * 60 * 60 * 1000));
            addZulu = true;
        }

        // yyyy-MM-DDTHH:mm:ss
        return dateObj.getFullYear() + '-' +
            padZero(dateObj.getMonth() + 1) + '-' +
            padZero(dateObj.getDate()) + 'T' +
            padZero(dateObj.getHours()) + ':' +
            padZero(dateObj.getMinutes()) + ':' +
            padZero(dateObj.getSeconds()) + (addZulu ? 'Z' : '');
    };

    // Default options for toLocaleString
    abp.clock.toLocaleStringOptions = abp.clock.toLocaleStringOptions || {
        "year": "numeric",
        "month": "long",
        "day": "numeric",
        "hour": "numeric",
        "minute": "numeric",
        "second": "numeric"
    };

    // Normalize date string to locale date string that will be displayed to user
    abp.clock.normalizeToLocaleString = function (dateString, options) {
        if (!dateString) {
            return dateString;
        }

        var date = new Date(dateString);
        if (isNaN(date)) {
            return dateString;
        }

        var culture = abp.localization.currentCulture.cultureName;
        options = options || abp.clock.toLocaleStringOptions;
        if (abp.clock.supportsMultipleTimezone()) {
            var timezone = abp.clock.timeZone();
            if (timezone) {
                return date.toLocaleString(culture, Object.assign({}, options, { timeZone: timezone }));
            }
        }
        return date.toLocaleString(culture, options);
    }

    abp.clock.browserTimeZone = function () {
        return Intl.DateTimeFormat().resolvedOptions().timeZone;
    }

    abp.clock.trySetBrowserTimeZoneToCookie = true;

    abp.clock.setBrowserTimeZoneToCookie = function () {
        if (!abp.clock.trySetBrowserTimeZoneToCookie || !abp.clock.supportsMultipleTimezone()) {
            return;
        }

        abp.utils.setCookieValue('__timezone', abp.clock.browserTimeZone(), new Date(new Date().setFullYear(new Date().getFullYear() + 1)), '/');
    }

    abp.event.on('abp.configurationInitialized', function () {
        abp.clock.setBrowserTimeZoneToCookie();
    });

    /* FEATURES *************************************************/

    abp.features = abp.features || {};

    abp.features.values = abp.features.values || {};

    abp.features.isEnabled = function(name){
        var value = abp.features.get(name);
        return value == 'true' || value == 'True';
    }

    abp.features.get = function (name) {
        return abp.features.values[name];
    };

    /* GLOBAL FEATURES *************************************************/

    abp.globalFeatures = abp.globalFeatures || {};

    abp.globalFeatures.enabledFeatures = abp.globalFeatures.enabledFeatures || [];

    abp.globalFeatures.isEnabled = function(name){
        return abp.globalFeatures.enabledFeatures.indexOf(name) != -1;
    }

    // Set Custom Warehouse Logo Favicon dynamically
    (function () {
        var faviconData = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAA8mSURBVHhe7VoJcBTXmf7fiGRtcHkdNN0Cg1dSQNM9Ao2gMJemew4JCJAlXg6hWwJ0IIQuxBEnG9Y51puNk0oQFAYFY9CIw8hrxwchMcFJNq44lV0bHEtCNzpHFwgJCQhoevrfeq81QmpDCgNrm9R8VX+97ve/N3r/9/73//+bEYAffvjhhx9++OGHH3744YcfnwFyKh+D7ZcL4FvX98D2Hrte/feL2PIA2N6fBTuuNMB3VGTyzUGErZfLoaBvln743xeKemNhR/85+I6CsGMAobDLC4WdXijsQnj2BsLWXgW2XS6Bwq6v6qc+3Ci6GANFfb+HZ28i7LiKUNChQoFbhW1XEHZcRyjsQchr14j41k2ELZcGoLDnP2Bze6D+ox4u5PXMhi0XX4OtVxC2X0PI71Ah1+2Foj6E7VcR8txnIM/9Asnv7IUdf0XI70TY3OqF/C6Eb/4VoaDbDQVdhZBX9w/6j/5iI7czFA6SqDgogLbriHkuhE2t3khvwdhKyWiuwryuxJHxuddmgoFXXshv0uBogFKAkJuqwIF3ajN7zoPOe6kMX/jC4m8Og7yup+HvO4BtuOb3QibWrysLbpKDXHD5r+xo9ntsyG38w0o6EUovIyQ3aRCdrMX8i4iFF5B2OR+H3I6luqnff7Y0voo5HRshdzOTthyFWFzBzVcgU0tSOjCczquwyb3C5BVY9RPvS1yWpeS3I73oaAfYXMnQtYFL2RdUBkRuT0I2e63ILNtvn7a54Ps9jTI6aiFwgGEnE6EzCYvZDWpkNtLDUfIcZdBRrtJP+2ukN2aQbJbWyCvD2FjC0J6gxcyGhE2X0LIakXIaH4ZNjaE6ad9NtjYthyy2/8M+X0ImzoR0hu9TLI7kdAFbmx/F7JaJP20T420c09AZsv3SWbzIORcREZARr0CGxoQNl2iJFyF9Mb/vGvvum9kNs6FzNaTsKkbIacbIaPBC+kNCmS0aAtKb66EjJa1+mn3jdS6aSS9sRTSmxE2uhHW1aqQVqvAugaErB6E1IZOSG3cAktP3T6+3Dey3EbIaN0Pma0qbLqIsL4BYUO9AumNw4a3t0Gusk4/7YFjh2qGDU2/IRmdCOuaEFKqFUiq9kJKA0J6N0JKYyUkVX5DP+3+sb7+EOTfQFjXSNlXIK1WhcwuhPXNCqS37+IcO60m0bIzNEIuDo2MfjF0Vsz+UIujJNTi+HlIpPPAiEQ4XwqxOA6EWJwHgiPsL7H3SCqOgyGj32mrl0jHwWnCrG0LAB6F1NpUSGlohA1dCMl1CElVCiRUeSG1FSGxDiHu/L3FnTsire4UZHYipFZ7Ib0VYUM7wobmY7CuIYKqQ2ZEvSrMX4HTn16KYXOXDQt99r3Tdjl7ZmNGi2/cyNylbMztxDR/BYZG2LQaIuuD8ZBUXwQJdQOQ6kaIq9QksR4hvuoBZ4mkmrcgsxchoxshrekPkFQTA0l1CyCp7nGqfsoSvSI0ctGvQizO01RCLY53giOcvw6NdP7KJyEW56kQi+OXVEJZ6zypiV2TSMfb7D3C8fYtsZ/UZFhniTk0NTJ6CltTbOVjzNDYyn+CtbX7II56QgtCXC3Cyoq5ehPuDwkV78L6zl5IadoAidURkNr0EqQ2IyTWshvc1NlLpofMWpwfGuncEhoZo8nsmKJQS0xRcATtc26h7Vcjowup0PeQCEdBcIT2THVMIqMLaV8w/RyLoyjY4tjKZKajKCTCuT1khjM1ONj+CFvTmtopkHAeIenCW7D2bCSsrZkL8XV/hCQ3wsqzD5iAxMpZsOb9KZBSkwLrW64zT0iupd4gUnWwxdlimvf1YVelbr4Mp89dPsZ9p80ZpaN97H05htH3OctwGhWfzid0DBtH26+had4KDImI3sHWFFs5CeIrr8KGToSUCwjxlf/O+tfULYbFH00Ya8CDQnLtKcikaacGIbUWIblhJu0OtkT/IHRWTH2IxeGTutBIZ11IpLM+hLYWKo7hdlgio+tDLE4qTBc88uysC45w1FIJoe1M2rIxDaGRiz4MjrAvYGtZeX4yJFQMQkoN3QiEhEq3frkPHkk15bChAyGlFiG1jnpB5C3lc4bY2NiAuxW73T5O3wc+gecMt4S+xwaA3T5u9FJgzbkpEF91FZJraPqjga8aAMmYMQ8ciTXlsN6NGuv1lPnP71ucpLqpEF91DZIoARcQEj8TAs6fgPXtyFhPrUdIaZqtH+LD1UMwSXnFsEMpM+xUThj+TT0OT904COH0WSljUqSWAcsi94S4+qcgvsrDcv+6ToS1la3//wQk+wio1o5AWsMc/RAfhsoMhXjGgPgGQTxNcMg1LkpxQTyeIVrfKYI3Dgcs08/zIciyeEKQWY7jTVLyP0ZIX9HrYU3ldEioa4bk9t9Date7kNT2sn7IgwcjoI1WXiqLA3+LgFLDVvwFQU8pUbGcoOdowGrFBbn4GkFPGVHxv6guYLl+ng9Gk3XZpJkxGDRzERpNtk/eL57DLz82r/CZIFGKDzLNj+PDFiZwoi2VE+zrjWbbBl6UMgOFKFoWP0CvSKwqh7RWhIQqLQ0mVt+ZAJdhC76uEaCUEa/iIjeUUnLT4yIqI+AEwRsld/YAoyj9M2+2Ix/uRGOYFK/Xc6YFS3nRem3SDCdSophELMbJEYtxUsQiDJrhxKAZMWg0WT9J3j2DEkDPXHwlasGn+mn9EB+UUkMh2+1SolJRywh6XQQpGYyAlw1442d3JiBIsK4YIcAsx+nUBl6Uq4NmRCMnyH2cIP/BKEjvGQX5PU6Q/tsoSOc4QfIyvWh9Vjf3PhBPCaDlZoVGQFLNHSsunwcoLuL1HCaqh7aa8d6hfQYV9xO8sfvOBPBC1DeCzHYMCncgJ9hjx+hMUjLb4XAn8ibpX0brKLhg+yROkK5TDwgU5G16/b0jofLEMAEqJNTQ4mOefogPisuwjQY7pYyRoLVU9htQ2U0QDxD07A74un6eD4yAcM1ITpBuEWC3j+MEqY66PCdIfx4zaRiBQtSTRkG6FjQzBnnBulWvv3fEVZRDyrAHUALW/kWrym4D5bAhgwY6hZ55F1HxKEF8iaCnmKjePQS9+wyeod3jrPp5PgSa5Gd8BIw+x7woZVHXph5gNNtu60ETp8+bygjQjsh2vf7eEVtRDsktNOdqBCR8vNCn+qAEvjR4DILUYxDUfxS+gofgEfVowHKPK2C150jAqqHD42xDu2CuZ1/AM2pJwGpPScAyLIcvu0tgPJ1Dpe8QPOH7PN4kP8OHO8YQMHXqgkc5QWrWdl9+3zdWj4lh0VNuEWB9gEeAegAlILYCIf4WAVgOAUOHyWn1OBnwvkIGFRc5Q/sVF/mteoK41VeJ21tKfuQ5Dku8r5M25QhpU18hHephmHHTBenqa2zOoPcY6b5xCNgFi6Yw3uxASoLvCBgFKV87+w7kzdIi3epGMHH6vMc5HwFh1ly9/t5BPSCpGWHNxwhxNQhx1VG0G9+aPN5zmFyi+Z7mfu9R8ifar7hILc0EVCgZXhf5Pr5JEI8TpAHy+hEIVsogDd8miMe0vqGj49ivxfQIUONpFggUrCvofcAoWDuZUSbpd/qlUdA6gA+3H+QF6SQvygov2lVOkCqNglTKibYyo0kq0u4W94q1lScgUSOA0C8ehgmgbuwpJW3UMO8xFuzcHhf5oaeU9HqPEhVfZQScUsoM32ZklBEv7fe4yEGljJz2Hieqt0wjZugAsLgQaIoaIcAoSnajKGVS43nRhrxo/UTsMQrSt2kdQAMf9RJetKmcIKs0lWo1QTSrDwIF661fpT411lSegIRmhNV/QVhbg7DmvHYEfAS8Mlz50YD3OkGa+2nao/3KvrEE0LRIgyQtiGiqVI4QxJ8THPqpRgDvI8BspwRkGQWpnqY1TpBe1y9rYpg8jzfbFN5so7FB5URZ5UWZETDyLMoqJSJQsH5XP//usaaiHBKaNAJiRx2BEhivDHuA4jJold8RTZihBwkqxeSXjAAtM7D+kdqAjnuRIO4l6MsMWhCkKZAZ0sLRnTezeNDBidL/8KL8AS/KZ3mt6LlCiaJGamIb2XVWKwzrgsKj0ShI/6o36+6x+uNXIakJIXY4BqytYItVXTBBcZF2eo4VX+k7TMDQi8RLi56hPbcI8BnuGze0z+D17CKILxIc2juOkTqKAHYMmDcIUh0vyh9xouxl5S7LEtRLbCPGs6Ap2oaMglzKi/JOoyjv5kRbl48Qo8m6U2/W3WPVR+WQOExAfO0IAVgOj9HzTt0ZqSvTgEYDXQlB3KPl/6Fi8q73CPkeC4JjxhgQaWG0VyuOrhdrQZASwHZPM7CDlsOcIK9mgS5MWsSL9h+xImmUizPjzfYLQWZpMS/ad2pxTMCNxtj0lh6C+z2eY/gS798t+H5k4h3C7eWwS9kY/l0n0P5+kP7+k3+F/wM1dZ3D919V/QAAAABJRU5ErkJggg==';

        function applyWarehouseFavicon() {
            try {
                var links = document.querySelectorAll("link[rel*='icon']");
                if (links && links.length > 0) {
                    for (var i = 0; i < links.length; i++) {
                        links[i].type = 'image/png';
                        links[i].href = faviconData;
                    }
                } else {
                    var link = document.createElement('link');
                    link.type = 'image/png';
                    link.rel = 'icon';
                    link.href = faviconData;
                    document.head.appendChild(link);
                }

                var shortcut = document.querySelector("link[rel='shortcut icon']");
                if (!shortcut) {
                    shortcut = document.createElement('link');
                    shortcut.rel = 'shortcut icon';
                    shortcut.type = 'image/png';
                    document.head.appendChild(shortcut);
                }
                shortcut.href = faviconData;

                var apple = document.querySelector("link[rel='apple-touch-icon']");
                if (!apple) {
                    apple = document.createElement('link');
                    apple.rel = 'apple-touch-icon';
                    document.head.appendChild(apple);
                }
                apple.href = faviconData;
            } catch (e) {
                console.error('Error setting warehouse favicon:', e);
            }
        }

        applyWarehouseFavicon();
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', applyWarehouseFavicon);
        }
        window.addEventListener('load', applyWarehouseFavicon);

        var count = 0;
        var interval = setInterval(function () {
            applyWarehouseFavicon();
            count++;
            if (count > 10) clearInterval(interval);
        }, 300);

        function applyLoginPlaceholders() {
            var userField = document.getElementById('LoginInput_UserNameOrEmailAddress') ||
                            document.querySelector('input[name="LoginInput.UserNameOrEmailAddress"]') ||
                            document.querySelector('form input[type="text"]');
            if (userField) {
                userField.placeholder = 'Enter your email address';
                userField.setAttribute('placeholder', 'Enter your email address');
            }

            var passField = document.getElementById('LoginInput_Password') ||
                            document.querySelector('input[name="LoginInput.Password"]') ||
                            document.querySelector('form input[type="password"]');
            if (passField) {
                passField.placeholder = 'Enter your password';
                passField.setAttribute('placeholder', 'Enter your password');
            }
        }

        document.addEventListener('input', function (e) {
            if (e.target && (e.target.id === 'LoginInput_UserNameOrEmailAddress' || e.target.id === 'LoginInput_Password')) {
                var p = e.target.closest('.mb-3');
                if (p) {
                    if (e.target.value.length > 0) {
                        p.classList.add('has-value');
                    } else {
                        p.classList.remove('has-value');
                    }
                }
            }
        }, true);

        applyLoginPlaceholders();
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', applyLoginPlaceholders);
        }
        window.addEventListener('load', applyLoginPlaceholders);

        var pCount = 0;
        var pInterval = setInterval(function () {
            applyLoginPlaceholders();
            pCount++;
            if (pCount > 25) clearInterval(pInterval);
        }, 80);
    })();

})();
