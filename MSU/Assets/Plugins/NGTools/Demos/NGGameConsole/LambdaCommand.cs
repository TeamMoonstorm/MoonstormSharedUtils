using NGTools.NGGameConsole;
using UnityEngine;

namespace NGTools.Tests
{
	public class LambdaCommand : MonoBehaviour
	{
		public NGTools.NGGameConsole.NGGameConsole	console;

		[Command("instanceInteger", "")]
		public int a { get; set; }
		[Command("instanceString", "")]
		public string b { get; set; }
		[Command("instanceFloat", "")]
		public float c { get; set; }
		[Command("instanceBoolean", "")]
		public bool d { get; set; }
		[Command("staticDecimal", "")]
		public static decimal e { get; set; }
		[Command("staticInt", "")]
		public static int f { get; set; }
		[Command("staticString", "")]
		public static string g { get; set; }
		[Command("staticFunction", "")]
		public static string Fn1()
		{
			return "A lambda result.";
		}
		[Command("staticFunctionWithIntArgument", "")]
		public static string Fn2(int b)
		{
			return "Func(" + b + ")";
		}
		[Command("SetConsoleVisibility", "")]
		public string	consoleVisible(bool visible)
		{
			console.enabled = visible;
			return visible.ToString();
		}
	}
}