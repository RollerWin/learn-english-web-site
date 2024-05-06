$(document).ready(function() {
    $(window).scroll(function() {
        if ($(this).scrollTop() > 50) {
            $('header').addClass('scrolled').removeClass('navbar-dark').addClass('navbar-light');
            $('button').addClass('scrolled');
        } else {
            $('header').removeClass('scrolled').addClass('navbar-dark').removeClass('navbar-light');
            $('button').removeClass('scrolled');
        }
    });
});