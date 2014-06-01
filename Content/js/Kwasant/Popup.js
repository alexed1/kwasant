if (typeof(Kwasant) === 'undefined') {
    Kwasant = {};
}

(function () {
    function setDefault(options) {
        if (options === undefined)
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

    Kwasant.DisplayIFrame = function displayForm(url, options) {
        options = setDefault(options);
        
        var paddingAmount = options.paddingAmount;
        var iframe = $('<iframe/>', {
            id: 'PopupIframe',
            src: url,
            style: 'position: absolute;width: 500px;background-color: #FFFFFF;display: none;z-index: 9999;border: 1px solid #333;-moz-box-shadow:0 0 10px #000;-webkit-box-shadow:0 0 10px #000;box-shadow: #000 0px 0px 10px;padding:' + paddingAmount + 'px;',
            load: function() {
                var iframeDoc = $(this).contents();
                var that = $(this);
                var reposition = function() {
                    var winH = $(window).height();
                    var winW = $(window).width();

                    var scrollTop = $(window).scrollTop();
                    var scrollLeft = $(window).scrollLeft();

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
                    $(window).resize(reposition);
                    $(window).scroll(reposition);
                }

                var mask;
                if (options.modal) {
                    mask = $('<div></div>');
                    mask.css({ 'position': 'absolute', 'z-index': 200, 'background-color': '#FFF', 'display': 'none' });
                    $('body').append(mask);
                    
                    
                    var maskHeight = $(document).height();
                    var maskWidth = $(document).width();
                    mask.css({ 'left': 0, 'top': 0, 'width': maskWidth, 'height': maskHeight });
                    mask.fadeTo("fast", 0.8);
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
                });

                $('body').bind('popupFormClosing', function(event, document) {
                    if (iframeDoc.get(0) === document) {
                        that.hide();
                        if (mask)
                            mask.hide();
                    }
                });
            }
        });
        $('body').append(iframe);
        iframe.hide();
    };

})();