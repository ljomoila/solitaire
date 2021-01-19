/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       ScreenProfile.cs
 *  Content:    Sample script to get user profile info
 * 				UnityFB.FB.API( \"/me\")
 *
 ****************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class UIScreenProfile : UIScreen
{

	private const int BACK_BUTTON_HEIGHT = 60;
	private const int JSON_TEXT_HEIGHT = 200;

	private FBProfile mProfile = null;
	private Texture2D mImage = null;

	private string mTextJsonMe = null;
	private Vector2 mScrollJson = Vector2.zero;

	public override void OnEnter()
	{
		UnityFB.FB.API( "/me", GetProfileCallback );
	}

	private void GetProfileCallback( UnityFB.FBResponse response )
	{
		mTextJsonMe = response.result;

		object o = JsonSerializer.Decode( mTextJsonMe );
		Hashtable hashtable = o as Hashtable;
		if( hashtable != null )
		{
			mProfile = new FBProfile( hashtable );
			FBImageLoader.Get.RequestProfileImage( mProfile.id, 256, 256, OnImageReady );
		}
	}

	private void OnImageReady( string id, int width, int height, Texture2D image )
	{
		mImage = image;
	}

	public override void OnGUI()
	{
		GUILayout.BeginVertical( GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
		{
			if( mProfile != null )
			{
				GUILayout.Space( 128 );
				if( mImage != null )
				{
					GUI.DrawTexture( new Rect( 0, 0, 128, 128 ), mImage );
				}

				GUILayout.Label( "id : " + mProfile.id, GUILayout.ExpandWidth( true ), GUILayout.Height( 20 ) );
				GUILayout.Label( "name : " + mProfile.name, GUILayout.ExpandWidth( true ), GUILayout.Height( 20 ) );
			}
			else
			{
				GUILayout.Label( "Waiting" );
			}
		}
		GUILayout.EndVertical();

		GUILayout.BeginVertical( "box", GUILayout.ExpandWidth( true ), GUILayout.Height( JSON_TEXT_HEIGHT ) );
		{
			mScrollJson = GUILayout.BeginScrollView( mScrollJson );
			GUILayout.Label( mTextJsonMe );
			GUILayout.EndScrollView();
		}
		GUILayout.EndVertical();

		if( GUILayout.Button( "Back", GUILayout.ExpandWidth( true ), GUILayout.Height( BACK_BUTTON_HEIGHT ) ) )
		{
			UIScreenManager.Pop();
		}
	}

}
