/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       FBImageLoader.cs
 *  Content:    Image loader
 *
 ****************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FBImageLoader : MonoBehaviour
{

	public delegate void OnImageReady( string id, int width, int height, Texture2D image );

	private class FBImageRequest
	{
		public string id;
		public int width;
		public int height;
		public OnImageReady onImageReady;
	}

	private static FBImageLoader instance = null;
	public static FBImageLoader Get
	{
		get
		{
			if( instance == null )
			{
				GameObject go = new GameObject( "FBImageLoader" );
				instance = go.AddComponent<FBImageLoader>();
			}
			return instance;
		}
	}

	private List<FBImageRequest> imageRequestList = new List<FBImageRequest>();

	public void RequestProfileImage( string id, int width, int height, OnImageReady onImageReady )
	{
		FBImageRequest imageRequest = new FBImageRequest();
		imageRequest.id = id;
		imageRequest.width = width;
		imageRequest.height = height;
		imageRequest.onImageReady = onImageReady;
		imageRequestList.Add( imageRequest );
	}

	void Update()
	{
		if( downloadingCount < 16 && imageRequestList.Count > 0 )
		{
			FBImageRequest imageRequest = imageRequestList[0];
			imageRequestList.RemoveAt( 0 );
			StartCoroutine( RequestProfileImage_Coroutine( imageRequest ) );
		}
	}

	private int downloadingCount = 0;
	private IEnumerator RequestProfileImage_Coroutine( FBImageRequest imageRequest )
	{
		downloadingCount++;

		Texture2D image = null;

		WWW www = null;
		string url = string.Format( "https://graph.facebook.com/{0}/picture?width={1}&height={2}", imageRequest.id, imageRequest.width, imageRequest.height );

		int trycount = 0;
		string error = "";
		do
		{
			try
			{
				www = new WWW( url );
			}
			catch( System.Exception ex )
			{
				error = "[FBImageLoader] " + ex.Message;
			}

			if( error == "" )
			{
				yield return www;

				if( !string.IsNullOrEmpty( www.error ) )
				{
					error = "[FBImageLoader] " + www.error;
				}

				if( error == "" )
				{
					try
					{
						image = www.texture;
					}
					catch( System.Exception ex )
					{
						error = "[FBImageLoader] " + ex.Message;
					}
				}
				www.Dispose();
			}

			if( error != "" )
			{
				yield return new WaitForEndOfFrame();
				trycount++;
			}
		} while( error != "" && trycount < 5 );

		if( error != "" )
		{
			Debug.LogError( error + " " + url );
		}

		downloadingCount--;

		if( imageRequest.onImageReady != null )
			imageRequest.onImageReady( imageRequest.id, imageRequest.width, imageRequest.height, image );
	}

}
