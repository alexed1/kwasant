
var CONTROLLER_NAME = 'Calendar';


function getConfiguration() {

    var configFilePath = getBaseCalendarURL();

    var retValue = '';

    $.get(configFilePath,
        function (txt, status, jqXHR) {
            $("#txtConfigValue").val(txt);
        },
        "text"
    );

    return retValue;
}

function getURL(key) {
    var configValue = $("#txtConfigValue").val();

    var retValue = '';
    var lines = configValue.split("\n");

    for (var i = 0, len = lines.length; i < len; i++) {
        var arrLine = lines[i].split('=');
        if ($.trim(arrLine[0]) == $.trim(key)) {
            retValue = arrLine[1];
            break;
        }
    }

    return retValue;
}

var guid = (function () {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
                   .toString(16)
                   .substring(1);
    }
    return function () {
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
               s4() + '-' + s4() + s4() + s4();
    };
})();

function isEmail(email) {
    var regex = /^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
    return regex.test(email);
}

function getURLParameter(name) {
    return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [, ""])[1].replace(/\+/g, '%20')) || null;
}

function close(saved) {
    if (saved === undefined || saved == null)
        saved = false;
    Kwasant.IFrame.CloseMe(saved);
}

/*** Check Email Validation ***/
function getValidEmailAddress(attendeesSel) {
    $(attendeesSel).select2({
        createSearchChoice: function (term) {
            return { id: term, text: term };
        },
        validateFormat: function (term) {
            if (isValidEmail(term)) {
                return null;
            }
            return 'Invalid Email';
        },
        multiple: true,
        data: [],
        width: '100%',
    });
}

function isValidEmail(term) {
    var atIndex = term.indexOf('@');
    var dotIndex = term.lastIndexOf('.');
    if (atIndex > 0 //We need something before the at sign
        && dotIndex > atIndex + 1 //We need a dot, and it should have at least one character between the at and the dot
        && dotIndex < term.length - 1 //The dot can't be at the end
    )
        return true;
    return false;
}


function convertToDateString(dateFormat) {
    var datevalue = new Date(dateFormat);
    var timeSuffix = "AM"; var hour = 0;
    if (datevalue.getHours() >= 12) {
        hour = parseInt(datevalue.getHours()) - 12;
        timeSuffix = "PM";
    } else {
        hour = datevalue.getHours();
        timeSuffix = "AM";
    }
    dateFormat = datevalue.getMonth() + 1 + "-" + datevalue.getDate() + "-" + datevalue.getFullYear().toString().substring(2, 4) + " " + hour + ":" + datevalue.getMinutes() + " " + timeSuffix;
    return dateFormat;
}


function autoResizeTextArea(e) {
    $(e).css({ 'height': 'auto', 'overflow-y': 'hidden' }).height(e.scrollHeight);
}