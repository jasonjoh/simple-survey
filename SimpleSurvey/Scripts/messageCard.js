function parsePicture(container, json, defaultSize, defaultStyle) {
    if (defaultSize === void 0) { defaultSize = Size.Medium; }
    if (defaultStyle === void 0) { defaultStyle = PictureStyle.Normal; }
    var picture = new Picture(container);
    picture.url = json["image"];
    picture.altText = json["title"];
    picture.size = stringToSize(json["size"], defaultSize);
    return picture;
}
function parsePictureGallery(container, json) {
    var pictureGallery = new PictureGallery(container);
    var pictureArray = json;
    for (var i = 0; i < pictureArray.length; i++) {
        var picture = parsePicture(container, pictureArray[i], Size.Large);
        picture.height = 100;
        pictureGallery.items.push(picture);
    }
    return pictureGallery;
}
function parseFactGroup(container, json) {
    var factGroup = new FactGroup(container);
    var factArray = json;
    for (var i = 0; i < factArray.length; i++) {
        var fact = new Fact();
        fact.parse(factArray[i]);
        factGroup.items.push(fact);
    }
    return factGroup;
}
function parseActionGroup(container, json) {
    var actionGroup = new ActionGroup(container);
    var actionArray = json;
    for (var i = 0; i < actionArray.length; i++) {
        var action = Action.create(actionGroup, actionArray[i]["@type"]);
        action.parse(actionArray[i]);
        actionGroup.actions.push(action);
    }
    return actionGroup;
}
function parseSection(container, json) {
    var section = new Container(container, ["Section"]);
    section.startGroup = json["startGroup"];
    if (json["title"] != undefined) {
        var textBlock = new TextBlock(section);
        textBlock.text = json["title"];
        textBlock.textSize = TextSize.Medium;
        section.addElement(textBlock);
    }
    section.style = json["style"] == "emphasis" ? ContainerStyle.Emphasis : ContainerStyle.Normal;
    if (json["activityTitle"] != undefined || json["activitySubtitle"] != undefined ||
        json["activityText"] != undefined || json["activityImage"] != undefined) {
        var columnGroup = new ColumnGroup(container);
        // Image column
        if (json["activityImage"] != null) {
            var column_1 = columnGroup.addColumn();
            column_1.size = Size.Auto;
            var picture = new Picture(column_1);
            picture.size = stringToSize(json["activityImageSize"], Size.Small);
            picture.style = stringToPictureStyle(json["activityImageStyle"], PictureStyle.Person);
            picture.url = json["activityImage"];
            column_1.addElement(picture);
        }
        // Text column
        var column = columnGroup.addColumn();
        column.size = Size.Stretch;
        if (json["activityTitle"] != null) {
            var textBlock = new TextBlock(column);
            textBlock.text = json["activityTitle"];
            column.addElement(textBlock);
        }
        if (json["activitySubtitle"] != null) {
            var textBlock = new TextBlock(column);
            textBlock.text = json["activitySubtitle"];
            textBlock.textWeight = TextWeight.Lighter;
            textBlock.isSubtle = true;
            textBlock.topSpacing = Spacing.None;
            column.addElement(textBlock);
        }
        if (json["activityText"] != null) {
            var textBlock = new TextBlock(column);
            textBlock.text = json["activityText"];
            textBlock.topSpacing = Spacing.None;
            column.addElement(textBlock);
        }
        section.addElement(columnGroup);
    }
    var heroImage = json["heroImage"];
    if (heroImage != undefined) {
        var picture = parsePicture(section, heroImage);
        picture.size = Size.Auto;
        section.addElement(picture);
    }
    if (json["text"] != undefined) {
        var text = new TextBlock(section);
        text.text = json["text"];
        section.addElement(text);
    }
    if (json["facts"] != undefined) {
        var factGroup = parseFactGroup(section, json["facts"]);
        section.addElement(factGroup);
    }
    if (json["images"] != undefined) {
        var pictureGallery = parsePictureGallery(section, json["images"]);
        section.addElement(pictureGallery);
    }
    if (json["potentialAction"] != undefined) {
        var actionGroup = parseActionGroup(section, json["potentialAction"]);
        section.addElement(actionGroup);
    }
    return section;
}
var MessageCard = (function () {
    function MessageCard() {
    }
    MessageCard.prototype.parse = function (json) {
        this.summary = json["summary"];
        this.themeColor = json["themeColor"];
        this._rootContainer = new Container(null);
        this._rootContainer.topSpacing = Spacing.None;
        if (json["title"] != undefined) {
            var textBlock = new TextBlock(this._rootContainer);
            textBlock.text = json["title"];
            textBlock.textSize = TextSize.Large;
            this._rootContainer.addElement(textBlock);
        }
        if (json["text"] != undefined) {
            var textBlock = new TextBlock(this._rootContainer);
            textBlock.text = json["text"],
                this._rootContainer.addElement(textBlock);
        }
        if (json["sections"] != undefined) {
            var sectionArray = json["sections"];
            for (var i = 0; i < sectionArray.length; i++) {
                var section = parseSection(this._rootContainer, sectionArray[i]);
                if (i == 0 && this._rootContainer.elementCount == 0) {
                    section.topSpacing = Spacing.None;
                }
                this._rootContainer.addElement(section);
            }
        }
        if (json["potentialAction"] != undefined) {
            var actionGroup = parseActionGroup(this._rootContainer, json["potentialAction"]);
            this._rootContainer.addElement(actionGroup);
        }
    };
    MessageCard.prototype.render = function () {
        ActionGroup.buttonStyle = ActionButtonStyle.Link;
        var element = document.createElement("div");
        if (isNullOrEmpty(this.themeColor)) {
            element.style.border = "1px solid #F1F1F1";
        }
        else {
            element.style.borderTop = "1px solid #F1F1F1";
            element.style.borderRight = "1px solid #F1F1F1";
            element.style.borderBottom = "1px solid #F1F1F1";
            element.style.borderLeft = "3px solid #" + this.themeColor;
        }
        var renderedContainer = this._rootContainer.internalRender();
        renderedContainer.className = "rootContainer";
        appendChild(element, renderedContainer);
        return element;
    };
    return MessageCard;
}());
//# sourceMappingURL=messageCard.js.map