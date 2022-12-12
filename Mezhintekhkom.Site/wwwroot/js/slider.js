$(
    function () {
        $("#slider-range").slider({
            range: true,
            min: 1,
            max: 24,
            values: [ 1, 24 ],
            slide: function( event, ui ) {
            $( "#amount" ).val("от " + ui.values[ 0 ] + " до " + ui.values[ 1 ]  + " месяцев");
            }
        });
    }
);