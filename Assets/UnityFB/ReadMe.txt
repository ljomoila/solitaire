"Facebook Plugin for Unity" Version 1.0

For question and support, please email support@unityfacebook.com

-----------------------------------
Create a Facebook Application
-----------------------------------

If you haven't create a facebook application.  You should create a facebook application.

1. Go to link http://developers.facebook.com and click on the "Apps" link in the 
   navigation bar.  
2. If you aren’t register as a developer click on "Register as a Developer" and go 
   through the wizard to register as a Developer.
3. On the Developer App homepage, click on Create New App in the upper right corner
   of the page.
4. Fill in App Display Name and App Namespace
5. After some obligatory captcha checking, you’ll be redirected to your newly-minted 
   application’s page. Here you’ll see the APP ID and APP SECRET that we need. 
   Copy and paste these values somewhere for later use.
   
-----------------------------------
SAMPLE USUAGE NOTES
-----------------------------------

1.  Open the sample scene under:
	../Assets/UnityFB/Samples/UnityFBSample.unity
2.  Select the UnityFBSample gameObject in the scene.
3.  Enter the APP ID you want to integrate under the UnityFBSample component's 
    appID property
4.	After you entered the appID.  The integration is basically done.
5.  Now if you run the game and hit the login button.
6.  Enter your facebook login.
7.  Once you logged in your facebook app now have access to user's user's email 
    info, user birthday info, location info, friends list etc.
8.  For now.  Once you logged in you you will see 3 button as labeled how you would 
	call the plugin functions.
	
UnityFB.FB.API( "/me" )           - get user's profile info
UnityFB.FB.API( "/me/friends" )   - get user's friend lists
UnityFB.FB.API( "/me/feed" )	  - post message to user's feed

Should you have any questions or encounter any problems while integrating the plugin or 
suggestions, please do not hesitate to shoot us an email at support@unityfacebook.com, 
we will be more than happy to help you. Thanks for choosing Facebook Plugin for Unity!


        


