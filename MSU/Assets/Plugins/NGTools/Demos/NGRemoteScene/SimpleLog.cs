using System;
using System.Reflection;
using UnityEngine.Events;

namespace NGTools.Tests
{
	using UnityEngine;

	[Serializable]
	public class IntStringTest : UnityEvent<int, string>
	{
	}

	public class SimpleLog : MonoBehaviour, IResolver
	{
		public IntStringTest testA;

		[Serializable]
		public class CE1 : UnityEvent<string>
		{
		}

		[Serializable]
		public class AuxiliaryClass
		{
			public int		doubleNestedFieldInteger;
			public int[]	doubleNestedArrayFieldInteger;
			public Rect		rect;
			//public RefClass	b;
			//public Coo abc;
		}

		[Serializable]
		public class Coo
		{
			//public AuxiliaryClass		nestedComplex;
			//public Coo ab;
			//public UnityEngine.Object	a;
		}

		[Serializable]
		public class RefClass
		{
			//public int					nestedFieldInteger;
			//public int[]				nestedArrayFieldInteger;
			//public AuxiliaryClass		nestedComplex;
			public Coo					c;
			//public UnityEngine.Object	a;
		}

		public enum ActionType
		{
			Attack,
			Heal
		}

		public enum AttackCharged
		{
			One,
			Two
		}

		public ActionType actionType;
		[NGTools.ShowIf("actionType", NGTools.Op.Equals, ActionType.Attack)]
		public AttackCharged attackCharged;

		public UnityEngine.Object C;
		public UnityEvent e0;
		public UnityEvent<int> e1;
		public UnityEvent<RefClass> e2;
		public UnityEvent<string> e3;
		public CE1 e4;
		public LayerMask test1;
		[Group("AAA")]
		public UnityEngine.Object A;
		[Group("AAA")]
		public UnityEngine.Object B;
		//public bool	dontDestroyOnLoad;

		//public GUIStyle style;
		//public AudioSource		audioSource1;
		//public AudioSource audioSource2;
		//public UnityEngine.Object		anyObject;
		[Group]
		public UnityEngine.Object all2;
		//public AudioSource	source1;
		//public AudioSource	source2;
		//public AudioSource	source3;
		[Group]
		public SimpleLog ref1;
		[Group("BB")]
		public Component component;
		//public Component	componentRef2;
		//public MonoBehaviour	MonoBomponentRef1;
		//public MonoBehaviour	MonoBomponentRef2;
		//public IResolver interface1;
		//public IResolver interface2;
		[Group]
		public GameObject aGameObject;
		//public GameObject   unityObject;
		public Texture texture;
		public Texture2D texture2D;
		public Sprite sprite;
		//public Camera		camcam;
		//public AudioListener AudioListener;
		//public Projector	pro;
		//public TrailRenderer	tr;
		//public Shader		shaderr;
		//public Material		mat1;
		//public TextAsset	textAsset;
		//public Material mat2;
		//public SimpleLog log;
		//public SimpleLog[] arrayLog;

		//public UnityEngine.Object[] arrayObjects;

		//public Vector2 vector2;
		//public Vector3 vector3;
		//public Vector4 vector4;
		//public Quaternion quat;
		//public Color color;
		//public Rect rect;
		//public Bounds bounds;

		//public Material		material { get; set; }
		//public Material		sharedMaterial { get; set; }

		//public string stringer;
		//public int fieldInteger;
		//public int[] arrayFieldInteger;
		//public int refClas;
		public RefClass refClass;
		//public RefClass[] arrayRefClass;
		//public Color[] arrayColor;
		//public Coo c;

		//private AsyncOperation	loadScene;

		private void	Awake()
		{
			//if (this.dontDestroyOnLoad == true)
			//	GameObject.DontDestroyOnLoad(this);
		}

		protected virtual void OnEnable()
		{
			//NGDebug.Snapshot(this.A, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, this, "=A");
			//NGDebug.Snapshot(this.B, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, this, "=B");
			//NGDebug.Snapshot(this.texture, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, this, "=texture");
			//NGDebug.Snapshot(this.texture2D, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, this, "=texture2D");
			//if (this.fieldInteger > 0)
			//{
			//	this.all2 = new Mesh();
			//	this.all2.name = "Mesh" + UnityEngine.Random.Range(0, 1000);
			//}
			//else
			//{
			//	this.texture2D = new Texture2D(2,2);
			//	this.texture2D.name = "Texture" + UnityEngine.Random.Range(0, 1000);
			//	this.texture2D.SetPixel(0, 1, new Color(0F, 0F, UnityEngine.Random.Range(0, 1F), 1F));
			//	this.texture2D.Apply();
			//}
			//Debug.Log("Test with a Component", this);
			//Debug.Log("Test with a GameObject", this.gameObject);
			//Debug.Log("Test with a Transform", this.transform);
			//Debug.Log("Test with the main Camera", Camera.main);
		}
		/*
		protected virtual void	OnGUI()
		{
			GUILayout.Label(SceneManager.GetActiveScene().name);

			if (GUILayout.Button("Scene 0") == true)
				SceneManager.LoadScene(0);
			if (GUILayout.Button("Scene 1") == true)
				SceneManager.LoadScene(1);
			if (GUILayout.Button("Scene Async 0") == true)
				this.loadScene = SceneManager.LoadSceneAsync(0);
			if (GUILayout.Button("Scene Async 1") == true)
				this.loadScene = SceneManager.LoadSceneAsync(1);

			if (GUILayout.Button("Scene 0") == true)
				SceneManager.LoadScene(0, LoadSceneMode.Additive);
			if (GUILayout.Button("Scene 1") == true)
				SceneManager.LoadScene(1, LoadSceneMode.Additive);
			if (GUILayout.Button("Scene Async 0") == true)
				this.loadScene = SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
			if (GUILayout.Button("Scene Async 1") == true)
				this.loadScene = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

			if (this.loadScene != null)
			{
				GUILayout.Label(this.loadScene.progress.ToString());
				if (this.loadScene.isDone == true)
				{
					if (GUILayout.Button("Accept") == true)
						this.loadScene.allowSceneActivation = true;
				}
			}
		}*/

		public void	Test()
		{
			Debug.Log("test");
		}

		float	OnHierarchyGUI(Rect r)
		{
			r.xMin = r.xMax - 20F;
			GUI.Button(r, "b");
			return r.xMin;
		}

		public void GetResolver(GameObject selectedGameObject, out string identifier, out Func<string, Object> resolver)
		{
			identifier = "20";
			resolver = a;
			//resolver = (int id ) => { return Camera.main != null ? Camera.main.gameObject : null; };
		}

		private static Object	a(string id)
		{
			return Camera.main.gameObject;
		}
	}
}