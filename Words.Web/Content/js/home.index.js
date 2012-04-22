(function (w, $) {
    w.template = _.template($("#home-results-header").text());
    w.tableTemplate = _.template($("#home-results-table").text());
    w.searchSuccess = function (data) {
        var words = data.Words;
        var anagrams = data.Anagrams;
        var near = data.Near;
        var count = data.Count;
        var query = data.Query;
        var nodes = data.Nodes;
        var elapsedMilliseconds = data.ElapsedMilliseconds;
        if (typeof window._gaq !== "undefined" && window._gaq !== null) {
            window._gaq.push(['_trackEvent', 'system', 'search', query, nodes]);
        }

        var html = w.template({ elapsedMilliseconds: elapsedMilliseconds, count: count });
        $("#content").html(html);

        if (words.length > 0) {
            var wordsHtml = w.tableTemplate({ header: "Ord", items: words });
            $("#home-results-words").html(wordsHtml);
        }

        if (anagrams.length > 0) {
            var anagramsHtml = w.tableTemplate({ header: "Anagram", items: anagrams });
            $("#home-results-anagrams").html(anagramsHtml);
        }

        if (near.length > 0) {
            var nearHtml = w.tableTemplate({ header: "Nära", items: near });
            $("#home-results-near").html(nearHtml);
        }
    };
} (window.w = window.w || {}, jQuery));
