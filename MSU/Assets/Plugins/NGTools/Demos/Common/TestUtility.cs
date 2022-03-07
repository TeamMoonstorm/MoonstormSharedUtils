#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;

namespace NGTools.Tests
{
	using UnityEngine;

	public static class TestUtility
	{
		private static GUIStyle	centeredLabel;
		public static GUIStyle	CenteredLabel
		{
			get
			{
				if (TestUtility.centeredLabel == null)
				{
					TestUtility.centeredLabel = new GUIStyle(GUI.skin.label);
					TestUtility.centeredLabel.alignment = TextAnchor.MiddleCenter;
					TestUtility.centeredLabel.fontStyle = FontStyle.Bold;
					TestUtility.centeredLabel.normal.textColor = Color.black;
				}
				return TestUtility.centeredLabel;
			}
		}

		private static Rect	r = default(Rect);
		private const string		WindowLayoutType = "UnityEditor.WindowLayout";
		private const string		IsMaximizedMethod = "IsMaximized";
		private const string		UnmaximizeMethod = "Unmaximize";
		private const string		MaximizeMethod = "Maximize";
		private static Type			windowLayout;
		private static MethodInfo	isMaximized;
		private static MethodInfo	unmaximize;
		private static MethodInfo	maximize;

		public static bool	IsMaximized(UnityEditor.EditorWindow window)
		{
			if (TestUtility.windowLayout == null)
			{
				TestUtility.windowLayout = typeof(UnityEditor.EditorWindow).Assembly.GetType(TestUtility.WindowLayoutType);
				if (TestUtility.windowLayout == null)
					Debug.LogError("Type \"" + TestUtility.WindowLayoutType + "\" was not found in assembly. It may happens in newer version of Unity. Please contact " + Constants.PackageTitle + ".");
			}
			if (TestUtility.windowLayout != null && TestUtility.isMaximized == null)
			{
				TestUtility.isMaximized = TestUtility.windowLayout.GetMethod(TestUtility.IsMaximizedMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (TestUtility.isMaximized == null)
					Debug.LogError("Method \"" + TestUtility.IsMaximizedMethod + "\" was not found in type \"" + TestUtility.WindowLayoutType + "\". It may happens in newer version of Unity. Please contact " + Constants.PackageTitle + ".");
			}

			return (bool)TestUtility.isMaximized.Invoke(null, new object[] { window });
		}

		public static void	UnmaximizeWindow(UnityEditor.EditorWindow window)
		{
			if (TestUtility.windowLayout == null)
			{
				TestUtility.windowLayout = typeof(UnityEditor.EditorWindow).Assembly.GetType(TestUtility.WindowLayoutType);
				if (TestUtility.windowLayout == null)
					Debug.LogError("Type \"" + TestUtility.WindowLayoutType + "\" was not found in assembly. It may happens in newer version of Unity. Please contact " + Constants.PackageTitle + ".");
			}
			if (TestUtility.windowLayout != null && TestUtility.unmaximize == null)
			{
				TestUtility.unmaximize = TestUtility.windowLayout.GetMethod(TestUtility.UnmaximizeMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (TestUtility.unmaximize == null)
					Debug.LogError("Method \"" + TestUtility.UnmaximizeMethod + "\" was not found in type \"" + TestUtility.WindowLayoutType + "\". It may happens in newer version of Unity. Please contact " + Constants.PackageTitle + ".");
			}

			TestUtility.unmaximize.Invoke(null, new object[] { window });
		}

		public static void	MaximizeWindow(UnityEditor.EditorWindow window)
		{
			if (TestUtility.windowLayout == null)
			{
				TestUtility.windowLayout = typeof(UnityEditor.EditorWindow).Assembly.GetType(TestUtility.WindowLayoutType);
				if (TestUtility.windowLayout == null)
					Debug.LogError("Type \"" + TestUtility.WindowLayoutType + "\" was not found in assembly. It may happens in newer version of Unity. Please contact " + Constants.PackageTitle + ".");
			}
			if (TestUtility.windowLayout != null && TestUtility.maximize == null)
			{
				TestUtility.maximize = TestUtility.windowLayout.GetMethod(TestUtility.MaximizeMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (TestUtility.maximize == null)
					Debug.LogError("Method \"" + TestUtility.MaximizeMethod + "\" was not found in type \"" + TestUtility.WindowLayoutType + "\". It may happens in newer version of Unity. Please contact " + Constants.PackageTitle + ".");
			}

			TestUtility.maximize.Invoke(null, new object[] { window });
		}

		public static bool	RequireUnmaximized()
		{
			if (EditorWindow.focusedWindow != null && TestUtility.IsMaximized(EditorWindow.focusedWindow) == true)
			{
				TestUtility.r.x = 0F;
				TestUtility.r.y = 0F;
				TestUtility.r.width = Screen.width;
				TestUtility.r.height = Screen.height;
				GUI.Label(TestUtility.r, "This tutorial requires to unmaximized your current window.", TestUtility.CenteredLabel);
				TestUtility.r.x = Screen.width * .5F - 100F;
				TestUtility.r.y = Screen.height * .5F + 50F;
				TestUtility.r.width = 200F;
				TestUtility.r.height = 50F;
				if (GUI.Button(TestUtility.r, "Unmaximized window") == true)
				{
					TestUtility.UnmaximizeWindow(EditorWindow.focusedWindow);
				}
				return true;
			}
			return false;
		}
	}
}
#endif