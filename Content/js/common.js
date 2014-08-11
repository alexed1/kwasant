
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

/*** Kwasant.IFrame functions ***/
function closeWithUnsavedDataCheck(modifiedState) {
    if (!modifiedState.modified) {
        close();
    }
    else if (confirm("you are about to lose data, continue?")) {
        modifiedState.modified = false;
        close();
    }
    return false;
}

function close(saved) {
    if (saved === undefined || saved == null)
        saved = false;
    Kwasant.IFrame.CloseMe(saved);
}
