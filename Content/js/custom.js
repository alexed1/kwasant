$(document).ready(function () {

	var bgHeight = $(window).height() - $('.body-content').height() - $('.site-footer').height() - $('.site-header-wrap').height() - 21;
	if ($('#loginform').length > 0) {
		$('.site-header').addClass('login-bg');
		$(window).resize(function () {
			bgHeight = $(window).height() - $('.body-content').height() - $('.site-footer').height() - $('.site-header-wrap').height() - 21;
			if (bgHeight <= 100) {
				bgHeight = 100;
			}
			showLogo();
			$('.site-header').css('padding-bottom', bgHeight);
		});
		$('.site-header').css('padding-bottom', bgHeight);
	} else {
		$('.site-header').removeClass('login-bg');
	}
	showLogo();
	
});

function showLogo() {
	if ($(window).height() <= 768) {
		$('.login-bg .logotext').fadeOut();
	} else {
		$('.login-bg .logotext').fadeIn();
	}
}