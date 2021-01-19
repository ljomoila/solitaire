/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       ScreenIntro.cs
 *  Content:    Intro screen
 *
 ****************************************************************************/

using UnityEngine;
using UnityFB;
using System.Collections;


public class UIScreenIntro : UIScreen
{

	private const float LOGO_TIME = 0.5f;

	private float mLogoTimer;
	private bool mConnecting = false;

	public override void OnEnter()
	{
		mConnecting = false;
		mLogoTimer = Time.realtimeSinceStartup;
	}

	public override void OnUpdate()
	{
		if( !mConnecting )
		{
			if( Time.realtimeSinceStartup > mLogoTimer + LOGO_TIME )
			{
				ConnectFB();
			}
		}
	}

	public override void OnGUI()
	{
		Rect rect = new Rect( ( Screen.width - AppSettings.mLogo.width ) / 2, ( Screen.height - AppSettings.mLogo.height ) / 2, AppSettings.mLogo.width, AppSettings.mLogo.height );
		GUI.DrawTexture( rect, AppSettings.mLogo );
		if( mConnecting )
		{
			// NOTE:
		}
	}

	private void ConnectFB()
	{
		mConnecting = true;
        FB.Connect(AppSettings.mAppID+",debugging", "email, publish_actions", OnHandleConnect);
	}

	private void OnHandleConnect( FBResponse response )
	{
		if( !string.IsNullOrEmpty( response.error ) )
		{
			Debug.LogError( response.error );
			return;
		}

		FBLoginStatus loginStatus = FB.GetLoginStatus();
		if( loginStatus == FBLoginStatus.Unknown )
		{
			mConnecting = false;

			// NOTE: need to login
			UIScreenManager.Push( new UIScreenMenu() );
		}
		else if( loginStatus == FBLoginStatus.NotAuthorized )
		{
			// NOTE: request authorization automatically
			FB.Authorize( OnHandleConnect );
		}
		else if( loginStatus == FBLoginStatus.Connected )
		{
			// NOTE: handle connection
			mConnecting = false;

			UIScreenManager.Push( new UIScreenMenu() );
		}
	}

}
