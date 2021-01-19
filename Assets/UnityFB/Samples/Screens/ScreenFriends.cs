/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       ScreenFriends.cs
 *  Content:    Get User's Friend list by calling 
 * 				UnityFB.FB.API( "/me/friends")
 *
 ****************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class UIScreenFriends : UIScreen
{

	private const int BACK_BUTTON_HEIGHT = 60;
	private const int JSON_TEXT_HEIGHT = 200;

	private FBFriends mFriends = null;
	private Vector2 mScroll = Vector2.zero;
	private Dictionary<string, Texture2D> mImages = new Dictionary<string, Texture2D>();

	private string mTextJsonMeFriends = null;
	private Vector2 mScrollJson = Vector2.zero;

	/// <summary>
	/// Calling the api for friend list.
	/// </summary>
	public override void OnEnter()
	{
		UnityFB.FB.API( "/me/friends", GetFriendsCallback );
	}


	private void GetFriendsCallback( UnityFB.FBResponse response )
	{
		mTextJsonMeFriends = response.result;

		object o = JsonSerializer.Decode( mTextJsonMeFriends );
		Hashtable hashtable = o as Hashtable;

		if( hashtable != null )
		{
			ArrayList friendsData = hashtable["data"] as ArrayList;
			List<FBProfile> friendList = new List<FBProfile>();

			foreach( object friendData in friendsData )
			{
				Hashtable h = friendData as Hashtable;

				string id = h["id"] as string;
				string name = h["name"] as string;

				FBProfile friend = new FBProfile( id, name );
				friendList.Add( friend );
			}

			mFriends = new FBFriends( friendList );
			for( int i = 0; i < mFriends.GetFriendCount(); ++i )
			{
				FBProfile profile = mFriends.GetFriend( i );
				FBImageLoader.Get.RequestProfileImage( profile.id, 64, 64, OnImageReady );
			}
		}
	}

	private void OnImageReady( string id, int width, int height, Texture2D image )
	{
		mImages[id] = image;
	}

	public override void OnGUI()
	{
		GUILayout.BeginVertical( GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
		{
			if( mFriends != null )
			{
				mScroll = GUILayout.BeginScrollView( mScroll );
				for( int i = 0; i < mFriends.GetFriendCount(); ++i )
				{
					FBProfile profile = mFriends.GetFriend( i );

					GUILayout.BeginHorizontal();
					{
						GUILayout.Label( profile.name, GUILayout.ExpandWidth( true ), GUILayout.Height( 64 ) );
						GUILayout.Label( string.Empty, GUILayout.Width( 64 ), GUILayout.Height( 64 ) );

						Texture2D image = null;
						if( mImages.TryGetValue( profile.id, out image ) )
						{
							if( image != null )
							{
								Rect rect = GUILayoutUtility.GetLastRect();
								GUI.DrawTexture( rect, image );
							}
						}
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndScrollView();
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
			GUILayout.Label( mTextJsonMeFriends );
			GUILayout.EndScrollView();
		}
		GUILayout.EndVertical();

		if( GUILayout.Button( "Back", GUILayout.ExpandWidth( true ), GUILayout.Height( BACK_BUTTON_HEIGHT ) ) )
		{
			UIScreenManager.Pop();
		}
	}

}
