GleamTech.Util.Language.name = "{${lang}$}";
GleamTech.Util.Language.entries = "{${entries}$}";
(function (n) {
    var t;
    t = typeof require != "undefined" && typeof exports != "undefined" && typeof module != "undefined" ? require("globalize") : n.Globalize;
    t.addCultureInfo("en-US", "default", {
        name: "en-US"
        , englishName: "English (United States)"
    })
})(this);
GleamTech.Util.Culture.cultureInfo = Globalize.cultures["{${culture}$}"];
