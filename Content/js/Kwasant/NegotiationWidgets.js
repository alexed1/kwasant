(function ($) {
    
    var that;
    var settings;
    var initValues;
    
    var nodes = {
        Name: null,
        Attendees: null,
        
        QuestionHolder: null,

        Questions: []
    };

    $.fn.NegotiationWidget = function (options, initialValues) {
        that = this;
        settings = $.extend({
            DisplayMode: 'edit',

            MaxAdditionalAnswers: -1,

            AllowModifyNegotiationRequest: true,

            AllowAddQuestion: true,
            AllowModifyQuestion: true,
            AllowDeleteQuestion: true,

            AllowAddAnswer: true,
            AllowModifyAnswer: true,
            AllowDeleteAnswer: true,
        }, options);

        initValues = $.extend({
            Id: 0,
            Name: 'Negotiation 1',
            Attendees: '',
        }, initialValues);
        
        //Sanitize attendees
        if (initValues.Attendees != null) {
            var sanitizedAttendees = [];
            for (var i = 0; i < initValues.Attendees.length; i++) {
                var obj = initValues.Attendees[0];
                if (typeof obj === "string") {
                    sanitizedAttendees.push({
                        id: obj,
                        text: obj
                    });
                } else {
                    sanitizedAttendees.push(obj);
                }
            }
            initValues.Attendees = sanitizedAttendees;
        }

        buildBaseWidget();

        this.getValues = function() {
            var returnNeg = {};
            returnNeg.Id = initValues.Id,
            returnNeg.BookingRequestID = initValues.BookingRequestID,
            returnNeg.Name = nodes.Name.val();
            returnNeg.Attendees = nodes.Attendees.val();
            returnNeg.Questions = [];
            
            for (var q = 0; q < nodes.Questions.length; q++) {
                var question = nodes.Questions[q];

                returnNeg.Questions.push(question.getValues());
            }

            return returnNeg;
        };

        return this;
    };
    
    function buildBaseWidget() {
        that.empty();

        that.addClass('negotiationsidebar');
        that.css('height', '100%');

        var baseInfoDiv = $('<div></div>')
            .addClass('form-group')
            .addClass('negotiation-mrbottom');

        var baseInfoTable = $('<table></table>')
            .css('width', '100%');

        /* Build the name input object */
        var nameInput = $('<input type="text" />')
            .addClass('form-control')
            .addClass('col-md-1')
            .val(initValues.Name);
            
        var nameRow = $('<tr />')
                .append($('<td />')
                    .append('&nbsp;<label>Name:</label>'))
                .append($('<td />')
                    .append(nameInput));

        baseInfoTable.append(nameRow);

        /* Build the attendees input object */
        var attendeesInput = $('<input type="hidden" />');
            
        var attendeesRow = $('<tr />');
        attendeesRow.append($('<td />').append('&nbsp;<label>Attendees:</label>'));
        attendeesRow.append($('<td />').append(attendeesInput));
            
        attendeesInput.select2({
            createSearchChoice: function (term) {
                return { id: term, text: term };
            },
            validateFormat: function (term) {
                if (!isEmail(term))
                    return "Invalid Email";
                return null;
            },
            multiple: true,
            data: [],
            width: '100%',
        });
        attendeesInput.select2('data', initValues.Attendees);
        
        if (!settings.AllowModifyNegotiationRequest) {
            nameInput.attr('disabled', 'disabled');
            attendeesInput.attr('disabled', 'disabled');
        }
            
        baseInfoTable.append(attendeesRow);
        baseInfoDiv.append(baseInfoTable);
        
        var questionHolder = $('<div></div>');
        
        nodes.QuestionHolder = questionHolder;

        var addQuestionSpan = $(' \
        <span class="form-group handIcon"> \
                &nbsp; &nbsp; \
            <img src="/Content/img/plus.png" /> \
            <label class="handIcon">Add Question</label> \
        </span>')
            .addClass('form-group')
            .addClass('handIcon')
            .click(function() {
                addQuestion();
            });

        if (!settings.AllowAddQuestion)
            addQuestionSpan.hide();
        
        that.append(baseInfoDiv);
        that.append(questionHolder);
        that.append(addQuestionSpan);

        nodes.Name = nameInput;
        nodes.Attendees = attendeesInput;
        
        if (initValues.Questions !== null && initValues.Questions !== undefined) {
            for (var i = 0; i < initValues.Questions.length; i++) {
                var questionValues = initValues.Questions[i];
                addQuestion(questionValues, true);
            }
        }

    }
    
    function addQuestion(initialValues, immediate) {
        if (immediate === undefined)
            immediate = false;
        
        var questionInitValues = $.extend({
            Id: 0,
            QuestionGUID: guid(),
            Type: 'Text',
            Text: 'Enter question text'
        }, initialValues);

        var questionObject = createQuestionObject(questionInitValues);
        var questionNode = questionObject.Node;
        questionNode.hide();
        
        nodes.QuestionHolder.append(questionNode);

        if (immediate)
            questionNode.show();
        else
            questionNode.slideDown();
        
        if (questionInitValues.Answers !== null && questionInitValues.Answers !== undefined) {
            for (var i = 0; i < questionInitValues.Answers.length; i++) {
                var answerValues = questionInitValues.Answers[i];
                questionObject.addAnswer(answerValues, true);
            }
        }

        return questionObject;
    }
    
    function createQuestionObject(questionInitValues) {
        var questionObject = {};

        var groupID = guid();

        var answerHolder = $('<div></div>');

        var questionTypeText = $('<input type="radio"/>')
            .attr('name', groupID);

        var questionTypeCalendar = $('<input type="radio"/>')
            .attr('name', groupID);

        if (questionInitValues.Type == 'Timeslot')
            questionTypeCalendar.get(0).checked = true;
        else
            questionTypeText.get(0).checked = true;

        var edittableType;
        if (settings.AllowModifyQuestion) {
            edittableType =
                $('<td />')
                    .append(
                        $('<label>Type:</label>')
                    )
                    .append(
                        $('<label></label>')
                            .append(
                                questionTypeText
                            ).append("Text")
                    ).append(
                        $('<label></label>')
                            .append(
                                questionTypeCalendar
                            ).append("Timeslot")
                    );
        } else {
            edittableType =
                $('<td />')
                    .append(
                        $('<label>Type:</label>')
                    )
                    .append(
                        $('<label></label>')
                            .append(questionInitValues.Type)
                    );
        }


        var questionName = $('<input type="text" />')
            .addClass('form-control')
            .addClass('col-md-1')
            .val(questionInitValues.Text);

        var removeMeIcon = $('<img src="/Content/img/Cross.png"></img>')
            .addClass('handIcon')
            .click(function () {
                numAnswersAdded--;
                questionObject.RemoveMe();
                
            });

        var addAnswerSpan = $('<span>')
            .addClass('form-group')
            .addClass('handIcon')
            .click(function () {
                numAnswersAdded++;
                questionObject.addAnswer();
            })
            .append(
                $('<img src="/Content/img/plus.png" />')
            ).append(
                $('<label>Add Answer</label>')
                    .addClass('handIcon')
            );

        if (!settings.AllowDeleteQuestion && questionInitValues.Id > 0)
            removeMeIcon.hide();

        if (!settings.AllowAddAnswer && questionInitValues.Id > 0)
            addAnswerSpan.hide();

        if (!settings.AllowModifyQuestion)
            questionName.attr('disabled', 'disabled');

        var questionDiv = $('<div></div>')
            .addClass('questionBox')
            .append(
                $('<table />')
                    .css('width', '100%')
                    .append(
                        $('<tr />')
                            .append(
                                $('<td />')
                                    .append(
                                        $('<label>Question: </label>')
                                    )
                            ).append(
                                $('<td />')
                                    .css('width', '100%')
                                    .append(
                                        questionName
                                    )
                            ).append(
                                $('<td />')
                                    .append(
                                        removeMeIcon
                                    )
                            )
                    )
                    .append(
                        $('<tr />')
                            .append(
                                $('<td />')
                            ).append(
                                edittableType
                            )
                    )
            )
            .append(
                answerHolder
            )
            .append(
                addAnswerSpan
            );
        
        questionObject.Node = questionDiv;
        questionObject.Answers = [];

        questionObject.getQuestionType = function() {
            return questionTypeText.get(0).checked ? 'Text' : 'Timeslot';
        };

        questionObject.getValues = function () {
            var answers = [];
            for (var i = 0; i < this.Answers.length; i++) {
                var answer = this.Answers[i];
                answers.push(answer.getValues());
            }
            return {
                Id: questionInitValues.Id,
                Text: questionName.val(),
                Answers: answers,
                AnswerType: this.getQuestionType(),
            };
        };
        
        questionObject.AnswerHolder = answerHolder;
        questionObject.Id = questionInitValues.Id;

        var adjustRadioButtonEnabled = function() {
            if (questionObject.Answers.length == 0) {
                questionTypeText.removeAttr('disabled');
                questionTypeCalendar.removeAttr('disabled');
            } else {
                questionTypeText.attr('disabled', 'disabled');
                questionTypeCalendar.attr('disabled', 'disabled');
            }
        };

        var numAnswersAdded = 0;

        questionObject.addAnswer = function (initialValues, immediate) {
            if (immediate === undefined)
                immediate = false;

            var answerInitValues = $.extend({
                Id: 0,
                Selected: this.Answers.length == 0 ? true: false,
                QuestionGUID: questionInitValues.QuestionGUID,
                Text: 'Enter answer text'
            }, initialValues);

            var answerObject;
            if (this.getQuestionType() == 'Timeslot') {
                answerObject = createCalendarAnswerObject(this, answerInitValues);
            } else {
                answerObject = createTextAnswerObject(this, answerInitValues);
            }
            this.Answers.push(answerObject);

            var answerNode = answerObject.Node;
            answerNode.hide();

            this.AnswerHolder.append(answerNode);
            if (immediate)
                answerNode.show();
            else
                answerNode.slideDown();

            adjustRadioButtonEnabled();

            if (settings.MaxAdditionalAnswers != -1 && numAnswersAdded >= settings.MaxAdditionalAnswers) {
                addAnswerSpan.hide();
            }

            return answerObject;
        };
        
        questionObject.removeAnswer = function (answerObject) {
            this.Answers.splice(this.Answers.indexOf(answerObject), 1);
            answerObject.Node.slideUp();

            numAnswersAdded--;
            if (settings.MaxAdditionalAnswers == -1 || numAnswersAdded < settings.MaxAdditionalAnswers) {
                addAnswerSpan.show();
            }

            adjustRadioButtonEnabled();
        };

        questionObject.RemoveMe = function () {
            nodes.Questions.splice(nodes.Questions.indexOf(questionObject), 1);
            questionDiv.slideUp();
        };
        
        nodes.Questions.push(questionObject);
        
        return questionObject;
    }
    
    function createTextAnswerObject(question, answerInitValues) {
        var answerObject = {};

        var radioSelect = $('<input type="radio"/>')
            .attr('name', answerInitValues.QuestionGUID);

        if (answerInitValues.Id == 0)
            radioSelect.click();

        if (answerInitValues.Selected)
            radioSelect.attr('checked', true);

        if (settings.DisplayMode != 'view')
            radioSelect.hide();

        var answerText = $('<input />')
            .addClass('form-control')
            .addClass('col-md-1')
            .val(answerInitValues.Text);

        if (!settings.AllowModifyAnswer && answerInitValues.Id > 0)
            answerText.attr('disabled', 'disabled');

        var deleteButton = $('<img src="/Content/img/Cross.png" />')
            .addClass('handIcon')
            .click(function() {
                answerObject.RemoveMe();
            });

        if (!settings.AllowDeleteAnswer && answerInitValues.Id > 0)
            deleteButton.hide();

        var answerDiv =
            $('<div />')
                .addClass('answerBox')
                .append(
                    $('<table />')
                        .css('width', '100%')
                        .append(
                            $('<tr />')
                                .append(
                                    $('<td />')
                                        .append(
                                            radioSelect
                                        )
                                ).append(
                                    $('<td />')
                                        .css('width', '100%')
                                        .append(
                                            answerText
                                        )
                                ).append(
                                    $('<td />')
                                        .append(
                                            deleteButton
                                        )
                                )
                        )
                );

        answerObject.Question = question;
        answerObject.getValues = function () {
            return {
                Id: answerInitValues.Id,
                Text: answerText.val(),
                Selected: radioSelect.get(0).checked
            };
        };

        answerObject.RemoveMe = function () {
            this.Question.removeAnswer(answerObject);
        };

        answerObject.Node = answerDiv;
        return answerObject;
    }
    
    function createCalendarAnswerObject(question, answerInitValues) {
        var answerObject = {};
        answerObject.CalendarID = answerInitValues.CalendarID;

        var radioSelect = $('<input type="radio"/>')
            .attr('name', answerInitValues.QuestionGUID);

        if (answerInitValues.Id == 0)
            radioSelect.click();

        if (answerInitValues.Selected)
            radioSelect.attr('checked', true);

        if (settings.DisplayMode != 'view')
            radioSelect.hide();

        var topDiv = $('<div>');
        var renderer = $('<div>')
            .addClass('eventWindowRendererTemplate');

        var group = $('<div>')
            .css('padding-bottom', '10px')
            .css('min-height', '65px');

        var deleteButton = $('<img src="/Content/img/Cross.png" />')
            .addClass('handIcon')
            .click(function() {
                answerObject.RemoveMe();
            });
        
        if (!settings.AllowDeleteAnswer && answerInitValues.Id > 0)
            deleteButton.hide();

        var editEventWindowsButton = $('<a>Edit Event Windows</a>')
            .addClass('cancel-btn')
            .addClass('handIcon')
            .css('margin', '10px')
            .css('width', '100%')
            .click(function() {
                answerObject.OpenEventWindowSelection();
            });

        if (!settings.AllowModifyAnswer && answerInitValues.Id > 0)
            editEventWindowsButton.hide();

        group.append(renderer);
        group.append(editEventWindowsButton);

        var answerDiv =
            $('<div />')
                .addClass('eventWindowAnswerBox')
                .append(
                    $('<table />')
                        .css('width', '100%')
                        .append(
                            $('<tr />')
                                .append(
                                    $('<td />')
                                        .append(
                                            radioSelect
                                        )
                                ).append(
                                    $('<td />')
                                    .css('width', '100%')
                                    .append(
                                        group
                                    )
                                ).append(
                                    $('<td />')
                                        .append(
                                            deleteButton
                                        )
                                )
                        )
                );

        answerObject.RenderEvents = function(events) {
            renderer.empty();

            var padMins = function (mins) {
                if (mins < 10)
                    return mins + '0';
                return mins;
            };
            var padHours = function(hours) {
                if (hours < 10)
                    return '0' + hours;
                return hours;
            };

            if (events === null || events === undefined || events.length == 0) {
                renderer.append('No events.');
            } else {
                var sortFunc = function(left, right) {
                    var leftHours = left.startDate.getHours();
                    var rightHours = right.startDate.getHours();
                    return leftHours - rightHours;
                };
                events.sort(sortFunc);

                for (var i = 0; i < events.length; i++) {
                    var currEvent = events[i];

                    var startDateString;
                    if (currEvent.startDate.toDateString === undefined) {
                        startDateString = currEvent.startDate.d.toDateString();
                    } else {
                        startDateString = currEvent.startDate.toDateString();
                    }

                    var startDateTime = padHours(currEvent.startDate.getHours()) + ':' + padMins(currEvent.startDate.getMinutes());
                    var endDateTime = padHours(currEvent.endDate.getHours()) + ':' + padMins(currEvent.endDate.getMinutes());
                    var eventStr = startDateString + ' ' + startDateTime + ' - ' + endDateTime;

                    var clonedHTML = $('<div>');
                    clonedHTML.html(eventStr);

                    renderer.append(clonedHTML);
                }
            }

            renderer.show();
        };

        answerObject.OpenEventWindowSelection = function () {
            var _that = this;
            
            var launchCalendar = function(calID) {
                _that.CalendarID = calID;

                Kwasant.IFrame.Display('/Calendar/GetNegotiationCalendars?calendarID=' + calID,
                    {
                        horizontalAlign: 'left',
                        callback: function (result) {
                            var filteredEvents = $.grep(result.events, function(elem) {
                                if (elem.tag[0] == calID)
                                    return true;
                                return false;
                            });
                            _that.RenderEvents($.map(filteredEvents, function (elem) {
                                return {
                                    startDate: elem.start,
                                    endDate: elem.end
                                };
                            }));
                        }
                    });
            };

            if (this.CalendarID == null) {
                Kwasant.IFrame.DispatchUrlRequest('/Question/EditTimeslots?calendarID=null&negotiationID=' + initValues.Id, launchCalendar);
            } else {
                launchCalendar(_that.CalendarID);
            }
        };
        
        answerObject.Question = question;
        answerObject.RemoveMe = function () {
            this.Question.removeAnswer(this);
        };

        answerObject.getValues = function() {
            return {
                Id: answerInitValues.Id,
                CalendarID: this.CalendarID,
                Selected: radioSelect.get(0).checked
            };
        };

        topDiv.append(answerDiv);
        
        answerObject.RenderEvents(answerInitValues.CalendarEvents);

        answerObject.Node = topDiv;
        return answerObject;
    }

}(jQuery));