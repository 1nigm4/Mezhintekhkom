$(
    function () {
        $("#hour-range").slider({
            range: true,
            min: 16,
            max: 1440,
            values: [16, 1440],
            slide: function (event, ui) {
                $("#hours").val("от " + ui.values[0] + " до " + ui.values[1] + " часов");
            }
        });

        $("#price-range").slider({
            range: true,
            min: 0,
            max: 99999,
            values: [0, 99999],
            slide: function (event, ui) {
                $("#price").val("от " + ui.values[0] + " до " + ui.values[1] + " рублей");
            }
        });
    }
);