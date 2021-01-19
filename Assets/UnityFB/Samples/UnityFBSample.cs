/*==========================================================================;
 *
 *  Copyright (C) 2013 TenB.
 *
 *  File:       UnityFBSample.cs
 *  Content:    UnityFB
 *
 ****************************************************************************/

using UnityEngine;
using System;
using System.IO;

public class UnityFBSample : MonoBehaviour
{

    public Texture2D mLogo = null;

    // NOTE: input your app id
	public string mAppID = "662754383740260";

    void Awake()
    {
        AppSettings.mLogo = mLogo;
        AppSettings.mAppID = mAppID;
    }

    void Start()
    {
        UIScreenManager.Push(new UIScreenIntro());
    }

    void Update()
    {
        UIScreenManager.OnUpdate();
    }

    void OnGUI()
    {
        GUILayout.BeginVertical(GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));
        UIScreenManager.OnGUI();
        GUILayout.EndVertical();
    }

}
	
public class AppSettings
{
    public static Texture2D mLogo;
    public static string mAppID;
}
