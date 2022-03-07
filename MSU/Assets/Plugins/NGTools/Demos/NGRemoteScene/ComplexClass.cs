#pragma warning disable 414
using System;
using System.Collections.Generic;

namespace NGTools.Tests
{
	using UnityEngine;

	public class ComplexClass : MonoBehaviour
	{
		[Flags]
		public enum FlagsDummyEnum
		{
			None = 0x1,
			Ya = 0x2,
			Why = 0x4,
			NotAtAll = 0x8,
			ABit = 0x16,
			All = None | Ya | Why | NotAtAll | ABit
		}

		public enum DummyEnum
		{
			No,
			Yes,
			WhyNot
		}

		[Serializable]
		public class SerializableDummyNestedClass
		{
			//public int			classInt;
			//public bool			classBool;
			//public string		classString;
			//public Object		classObject;
			//public GameObject	classGameObject;
			//public Component	classComponent;
			//public Object[]		classArray;
			//public SerializableDummyClass	nestedClass;
			public SerializableDummyClass[]	nestedArray;
		}

		[Serializable]
		public class SerializableDummyClass
		{
			//public int          classInt;
			//public bool         classBool;
			public string       classString;
			//public Object       classObject;
			//public GameObject   classGameObject;
			//public Component    classComponent;
			//public Object[]		classArray;
		}

		[Serializable]
		public struct SerializableDummyStruct
		{
			public int			structInt;
			public bool			structBool;
			public string		structString;
			public Object		structObject;
			public GameObject	structGameObject;
			public Component	structComponent;
			public Object[]		classArray;
		}

		public class DummyClass
		{
			public int			classInt;
			public bool			classBool;
			public string		classString;
			public Object		classObject;
			public GameObject	classGameObject;
			public Component	classComponent;
		}

		public struct DummyStruct
		{
			public int			structInt;
			public bool			structBool;
			public string		structString;
			public Object		structObject;
			public GameObject	structGameObject;
			public Component	structComponent;
		}

		public static Rect s_aa1;
		public static Rect M_ah2;
		public static Rect t1;
		public static Vector2 t2;
		public static Vector3 t3;
		public static Vector4 t4;
		public static Bounds t5;
		public static Color t6;
		public static AnimationCurve t7;
		public static string[] a = new string[] { "aze", "erhser" };
		public static string[] A { get { return a; } }

		private static string[] b = new string[] { "perhj", "hzehj" };
		public static List<string> la = new List<string>() { "aze", "erhser" };
		private static List<string> lb = new List<string>() { "perhj", "hzehj" };

		public int	this[int a]
		{
			get
			{
				return 0;
			}
		}

		public int	this[int a, string b]
		{
			get
			{
				return 0;
			}
		}

		public const float	constFloat = 5F;
		public const string	constString = "tototoq";

		public static float		staticFloat = 5F;
		public static string	staticString = "tototoq";

		public static float		GetFloatProp { get { return 0F; } }
		public static string	GetStringProp { get { return ""; } }

		public static float		SetFloatProp { set {} }
		public static string	SetStringProp { set {} }

		public static float		GetPSetFloatProp { get; private set; }
		public static string	GetPSetStringProp { get; private set; }

		public static float		PGetSetFloatProp { private get; set; }
		public static string	PGetSetStringProp { private get; set; }

		//		[InGroup("Test"), GHeader("FIRST EVENT")]
		//		public UnityEvent   firstEvent;
		//		[InGroup("Test"), GHeader("FIRSTé EVENT")]
		//		public UnityEvent   firstEvent2;
		//		[Group("Test"), Tooltip("GO Tooltio"), GHeader("GO", true)]
		//		public GameObject   gameObj;
		//		public Object       obj;
		//		[GHeader("HEADER TRANFO"), Group("Test")]
		//		public Transform    tran;
		//		public Material     mat;
		//		[Group("Test"), GSpace(30F), GHeader("HEADER")]
		//		public Shader       shad;
		//		public Projector    projector;
		//		public Light        lighto;

