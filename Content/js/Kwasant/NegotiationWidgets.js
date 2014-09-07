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
            
            //Based on AnswerState.cs - this is overridable via the options
            AnswerProposedStatus: 2, 
            AnswerSelectedStatus: 3,

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
            Id: null,
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
            returnNeg.Attendees = nodes.Attendees.val().split(',');
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
        
        if (settings.DisplayMode == 'reply') {
            nameRow.hide();
            attendeesRow.hide();
        }

    }
    
    function addQuestion(initialValues, immediate) {
        if (immediate === undefined)
            immediate = false;

        if (initialValues === undefined || initialValues === null)
            initialValues = {};

        var questionInitValues = $.extend({
            Id: 0,
            CalendarID: initialValues.CalendarID,
            QuestionGUID: guid(),
            Type: 'Text',
            Text: ''
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
                if (!answerValues.EventID)
                    questionObject.addTextAnswer(answerValues, true);
                else 
                    questionObject.addAnswer(answerValues, true);
            }
        }

        return questionObject;
    }
    
    function createQuestionObject(questionInitValues) {
        var questionObject = {};

        questionObject.CalendarID = questionInitValues.CalendarID;

        var groupID = guid();

        var answerHolder = $('<div></div>');

        var questionTypeText = $('<input type="radio"/>')
            .attr('name', groupID)
            .attr('QuestionType', 'Text');

        var questionTypeCalendar = $('<input type="radio"/>')
            .attr('name', groupID)
            .attr('QuestionType', 'Timeslot');

        questionObject.OpenEventWindowSelection = function () {
            var _that = this;

            var launchCalendar = function (calID) {
                _that.CalendarID = calID;
                Kwasant.IFrame.Display('/Calendar/GetNegotiationCalendars?calendarID=' + calID,
                    {
                        horizontalAlign: 'left',
                        callback: function (result) {
                            var filteredEvents = $.grep(result.events, function (elem) {
                                if (elem.tag[0] == calID)
                                    return true;
                                return false;
                            });
                            
                            //Here we need to find out which answers to leave, which to delete, and which to add
                            var answersToAdd = [];
                            var touchedAnswers = [];
                            $.each(filteredEvents, function (i, event) {
                                var foundMatchingAnswer = false;
                                $.each(_that.Answers, function (j, answer) {
                                    if (foundMatchingAnswer)
                                        return;

                                    if (answer.EventID == event.id) {
                                        touchedAnswers.push(answer);
                                        foundMatchingAnswer = true;
                                    }
                                });
                                
                                if (!foundMatchingAnswer) {
                                    answersToAdd.push({
                                        EventStart: event.start,
                                        EventEnd: event.end,
                                        EventID: event.id,
                                        Type: _that.Type
                                    });
                                }
                            });

                            //We need to copy the array, because it's being modified
                            var tmpAnswers = _that.Answers.slice(0);
                            $.each(tmpAnswers, function (j, answer) {
                                if ($.inArray(answer, touchedAnswers) == -1) {
                                    //Remove it!
                                    answer.RemoveMe();
                                }
                            });
                            $.each(answersToAdd, function(k, newAnswer) {
                                _that.addAnswer(newAnswer);
                            });
                        }
                    });
            };

            if (this.CalendarID == null) {
                Kwasant.IFrame.DispatchUrlRequest('/Question/EditTimeslots?calendarID=null&negotiationID=' + initValues.Id, launchCalendar);
            } else {
                launchCalendar(_that.CalendarID);
            }
        };
        
        var selectEventWindowsButton = $('<a>')
            .addClass('handIcon')
            .append('Select Times')
            .click(function () { questionObject.OpenEventWindowSelection(); });

        var radioButtons = [questionTypeText, questionTypeCalendar];


        var topWidget = $('<div>');
        var edittableType =
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

        topWidget.append(edittableType);
        topWidget.append(selectEventWindowsButton);
        
        if (!settings.AllowModifyQuestion) {
            edittableType.hide();
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

        var configureAnswerButton = function(isCalendar) {
            if (isCalendar) {
                if (settings.DisplayMode === 'review')
                    selectEventWindowsButton.hide();
                else {
                    selectEventWindowsButton.show();
                }
                
            } else {
                selectEventWindowsButton.hide();
                
                if (settings.MaxAdditionalAnswers != -1 && numAnswersAdded >= settings.MaxAdditionalAnswers) {
                    addAnswerSpan.hide();
                } else if (settings.MaxAdditionalAnswers == -1 || numAnswersAdded < settings.MaxAdditionalAnswers) {
                    addAnswerSpan.show();
                }
            }
        };

        $.each(radioButtons, function (index, elem) {
            var closedFunc = function () {
                reconfigureAnswerButton();
            };
            elem.change(closedFunc);
        });

        var addAnswerSpan = $('<span>')
            .addClass('form-group')
            .addClass('handIcon')
            .click(function () {
                numAnswersAdded++;
                questionObject.addTextAnswer();
            })
            .append(
                $('<img src="/Content/img/plus.png" />')
            ).append(
                $('<label>Add answer</label>')
                .addClass('handIcon')
            );

        var reconfigureAnswerButton = function() {
            if (questionObject.getQuestionType() == 'Timeslot') {
                questionTypeCalendar.get(0).checked = true;
                configureAnswerButton(true);
            } else {
                questionTypeText.get(0).checked = true;
                configureAnswerButton(false);
            }
        };

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
                                topWidget
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

        questionObject.getQuestionType = function () {
            for (var i = 0; i < radioButtons.length; i++) {
                var button = radioButtons[i];
                if (button.get(0).checked) {
                    return button.attr('QuestionType');
                }
            }
            return 'Text';
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
                CalendarID: this.CalendarID,
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

        questionObject.addTextAnswer = function (initialValues, immediate) {
            if (!initialValues)
                initialValues = {};
            initialValues.ForceTextAnswer = true;
            this.addAnswer(initialValues, immediate);
        };

        questionObject.addAnswer = function (initialValues, immediate) {
            if (immediate === undefined)
                immediate = false;

            if (initialValues === null || initialValues === undefined)
                initialValues = {};

            var answerInitValues = $.extend({
                Id: 0,
                VotedBy: [],
                StartDate: initialValues.StartDate,
                EndDate: initialValues.EndDate,
                AnswerState: settings.AnswerProposedStatus,
                Selected: this.Answers.length == 0 ? true: false,
                QuestionGUID: questionInitValues.QuestionGUID,
                Text: ''
            }, initialValues);

            var answerObject;
            if (!initialValues.ForceTextAnswer && this.getQuestionType() == 'Timeslot') {
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

            reconfigureAnswerButton();
          
            return answerObject;
        };

        questionObject.UnselectOtherAnswers = function(exceptObject) {
            for (var i = 0; i < this.Answers.length; i++) {
                var currAnswer = this.Answers[i];
                if (currAnswer !== exceptObject) {
                    currAnswer.unmarkMeSelected();
                }
            }
        };

        questionObject.removeAnswer = function (answerObject) {
            this.Answers.splice(this.Answers.indexOf(answerObject), 1);
            answerObject.Node.slideUp();

            numAnswersAdded--;

            reconfigureAnswerButton();

            adjustRadioButtonEnabled();
        };

        questionObject.RemoveMe = function () {
            nodes.Questions.splice(nodes.Questions.indexOf(questionObject), 1);
            questionDiv.slideUp();
        };
        

        //Check radio buttons based on original settings
        for (var i = 0; i < radioButtons.length; i++) {
            var button = radioButtons[i];
            if (button.attr('QuestionType') == questionInitValues.Type) {
                button.get(0).checked = true;
            }
        }
        
        reconfigureAnswerButton();

        nodes.Questions.push(questionObject);
        
        return questionObject;
    }
    
    function createTextAnswerObject(question, answerInitValues) {
        var answerObject = {};
        answerObject.AnswerState = answerInitValues.AnswerState;

        var radioSelect = $('<input type="radio"/>')
            .attr('name', answerInitValues.QuestionGUID);

        if (answerInitValues.Id == 0)
            radioSelect.click();

        if (answerInitValues.Selected)
            radioSelect.attr('checked', true);

        if (settings.DisplayMode != 'reply')
            radioSelect.hide();

        var answerText = $('<input />')
            .addClass('form-control')
            .addClass('col-md-1')
            .val(answerInitValues.Text);

        if (answerInitValues.DisableManualEdit || (!settings.AllowModifyAnswer && answerInitValues.Id > 0))
            answerText.attr('disabled', 'disabled');

        var deleteButton = $('<img src="/Content/img/Cross.png" />')
            .addClass('handIcon')
            .click(function() {
                answerObject.RemoveMe();
            });

        var peopleWhoVoted = 'No one voted for this answer.';
        for (var i = 0; i < answerInitValues.VotedBy.length; i++) {
            if (i > 0)
                peopleWhoVoted += ', ';
            else
                peopleWhoVoted = 'The following people voted for this answer: ';
            
            peopleWhoVoted += answerInitValues.VotedBy[i];
        }

        var votesIcon = $('<label>')
            .append(answerInitValues.VotedBy.length);

        answerObject.markMeSelected = function() {
            answerDiv
                .css('background-color', 'rgb(183, 228, 195)');
            question.UnselectOtherAnswers(answerObject);

            answerObject.AnswerState = settings.AnswerSelectedStatus;

            btnMarkProposed
                .val('Unmark as selected')
                .css('background-color', '#E01E26')
                .unbind('click')
                .click(function() {
                    answerObject.unmarkMeSelected();
                });
        };

        answerObject.unmarkMeSelected = function() {
            answerDiv
                .css('background-color', '');
            
            answerObject.AnswerState = settings.AnswerProposedStatus;
            
            btnMarkProposed
                .val('Mark as selected')
                .css('background-color', '#3cc05e')
                .unbind('click')
                .click(function () {
                    answerObject.markMeSelected();
                });
        };
        
        var btnMarkProposed = $('<input type="button"/>')
            .val('Mark as selected')
            .addClass('btn')
            .addClass('handIcon')
            .css('background-color', '#3cc05e')
            .css('border-width', '0')
            .css('margin', '5px')
            .click(function () {
                answerObject.markMeSelected();
            });

        if (settings.DisplayMode != 'review') {
            votesIcon.hide();
            btnMarkProposed.hide();
        }

        if (answerInitValues.DisableManualEdit || !settings.AllowDeleteAnswer && answerInitValues.Id > 0)
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
                                ).append(
                                    $('<td />')
                                        .append(
                                            votesIcon
                                        )
                                )
                        )
                ).append(
                    $('<div />')
                        .append(btnMarkProposed)
                );


        if (settings.DisplayMode == 'review')
            answerDiv.attr('title', peopleWhoVoted);

        answerObject.Id = answerInitValues.Id;
        answerObject.Question = question;
        answerObject.getValues = function () {
            return {
                Id: answerObject.Id,
                AnswerState: answerObject.AnswerState,
                Text: answerText.val(),
                Selected: radioSelect.get(0).checked
            };
        };

        answerObject.RemoveMe = function () {
            this.Question.removeAnswer(answerObject);
        };

        if (answerObject.AnswerState == settings.AnswerSelectedStatus)
            answerObject.markMeSelected();

        answerObject.Node = answerDiv;
        return answerObject;
    }
    
    function createCalendarAnswerObject(question, answerInitValues) {

        var padMins = function (mins) {
            if (mins < 10)
                return mins + '0';
            return mins;
        };
        var padHours = function (hours) {
            if (hours < 10)
                return '0' + hours;
            return hours;
        };

        var startDateString;
        if (answerInitValues.EventStart.toDateString === undefined) {
            startDateString = answerInitValues.EventStart.d.toDateString();
        } else {
            startDateString = answerInitValues.EventStart.toDateString();
        }

        var startDateTime = padHours(answerInitValues.EventStart.getHours()) + ':' + padMins(answerInitValues.EventStart.getMinutes());
        var endDateTime = padHours(answerInitValues.EventEnd.getHours()) + ':' + padMins(answerInitValues.EventEnd.getMinutes());
        var eventStr = startDateString + ' ' + startDateTime + ' - ' + endDateTime;

        answerInitValues.Text = eventStr;
        answerInitValues.DisableManualEdit = true;

        var answerObject = createTextAnswerObject(question, answerInitValues);
        answerObject.EventID = answerInitValues.EventID;
        answerObject.EventStart = answerInitValues.EventStart;
        answerObject.EventEnd = answerInitValues.EventEnd;
        
        answerObject.baseGetValues = answerObject.getValues;
        answerObject.getValues = function() {
            var baseReturn = this.baseGetValues();
            baseReturn.EventID = this.EventID;
            
            return baseReturn;
        };

        return answerObject;
    }

}(jQuery));