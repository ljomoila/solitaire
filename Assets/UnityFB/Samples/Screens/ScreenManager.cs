/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       ScreenManager.cs
 *  Content:    ScreenManager
 *
 ****************************************************************************/

using UnityEngine;
using System.Collections.Generic;


public class UIScreenManager
{

	public static void OnUpdate()
	{
		UIScreen screen = GetCurrentScreen();
		if( screen != null )
		{
			screen.OnUpdate();
		}
	}

	public static void OnGUI()
	{
		UIScreen screen = GetCurrentScreen();
		if( screen != null )
		{
			screen.OnGUI();
		}
	}

	public static void Push( UIScreen newScreen )
	{
		if( newScreen == null )
			return;

		UIScreen screen = GetCurrentScreen();
		if( screen != null )
		{
			// exit
			screen.OnExit();
		}


		// add
		screens.Add( newScreen );

		// enter
		newScreen.OnEnter();
	}

	public static void Pop()
	{
		UIScreen screen = GetCurrentScreen();
		if( screen != null )
		{
			// exit
			screen.OnExit();

			// remove
			screens.RemoveAt( screens.Count - 1 );
		}

		screen = GetCurrentScreen();
		if( screen != null )
		{
			// enter
			screen.OnEnter();
		}
	}

	public static UIScreen GetCurrentScreen()
	{
		UIScreen screen = null;
		if( screens.Count > 0 )
		{
			screen = screens[screens.Count - 1];
		}
		return screen;
	}

	private static List<UIScreen> screens = new List<UIScreen>();

}

public abstract class UIScreen
{

	public virtual void OnEnter()
	{
	}

	public virtual void OnExit()
	{
	}

	public virtual void OnUpdate()
	{
	}

	public abstract void OnGUI();

}
