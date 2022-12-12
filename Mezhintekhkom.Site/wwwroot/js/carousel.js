$('.owl-carousel').owlCarousel({
    stagePadding: 50,
    items: 5,
    loop: true,
    mouseDrag: false,
    autoWidth: true,
    autoplay: true,
    autoplayTimeout: 3000,
    autoplayHoverPause: true,
    margin: 10,
    responsive: {
        0: {
            items: 1
        },
        600: {
            items: 3
        },
        1000: {
            items: 5
        }
    }
})