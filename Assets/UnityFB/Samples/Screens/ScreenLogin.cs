/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       ScreenLogin.cs
 *  Content:    Login screen
 *
 ****************************************************************************/

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
#define UNITY_MOBILE_OS
#endif //

using UnityEngine;
using UnityFB;
using System.Collections;


public class UIScreenLogin : UIScreen
{

	private string email = "sample@email.com";
	private string password = "password";

	private bool waiting = false;

	public override void OnEnter()
	{
	}

#if UNITY_MOBILE_OS
	private string securePassword = string.Empty;
	private bool emailKeyboard = false;
	private bool passwordKeyboard = false;
	private TouchScreenKeyboard keyboard;

	private void UpdateSecurePassword()
	{
	    securePassword = new string('*', password.Length);
	}

	public override void OnUpdate()
	{
	    if (keyboard != null)
	    {
	        if (emailKeyboard)
            {
	            email = keyboard.text;
            }
            else if (passwordKeyboard)
	        {
	            password = keyboard.text;
	            UpdateSecurePassword();
	        }

            if(!keyboard.active || !TouchScreenKeyboard.visible)
            {
	            emailKeyboard = false;
	            passwordKeyboard = false;
	            keyboard = null;
            }
	    }

	    if (keyboard != null && keyboard.done)
	    {
	        emailKeyboard = false;
	        passwordKeyboard = false;
	        keyboard = null;
	    }
	}

	private void OnGUI_Account()
	{
	    if (GUILayout.Button(email, GUILayout.ExpandWidth(true), GUILayout.Height(100)))
	    {
	        keyboard = TouchScreenKeyboard.Open(email, TouchScreenKeyboardType.EmailAddress, false);
	        emailKeyboard = true;
	    }

	    if (GUILayout.Button(securePassword, GUILayout.ExpandWidth(true), GUILayout.Height(100)))
	    {
	        keyboard = TouchScreenKeyboard.Open(password, TouchScreenKeyboardType.Default, false, false, true);
	        passwordKeyboard = true;
	    }
	}
#else // UNITY_MOBILE_OS
    private void OnGUI_Account()
	{
		email = GUILayout.TextField( email, GUILayout.ExpandWidth( true ), GUILayout.Height( 60 ) );
		password = GUILayout.PasswordField( password, '*', GUILayout.ExpandWidth( true ), GUILayout.Height( 60 ) );
	}
#endif // UNITY_MOBILE_OS

	public override void OnGUI()
	{
		if( waiting )
			GUI.enabled = false;

		GUILayout.BeginVertical( GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
		{
			GUILayout.Box( "Unity Facebook", GUILayout.ExpandWidth( true ), GUILayout.Height( 100 ) );

			OnGUI_Account();

			if( GUILayout.Button( "Connect", GUILayout.Height( 100 ) ) )
			{
				waiting = true;
				FB.Login( email, password, OnHandleLogin );
			}
		}
		GUILayout.EndVertical();

		GUI.enabled = true;
	}

	private void OnHandleLogin( FBResponse response )
	{
		waiting = false;

		if( !string.IsNullOrEmpty( response.error ) )
		{
			Debug.LogError( response.error );
			return;
		}

		FBLoginStatus loginStatus = FB.GetLoginStatus();
		if( loginStatus == FBLoginStatus.NotAuthorized )
		{
			// NOTE: request authorization automatically
			waiting = true;
			FB.Authorize( OnHandleLogin );
		}
		else if( loginStatus == FBLoginStatus.Connected )
		{
			// NOTE: handle connection
            UIScreenManager.Pop();
		}
	}

}
