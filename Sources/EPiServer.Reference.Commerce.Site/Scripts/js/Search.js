var Search = {
    init: function() {
        $(document)
            .on('keyup', '.jsQuickSearch', Search.quickSearch)
            .on('focus', '.jsQuickSearch, .jsQuickSearchResult', Search.showResults)
            .on('focusin.jsQuickSearchResult click.jsQuickSearchResult', function (e) {
                if ($(e.target).closest('.jsQuickSearchResult, .jsQuickSearch').length) return;
                $('.jsQuickSearchResult').fadeOut('medium');
            });
        $(window).bind('popstate ', function (event) {
            //this will handle browser back/forward navigation
            Search.updatePage(location.search);
        });
        $('.js-search-input').on('focusout', function () { $('.search-input').removeClass("is-active"); });
        $('.js-search-input').on('focusin', function () { $('.search-input').addClass("is-active"); });
        if ($('.jsSearch').length == 1) {
            Search.infinityScroll();
            $(document)
                .on('change', '.jsSearchSort', Search.sort)
                .on('change', '.jsSearchFacet', Search.sort)
                .on('click', '.jsSearchFacetRemoveAll', Search.removeAll);
        }

        // Set input width
        $('.js-search-input').focus(function () {
            $(this).addClass('has-focus');
        });
        $('.js-search-input').blur(function () {
            $(this).removeClass('has-focus');
        });
    },
    fetchingNewPage: false,
    lastPage: false,
    lastKeyWord: "",
    quickSearch: function() {
        if ($(this).val().length > 1 && Search.lastKeyWord != $(this).val()) {
            var url = $(this).data('url');
            var form = $(this).closest('form');
            var jqXhr = $(this).data('jqXhr');
            if (jqXhr)
                jqXhr.abort();
            Search.lastKeyWord = $(this).val();
            $(this).data('jqXhr', $.ajax({
                    type: "POST",
                    url: url,
                    data: form.serialize(),
                    context: this,
                    success: function(result) {
                        $(this).removeData('jqXhr');
                        $('.jsQuickSearchResult').empty();
                        $('.jsQuickSearchResult').append(result);
                    }
                })
            );
        }
    },
    showResults: function() {
        $('.jsQuickSearchResult').show();
    },
    infinityScroll: function () {
        $(window).scroll(function () {
            if (Search.fetchingNewPage == true || Search.lastPage) {
                return null;
            }
            if ($(window).scrollTop() >= ($(document).height() - $(window).height()) - 1000) {
                Search.fetchingNewPage = true;
                var form = $(document).find('.jsSearchForm');
                $.ajax({
                    url: Search.getUrlWithFacets(),
                    type: "POST",
                    data: form.serialize(),
                    success: function (result) {
                        Search.fetchingNewPage = false;
                        if ($(result).find('.product').length > 0) {
                            $('.jsSearchPage').replaceWith($(result).find('.jsSearchPage'));
                            $('.jsSearch').append($(result).find('.jsSearch').children());
                            $('.jsSearchFacets').replaceWith($(result).find('.jsSearchFacets'));
                        } else {
                            Search.lastPage = true;
                        }
                    }
                });
            }
        });
    },
    sort: function () {
        Search.lastPage = false;
        var form = $(document).find('.jsSearchForm');
        $('.jsSearchPage').val(1);
        $('.jsSelectedFacet').val($(this).data('facetgroup') + ':' + $(this).data('facetkey'));
        var url = Search.getUrlWithFacets();
        Search.updatePage(url, form.serialize(), function () {
            history.pushState({ url: url }, "", url); //put the new url to browser history
        })
    },
    getUrlWithFacets: function () {
        var facets = [];
        $('.jsSearchFacet:input:checked').each(function () {
            var selectedFacet = encodeURIComponent($(this).data('facetkey'));
            facets.push(selectedFacet);
        });
        return Search.getUrl(facets);
    },
    getUrl: function (facets) {
        var urlParams = Search.getUrlParams();
        urlParams.facets = facets ? facets.join(',') : null;
        var sort = $('.jsSearchSort')[0].value;
        urlParams.sort = sort;
        var url = "?";
        for (var key in urlParams) {
            var value = urlParams[key];
            if (value) {
                url += key + '=' + value + '&';
            }
        }
        return url.slice(0, -1); //remove last char
    },
    getUrlParams: function () {
        var match,
            search = /([^&=]+)=?([^&]*)/g, //regex to find key value pairs in querystring
            query = window.location.search.substring(1);

        var urlParams = {};
        while (match = search.exec(query)) {
            urlParams[match[1]] = match[2];
        }
        return urlParams;
    },
    removeAll: function () {
        Search.lastPage = false;
        $('.jsSearchFacet:input:checked').each(function () { $(this).attr('checked', false); });
        var form = $(document).find('.jsSearchForm');
        $('.jsSearchPage').val(1);
        var url = Search.getUrl();
        Search.updatePage(url, form.serialize(), function () {
            history.pushState({ url: url }, "", url); //put the new url to browser history
        })
    },
    updatePage: function (url, data, onSuccess) {
        $.ajax({
            type: "POST",
            url: url || "",
            data: data,
            success: function (result) {
                $('.jsSearchPage').replaceWith($(result).find('.jsSearchPage'));
                $('.jsSearch').replaceWith($(result).find('.jsSearch'));
                $('.jsSearchFacets').replaceWith($(result).find('.jsSearchFacets'));
                if (onSuccess) {
                    onSuccess(result);
                }
            }
        });
    }
}