var survey;
var InputChoice = (function () {
    function InputChoice() {
    }
    return InputChoice;
}());
var Survey = (function () {
    function Survey() {
        this.choices = [];
    }
    Survey.prototype.isLastChoice = function (choice) {
        return this.choices.indexOf(choice) === this.choices.length - 1;
    };
    Survey.prototype.removeChoice = function (choice) {
        document.getElementById("choicesContainer").removeChild(choice.element);
        var index = this.choices.indexOf(choice, 0);
        if (index > -1) {
            this.choices.splice(index, 1);
        }
        previewSurvey();
    };
    Survey.prototype.addChoice = function () {
        var _this = this;
        var choice = new InputChoice();
        choice.element = document.createElement("div");
        choice.element.style.display = "flex";
        choice.element.style.marginBottom = "10px";
        choice.element.style.width = "100%";
        choice.input = document.createElement("input");
        choice.input.style.flex = "1 1 100%";
        choice.input.type = "text";
        choice.input.placeholder = "Enter a choice";
        choice.input.className = "field";
        choice.input.oninput = function (e) {
            choice.value = choice.input.value;
            if (_this.isLastChoice(choice)) {
                choice.button.style.display = "block";
                _this.addChoice();
            }
            previewSurvey();
        };
        choice.button = document.createElement("div");
        choice.button.className = "removeChoiceButton";
        choice.button.style.display = "none";
        choice.button.onclick = function (e) {
            _this.removeChoice(choice);
            _this.choices[_this.choices.length - 1].button.style.display = "none";
        };
        var divElement = document.createElement("div");
        divElement.style.flex = "0 0 36px";
        divElement.style.marginLeft = "8px";
        divElement.appendChild(choice.button);
        choice.element.appendChild(choice.input);
        choice.element.appendChild(divElement);
        document.getElementById("choicesContainer").appendChild(choice.element);
        this.choices.push(choice);
        return choice;
    };
    Survey.prototype.toActionCard = function () {
        var choices = "";
        for (var i = 0; i < survey.choices.length - 1; i++) {
            choices += '{ "display": "' + survey.choices[i].value + '", "value": "' + i.toString() + '"}';
            if (i < survey.choices.length - 2) {
                choices += ',';
            }
        }
        var closingTime = new Date();
        closingTime.setMinutes(closingTime.getMinutes() + survey.duration);
        var result = '{' +
            '"@type": "MessageCard",' +
            '"@context": "http://schema.org/extensions",' +
            '"themeColor": "00B200",' +
            '"title": "' + survey.name + '",' +
            '"text": "Survey closes at **' + closingTime.toLocaleString() + '**",' +
            '"sections": [' +
            '{' +
            '"title": "' + survey.question + '"';
        if (survey.choices.length > 1) {
            result += ',' +
                '"potentialAction": [' +
                '{' +
                '"@type": "ActionCard",' +
                '"name": "Respond",' +
                '"inputs": [' +
                '{' +
                '"@type": "MultichoiceInput",' +
                '"title": "Select an option",' +
                '"id": "input",' +
                '"choices": [' +
                choices +
                ']' +
                '}' +
                '],' +
                '"actions": [' +
                '{' +
                '"@type": "HttpPOST",' +
                '"name": "Submit"' +
                '}' +
                ']' +
                '}' +
                ']';
        }
        result +=
            '}' +
                ']' +
                '}';
        return result;
    };
    return Survey;
}());
function previewSurvey() {
    survey.name = document.getElementById("surveyName").value;
    survey.question = document.getElementById("questionTitle").value;
    var timeSelect = document.getElementById("surveyTime");
    survey.duration = Number(timeSelect.options[timeSelect.selectedIndex].value);
    var renderedPreview;
    if (!isNullOrEmpty(survey.name) || !isNullOrEmpty(survey.question) || survey.choices.length > 1) {
        var messageCard = new MessageCard();
        messageCard.parse(JSON.parse(survey.toActionCard()));
        renderedPreview = messageCard.render();
    }
    else {
        renderedPreview = document.createElement("div");
        renderedPreview.innerHTML = "Please fill in the fields on the left to see a preview of your poll.";
    }
    var node = document.getElementById('surveyPreviewContainer');
    node.innerHTML = '';
    node.appendChild(renderedPreview);
}
function sendSurvey() {
    var allInputs = document.getElementsByTagName("input");
    for (var i = 0; i < allInputs.length; i++) {
        allInputs[i].readOnly = true;
        allInputs[i].className = "field readonly";
    }
    document.getElementById("btnSendSurvey").disabled = true;
    document.getElementById("progressIndicator").style.display = "inline-block";
    document.getElementById("status").style.display = "none";
    var request = new XMLHttpRequest();
    request.onload = function () {
        for (var i = 0; i < allInputs.length; i++) {
            allInputs[i].readOnly = false;
            allInputs[i].className = "field";
        }
        document.getElementById("btnSendSurvey").disabled = false;
        document.getElementById("progressIndicator").style.display = "none";
        document.getElementById("status").style.display = "inline-block";
        document.getElementById("errorMessages").style.display = "none";
        var messages = JSON.parse(request.responseText);
        if (messages.length > 0) {
            document.getElementById("status").textContent = "There was a problem sending the survey.";
            var messagePre = document.createElement("pre");
            var messageCode = document.createElement("code");
            messageCode.textContent = JSON.stringify(messages, null, 2);
            messagePre.appendChild(messageCode);
            document.getElementById("errorMessages").appendChild(messagePre);
            document.getElementById("errorMessages").style.display = "block";
        }
    };
    request.open("POST", "/Survey/SendSurvey");
    request.setRequestHeader("Content-Type", "application/json");
    var choices = "";
    for (var i = 0; i < survey.choices.length - 1; i++) {
        choices += survey.choices[i].value;
        if (i < survey.choices.length - 2) {
            choices += ';';
        }
    }
    var requestBody = '{' +
        '  "Survey": {' +
        '    "Name": "' + survey.name + '",' +
        '    "Duration": "' + survey.duration + '",' +
        '    "QuestionTitle": "' + survey.question + '",' +
        '    "QuestionChoices": "' + choices + '"' +
        '  },' +
        '  "ToRecipients": "' + document.getElementById("toRecipients").value + '"' +
        '}';
    request.send(requestBody);
}
window.onload = function () {
    survey = new Survey();
    survey.addChoice();
    previewSurvey();
    document.getElementById("toRecipients").focus();
};
//# sourceMappingURL=survey.js.map