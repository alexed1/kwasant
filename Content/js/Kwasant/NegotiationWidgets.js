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
            AllowAddQuestion: false,
            AllowModifyQuestion: false,
            AllowDeleteQuestion: false,

            AllowAddAnswer: true,
            AllowDeleteAnswer: false,
        }, options);

        initValues = $.extend({            
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

        that.append('<h4>Negotiation<h4>');

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
            
        baseInfoTable.append(attendeesRow);
        baseInfoDiv.append(baseInfoTable);
        
        var questionHolder = $('<div></div>');
        baseInfoDiv.append(questionHolder);
        
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
                addQuestion(); //Make it nice and slide down
            });
        
        baseInfoDiv.append(addQuestionSpan);

        that.append(baseInfoDiv);

        nodes.Name = nameInput;
        nodes.Attendees = attendeesInput;
    }
    
    function addQuestion(initialValues) {
        var questionInitValues = $.extend({            
            Name: 'Enter question text'
        }, initialValues);

        var questionObject = createQuestionObject(questionInitValues);
        var questionNode = questionObject.Node;
        questionNode.hide();
        
        nodes.QuestionHolder.append(questionNode);
        questionNode.slideDown();
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
            
        var questionName = $('<input type="text" />')
            .addClass('form-control')
            .addClass('col-md-1')
            .val(questionInitValues.Name);

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
                            $('<label />')
                            .val('Question: ')
                        )
                    ).append(
                        $('<td />')
                        .append(
                            questionName
                        )
                    ).append(
                        $('<td />')
                        .append(
                            $('<img src="/Content/img/Cross.png"></img>')
                            .addClass('handIcon')
                            .click(function() {
                                questionObject.RemoveMe();
                            })
                        )
                    )
                )
                .append(
                    $('<tr />')
                    .append(
                        $('<td />')
                    ).append(
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
                        )
                    )
                )
            )
            .append(
                answerHolder
            )
            .append(
                $('<span>')
                .addClass('form-group')
                .addClass('handIcon')
                .click(function () { questionObject.addAnswer(); })
                .append(
                    $('<img src="/Content/img/plus.png" />')
                ).append(
                    $('<label>Add Answer</label>')
                    .addClass('handIcon')
                )
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
                Name: questionName.val(),
                Type: this.getQuestionType(),
                Answers: answers
            };
        };
        questionObject.AnswerHolder = answerHolder;

        questionObject.addAnswer = function (initialValues) {
            var answerInitValues = $.extend({
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
            answerNode.slideDown();
            return answerObject;
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
        
        var answerText = $('<input />')
            .addClass('form-control')
            .addClass('col-md-1')
            .val(answerInitValues.Text);
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
                                            $('<img src="/Content/img/minus.png" />')
                                                .addClass('handIcon')
                                        )
                                ).append(
                                    $('<td />')
                                        .append(
                                            answerText
                                        )
                                ).append(
                                    $('<td />')
                                        .append(
                                            $('<img src="/Content/img/Cross.png" />')
                                                .addClass('handIcon')
                                                .click(function() {
                                                    answerObject.RemoveMe();
                                                })
                                        )
                                )
                        )
                );

        answerObject.Question = question;
        answerObject.getValues = function () {
            return {
                Text: answerText.val()
            };
        };

        answerObject.RemoveMe = function () {
            this.Question.Answers.splice(this.Question.Answers.indexOf(answerObject), 1);
            answerDiv.slideUp();
        };

        answerObject.Node = answerDiv;
        return answerObject;
    }
    function createCalendarAnswerObject(answerInitValues) {
        alert('Calendar...');
    }

}(jQuery));