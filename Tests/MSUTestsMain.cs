using BepInEx;
using R2API;
using R2API.ScriptableObjects;
using R2API.Utils;
using R2API.ContentManagement;
using UnityEngine;
using System.Security;
using System.Security.Permissions;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[module: UnverifiableCode]
namespace Moonstorm
{
	[BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
	[BepInPlugin(GUID, MODNAME, VERSION)]
	public class MSUTestsMain : BaseUnityPlugin
	{
		public const string GUID = "com.TeamMoonstorm.MSU.Tests";
		public const string MODNAME = "MoonstormSharedUtils Tests";
		public const string VERSION = "1.0.0";

		public static MSUTestsMain Instance { get; private set; }
		public static PluginInfo PluginInfo { get; private set; }
		private void Awake()
		{
			Instance = this;
			PluginInfo = Info;
			new MSUTLog(Logger);
			MSUTLog.Info("Mod Awake, initializing loaders.");
			new MSUTAssets().Init();
			new MSUTConfig().Init();
			new MSUTContent().Init();
			new MSUTLanguage().Init();
			MSUTLog.Info("Loaders initialized, adding mod to MSU managers");
			ConfigurableFieldManager.AddMod(this);
			TokenModifierManager.AddToManager();
			MSUTLog.Info("Finalized Awake.");
		}	
	}
}