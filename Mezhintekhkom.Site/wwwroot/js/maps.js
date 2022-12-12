var myMap;

// Дождёмся загрузки API и готовности DOM.
ymaps.ready(init);

function init () {
    // Создание экземпляра карты и его привязка к контейнеру с
    // заданным id ("map").
    myMap = new ymaps.Map('map', {
        // При инициализации карты обязательно нужно указать
        // её центр и коэффициент масштабирования.
        center:[55.648214, 37.540856],
        zoom:16
    });

    var myPlacemark = new ymaps.Placemark([55.648214, 37.540856]);
    myMap.geoObjects.add(myPlacemark);

    document.getElementById('destroyButton').onclick = function () {
        // Для уничтожения используется метод destroy.
        myMap.destroy();
    };

}