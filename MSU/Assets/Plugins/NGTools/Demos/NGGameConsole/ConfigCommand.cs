using UnityEngine;

namespace NGTools.Tests
{
	using NGTools.NGGameConsole;

	public class ConfigCommand : MonoBehaviour
	{
		[Command("IntMin20", ""), Min(20)]
		public int a { get; set; }
		[Command("IntSet", ""), Set(1, 2, 98, -65)]
		public int b { get; set; }
		[Command("Int", "")]
		public int c { get; set; }
		[Command("EnumKeyCode", "")]
		public KeyCode d { get; set; }
		[Command("Float1", "")]
		public float f { get; set; }
		[Command("AnotherFloat", "")]
		public float g { get; set; }
		[Command("Decimal", "")]
		public decimal h { get; set; }
		[Command("AnotherDecimal", "")]
		public decimal i { get; set; }
		[Command("String", "")]
		public string j { get; set; }
		[Command("Bool", "")]
		public bool	k { get; set; }
		[Command("Byte", "")]
		public byte	l { get; set; }
		[Command("SByte", "")]
		public sbyte m { get; set; }
	}
}