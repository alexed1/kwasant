
var CONTROLLER_NAME = 'Calendar';

function getBaseCalendarURL(){
    var tempPath = ''
    tempPath = $(location).attr('href');    

    arrPath = tempPath.split('//');
    //tempPath = arrPath[0] + '//' + arrPath[1].substring(0, arrPath[1].indexOf('/'));

    tempPath = arrPath[0] + '//' + arrPath[1].substring(0, arrPath[1].indexOf('/')) + '/' + CONTROLLER_NAME;

    return tempPath
}


function getBaseURL() {
    var tempPath = ''
    tempPath = $(location).attr('href');

    arrPath = tempPath.split('//');
    //tempPath = arrPath[0] + '//' + arrPath[1].substring(0, arrPath[1].indexOf('/'));

    tempPath = arrPath[0] + '//' + arrPath[1].substring(0, arrPath[1].indexOf('/'));

    return tempPath
}



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