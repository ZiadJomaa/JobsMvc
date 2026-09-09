(function ($) {
    "use strict";

    $(document).ready(function () {
        // Spinner
        if ($('#spinner').length > 0) {
            $('#spinner').removeClass('show');
        }

        // Initiate WOW.js safely
        try {
            if (typeof WOW !== 'undefined') {
                new WOW().init();
            }
        } catch (e) {
            console.warn("WOW.js error:", e);
        }

        // Sticky Navbar
        $(window).scroll(function () {
            if ($(this).scrollTop() > 300) {
                $('.sticky-top').css('top', '0px');
            } else {
                $('.sticky-top').css('top', '-100px');
            }
        });

        // Back to top button
        $(window).scroll(function () {
            if ($(this).scrollTop() > 300) {
                $('.back-to-top').fadeIn('slow');
            } else {
                $('.back-to-top').fadeOut('slow');
            }
        });
        $('.back-to-top').click(function () {
            $('html, body').animate({scrollTop: 0}, 1500);
            return false;
        });

        // Header carousel
        if ($(".header-carousel").length > 0 && typeof $.fn.owlCarousel !== 'undefined') {
            $(".header-carousel").owlCarousel({
                autoplay: true,
                smartSpeed: 1500,
                items: 1,
                dots: true,
                loop: true,
                nav: true,
                navText: [
                    '<i class="bi bi-chevron-left"></i>',
                    '<i class="bi bi-chevron-right"></i>'
                ]
            });
        } else {
            console.warn("Owl Carousel not loaded or .header-carousel not found!");
        }

        // Testimonials carousel
        if ($(".testimonial-carousel").length > 0 && typeof $.fn.owlCarousel !== 'undefined') {
            $(".testimonial-carousel").owlCarousel({
                autoplay: true,
                smartSpeed: 1000,
                center: true,
                margin: 24,
                dots: true,
                loop: true,
                nav: false,
                responsive: {
                    0: { items: 1 },
                    768: { items: 2 },
                    992: { items: 3 }
                }
            });
        }
    });

})(jQuery);