(function (w, $) {
    w.template = _.template($("#nian-results").text());
    w.searchSuccess = function (data) {
        var anagrams = data.Anagrams;
        var query = data.Query;
        var nodes = data.Nodes;
        if (typeof window._gaq !== "undefined" && window._gaq !== null) {
            window._gaq.push(['_trackEvent', 'system', 'search', query, nodes]);
        }

        var html = w.template({ anagrams: anagrams });
        $("#content").html(html);
    };
    w.searchFailure = function (data) {
        $("#content").html("<span class='label label-important'>Ange 9 tecken!</span>");
    };
} (window.w = window.w || {}, jQuery));