		//		public Char         charchar;
		//		public Int16        int16;
		//		[GRange(-5, 20), Group("Test"), GHeader("INT SOGLE")]
		//		public Int32        int32;
		//		[GRange(-2, 200), Group("Test"), GHeader("INT asdasd")]
		//		public Int32        int322;
		//		[GRange(-2, 200), Group("Test"), GHeader("INT asdasd")]
		//		public Int32        int3222;
		//		[GRange(-2, 200), Group("Test"), GHeader("INT asdasd")]
		//		public Int32        int32222;
		//		[Group("Test")]
		//		public Int32        int39;
		//		[Group("Test"), GRange(-2, 200)]
		//		public Int32        int3225;
		//		[GRange(-2, 200), Group("Test"), GHeader("INT asdasd")]
		//		public Int32        int897;
		//		[GRange(-2, 200), Group("Test"), GHeader("INT asdasd")]
		public Int32        int322222222;
		//		[Group("Test"), GHeader("INT asdasd")]
		//		public Int32        int32222222221;
		//		[Group("Test"), GHeader("INT asdasd")]
		//		public Int32        int322222222222;
		//		[GRange(-2, 200), Group("Test"), GHeader("INT asdasd")]
		//		public Int32        int323;
		//		public Int64        int64;
		//		public UInt16       uint16;
		//		public UInt32       uint32;
		//		public UInt64       uint64;
		//		public Byte         fByte;
		//		public SByte        fSByte;
		//		[GRange(0F, 10F), Group("Test"), GHeader("SOGLE")]
		//		public Double       fDouble;
		//		public Single       fSingle;
		//		public Decimal      fDecimal;
		//		[InGroup("Test")]
		//		public UnityEvent   secondEvent;
		//		[InGroup("Test")]
		//		public UnityEvent   thirdEvent;
		//		public UnityEvent   fourthEvent;

		//		public string       fString;

		//		public Vector2      vector2;
		//		public Vector3      vector3;
		//		public Vector4      vector4;
		//		public Quaternion   quat;
		//		public Color        color;
		//		public Rect         rect;

		//		public DummyStruct  dummyStruct;
		//		public DummyClass   dummyClass;

		//		public SerializableDummyStruct  serializableDummyStruct;
		//		public SerializableDummyClass   serializableDummyClass;
		public SerializableDummyNestedClass serializationDummyNestedClass;

		//public DummyStruct[]    arrayDummyStruct;
		//public DummyClass[]     arrayDummyClass;

		//public SerializableDummyStruct[]    arraySerializableDummyStruct;
		//public SerializableDummyClass[]     arraySerializableDummyClass;

		//public List<DummyStruct>    listDummyStruct;
		//public List<DummyClass>     listDummyClass;

		//public List<SerializableDummyStruct>    listSerializableDummyStruct;
		//public List<SerializableDummyClass>     listSerializableDummyClass;

		//[HideInInspector]
		//public DummyStruct              hiddenDummyStruct;
		//[HideInInspector]
		//public DummyClass               hiddenDummyClass;
		//[HideInInspector]
		//public SerializableDummyStruct  hiddenSerializableDummyStruct;
		//[HideInInspector]
		//public SerializableDummyClass   hiddenSerializableDummyClass;


		//public AnimationCurve   curve;

		//public FlagsDummyEnum       flagsDummyEnum;
		//public DummyEnum            dummyEnum;
		//public KeyCode              keyEnum;

		//public IList			list;
		//public List<GameObject> listGameObj;
		//public List<Object>     listObj;
		//public List<Transform>  listTran;
		//public List<Material>	listMat;
		//public List<Shader>		listShad;
		//public List<Projector>	listProjector;
		//public List<Light>		listLighto;

		//public List<Int16>		listInt16;
		//public List<Int32>		listInt32;
		//public List<Int64>		listInt64;
		//public List<UInt16>		listUint16;
		//public List<UInt32>		listUint32;
		//public List<UInt64>		listUint64;
		//public List<Byte>		listFByte;
		//public List<SByte>		listFSByte;
		//public List<Double>		listFDouble;
		//public List<Double>		listFDouble;
		//public List<Decimal>	listFDecimal;

		//public List<string>		listString;

		//public List<Vector2>	listVector2;
		//public List<Vector3>	listVector3;
		//public List<Vector4>	listVector4;
		//public List<Quaternion>	listQuat;
		//public List<Color>		listColor;
		//public List<Rect>		listRect;

		//public List<DummyStruct>	listDummyStruct;
		//public List<DummyClass>		listDummyClass;

		//public List<SerializableDummyStruct>	listSerializableDummyStruct;
		//public List<SerializableDummyClass>		listSerializableDummyClass;

		//[HideInInspector]
		//public List<DummyStruct>	listHiddenDummyStruct;
		//[HideInInspector]
		//public List<DummyClass>	listHiddenDummyClass;

