(function ($) {
    var dayCalendar;
    var dayCalendarDiv;
    
    var weekCalendar;
    var weekCalendarDiv;
    
    var monthCalendar;
    var monthCalendarDiv;

    var settings;

    $.fn.KCalendar = function (options) {
        
        //Setup defaults
        settings = $.extend({
            topElement: this,

            showDay: true,
            showWeek: true,
            showMonth: true,
            
            requireConfirmation: true,
            
            getEditURL: function (id) { alert('getEditURL function must be set in the options, unless providing an onEdit function override.'); },
            getNewURL: function (id, start, end) { alert('getNewURL function must be set in the options, unless providing an onEdit function override.'); },
            getDeleteURL: function (id) { alert('getDeleteURL function must be set in the options, unless providing an onEdit function override.'); },
            getMoveURL: function (id, newstart, newend) { alert('getMoveURL function must be set in the options, unless providing an onEdit function override.'); },

            onEventClick: onEventClick,
            onEventNew: onEventNew,
            onEventDelete: onEventDelete,
            onEventMove: onEventMove,

        }, options);

        createCalendars();
    };

    var createCalendars = function() {
        //First, setup the HTML

        //This displays the toolbar to swap between day, week and month
        var toolbar = $("<div id='toolbar' class='toolbar'>");
        var inner = $("<div class='divCalendar-inner'></div>");

        var switcher = new DayPilot.Switcher();
        var calendarsToInit = [];

        var queueCalendarForInit = function(createFunc, name) {
            var calendarPair = createFunc();

            if (calendarPair === null ||
                calendarPair === undefined ||
                calendarPair.dp === null ||
                calendarPair.dp === undefined) {
                return;
            }

            var swapButton = $("<a id=" + getRandomID() + " href='#'>" + name + "</a>");
            calendarsToInit.push(calendarPair.dp);
            switcher.addView(calendarPair.dp);
            switcher.addButton(swapButton.get(0), calendarPair.dp);

            inner.append(calendarPair.div);
            toolbar.append(swapButton);
        };

        if (settings.showDay) {
            queueCalendarForInit(createDayCalendar, 'Day');
        }
        if (settings.showWeek) {
            queueCalendarForInit(createWeekCalendar, 'Week');
        }
        if (settings.showMonth) {
            queueCalendarForInit(createMonthCalendar, 'Month');
        }

        var toolbarRow = $("<div class='row'></div>");
        toolbarRow.append(toolbar);

        var calendarRow = $("<div class='row'></div>");
        calendarRow.append(inner);

        var calendarBox = $("<div class='col-md-6 container_box divCalender'></div>");
        calendarBox.append(toolbarRow);
        calendarBox.append(calendarRow);

        settings.topElement.append(calendarBox);

        var firstToDisplay = null;
        for (var i = 0; i < calendarsToInit.length; i++) {
            calendarsToInit[i].init();
            if (i == 0)
                firstToDisplay = calendarsToInit[0];
        }

        if (firstToDisplay !== null)
            switcher.show(firstToDisplay);

    };

    var getRandomID = function() {
        var idLength = 10;
        return new Array(idLength + 1).join((Math.random().toString(36) + '00000000000000000').slice(2, 18)).slice(0, idLength);
    };

    var createDayCalendar = function () {
        var calendar = createDefaultCalendar();
        calendar.dp.viewType = 'Day';
        return calendar;
    };
    var createWeekCalendar = function () {
        var calendar = createDefaultCalendar();
        calendar.dp.viewType = 'Week';
        return calendar;
    };
    var createMonthCalendar = function () {
        var id = getRandomID();
        var divHolder = $("<div id='" + id + "'></div>");

        var dp = new DayPilot.Month(id);
        dp.cssClassPrefix = 'calendar_white';
        dp.eventBackColor = '#638EDE';

        dp.onEventClick = settings.onEventClick;
        dp.onTimeRangeSelected = settings.onEventNew;
        dp.onEventDelete = settings.onEventDelete;
        dp.onEventMove = settings.onEventMove;

        return { dp: dp, div: divHolder };
    };

    var createDefaultCalendar = function() {
        var id = getRandomID();
        var divHolder = $("<div id='" + id + "'></div>");

        var dp = new DayPilot.Calendar(id);
        dp.cssClassPrefix = 'calendar_white';
        dp.eventBackColor = '#638EDE';
        
        dp.onEventClick = settings.onEventClick;
        dp.onTimeRangeSelected = settings.onEventNew;
        dp.onEventDelete = settings.onEventDelete;
        dp.onEventMove = settings.onEventMove;

        return { dp: dp, div: divHolder };
    };

    var onEventClick = function(e) {

    };
    var onEventNew = function (e) {

    };
    var onEventDelete = function (e) {

    };
    var onEventMove = function (e) {

    };
}(jQuery));