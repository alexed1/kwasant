if (typeof(Kwasant) === 'undefined') {
    Kwasant = {};
}

if (typeof (Kwasant.IFrame) === 'undefined') {
    Kwasant.IFrame = {};
}

(function() {

    function setDefault(options) {
        if (options === undefined || options === null)
            options = {};

        if (options.paddingAmount === undefined)
            options.paddingAmount = 7;

        if (options.horizontalAlign === undefined || (options.horizontalAlign != 'middle' && options.horizontalAlign != 'left' && options.horizontalAlign != 'right'))
            options.horizontalAlign = 'middle';

        if (options.verticalAlign === undefined || (options.verticalAlign != 'centre' && options.verticalAlign != 'top' && options.verticalAlign != 'bottom'))
            options.verticalAlign = 'centre';

        if (options.modal === undefined)
            options.modal = false;

        if (options.pinned === undefined)
            options.pinned = true;

        if (typeof(options.callback) !== 'function')
            options.callback = null;

        if (options.displayLoadingSpinner === undefined)
            options.displayLoadingSpinner = true;
       
        return options;
    }

    function getScrollbarWidth() {
        var outer = document.createElement("div");
        outer.style.visibility = "hidden";
        outer.style.width = "100px";
        outer.style.msOverflowStyle = "scrollbar"; // needed for WinJS apps

        document.body.appendChild(outer);

        var widthNoScroll = outer.offsetWidth;
        // force scrollbars
        outer.style.overflow = "scroll";

        // add innerdiv
        var inner = document.createElement("div");
        inner.style.width = "100%";
        outer.appendChild(inner);

        var widthWithScroll = inner.offsetWidth;

        // remove divs
        outer.parentNode.removeChild(outer);

        return widthNoScroll - widthWithScroll;
    }

    var activePopups = [];

    Kwasant.IFrame.RegisterCloseEvent = function(iframe, callback) {
        if (document !== top.document) {
            top.Kwasant.IFrame.RegisterCloseEvent(iframe, callback);
            return;
        }
        
        activePopups.push({ document: $(iframe).contents().get(0), iframe: iframe, callback: callback });
        $(document.body).on('popupFormClosing', function(document, args) {
            close(args.document, args.args);
        });
    };

    Kwasant.IFrame.CloseMe = function (args, doc) {
        if (doc == null)
            doc = document;
        
        if (document !== top.document) {
            top.Kwasant.IFrame.CloseMe(args, doc);
            return;
        }

        parent.$('body').trigger('popupFormClosing', { document: doc, args: args });
    };

    function close(document, args) {
        for (var i = activePopups.length - 1; i >= 0 ; i--) {
            var obj = activePopups[i];
            if (obj.document == document) {
                if (obj.callback !== null && obj.callback !== undefined) {
                    obj.callback(args);
                }
                obj.iframe.hide();
                if (obj.iframe.mask)
                    obj.iframe.mask.hide();

                activePopups.splice(i, 1);
                break;
            }
        }
    };
    
    function displayLoadingSpinner() {
        var mask = $('<div></div>');
        mask.css({ 'position': 'absolute', 'z-index': 9999, 'background-color': '#FFF', 'display': 'none' });
        $(top.document.body).append(mask);

        var maskHeight = $(top.document).height();
        var maskWidth = $(top.document).width();
        mask.css({ 'left': 0, 'top': 0, 'width': maskWidth, 'height': maskHeight, 'margin': '0px', 'padding': '0px' });
        mask.fadeTo("fast", 0.8);

        var spinner = $('<img src="/Content/img/ajax-loader.gif"></a>');
        mask.append(spinner);
        spinner.show();

        var winH = $(top).height();
        var winW = $(top).width();

        var scrollTop = $(top).scrollTop();
        var scrollLeft = $(top).scrollLeft();

        var topPos = scrollTop + (winH - spinner.height()) / 2;
        var leftPos = scrollLeft + (winW - spinner.width()) / 2;

        spinner.css('position', 'absolute');
        spinner.css('top', topPos);
        spinner.css('left', leftPos);

        spinner.fadeTo("fast", 1);

        return mask;
    }

    Kwasant.IFrame.Display = function displayForm(url, options) {
        options = setDefault(options);

        var paddingAmount = options.paddingAmount;

        var spinner;
        if (options.displayLoadingSpinner)
            spinner = displayLoadingSpinner();
        var iframe = $('<iframe/>', {
            src: url,
            style: 'position: absolute;width: 500px;background-color: #FFFFFF;display: none;z-index: 9999;border: 1px solid #333;-moz-box-shadow:0 0 10px #000;-webkit-box-shadow:0 0 10px #000;box-shadow: #000 0px 0px 10px;padding:' + paddingAmount + 'px;',
            load: function () {                
                var iframeDoc = $(this).contents();
                var that = $(this);
                var reposition = function() {
                    var winH = $(top).height();
                    var winW = $(top).width();

                    var scrollTop = $(top).scrollTop();
                    var scrollLeft = $(top).scrollLeft();

                    var iframeWidth = iframeDoc.get(0).body.offsetHeight + (paddingAmount * 2);
                    var iframeHeight = iframeDoc.get(0).body.offsetWidth + (paddingAmount * 2);

                    var sidePadding = 2;
                    var topPos;
                    if (options.verticalAlign === 'top') {
                        topPos = scrollTop + sidePadding;
                    } else if (options.verticalAlign === 'centre') {
                        topPos = scrollTop + (winH - that.height()) / 2;
                    } else {
                        topPos = scrollTop + (winH - that.height()) - getScrollbarWidth() - sidePadding;
                    }

                    var leftPos;
                    if (options.horizontalAlign === 'right') {
                        leftPos = scrollLeft + (winW - that.width()) - getScrollbarWidth() - sidePadding;
                    } else if (options.horizontalAlign === 'middle') {
                        leftPos = scrollLeft + (winW - that.width()) / 2;
                    } else {
                        leftPos = scrollLeft + sidePadding;
                    }

                    that.css('top', topPos);
                    that.css('left', leftPos);

                    that.css('height', iframeWidth + 'px');
                    that.css('width', iframeHeight + 'px');
                };

                if (options.pinned) {
                    $(top).resize(reposition);
                    $(top).scroll(reposition);
                }

                if (options.modal) {
                    that.mask = $('<div></div>');
                    that.mask.css({ 'position': 'absolute', 'z-index': 9999, 'background-color': '#FFF', 'display': 'none' });
                    iframe.before(that.mask);

                    var maskHeight = $(top.document).height();
                    var maskWidth = $(top.document).width();
                    that.mask.css({ 'left': 0, 'top': 0, 'width': maskWidth, 'height': maskHeight });
                    that.mask.fadeTo("fast", 0.8);
                }

                $(this).fadeTo("fast", 1, function() {
                    $(this).focus();
                    //We need to position it twice. The first position allows the browser to calculate the dimensions. The second reposition moves it based on dimensions.
                    reposition();
                    reposition();

                    //We need a third time if we're sticking to the bottom. This is because the reposition has the potential to mess with the scroll bars - so we need to re-calculate it with that in mind.
                    if (options.verticalAlign === 'bottom') {
                        setTimeout(reposition, 100);
                    }
                    if (spinner)
                        spinner.hide();
                });

                Kwasant.IFrame.RegisterCloseEvent(that, options.callback);
            }
        });

        $(top.document.body).append(iframe);
        iframe.hide();
    };
})();