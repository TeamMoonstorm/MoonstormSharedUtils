using UnityEngine;

namespace NGTools.Tests
{
	public class TestRemoteScene : MonoBehaviour
	{
		private const float	XOffset = 0F;
		private const float	YOffset = 20F;
		private const float	Height = 30F;

		private bool		display = true;
		private GUIStyle	labelStyle;
		private Rect		r = new Rect(0F, 0F, 0, 30F);
		private Vector2		scrollPosition;

		protected virtual void	OnGUI()
		{
			if (this.labelStyle == null)
			{
				this.labelStyle = new GUIStyle(GUI.skin.label);
				this.labelStyle.wordWrap = true;
				this.labelStyle.onNormal.textColor = Color.black;
				this.labelStyle.normal.textColor = Color.black;
			}

#if UNITY_EDITOR
			if (TestUtility.RequireUnmaximized() == true)
				return;
#endif

			if (GUI.Button(new Rect(1F, 1F, 120F, 20F), "Toggle Tutorial") == true)
				this.display = !this.display;

			if (this.display == false)
				return;

			this.r.x = TestRemoteScene.XOffset;
			this.r.y = TestRemoteScene.YOffset;
			this.r.width = Screen.width - this.r.x - this.r.x;
			this.r.height = Screen.height - this.r.y;

			GUI.Box(this.r, "Tutorial NG Remote Scene");

			GUILayout.Space(40F);

			using (var scroll = new GUILayout.ScrollViewScope(this.scrollPosition))
			{
				this.scrollPosition = scroll.scrollPosition;

				GUILayout.Label("1. Generate a standalone build with this current scene.", this.labelStyle);

				GUILayout.Label("2. Start the build.", this.labelStyle);

				GUILayout.Label("3. Open a <b>NG Remote Hierarchy</b> window. You will find it in the menu at <b>Window/" + Constants.PackageTitle + "/NG Remote Hierarchy</b>.", this.labelStyle);

				GUILayout.Label("4. In <b>NG Remote Hierarchy</b>, type \"<i>127.0.0.1</i>\" in <b>Address</b> and \"<i>17257</i>\" in <b>Port</b>. Now press the button \"<b>Connect</b>\".", this.labelStyle);
				GUILayout.Label("   You should see the same Game Object as those in this current scene.", this.labelStyle);

				GUILayout.Label("5. Open a <b>NG Remote Inspector</b> window. You will find it in the menu at <b>Window/" + Constants.PackageTitle + "/NG Remote Inspector</b>.", this.labelStyle);
				GUILayout.Label("   This is the equivalent of Unity's <b>Inspector</b> window", this.labelStyle);

				GUILayout.Label("6. Open a <b>NG Remote Project</b> window. You will find it in the menu at <b>Window/" + Constants.PackageTitle + "/NG Remote Project</b>.", this.labelStyle);
				GUILayout.Label("   This is the equivalent of Unity's <b>Project</b> window", this.labelStyle);

				GUILayout.Label("7. To choose which assets appear in <b>NG Remote Project</b>, you need to check assets in <b>NG Remote Project Assets</b> in the component <b>NG Server Scene</b>.", this.labelStyle);
				GUILayout.Label("   This way you can avoid embedding unwanted assets and keep your test build lighter and faster to generate.", this.labelStyle);

				GUILayout.Label("Tips. When testing on device, make sure the port is open in both sides and the device is reachable from your network.", this.labelStyle);

				GUILayout.Label("Tips. When working on Game Object, DO NEVER DISABLE the Game Object containing <b>NG Server Scene</b>!", this.labelStyle);
			}
		}
	}
}