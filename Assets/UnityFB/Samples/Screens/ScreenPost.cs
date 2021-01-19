/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       ScreenPost.cs
 *  Content:    Sample script for posting message to user's wall
 *				UnityFB.FB.API( "/me/feed")
 *
 ****************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class UIScreenPost : UIScreen
{

	private const int JSON_TEXT_HEIGHT = 200;
	private const int POST_BUTTON_HEIGHT = 100;
	private const int BACK_BUTTON_HEIGHT = 60;

	private string mMessage = string.Empty;

	private Vector2 mScrollJson = Vector2.zero;
	private string mTextJsonFeed = null;

	public override void OnGUI()
	{
		GUILayout.Label( "Message:" );
		mMessage = GUILayout.TextArea( mMessage, GUILayout.ExpandHeight( true ) );

		GUILayout.BeginVertical( "box", GUILayout.ExpandWidth( true ), GUILayout.Height( JSON_TEXT_HEIGHT ) );
		{
			mScrollJson = GUILayout.BeginScrollView( mScrollJson );
			GUILayout.Label( mTextJsonFeed );
			GUILayout.EndScrollView();
		}
		GUILayout.EndVertical();

		if( GUILayout.Button( "Post", GUILayout.ExpandWidth( true ), GUILayout.Height( POST_BUTTON_HEIGHT ) ) )
		{
			SendPost();
		}

		if( GUILayout.Button( "Back", GUILayout.ExpandWidth( true ), GUILayout.Height( BACK_BUTTON_HEIGHT ) ) )
		{
			UIScreenManager.Pop();
		}
	}

	private void SendPost()
	{
		Dictionary<string, string> param = new Dictionary<string, string>();
		param.Add( "message", mMessage );

		UnityFB.FB.API( "/me/feed", param, FeedCallback );
	}

	private void FeedCallback( UnityFB.FBResponse response )
	{
		mTextJsonFeed = response.result;
	}

}
