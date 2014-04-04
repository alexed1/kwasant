function EasyPeasyParallax() {
	var	windowWidth = $(window).width();
	if(windowWidth > 980){
		scrollPos = $(this).scrollTop();
		$('#welcome').css({
			'background-position' : '50%' + (-scrollPos/4)+"px"
		});
		$('#logotext').css({
			'margin-top': (scrollPos/4)+"px",
			'opacity': 1-(scrollPos/250)
		});
		var opacityValue = $('#logotext').css('opacity');
		if(opacityValue == 0){
			$('#logotext').hide();
		}else{
			$('#logotext').show();
		}
	}	
}
function someResize(){
	EasyPeasyParallax();
	var	windowWidth = $(window).width(),
		maxHeight = $('.near-big').height() - 42
	if(windowWidth > 768){
		$('.big-preview .thumbnail').css('height', maxHeight);
	}
	else{
		$('.big-preview .thumbnail').css('height', 'auto');
	}	
	if($('.colorbox').length){
		$('a.colorbox').colorbox({
			rel:'gal',
			retinaImage: true,
			opacity: 1,
			current: false,
			maxWidth: '95%',
			maxHeight: '95%'
		})
	} 
}
$(document).ready(function(){

	if($('input, textarea').length) {
		$('input, textarea').placeholder();
	}
	
	someResize();
	
	$('a[rel="popover"]').popover();
	$('a[rel="tooltip"]').tooltip();
	$('.carousel').carousel({
		interval: false
	})
	
	/*menu*/
	
	var links = $('.navbar').find('li[data-section]'),
		section = $('.text-block'),
		button = $('.toSection'),
		mywindow = $(window),
		htmlbody = $('html,body'),
		offsetTop = $('.navbar').height();
	section.waypoint({
		 handler: function (direction) {
			var datasection = $(this).attr('data-section');
			if (direction === 'down') {
				$('.navbar li[data-section="' + datasection + '"]').addClass('active').siblings().removeClass('active');
			}
			else {
				var newsection = parseInt(datasection) - 1;
				$('.navbar li[data-section="' + newsection + '"]').addClass('active').siblings().removeClass('active');
			}
		}, 
		offset: offsetTop + 5
	});
	mywindow.scroll(function () {
		if (mywindow.scrollTop() < $('#welcome').height()- offsetTop - 5) {
			$('.navbar li[data-section="1"]').removeClass('active');
		}
	});
	function goToByScroll(datasection) {
		htmlbody.animate({
			scrollTop: $('.text-block[data-section="' + datasection + '"]').offset().top - offsetTop
		}, 1000);
	}
	var	windowWidth = $(window).width();
	if(windowWidth < 980){
		function goToByScroll(datasection) {
			htmlbody.animate({
				scrollTop: $('.text-block[data-section="' + datasection + '"]').offset().top}, 1000);
		}
	}
	links.click(function (e) {
		e.preventDefault();
		var datasection = $(this).attr('data-section');
		goToByScroll(datasection);
	});
	button.click(function (e) {
		e.preventDefault();
		var datasection = $(this).attr('data-section');
		goToByScroll(datasection);
	}); 

	
	$(window).scroll(function() {
		EasyPeasyParallax();
		if ($(this).scrollTop() > 300) {
			$('.goTop-link').fadeIn();
		} else {
			$('.goTop-link').fadeOut();
		}
	});
	
	$('.goTop').on('click', function(){
		$('body,html').animate({scrollTop: 0}, 1000);
		return false;
	}); 
	
	
});

$(window).resize(function(){
	someResize();
})