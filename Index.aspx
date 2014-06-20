<!DOCTYPE html>
<!--[if lt IE 7]>      <html class="no-js lt-ie9 lt-ie8 lt-ie7"> <![endif]-->
<!--[if IE 7]>         <html class="no-js lt-ie9 lt-ie8"> <![endif]-->
<!--[if IE 8]>         <html class="no-js lt-ie9"> <![endif]-->
<!--[if gt IE 8]><!-->
<%@ Page Language="C#"%>
<%@ Import Namespace="System.Web.Optimization" %>
<html class="no-js">
 <!--<![endif]-->
    
   <!--Google Analytics code block. should probably be moved to a common js file -->
  <script>
  (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
  (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
  m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
  })(window,document,'script','//www.google-analytics.com/analytics.js','ga');

  ga('create', 'UA-52048536-1', 'kwasant.com');
  ga('send', 'pageview');

</script>
    
    
    
    
    

<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <title>Kwasant</title>
    <meta name="description" content="">
    <meta name="viewport" content="width=device-width, initial-scale=1"> 
	<link rel='shortcut icon' type='image/x-icon' href='Content/img/favicon.ico' />
    <% Response.Write(Styles.Render("~/bundles/css/bootstrap23").ToHtmlString()); %>
	<% Response.Write(Styles.Render("~/bundles/css/bootstrap-responsive").ToHtmlString()); %>
    <% Response.Write(Styles.Render("~/bundles/css/colorbox").ToHtmlString()); %>
    <% Response.Write(Styles.Render("~/bundles/css/frontpage").ToHtmlString()); %>
    <link href='http://fonts.googleapis.com/css?family=Open+Sans:400,800,700,300,600,400italic&subset=latin,cyrillic' rel='stylesheet' type='text/css'>
	<link href="Content/css/font-awesome.css" rel="stylesheet">
    <!--[if lt IE 9]>
        <script src="http://html5shim.googlecode.com/svn/trunk/html5.js"></script>
        <script src="Content/js/respond.min.js"></script>
    <![endif]-->
