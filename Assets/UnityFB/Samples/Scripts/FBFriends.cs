/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       FBFriends.cs
 *  Content:    Friends class
 *
 ****************************************************************************/

using UnityEngine;
using System.Collections.Generic;


public class FBFriends
{

	private List<FBProfile> mProfileList = new List<FBProfile>();

	public FBFriends( List<FBProfile> profileList )
	{
		mProfileList = profileList;
	}

	public int GetFriendCount()
	{
		return mProfileList.Count;
	}

	public FBProfile GetFriend( int index )
	{
		return mProfileList[index];
	}

}
