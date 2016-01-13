(function (window) {

    // #region Polyfills

    //trim
    if (!String.prototype.trim) {
        String.prototype.trim = function () {
            return this.replace(/^[\s\uFEFF\xA0]+|[\s\uFEFF\xA0]+$/g, '');
        };
    }

    //textContent
    if (Object.defineProperty &&
        Object.getOwnPropertyDescriptor &&
        Object.getOwnPropertyDescriptor(Element.prototype, "textContent") &&
        !Object.getOwnPropertyDescriptor(Element.prototype, "textContent").get) {
        (function () {
            var innerText = Object.getOwnPropertyDescriptor(Element.prototype, "innerText");
            Object.defineProperty(Element.prototype, "textContent",
             {
                 get: function () {
                     return innerText.get.call(this);
                 },
                 set: function (s) {
                     return innerText.set.call(this, s);
                 }
             }
           );
        })();
    }

    //WeakMap
    /* (The MIT License)
     *
     * Copyright (c) 2012 Brandon Benvie <http://bbenvie.com>
     *
     * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
     * associated documentation files (the 'Software'), to deal in the Software without restriction,
     * including without limitation the rights to use, copy, modify, merge, publish, distribute,
     * sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
     * furnished to do so, subject to the following conditions:
     *
     * The above copyright notice and this permission notice shall be included with all copies or
     * substantial portions of the Software.
     *
     * THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
     * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
     * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY  CLAIM,
     * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
     * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
     */
    void function (a, b, c) { function d(a, b, c) { return h(a, b, { configurable: !0, writable: !0, value: c }) } function e(a) { return "function" != typeof a ? "" : "name" in a ? a.name : i.call(a).match(l)[1] } function f(a) { function b(b, e) { return e || 2 === arguments.length ? d.set(b, e) : (e = d.get(b), e === c && (e = a(b), d.set(b, e))), e } var d = new n; return a || (a = o), b } var g = Object.getOwnPropertyNames, h = Object.defineProperty, i = Function.prototype.toString, j = Object.create, k = Object.prototype.hasOwnProperty, l = /^\n?function\s?(\w*)?_?\(/, m = function () { function a() { var a = f(), c = {}; this.unlock = function (d) { var e = l(d); if (k.call(e, a)) return e[a](c); var f = j(null, b); return h(e, a, { value: function (a) { return a === c ? f : void 0 } }), f } } var b = { value: { writable: !0, value: c } }, e = j(null), f = function () { var a = Math.random().toString(36).slice(2); return a in e ? f() : e[a] = a }, i = f(), l = function (a) { if (k.call(a, i)) return a[i]; if (!Object.isExtensible(a)) throw new TypeError("Object must be extensible"); var b = j(null); return h(a, i, { value: b }), b }; return d(Object, "getOwnPropertyNames", function (a) { var b = g(a); return k.call(a, i) && b.splice(b.indexOf(i), 1), b }), d(a.prototype, "get", function (a) { return this.unlock(a).value }), d(a.prototype, "set", function (a, b) { this.unlock(a).value = b }), a }(), n = function (f) { function g(b) { if (this === a || null == this || this === g.prototype || !(this instanceof arguments.callee)) throw "WeakMap should not be called as a function but constructed with new"; o(this, new m), q(this, b) } function h(a) { n(a); var d = p(this).get(a); return d === b ? c : d } function i(a, d) { return n(a), p(this).set(a, d === c ? b : d), this } function j(a) { return n(a), p(this).get(a) !== c } function k(a) { n(a); var b = p(this), d = b.get(a) !== c; return b.set(a, c), d } function l() { return p(this), "[object WeakMap]" } var n = function (a) { if (null == a || "object" != typeof a && "function" != typeof a) throw new TypeError("Invalid WeakMap key") }, o = function (a, b) { var c = f.unlock(a); if (c.value) throw new TypeError("Object is already a WeakMap"); c.value = b }, p = function (a) { var b = f.unlock(a).value; if (!b) throw new TypeError("WeakMap is not generic"); return b }, q = function (a, b) { null !== b && "object" == typeof b && "function" == typeof b.forEach && b.forEach(function (c, d) { c instanceof Array && 2 === c.length && a.set(b[d][0], b[d][1]) }) }, r = ("" + Object).split("Object"), s = function () { return r[0] + e(this) + r[1] }; d(s, "toString", s); var t = { __proto__: [] } instanceof Array ? function (a) { a.__proto__ = s } : function (a) { d(a, "toString", s) }; return t(g), d(g.prototype, "toString", l), t(l), d(g.prototype, "get", h), t(h), d(g.prototype, "set", i), t(i), d(g.prototype, "has", j), t(j), d(g.prototype, "delete", k), t(k), g }(new m), o = Object.create ? function () { return Object.create(null) } : function () { return {} }; "undefined" != typeof module ? module.exports = n : "undefined" != typeof exports ? exports.WeakMap = n : "WeakMap" in a || (a.WeakMap = n), n.createStorage = f, a.WeakMap && (a.WeakMap.createStorage = f) }(function () { return this }());

    //classList
    /*! @source http://purl.eligrey.com/github/classList.js/blob/master/classList.js */
    if ("document" in self) { if (!("classList" in document.createElement("_"))) { (function (j) { "use strict"; if (!("Element" in j)) { return } var a = "classList", f = "prototype", m = j.Element[f], b = Object, k = String[f].trim || function () { return this.replace(/^\s+|\s+$/g, "") }, c = Array[f].indexOf || function (q) { var p = 0, o = this.length; for (; p < o; p++) { if (p in this && this[p] === q) { return p } } return -1 }, n = function (o, p) { this.name = o; this.code = DOMException[o]; this.message = p }, g = function (p, o) { if (o === "") { throw new n("SYNTAX_ERR", "An invalid or illegal string was specified") } if (/\s/.test(o)) { throw new n("INVALID_CHARACTER_ERR", "String contains an invalid character") } return c.call(p, o) }, d = function (s) { var r = k.call(s.getAttribute("class") || ""), q = r ? r.split(/\s+/) : [], p = 0, o = q.length; for (; p < o; p++) { this.push(q[p]) } this._updateClassName = function () { s.setAttribute("class", this.toString()) } }, e = d[f] = [], i = function () { return new d(this) }; n[f] = Error[f]; e.item = function (o) { return this[o] || null }; e.contains = function (o) { o += ""; return g(this, o) !== -1 }; e.add = function () { var s = arguments, r = 0, p = s.length, q, o = false; do { q = s[r] + ""; if (g(this, q) === -1) { this.push(q); o = true } } while (++r < p); if (o) { this._updateClassName() } }; e.remove = function () { var t = arguments, s = 0, p = t.length, r, o = false, q; do { r = t[s] + ""; q = g(this, r); while (q !== -1) { this.splice(q, 1); o = true; q = g(this, r) } } while (++s < p); if (o) { this._updateClassName() } }; e.toggle = function (p, q) { p += ""; var o = this.contains(p), r = o ? q !== true && "remove" : q !== false && "add"; if (r) { this[r](p) } if (q === true || q === false) { return q } else { return !o } }; e.toString = function () { return this.join(" ") }; if (b.defineProperty) { var l = { get: i, enumerable: true, configurable: true }; try { b.defineProperty(m, a, l) } catch (h) { if (h.number === -2146823252) { l.enumerable = false; b.defineProperty(m, a, l) } } } else { if (b[f].__defineGetter__) { m.__defineGetter__(a, i) } } }(self)) } else { (function () { var b = document.createElement("_"); b.classList.add("c1", "c2"); if (!b.classList.contains("c2")) { var c = function (e) { var d = DOMTokenList.prototype[e]; DOMTokenList.prototype[e] = function (h) { var g, f = arguments.length; for (g = 0; g < f; g++) { h = arguments[g]; d.call(this, h) } } }; c("add"); c("remove") } b.classList.toggle("c3", false); if (b.classList.contains("c3")) { var a = DOMTokenList.prototype.toggle; DOMTokenList.prototype.toggle = function (d, e) { if (1 in arguments && !this.contains(d) === !e) { return e } else { return a.call(this, d) } } } b = null }()) } };

    //matches
    function matches(elm, selector) {
        if (elm.matches) return elm.matches(selector);
        else {
            var matches = (elm.document || elm.ownerDocument).querySelectorAll(selector),
                i = matches.length;
            while (--i >= 0 && matches.item(i) !== elm);
            return i > -1;
        }
    }

    //Emitter
    !function () { function t(t) { return t ? e(t) : void 0 } function e(e) { for (var s in t.prototype) e[s] = t.prototype[s]; return e } this.Emitter = t, t.prototype.on = t.prototype.addEventListener = function (t, e) { return this._callbacks = this._callbacks || {}, (this._callbacks["$" + t] = this._callbacks["$" + t] || []).push(e), this }, t.prototype.once = function (t, e) { function s() { this.off(t, s), e.apply(this, arguments) } return s.fn = e, this.on(t, s), this }, t.prototype.off = t.prototype.removeListener = t.prototype.removeAllListeners = t.prototype.removeEventListener = function (t, e) { if (this._callbacks = this._callbacks || {}, 0 == arguments.length) return this._callbacks = {}, this; var s = this._callbacks["$" + t]; if (!s) return this; if (1 == arguments.length) return delete this._callbacks["$" + t], this; for (var r, i = 0; i < s.length; i++) if (r = s[i], r === e || r.fn === e) { s.splice(i, 1); break } return this }, t.prototype.emit = function (t) { this._callbacks = this._callbacks || {}; var e = [].slice.call(arguments, 1), s = this._callbacks["$" + t]; if (s) { s = s.slice(0); for (var r = 0, i = s.length; i > r; ++r) s[r].apply(this, e) } return this }, t.prototype.listeners = function (t) { return this._callbacks = this._callbacks || {}, this._callbacks["$" + t] || [] }, t.prototype.hasListeners = function (t) { return !!this.listeners(t).length } }();

    //Symbol
    !function (t) { function e(t) { return t = t || 7, Math.random().toString(35).substr(2, t) } function n() { if (!(this instanceof n)) return new n; var t = this.__key__ = "__symbol__" + e(32); Object.defineProperty(Object.prototype, t, { enumerable: !1, get: function () { }, set: o(t) }) } function o(t) { return function (e) { Object.defineProperty(this, t, { enumerable: !1, configurable: !0, writable: !0, value: e }) } } n.prototype.toString = function () { return this.__key__ }, n.prototype.dispose = function () { delete Object.prototype[this.__key__] }, "undefined" == typeof this.Symbol && (this.Symbol = n) }(this);

    // #endregion

    //
    // Globals
    var content = {};
    var contentByElement = new WeakMap();
    var rxFullMatch = /^\s*~([^#]+)#([^~]*)~\s*$/;
    var rxPartialMatch = /~([^#]+)#([^~]*)~/g;
    var domWatcher;
    var elementWatcher;
    var toolbutton;
    var quickbar;
    var panel;
    var editing = false;
    var saving = false;
    var html = document.body.parentNode;
    var contentEvents = new Emitter;
    var lastHoveredElement = null;
    var cargoUrlBase = document.currentScript.src.replace(/\js\/\d+$/, "");

    //
    // ContentItem Type
    var ContentItem = function ContentItem(key, content) {
        this[ContentItem._key] = key;
        this[ContentItem._content] = content;
        this[ContentItem._originalContent] = content;
        this[ContentItem._elements] = new Set();
        this[ContentItem._emitReadOnly] = function (a, b, c, d) {
            this[ContentItem._readonly] = true;
            try {
                this.emit(a, b, c, d);
                this[ContentItem._readonly] = false;
            } catch (e) {
                this[ContentItem._readonly] = false;
                throw e;
            }
        }

        Object.defineProperty(this, "modified", {
            get: function () { return this[ContentItem._content] != this[ContentItem._originalContent] },
            enumerable: false
        });

        Object.defineProperty(this, "content", {
            get: function () { return this[ContentItem._content] },
            set: function (v) {
                if (this[ContentItem._readonly]) {
                    throw "this content item may not be modified at this time.";
                } else {
                    if (this[ContentItem._content] != v) {
                        var old = this[ContentItem._content];
                        this[ContentItem._content] = v;
                        this[ContentItem._emitReadOnly]("contentChanged", v, old);
                    }

                    //make sure all the elements are in line
                    var modified = v != this[ContentItem._originalContent];
                    this[ContentItem._elements].forEach(function (e) {
                        if (e.innerHTML != v) e.innerHTML = v;
                        e.classList.toggle("cargo-modified", modified);
                    });
                }
            },
            enumerable: true
        });

        Object.defineProperty(this, "originalContent", {
            get: function () { return this[ContentItem._originalContent]; },
            enumerable: true
        });

        Object.defineProperty(this, "readonly", {
            get: function () { return this[ContentItem._readonly]; },
            enumerable: false
        });

        Object.defineProperty(this, "elements", {
            get: function () { return this[ContentItem._elements]; },
            enumerable: false
        });

        Object.defineProperty(this, "key", {
            get: function () { return this[ContentItem._key]; },
            enumerable: true
        });
    }
    ContentItem._key = Symbol();
    ContentItem._content = Symbol();
    ContentItem._originalContent = Symbol();
    ContentItem._elements = Symbol();
    ContentItem._emitReadOnly = Symbol();
    ContentItem._readonly = Symbol();

    ContentItem.prototype.processChange = function processChange(element) {
        var newContent = element.innerHTML;

        if (this[ContentItem._content] != newContent) {
            var otherItems = this[ContentItem._elements];
            var modified = newContent != this[ContentItem._originalContent];

            otherItems.forEach(function (otherItem) {
                if (otherItem !== element) {
                    otherItem.innerHTML = newContent;
                }
                otherItem.classList.toggle("cargo-modified", modified);
            });

            var oldContent = this[ContentItem._content];
            this[ContentItem._content] = newContent;
            this[ContentItem._emitReadOnly]("contentEdited", newContent, oldContent);
        }
    };

    ContentItem.prototype.reset = function reset() {
        if (this[ContentItem._content] != this[ContentItem._originalContent]) {
            this.content = this[ContentItem._originalContent];
        }
    }

    Emitter(ContentItem.prototype);

    //
    // Helper functions
    function isInDOM(element) {
        return document.contains(element);
    }

    function trimHtml(html) {
        return html.replace(/^(?:&nbsp;|&#8194;|&ensp;|&#8195;|&emsp;|&#8196;|&#8197;|&#8198;|&#8199;|&#8200;|&#8201;|&thinsp;|&#8202;|[\s\uFEFF\xA0])+|(?:&nbsp;|&#8194;|&ensp;|&#8195;|&emsp;|&#8196;|&#8197;|&#8198;|&#8199;|&#8200;|&#8201;|&thinsp;|&#8202;|[\s\uFEFF\xA0])+$/i, "").trim();
    }

    function arrayfy(thing) {
        if (typeof thing == "array") return thing;
        else {
            var arr = [];
            for (var i = 0; i < thing.length; i++) {
                arr.push(thing[i]);
            }

            return arr;
        }
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

    function setOffset(element, offset) {
        element.style.top = offset.top + "px";
        element.style.left = offset.left + "px";
    }

    function htmlDecode(encoded) {
        var e = document.createElement("div");
        e.innerHTML = encoded;
        return e.textContent;
    }

    function regexSplit(text, regex) {
        var matches = [];
        if (typeof regex === "string") regex = new RegExp(regex, "g");

        var lastEnd = 0;

        var match;
        while ((match = regex.exec(text)) !== null) {
            if (lastEnd != match.index) matches.push(text.substring(lastEnd, match.index));
            matches.push(match);
            lastEnd = match.index + match[0].length;
        }

        if (lastEnd && lastEnd != text.length) matches.push(text.substring(lastEnd));

        return matches;
    }

    function confirm(message) {
        return new Promise(function (resolve, reject) {
            if (window.confirm(message)) resolve();
        });
    }

    function http(request) {

        return new Promise(function (resolve, reject) {
            var req = new XMLHttpRequest();

            req.onreadystatechange = (function (req) {
                return function onreadystatechange() {
                    if (req.readyState == XMLHttpRequest.DONE) {
                        var success = /^2\d\d$/.test(req.status);

                        if (!success) {
                            reject(req);
                        } else {
                            resolve(req);
                        }
                    }
                }
            })(req);

            var contentType = request.contentType || "application/json";
            var method = request.method || "GET";
            var responseType = "responseType" in request ? request.responseType : "json";

            req.open(request.method, request.url);
            req.setRequestHeader("Content-Type", contentType);
            req.responseType = responseType;

            if (/^(?:GET|HEAD)$/.test(method) || !req.data) req.send();
            else req.send(JSON.stringify(request.data));
        });
    }

    function post(url, data) {
        return http({ url: url, data: data, method: "POST" });
    }

    //
    // Core functions
    function processNode(node) {

        //situations:
        //1. the current node has a single text node child
        //   1.1 the entirety of the current node's text is content
        //   1.2 a subset of the node's text is content
        //2. the node contains one or more child nodes
        //   2.1 none of them are text nodes
        //   2.2 one or more of the text nodes contains content
        //3. the node is empty

        function processNodeInternal(node) {
            function contentItemFor(key, node) {
                var contentItem;

                if (key in content) {
                    //the content item exists. Get the content item
                    //and replace the HTML for the node.
                    var contentItem = content[key];
                    node.innerHTML = contentItem.content;
                } else {
                    //the content item does not exist. Create the node using
                    //the content of the node as the content.
                    var contentItem = new ContentItem(key, node.innerHTML);
                    content[key] = contentItem;
                }

                //add the node to the content item and set the associative array of elements.
                contentItem.elements.add(node);
                contentByElement.set(node, contentItem);

                return contentItem;
            }

            function registerEvents(element, contentItem, events) {
                events.forEach(function (event) {
                    element.addEventListener(event, function (e) {
                        if (editing) {
                            contentEvents.emit(event, e, contentItem);
                        }
                    });
                });
            }

            function registerNode(node, match) {
                var key = match[1];
                var content = htmlDecode(match[2]);
                node.innerHTML = content;

                var contentItem = contentItemFor(key, node);

                node.classList.add("cargo-has-content");
                node.setAttribute("data-cargo-key", key);
                registerEvents(node, contentItem, ["mouseenter", "mouseleave", "click", "focus", "blur"]);
                elementWatcher.observe(node, { childList: true, attributes: false, subtree: true, characterData: true });
                contentEvents.emit("contentAdded", contentItem, node, true);
            }

            function reRegisterNode(node) {
                //this node needs to be re-processed for some reason
                var key = node.getAttribute("data-cargo-key");
                var contentItem = contentItemFor(key, node);

                registerEvents(node, contentItem, ["mouseenter", "mouseleave", "click", "focus", "blur"]);
                elementWatcher.observe(node, { childList: true, attributes: false, subtree: true, characterData: true });
                contentEvents.emit("contentAdded", contentItem, node, false);
            }

            function processTextNode(textNode) {
                var text = textNode.textContent;
                var node = textNode.parentNode;
                var matches = regexSplit(text, rxPartialMatch);

                if (!matches || matches.length == 0) return;

                if (matches.length == 1) {
                    if (node.firstChild.nextSibling) {
                        //node contains other children besides textNode - replace just the text node
                        var span = document.createElement("span");
                        registerNode(span, m);
                        node.replaceChild(span, textNode);
                    } else {
                        //node contains a full match - promote node to become a content node
                        registerNode(node, matches[0]);
                    }
                } else {
                    //multiple matches in textNode - convert and append one by one and then remove text node
                    matches.forEach(function (m) {
                        if (typeof m == "string") {
                            node.insertBefore(document.createTextNode(m), textNode);
                        } else {
                            var span = document.createElement("span");
                            registerNode(span, m);
                            node.insertBefore(span, textNode);
                        }
                    });

                    node.removeChild(textNode);
                }
            }


            //process the node depending on what it is
            switch (node.nodeType) {
                case Node.ELEMENT_NODE:
                    //don't process script or style nodes
                    if (!/head|link|meta|script|style/i.test(node.tagName)) {
                        if (node.classList.contains("cargo-has-content") && node.hasAttribute("data-cargo-key")) {

                            //this is for the case where HTML randomly gets parsed and attached
                            //or for some other case where a node is attached that has the right
                            //attributes but for some reason isn't registered.
                            reRegisterNode(node);
                        } else {
                            if (node.hasChildNodes()) {
                                //do each child in turn
                                var childNodes = [];
                                var fc = node.firstChild;
                                while (fc) { childNodes.push(fc); fc = fc.nextSibling; }

                                childNodes.forEach(processNodeInternal);
                            }
                        }
                    }
                    break;
                case Node.TEXT_NODE:
                    processTextNode(node);
                    break;
            }
        }

        //exit if node is blank
        if (!node) return;

        //exit if node is a child of a content node (or a content node itself)
        if (isContainedInAContentNode(node)) return;

        //process the node
        processNodeInternal(node);
    }

    function isContainedInAContentNode(node) {
        if (!node) return false;
        else if (contentByElement.has(node)) return contentByElement.get(node);
        else if (node === document) return false;
        else return isContainedInAContentNode(node.parentNode);
    }

    function getAllElementsForContentItemWithKey(key) {
        return arrayfy(document.querySelectorAll('[data-cargo-key="' + key + '"]'));
    }

    function getAllElementsForContentItem(contentItem) {
        return contentItem && contentItem.key && getAllElementsForContentItemWithKey(contentItem.key);
    }

    function processCurrentDOM() {
        processNode(document.body);
    }

    function watchDOM() {

        function processMutation(mutation) {
            switch (mutation.type) {
                case "childList":
                    for (var j = 0; mutation.addedNodes && j < mutation.addedNodes.length; j++) {
                        processNode(mutation.addedNodes[j]);
                    }
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

        domWatcher = new MutationObserver(domCallback);
        domWatcher.observe(document.body, { childList: true, attributes: false, subtree: true });
    }

    function watchElements() {

        function processElementMutation(mutation) {
            function findContentElement(target) {
                if (contentByElement.has(target)) return target;
                else if (target == document) return null;
                else if (target.parentNode) return findContentElement(target.parentNode);
                else return null;
            }

            var contentElement = findContentElement(mutation.target);

            if (contentElement && contentElement.contentEditable) {
                var contentItem = contentByElement.get(contentElement);
                if (contentItem != null) {
                    contentItem.processChange(contentElement);
                }
            }
        }

        function elementCallback(mutations) {
            if (mutations && mutations.length) {
                for (var i = 0; i < mutations.length; i++) {
                    processElementMutation(mutations[i]);
                }
            }
        }

        elementWatcher = new MutationObserver(elementCallback);
    }

    //
    //Editing UI stuff
    function startEditMode() {
        if (!editing) {
            html.classList.add("cargo-editing");
            editing = true;
        }
    }

    function exitEditMode() {
        if (editing) {
            html.classList.remove("cargo-editing");
            editing = false;
        }
    }

    function save() {

        var promise = new Promise(function (resolve, reject) {
            saving = true;
            console.log(JSON.stringify(content, null, '  '));
            setTimeout(resolve, 1000);
        });

        promise.then(function () { saving = false; }, function () { saving = false; });

        return promise;
    }

    function listenForClicks() {
        contentEvents.on("click", function (event, contentItem) {
            var target = event.target;
            target.contentEditable = true;
            target.focus();

            contentItem.elements.forEach(function (i) { i.classList.add("cargo-itemediting"); });
            event.preventDefault();
        });

        contentEvents.on("blur", function (event, contentItem) {
            if (event.target.contentEditable) {
                event.target.contentEditable = false;

                var content = contentItem.content;
                var trimmed = trimHtml(content);

                if (!trimmed) {
                    contentItem.content = "[empty]";
                } else if (trimmed != content) {
                    contentItem.content = trimmed;
                }

                contentItem.elements.forEach(function (i) { i.classList.remove("cargo-itemediting"); });
            }
        });
    }

    function listenForHovers() {
        contentEvents.on("mouseenter", function (event, contentItem) {
            if (lastHoveredElement != event.target) {
                lastHoveredElement = event.target;
                var offset = getOffset(lastHoveredElement);
                offset.top -= 24;
                setOffset(quickbar, offset);
            }

            quickbar.classList.add("cargo-visible");
        });

        contentEvents.on("mouseleave", function (event, contentItem) {
            quickbar.classList.remove("cargo-visible");
        });
    }

    function addButtons() {

        var toolbuttons = [{
            text: "list",
            hint: "Show a list of all content on the page",
            click: function (event) {
                panel.classList.toggle("cargo-visible");
            }
        }, {
            text: "settings_backup_restore",
            hint: "Reset all changes made since the last save",
            click: function (event) {
                confirm("Are you sure you want to revert ALL CHANGES?").then(function () {
                    for (var key in content) {
                        var contentItem = content[key];
                        contentItem.reset();
                    }
                });
            }
        }];

        var quickbuttons = [{
            text: "cached",
            hint: "Reset this content item to its original value",
            click: function (event, element, contentItem) {
                contentItem.reset();
            }
        }];


        if (!toolbutton) {

            //add the google material font
            var link = document.createElement("link");
            link.href = "https://fonts.googleapis.com/icon?family=Material+Icons";
            link.rel = "stylesheet";
            document.head.appendChild(link);

            //add the tool button
            toolbutton = document.createElement("div"); document.body.appendChild(toolbutton);
            toolbutton.id = "cargo_toolbutton";
            toolbutton.classList.add("material-icons");

            //the main button (toolbutton)
            var mainButton = document.createElement("a"); toolbutton.appendChild(mainButton);
            mainButton.addEventListener("click", function () {
                if (!editing && !saving) {
                    //start editing
                    startEditMode();
                    mainButton.textContent = "save";
                } else if (editing && !saving) {
                    //save
                    exitEditMode();
                    mainButton.textContent = "cached";
                    mainButton.classList.add("cargo-saving");
                    save().then(function () {
                        mainButton.classList.remove("cargo-saving");
                        mainButton.textContent = "mode_edit";
                    }, function (req) {
                        alert("There was a problem sending the request: " + req.statusText);
                        mainButton.classList.remove("cargo-saving");
                        mainButton.textContent = "save";
                        startEditMode();
                    });
                }
            });
            mainButton.textContent = "mode_edit";

            //create toolbar buttons (toolbutton)
            var toolbar = document.createElement("ul"); toolbutton.appendChild(toolbar);
            toolbuttons.forEach(function (b) {
                var li = document.createElement("li"); toolbar.appendChild(li);
                li.title = b.hint;
                var a = document.createElement("a"); li.appendChild(a);
                a.innerText = b.text;
                a.addEventListener("click", function (e) { b.click(e); })
            });


            //create the quickbar
            quickbar = document.createElement("ul"); document.body.appendChild(quickbar);
            quickbar.id = "cargo_quickbar";
            quickbar.classList.add("material-icons");

            //create the quickbuttons (quickbar)
            quickbuttons.forEach(function (b) {
                var li = document.createElement("li"); quickbar.appendChild(li);
                li.title = b.hint;
                var a = document.createElement("a"); li.appendChild(a);
                a.innerText = b.text;
                a.addEventListener("click", function (e) {
                    if (lastHoveredElement) {
                        var contentItem = contentByElement.get(lastHoveredElement);
                        if (contentItem) {
                            b.click(e, lastHoveredElement, contentItem);
                        }
                    }
                });
            });


            //create the panel
            panel = document.createElement("div"); document.body.appendChild(panel);
            panel.id = "cargo_panel";

            //create the list (panel)
            var contentList = document.createElement("ul"); panel.appendChild(contentList);
            function addContentItemToPanel(contentItem) {
                var alreadyThere;
                contentItem.elements.forEach(function (e) { if (contentList.contains(e)) alreadyThere = true; });
                if (!alreadyThere && contentList.querySelectorAll("[data-cargo-key=" + contentItem.key + "]").length) alreadyThere = true;

                if (!alreadyThere) {
                    //taking advantage of the fact that the DOM watched will make this thing editable directly
                    var li = document.createElement("li");
                    var span = document.createElement("span"); li.appendChild(span);
                    span.innerHTML = contentItem.content;
                    span.setAttribute("data-cargo-key", contentItem.key);
                    span.classList.add("cargo-has-content");
                    contentList.appendChild(li);
                }
            }
            //add all current content to the list (panel)
            for (var key in content) {
                var contentItem = content[key];

                addContentItemToPanel(contentItem);
            }
            //if more come add them too
            contentEvents.on("contentAdded", addContentItemToPanel);
        }
    }

    //kick off processing of the current DOM and any changes to come
    watchElements();
    processCurrentDOM();
    watchDOM();
    listenForClicks();
    listenForHovers();

    if (document.body != null) addButtons();
    else document.addEventListener("load", addButtons);

    //uncloak
    document.getElementsByTagName("html")[0].classList.remove("cargo-cloak");

})(this);