</head>
<body>
    <!--[if lt IE 7]>
        <p class="browsehappy">You are using an <strong>outdated</strong> browser. Please <a href="http://browsehappy.com/">upgrade your browser</a> to improve your experience.</p>
    <![endif]-->
    <div class="navbar navbar-fixed-top">
        <div class="navbar-inner">
            <div class="container">
                <a class="btn btn-navbar" data-toggle="collapse" data-target=".nav-collapse">
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                </a>
                <a class="brand goTop" href="index.aspx"><img class="floating" src="Content/img/perfect-krawsant.png" alt="Kwasant" title="Kwasant">Kwasant</a>
                <!-- <img src="img/logo.png"></a> -->
                <br />                
                <!-- <a class="brand goTop"  style="background-image: url(Content/img/logo.png);"></a> -->
                <div class="nav-collapse">
                    <ul class="nav pull-right">
                        <li data-section="2"><a href="#how_to_works" title="How it Works">How it Works</a></li>
                        <li data-section="3"><a href="#faq" title="FAQ">FAQ</a></li>
                        <!-- 	<li data-section="3"><a href="#">About Us</a></li> -->
                        <li><a href="/Account/Index" title="SignUp/Login">SignUp/Login</a></li>
                        <!-- 	<li data-section="3"><a href="#">About Us</a></li> -->
                        <li data-section="4"><a href="#contacts" title="Contact">Contact</a></li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <div id="welcome" class="parallax" style="background-image: url(Content/img/background-test9.jpg);"  data-section="1">		
        <div id="logotext" class="text">
			<h1>Kwasant</h1>
			<!-- <div class="love text-center" style="background-image: url(Content/img/logo.png);"></div> -->
			<p>We schedule your meetings for you</p>
        </div>
    </div>

    <section id="about" class="row text-block" data-section="1">
        <div class="features dark-bg">
            <div class="container">
                <div class="row-fluid">
                    <h2 class="text-center"><b>Schedule a Meeting in One Step</b></h2>
                    <%--<article class="span4"> <i class="icon-resize-full"></i>--%>  
                    <img class="pull-center" src="Content/img/from_this_to_this.png" alt="Schedule a Meeting in One Step" />
                </div>
                
            </div>
        </div>
    </section>
    <section id="how_to_works" class="row text-block" data-section="2">
        <div class="light-bg">
            <div class="container how-it-works-section">
				<h2><i class="fa fa-cogs"></i> How it Works </h2>
                <div class="section clearfix">
                    <strong>1. Send one of your emails to Kwa@sant.com</strong>
                    <p></p>
					<strong>2. One of our Bookers will read the thread and build a meeting invitation</strong>  
					<p></p>
					<strong>3. The meeting invitation gets sent back to you and your attendees</strong>
					<p></p>
                    <strong>4. Add it to your calendar with a single click</strong>
                    <p></p>
                    <p></p>
                    <p></p>
                    <img class="pull-center" src="Content/img/example_using_kwasant_email.png" alt="Example using kwasant email" />
                </div>
               
			</div>
			<div>
                <!-- this is a section that breaks up the website -->
                <div id="services-bg" class="parallax" style="background-image: url(Content/img/homepage89.png);">
                    <br />
                    <br />
                    <div class="#fff-bg">
                        <div class="text">
                            <blockquote>
                                <h2>Kwasant</h2>
                                <small> It just takes one step..</small>
                            </blockquote>
                        </div>
						<a href="#" data-section="2" class="toSection"><i class="fa fa-angle-double-down"></i></a>
                    </div>
                </div>
				<div class="container">
					<section id="details" class="details-section">
						<div>
							<h2>Details</h2>
							<p></p>
							<p>We take the emails you provide to us and put them in front of of our human Booking Agents, who do what a personal assistant would do: they read the message, extract the key elements about the meeting, and enter them into a calendar.<p>
							This generates a Meeting Invitation that we email back to you using the open "ICS" standard. If you're using common calendaring software like Google Calendar, Apple iCal, and Microsoft Outlook, these emails show up in your inbox and allow you to add them to your calendar with one click.
							Think of us as your personal assistant. If we can read your email and figure out the meeting time, we'll do it. You don’t have to give the information to us in any special format.
							If the information you send us is too ambiguous, we'll send you back a request for clarification. We delete all email we receive 7 days after the event is scheduled.</p>
						</div>
						<div class="row-fluid section">
							<div class="span12 text-center">
								<!-- <h3 class="with-border">Watch who we are</h3> -->                        
								<img src="Content/img/How_it_works_diagram.png" alt="How it works" />
								<!-- <div class="videoWrapper">
								<iframe src="http://player.vimeo.com/video/20596477?color=39ae77" frameborder="0" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>
								</div> -->
							</div>
						</div>
					</section>
				</div>
                <!-- this is a section that breaks up the website -->
				<div id="services-bg-sec" class="parallax" style="background-image: url(Content/img/homepagesecond.jpg);">
                    <br />
                    <br />
                    <div class="#fff-bg">
                        <div class="text">
                            <blockquote>
                                <h2>Kwasant - The Next Thing You Can't Live Without</h2>
                                <small> We use your email to build your calendar.</small>
                            </blockquote>
                        </div>
						<a href="#" data-section="2" class="toSection"><i class="fa fa-angle-double-down"></i></a>
                    </div>
                </div>
				<div  class="text-block" data-section="3">
				<div class="container faq-section">
                <!-- this is the faq section -->
					<section id="faq">
						<h1 class="text-center">FAQ</h1>
						<div class="row span-12 text-center">
							<div class="panel-group" id="accordion">
								<div class="panel panel-default">
									<div class="panel-heading">
										<h4 class="panel-title">
											<a data-toggle="collapse" data-parent="#accordion" href="#collapseOne">
												Just what problem are you solving here?
											</a>
										</h4>
									</div>
									<div id="collapseOne" class="panel-collapse collapse in">
										<div class="panel-body">
											The internet offers fantastic services these days, but it’s still an annoying task to convert conversations into useful meeting requests. Some people have personal assistants to help with this, but most of us don’t. Now, you can get personal assistant help when you need it.
										</div>
									</div>
								</div>
								<div class="panel panel-default">
									<div class="panel-heading">
										<h4 class="panel-title">
											<a data-toggle="collapse" data-parent="#accordion" href="#collapseTwo">
												How do you make money?
											</a>
										</h4>
									</div>
									<div id="collapseTwo" class="panel-collapse collapse">
										<div class="panel-body">
											Someday, we’re probably going to charge a small monthly fee for booking services. But everyone using BookIt now will get at least one free year of service.
										</div>
									</div>
								</div>
								<div class="panel panel-default">
									<div class="panel-heading">
										<h4 class="panel-title">
											<a data-toggle="collapse" data-parent="#accordion" href="#collapseThree">
												What email addresses do you use?
											</a>
										</h4>
									</div>
									<div id="collapseThree" class="panel-collapse collapse">
										<div class="panel-body">
											We send your meeting request right back to the address you sent your information from. However, you’ll soon be able to specify which address to use.
										</div>
									</div>
								</div>
								<div class="panel panel-default">
									<div class="panel-heading">
										<h4 class="panel-title">
											<a data-toggle="collapse" data-parent="#accordion" href="#collapseFour">
												What else do you do?
											</a>
										</h4>
									</div>
									<div id="collapseFour" class="panel-collapse collapse">
										<div class="panel-body">
											We're working on adding a bunch of additional ways you can save time by tapping into our web assistants.
										</div>
									</div>
								</div>
								<div class="panel panel-default">
									<div class="panel-heading">
										<h4 class="panel-title">
											<a data-toggle="collapse" data-parent="#accordion" href="#collapseFive">
												Can you also send a meeting invite to the other attendees?
											</a>
										</h4>
									</div>
									<div id="collapseFive" class="panel-collapse collapse">
										<div class="panel-body">
											Yes, they'll get one too.
										</div>
									</div>
								</div>
								<div class="panel panel-default">
									<div class="panel-heading">
										<h4 class="panel-title">
											<a data-toggle="collapse" data-parent="#accordion" href="#collapseSix">
												What if I don't give you enough detail about the meeting? Do you just guess?
											</a>
										</h4>
									</div>
									<div id="collapseSix" class="panel-collapse collapse">
										<div class="panel-body">
											In general, we'll book the information we're sure about, and if necessary, email you for clarification.
										</div>
									</div>
								</div>
								<div class="panel panel-default">
									<div class="panel-heading">
										<h4 class="panel-title">
											<a data-toggle="collapse" data-parent="#accordion" href="#collapseSeven">
												Do you have a mascot?
											</a>
										</h4>
									</div>
									<div id="collapseSeven" class="panel-collapse collapse">
										<div class="panel-body">
											Yes. Feast your eyes on Kwasant the Kat
										</div>
									</div>
								</div>
							</div>
						</div>
					</section>
				</div>
			</div>			
        </div>
    </section>  
            

            <section id="contacts" class="text-block" data-section="4">
                <div class="dark-bg">
                    <div class="container">
                        <h2><i class="fa fa-map-marker"></i> Get in touch</h2>
                        <p>Drop us a line.</p>
                        <div class="row-fluid section">
                            <div class="span8">
                                <h3 class="with-border">Want to ask something?</h3>
                                <form class="form clearfix" action="#">
                                    <input type="text" class="span12" placeholder="Name" name="name" id="name">
                                    <input type="email" class="span12" placeholder="Email" name="emailAddress" id="emailId">
                                   <%-- <input type="text" class="span12" placeholder="Subject" name="subject" id="subject">--%>
                                    <textarea class="span12" placeholder="Message" name="message" rows="6" id="message"></textarea>
                                     <button class="btn btn-large pull-right" type="button" onclick="SendMail();">Submit</button>
                                
                                     <span id="spMessage" style="color:#D85E17; font-weight: bold;" ></span>
                                </form>
                            </div>

                        </div>
                    </div>
                </div>
            </section>
            <!-- <iframe width="100%" height="400" frameborder="0" scrolling="no" marginheight="0" marginwidth="0" src="https://maps.google.ru/?ie=UTF8&amp;ll=37.775735,-122.422543&amp;spn=0.070962,0.153122&amp;t=m&amp;z=13&amp;output=embed"></iframe> -->
            <footer>
                <div class="light-bg">
                    <div class="container">
                        <div class="row-fluid">
                            <div class="span4">
                                <h4>Get in touch</h4>
                                <div class="info-block">
                                    <article class="clearfix">
                                        <i class="fa fa-globe"></i>
                                        <address>
                                            3741 Buchanan Street<br />
                                            San Francisco, CA 94123
                                        </address>
                                    </article>
                                    <article class="clearfix">
                                        <i class="fa fa-phone"></i>
                                        <p>8 417 274 2933</p>
                                    </article>
                                    <article class="clearfix">
                                        <i class="fa fa-envelope"></i>
                                        <p><a href="mailto:info@kwasant.com">info@kwasant.com</a></p>
                                    </article>
                                </div>
                            </div>
                            <div class="span4">
                                <h4>Follow us</h4>
                                <div class="social">
                                    <ul class="unstyled clearfix">
                                        <li>
                                            <a target="_blank" title="Twitter" href="#">
                                                <i class="fa fa-twitter"></i>
                                            </a>
                                        </li>
                                        <li>
                                            <a target="_blank" title="Facebook" href="#">
                                                <i class="fa fa-facebook"></i>
                                            </a>
                                        </li>

                                        <li>
                                            <a target="_blank" title="Linkedin" href="#">
                                                <i class="fa fa-linkedin"></i>
                                            </a>
                                        </li>

                                        <li>
                                            <a target="_blank" title="Google+" href="#">
                                                <i class="fa fa-google-plus"></i>
                                            </a>
                                        </li>


                                    </ul>
                                </div>
                            </div>
                            <div class="span4">
                                <h4>Recent posts</h4>
                                <!-- <ul>
                                        <li><a href="#">Lorem ipsum dolor sit amet</a></li>
                                        <li><a href="#">Cum maiestatis necessitatibus ad</a></li>
                                        <li><a href="#">Dicat tantas copiosae eam id</a></li>
                                        <li><a href="#">Theophrastus, dicat tantas</a></li>
                                        <li><a href="#">Cras metus elit, consectetur sed</a></li>
                                        <li><a href="#">Lorem ipsum dolor sit amet</a></li>
                                    </ul>
                                    -->
                            </div>

                        </div>
                    </div>
                </div>
                <div class="copyright-block dark-bg clearfix">
                    <div class="container">
                        <div>
                            <p class="text-center">Copyright &copy; 2014 by Maginot Technologies, LLC. All Rights Reserved</p>
                        </div>

                    </div>
                </div>
            </footer>

            <a class="goTop goTop-link" title="Go Top"><i class="fa fa-arrow-up"></i></a>


            <% Response.Write(Scripts.Render("~/bundles/js/jquery").ToHtmlString()); %>
            <% Response.Write(Scripts.Render("~/bundles/js/bootstrap").ToHtmlString()); %>
            <% Response.Write(Scripts.Render("~/bundles/js/colorbox").ToHtmlString()); %>
            <% Response.Write(Scripts.Render("~/bundles/js/waypoints").ToHtmlString()); %>
            <% Response.Write(Scripts.Render("~/bundles/js/placeholder").ToHtmlString()); %>
            <% Response.Write(Scripts.Render("~/bundles/js/modernizr").ToHtmlString()); %>
            <% Response.Write(Scripts.Render("~/bundles/js/main").ToHtmlString()); %>
</body>
</html>

<script type="text/javascript">


    // this function is used to get values and sent pass these values to “ProcessSubmittedEmail” action method on "HomeController" using ajax
    //and display contant result to user using alert
    function SendMail()
    {
       $.ajax({
           url: "/Home/ProcessSubmittedEmail",
           type: "POST",
           async: true,
           data: { 'name': $('#name').val(), 'emailId': $('#emailId').val(), 'message': $('#message').val() },
           success: function (result) {
               if (result == "success") {
                   $('#name').val(""); $('#emailId').val(""); $('#message').val("");
                   $('#spMessage').html("Email Submitted");
               }
               else {
                   $('#spMessage').html(result);
               }
           }
       });
    }

</script>
