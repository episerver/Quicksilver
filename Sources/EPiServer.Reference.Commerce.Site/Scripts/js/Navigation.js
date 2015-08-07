var Navigation = {
    init: function () {
        $('.navbar').on('show.bs.collapse', function () {
            var actives = $(this).find('.collapse.in'),
                hasData;

            if (actives && actives.length) {
                hasData = actives.data('collapse')
                if (hasData && hasData.transitioning) return
                actives.collapse('hide')
                hasData || actives.data('collapse', null)
            }
        });
    }
}

