(function () {
    var head = document.getElementsByTagName('head')[0];

    function loadScript(url) {
        var script = document.createElement('script');
        script.type = 'text/javascript';
        script.src = url;
        head.appendChild(script);
    }
    function loadCss(url) {
        var link = document.createElement('link');
        link.rel = "stylesheet";
        link.href = url;
        head.appendChild(link);
    }

    var cs = document.currentScript;
    loadScript(cs.src.replace(/js$/, "cargo.js"));
    loadCss(cs.src.replace(/js$/, "cargo.css"));

    //debugger;
    document.getElementsByTagName("html")[0].className += " cargo-cloak";
})();