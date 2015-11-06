(function (window) {

    //
    // Globals
    //
    var content = [];
    var contentByElement = new WeakMap();
    var rxFullMatch = /^\s*~([^#]+)#([^~]*)~\s*$/;
    var rxPartialMatch = /~([^#]+)#([^~]*)~/g;

    //
    // Helper functions
    //
    function isInDOM(element) {
        return element === document || element && isInDOM(element.parentNode);
    }

    function addClass(element, _class) {
        if (element) {
            if (element.className) {
                if (element.className.indexOf(_class) < 0) {
                    element.className += " " + _class;
                }
            } else {
                element.className = _class;
            }
        }
    }

    function removeClass(element, _class) {
        if (element) {
            if (element.className) {
                if (element.className.indexOf(_class) >= 0) {
                    element.className = element.className.replace(_class, "").replace(/^\s+|\s+$/g, "");
                }
            }
        }
    }

    function setClass(element, _class, mustSet) {
        if (mustSet) addClass(element, _class);
        else removeClass(element, _class);
    }

    function getOffset(element) {
        if (isInDOM(element)) {
            var box = element.getBoundingClientRect();
            var docElem = document.documentElement;

            return {
                top: box.top + (window.pageYOffset || docElem.scrollTop) - (docElem.clientTop || 0),
                left: box.left + (window.pageXOffset || docElem.scrollLeft) - (docElem.clientLeft || 0)
            };
        } else {
            return {
                top: 0,
                left: 0
            };
        }
    }

    function setOffset(element, top, left) {
        element.style.top = top + "px";
        element.style.left = left + "px";
    }


    //
    // Core methods
    //

    function nodeAdded(node) {
        var contentItem = {
            element: node,
            content: [],
            attributes: []
        };

        var html = node.innerHTML;
        
        var fullMatch = html.match(rxFullMatch);
        if (fullMatch) {

        } else {
            var partialMatches = html.match(partialMatches);
            if (partialMatches.length) {

            }
        }
    }

    function nodeRemoved(node) {

    }


    function watchDOM() {

        function processMutation(mutation) {
            switch (mutation.type) {
                case "childList":
                    var added = [];
                    var removed = [];

                    for (var j = 0; mutation.addedNodes && j < mutation.addedNodes.length; j++) {
                        var node = mutation.addedNodes[j];
                        if (typeof node.children != "undefined" && typeof node.getAttribute != "undefined") {
                            added.push(node);
                        }
                    }
                    for (var j = 0; mutation.removedNodes && j < mutation.removedNodes.length; j++) {
                        var node = mutation.removedNodes[j];
                        if (typeof node.children != "undefined" && typeof node.getAttribute != "undefined") {
                            removed.push(node);
                        }
                    }

                    for (var i = 0; i < added.length; i++) nodeAdded(added[i]);
                    for (var i = 0; i < removed.length; i++) nodeRemoved(removed[i]);

                    break;
                case "attributes":
                    break;
            }
        }

        function domCallback(mutations, observer) {
            if (mutations && mutations.length) {
                for (var i = 0; i < mutations.length; i++) {
                    processMutation(mutations[i]);
                }
            }
        };

        var domObserver = new MutationObserver(domCallback);
        domObserver.observe(document.body, { childList: true, attributes: false, subtree: true });
    }





    //
    // do stuff
    //

    watchDOM();

})(this);