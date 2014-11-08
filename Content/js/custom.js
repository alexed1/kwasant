$(document).ready(function () {          
    var containerHeight, loginFormTop 
    getLogiTop();
    if ($('#loginform, .registration-section, .login-section.status-message').length > 0) {
	    $('#main-container').addClass('login-page');
		$(window).resize(function () {
		    getLogiTop();
		});   
	} else {    
	    $('#main-container').removeClass('login-page');
    }

    if ($('#emailInfoBox').length > 0) {
        $("body").removeClass('view-popup-window');
    }
	
});

function getLogiTop() {
    containerHeight = $(window).height() - $('.site-footer').height() - $('.site-header-wrap').height() - 30;
    loginFormTop = (containerHeight - $('.login-section').height()) / 2;
    if (loginFormTop > 0) {
        $('.login-section').css('margin-top', loginFormTop)
    }
}
