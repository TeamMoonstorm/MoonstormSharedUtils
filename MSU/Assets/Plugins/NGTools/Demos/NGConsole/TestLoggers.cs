#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace NGTools.Tests
{
	using UnityEngine;

	public class TestLoggers : MonoBehaviour
	{
		private const float	Spacing = 10F;
		private const float	XOffset = 20F;
		private const float	YOffset = 20F;
		private const float	SampleInterval = 1F;

		public float	currentTime;
		public float	randomThing;

		public bool	activeIntervalLogs;

		private Assembly	editorAssembly;

		private Type		ContactFormWizardType;
		private Type		NGConsoleType;
		private FieldInfo	VisibleModulesFieldInfo;
		private FieldInfo	SettingsFieldInfo;
		private MethodInfo	SetModuleMethodInfo;
		private MethodInfo	RepaintMethodInfo;

		private Type		ModuleType;
		private FieldInfo	IdFieldInfo;

		private Type		MainModuleType;

		//private Type		RecorderModuleType;
		//private FieldInfo	StreamsFieldInfo;

		private Object		NGConsoleInstance;
		private object		workingModule;
		private bool		switchModule = false;

		private GUIStyle	centeredTextField;
		private GUIStyle	centeredLabel;
		private Rect		r = new Rect(0F, 0F, 0, 30F);
		private Rect		fullScreenRect = new Rect();
		private Vector2		loggersScrollPosition;

		protected virtual void	Awake()
		{
			Debug.Log("[Core] Awake");

			Assembly[]	assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var assembly in assemblies)
			{
				if (this.ContactFormWizardType == null)
				{
					if (assembly.FullName.StartsWith("NGCoreEditor") == true ||
						assembly.FullName.StartsWith("Assembly-CSharp-Editor") == true)
					{
						this.ContactFormWizardType = assembly.GetType("NGToolsEditor.ContactFormWizard");
					}
				}

				if (this.MainModuleType == null)
				{
					if (assembly.FullName.StartsWith("NGConsoleEditor") == true ||
						assembly.FullName.StartsWith("Assembly-CSharp-Editor") == true)
					{
						try
						{
							this.editorAssembly = assembly;

							this.NGConsoleType = this.editorAssembly.GetType("NGToolsEditor.NGConsole.NGConsoleWindow");
							this.VisibleModulesFieldInfo = this.NGConsoleType.GetField("visibleModules", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
							this.SettingsFieldInfo = this.NGConsoleType.GetField("settings", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
							this.SetModuleMethodInfo = this.NGConsoleType.GetMethod("SetModule", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
							this.RepaintMethodInfo = this.NGConsoleType.GetMethod("Repaint", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

							this.ModuleType = this.editorAssembly.GetType("NGToolsEditor.NGConsole.Module");
							this.IdFieldInfo = this.ModuleType.GetField("id", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

							this.MainModuleType = this.editorAssembly.GetType("NGToolsEditor.NGConsole.MainModule");
							break;
						}
						catch
						{
						}
					}
				}
			}
		}

		protected virtual void	OnEnable()
		{
			Debug.Log("[Core] OnEnable");
			this.InvokeRepeating("Memory", 1F, .6F);
			this.InvokeRepeating("OnlyWarning", .5F, 3F);
		}

		protected virtual void	OnDisable()
		{
			Debug.Log("[Core] OnDisable");
			this.CancelInvoke();
		}

		protected virtual void	Update()
		{
			this.currentTime = Time.realtimeSinceStartup;
			this.randomThing = Random.Range(0F, 1F);

			if (this.activeIntervalLogs == false)
				return;

			Debug.Log("Update");
		}

		protected virtual void	OnGUI()
		{
			if (this.centeredTextField == null)
			{
				this.centeredTextField = new GUIStyle(GUI.skin.textField);
				this.centeredTextField.alignment = TextAnchor.MiddleCenter;
				this.centeredLabel = new GUIStyle(GUI.skin.label);
				this.centeredLabel.alignment = TextAnchor.MiddleCenter;
			}

			if (this.NGConsoleType == null)
			{
				this.fullScreenRect.x = 0F;
				this.fullScreenRect.y = 0F;
				this.fullScreenRect.width = Screen.width;
				this.fullScreenRect.height = Screen.height;
				if (GUI.Button(this.fullScreenRect, "The NG Console tutorial is encountering issues. Contact the author and tell him that it is broken.", this.centeredLabel) == true)
				{
					// Force the wizard to be unique.
					ScriptableWizard.GetWindow(this.ContactFormWizardType).Close();
					ScriptableWizard.DisplayWizard(string.Empty, this.ContactFormWizardType);
				}
				return;
			}

			if (TestUtility.RequireUnmaximized() == true)
				return;

			this.r.x = TestLoggers.XOffset;
			this.r.y = TestLoggers.YOffset;
			this.r.width = Screen.width - TestLoggers.XOffset - TestLoggers.XOffset;

			Object[]	instances = Resources.FindObjectsOfTypeAll(NGConsoleType);

			if (instances.Length == 0)
			{
				if (GUI.Button(r, "Open NG Console before starting the tutorial") == true)
					EditorApplication.ExecuteMenuItem("Window/" + Constants.PackageTitle + "/NG Console");
				return;
			}

			this.NGConsoleInstance = instances[0];

			if (this.SettingsFieldInfo != null)
			{
				if (this.SettingsFieldInfo.GetValue(this.NGConsoleInstance) == null)
				{
					this.fullScreenRect.x = 0F;
					this.fullScreenRect.y = 0F;
					this.fullScreenRect.width = Screen.width;
					this.fullScreenRect.height = Screen.height;
					GUI.Label(this.fullScreenRect, "NG Console requires an instance of NGSetting to run. Go to the console and link it to one.", this.centeredLabel);
					return;
				}
			}

			this.DrawTestLoggers();
		}

		private void	DrawTestLoggers()
		{
			this.SelectModuleOnNGConsole(this.MainModuleType);

			if (this.workingModule == null)
			{
				this.DisplayBrokenTutorialHelper("Loggers");
				return;
			}

			GUI.Label(r, "Press buttons below to test the loggers provided by the class NGDebug.");
			r.y += r.height;

			GUI.Label(r, "Copy code to test it yourself.");
			r.y += r.height;

			r.width = 150F;
			if (GUI.Button(r, this.activeIntervalLogs == true ? "Disable interval logs" : "Enable interval logs") == true)
				this.activeIntervalLogs = !this.activeIntervalLogs;
			r.x += r.width + 10F;

			r.width = Screen.width - TestLoggers.XOffset - TestLoggers.XOffset - r.width - 10F;
			GUI.Label(r, "Use interval logs to output logs with random content to feed NG Console.");
			r.y += r.height + 10F;

			r.x = TestLoggers.XOffset;
			r.width = Screen.width - TestLoggers.XOffset - TestLoggers.XOffset;

			Rect	pos = new Rect(0F, r.y, Screen.width, Screen.height - r.y);
			Rect	viewRect = new Rect(0F, 0F, 0F, r.height * 26 + TestLoggers.Spacing * 4);

			using (var scroll = new GUI.ScrollViewScope(pos, this.loggersScrollPosition, viewRect))
			{
				this.loggersScrollPosition = scroll.scrollPosition;

				r.y = 0F;

				if (GUI.Button(r, "NGTools.NGDebug.Log(GetComponents(typeof(Component)))") == true)
				{
					NGDebug.Log(this.GetComponents(typeof(Component)));
				}
				r.y += r.height;

				GUI.TextField(r, "NGDebug.Log(GetComponents(typeof(Component)));", this.centeredTextField);
				r.y += r.height + TestLoggers.Spacing;

				if (GUI.Button(r, "NGDebug.Log(Physics.OverlapSphere(Vector3.zero, 1))") == true)
				{
					NGDebug.Log(Physics.OverlapSphere(Vector3.zero, 1F));
				}
				r.y += r.height;

				GUI.TextField(r, "NGDebug.Log(Physics.OverlapSphere(Vector3.zero, 1));", this.centeredTextField);
				r.y += r.height + TestLoggers.Spacing;

				if (GUI.Button(r, "NGDebug.Log(Physics.RaycastAll(Vector3.zero, Vector3.up, 3))") == true)
				{
					NGDebug.Log(Physics.RaycastAll(Vector3.zero, Vector3.up, 3F));
				}
				r.y += r.height;

				GUI.TextField(r, "NGDebug.Log(Physics.RaycastAll(Vector3.zero, Vector3.up, 3));", this.centeredTextField);
				r.y += r.height + TestLoggers.Spacing;

				if (GUI.Button(r, "NGDebug.Log(this, gameObject, transform)") == true)
				{
					NGDebug.Log(this, this.gameObject, this.transform);
				}
				r.y += r.height;

				GUI.TextField(r, "NGDebug.Log(this, gameObject, transform);", this.centeredTextField);
				r.y += r.height + TestLoggers.Spacing;

				if (GUI.Button(r, "NGDebug.Log(NULL)") == true)
				{
					Object	o = null;

					NGDebug.Log(o);
				}
				r.y += r.height;

				GUI.TextField(r, "NGDebug.Log(NULL);", this.centeredTextField);
				r.y += r.height + TestLoggers.Spacing;

				if (GUI.Button(r, "NGDebug.Log(NULL, this, NULL)") == true)
				{
					NGDebug.Log(null, this, null);
				}
				r.y += r.height;

				GUI.TextField(r, "NGDebug.Log(NULL, this, NULL);", this.centeredTextField);
				r.y += r.height + TestLoggers.Spacing;

				if (GUI.Button(r, "NGDebug.LogCollection(new List<GameObject>(Resources.FindObjectsOfTypeAll<GameObject>()))") == true)
				{
					NGDebug.LogCollection(new List<GameObject>(Resources.FindObjectsOfTypeAll<GameObject>()));
				}
				r.y += r.height;

				GUI.TextField(r, "NGDebug.LogCollection(new List<GameObject>(Resources.FindObjectsOfTypeAll<GameObject>()));", this.centeredTextField);
				r.y += r.height;

				GUI.Label(r, "Use LogCollection with List, Stack or any array or collection that inherit from IEnumerable.");
				r.y += r.height;

				if (GUI.Button(r, "NGDebug.LogHierarchy(this)") == true)
				{
					NGDebug.LogHierarchy(this);
				}
				r.y += r.height;

				GUI.TextField(r, "NGDebug.LogHierarchy(this);", this.centeredTextField);
				r.y += r.height;

				GUI.Label(r, "Use LogHierarchy to display the upward hierarchy of any Game Object, Component or RaycastHit.");
				r.y += r.height;

				if (GUI.Button(r, "NGDebug.Snapshot(this)") == true)
				{
					NGDebug.Snapshot(this);
				}
				r.y += r.height;

				GUI.TextField(r, "NGDebug.Snapshot(this);", this.centeredTextField);
				r.y += r.height;

				GUI.Label(r, "Use Snapshot to output public fields of any object at the time you call it.");
				r.y += r.height;

				GUI.Label(r, "Tips:");
				r.y += r.height;

				GUI.Label(r, "Pass your mouse over the images in NG Console to display the related Game Object's name.");
				r.y += r.height;

				GUI.Label(r, "Click on an image to ping the Object.");
				r.y += r.height;

				GUI.Label(r, "Double click or shift + click on an image to select the Object.");
			}
		}

		private void	Memory()
		{
			if (this.activeIntervalLogs == false)
				return;

			for (int i = 0, max = Random.Range(1, 3); i < max; i++)
			{
				Debug.Log(i + " [Memory] "/* + GC.GetTotalMemory(false)*/);
			}
		}

		private void	OnlyWarning()
		{
			if (this.activeIntervalLogs == false)
				return;

			for (int i = 0, max = Random.Range(1, 15); i < max; i++)
				Debug.LogWarning(i + "Warning");
		}

		private void	DisplayBrokenTutorialHelper(string title)
		{
			if (GUI.Button(r, "Tutorial \"" + title + "\" is broken, due to missing module. Please contact the author.") == true)
			{
				// Force the wizard to be unique.
				ScriptableWizard.GetWindow(this.ContactFormWizardType).Close();
				ScriptableWizard.DisplayWizard(string.Empty, this.ContactFormWizardType);
			}
		}

		private void	SelectModuleOnNGConsole(Type targetModuleType)
		{
			if (this.switchModule == false || this.workingModule == null)
			{
				this.switchModule = true;

				if (targetModuleType != null)
				{
					Array	visibleModules = (Array)this.VisibleModulesFieldInfo.GetValue(this.NGConsoleInstance);

					this.workingModule = null;

					if (visibleModules != null)
					{
						foreach (var module in visibleModules)
						{
							if (module.GetType() == targetModuleType)
							{
								object	id = this.IdFieldInfo.GetValue(module);

								this.SetModuleMethodInfo.Invoke(this.NGConsoleInstance, new object[] { id });
								this.workingModule = module;
								break;
							}
						}
					}

					this.RepaintMethodInfo.Invoke(this.NGConsoleInstance, null);
				}
			}
		}
	}
}
#endif