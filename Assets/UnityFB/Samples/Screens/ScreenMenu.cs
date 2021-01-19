/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       ScreenMenu.cs
 *  Content:    The Menu Menu screen of UnityFBSample
 *
 ****************************************************************************/

using UnityEngine;
using UnityFB;
using System.Collections;


public class UIScreenMenu : UIScreen
{

	private bool mWaiting = false;

	public override void OnGUI()
	{
		GUILayout.BeginVertical( GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
		{
			if( FB.GetLoginStatus() != FBLoginStatus.Connected || mWaiting )
				GUI.enabled = false;

			if( GUILayout.Button( "Get User Profile\nUnityFB.FB.API( \"/me\")", GUILayout.ExpandWidth( true ), GUILayout.Height( 100 ) ) )
			{
				UIScreenManager.Push( new UIScreenProfile() );
			}

			if( GUILayout.Button( "Get User's FriendList\nUnityFB.FB.API( \"/me/friends\")", GUILayout.ExpandWidth( true ), GUILayout.Height( 100 ) ) )
			{
				UIScreenManager.Push( new UIScreenFriends() );
			}

			if( GUILayout.Button( "Post to User's wall\nUnityFB.FB.API( \"/me/feed\")", GUILayout.ExpandWidth( true ), GUILayout.Height( 100 ) ) )
			{
				UIScreenManager.Push( new UIScreenPost() );
			}

			GUI.enabled = !mWaiting;
			if( FB.GetLoginStatus() == FBLoginStatus.Connected )
			{
				if( GUILayout.Button( "Logout", GUILayout.ExpandWidth( true ), GUILayout.Height( 100 ) ) )
				{
					mWaiting = true;
					FB.Logout( OnLogout );
				}
			}
			else
			{
				if( GUILayout.Button( "Login", GUILayout.ExpandWidth( true ), GUILayout.Height( 100 ) ) )
				{
					UIScreenManager.Push( new UIScreenLogin() );
				}
			}
			GUI.enabled = true;
		}
		GUILayout.EndVertical();
	}

	private void OnLogout( FBResponse response )
	{
		mWaiting = false;
		UIScreenManager.Pop();
	}

}
