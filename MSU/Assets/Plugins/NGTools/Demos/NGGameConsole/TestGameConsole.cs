using UnityEngine;

namespace NGTools.Tests
{
	public class TestGameConsole : MonoBehaviour
	{
		private Rect		fullScreenRect = new Rect();
		private GUIStyle	centeredLabel;

		protected virtual void	OnGUI()
		{
			if (this.centeredLabel == null)
			{
				this.centeredLabel = new GUIStyle(GUI.skin.label);
				this.centeredLabel.alignment = TextAnchor.MiddleCenter;
			}

			this.fullScreenRect.x = 0F;
			this.fullScreenRect.y = 0F;
			this.fullScreenRect.width = Screen.width;
			this.fullScreenRect.height = Screen.height;
			GUI.Label(this.fullScreenRect, "Draw 3 clock circles around the center of the screen to open the game console.\n\nOr\n\nTouch the corners in the following order in less than 5 secondes: top-left -> top-right -> bottom-left -> bottom-right.", this.centeredLabel);
		}
	}
}