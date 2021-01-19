/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       FBProfile.cs
 *  Content:    Profile class
 *
 ****************************************************************************/

using UnityEngine;
using System.Collections;


public class FBProfile
{

	private string mID = string.Empty;
	private string mName = string.Empty;
	private Hashtable mHashtable = null;

	public string id { get { return mID; } }
	public string name { get { return mName; } }

	public object GetObject( string name )
	{
		return mHashtable[name];
	}

	public FBProfile( string id, string name )
	{
		mID = id;
		mName = name;
	}

	public FBProfile( Hashtable hashtable )
	{
		mHashtable = hashtable;

		mID = hashtable["id"] as string;
		mName = hashtable["name"] as string;
	}

}