		//public List<AnimationCurve>	listCurve;


		//public GameObject[]	arrayGameObj;
		//public Object[]     arrayObj;
		//public Transform[]  arrayTran;
		//public Material[]	arrayMat;
		//public Shader[]		arrayShad;
		//public Projector[]	arrayProjector;
		//public Light[]		arrayLighto;

		////public Int16[]		arrayInt16;
		//public Int32[]		arrayInt32;
		//public Int64[]		arrayInt64;
		////public UInt16[]		arrayUint16;
		////public UInt32[]		arrayUint32;
		////public UInt64[]		arrayUint64;
		//public Byte[]		arrayFByte;
		////public SByte[]		arrayFSByte;
		//public Double[]		arrayFDouble;
		////public Double[]		arrayFDouble;
		//public Decimal[]	arrayFDecimal;

		//public string[]		arrayString;

		////public Vector2[]	arrayVector2;
		//public Vector3[]	arrayVector3;
		//public Vector4[]	arrayVector4;
		////public Quaternion[]	arrayQuat;
		////public Color[]		arrayColor;
		//public Rect[]		arrayRect;

		////public DummyStruct[]	arrayDummyStruct;
		////public DummyClass[]		arrayDummyClass;

		//public SerializableDummyStruct[]	arraySerializableDummyStruct;
		//public SerializableDummyClass[]		arraySerializableDummyClass;

		////[HideInInspector]
		////public DummyStruct[]	arrayHiddenDummyStruct;
		////[HideInInspector]
		////public DummyClass[]	arrayHiddenDummyClass;

		//public AnimationCurve[]	arrayCurve;



		//public List<GameObject[]>	nestedArrayGameObj;
		//public List<Object[]>		nestedArrayObj;
		//public List<Transform[]>	nestedArrayTran;
		//public List<Material[]>		nestedArrayMat;
		//public List<Shader[]>		nestedArrayShad;
		//public List<Projector[]>	nestedArrayProjector;
		//public List<Light[]>		nestedArrayLighto;

		//public List<Int16[]>		nestedArrayInt16;
		//public List<Int32[]>		nestedArrayInt32;
		//public List<Int64[]>		nestedArrayInt64;
		//public List<UInt16[]>		nestedArrayUint16;
		//public List<UInt32[]>		nestedArrayUint32;
		//public List<UInt64[]>		nestedArrayUint64;
		//public List<Byte[]>		nestedArrayFByte;
		//public List<SByte[]>		nestedArrayFSByte;
		//public List<Double[]>		nestedArrayFDouble;
		//public List<Double[]>		nestedArrayFDouble;
		//public List<Decimal[]>	nestedArrayFDecimal;

		//public List<string[]>		nestedArrayString;

		//public List<Vector2[]>	nestedArrayVector2;
		//public List<Vector3[]>	nestedArrayVector3;
		//public List<Vector4[]>	nestedArrayVector4;
		//public List<Quaternion[]>	nestedArrayQuat;
		//public List<Color[]>		nestedArrayColor;
		//public List<Rect[]>		nestedArrayRect;

		//public List<DummyStruct[]>	nestedArrayDummyStruct;
		//public List<DummyClass[]>		nestedArrayDummyClass;

		//public List<SerializableDummyStruct[]>	nestedArraySerializableDummyStruct;
		//public List<SerializableDummyClass[]>		nestedArraySerializableDummyClass;

		//[HideInInspector]
		//public List<DummyStruct[]>	nestedArrayHiddenDummyStruct;
		//[HideInInspector]
		//public List<DummyClass[]>	nestedArrayHiddenDummyClass;

		//public List<AnimationCurve[]>	nestedArrayCurve;


		// Not supported
		//[Serializable]
		//public class test : List<GameObject>
		//{
		//	public int a;
		//}

		//public test	classArrayTest;
		//public test[]	classArrayArrayTest;

		public void	FnInt(int a)
		{
			Debug.Log(a);
		}
		public void	FnIntString(int a, string b)
		{
			Debug.Log(a);
			Debug.Log(b);
		}
		public void	FnIntStringFloat(int a, string b, float c)
		{
			Debug.Log(a);
			Debug.Log(b);
			Debug.Log(c);
		}
		public void	FnV3(Vector3 v)
		{
			Debug.Log(v);
		}
		public void	FnV3Int(Vector3 v, int a)
		{
			Debug.Log(v);
			Debug.Log(a);
		}
	}
}