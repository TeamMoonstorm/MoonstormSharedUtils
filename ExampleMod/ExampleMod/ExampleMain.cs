using BepInEx;
using MSU;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExampleMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ExampleMain : BaseUnityPlugin
    {

        //MAKE SURE TO CHANGE THE NAME OF YOUR EXAMPLE MOD, MSU WILL FLAT OUT IGNORE THIS GUID ON IT'S FRAMEWORKS.
        public const string GUID = "com.TeamName.MSUExampleMod";
        public const string VERSION = "0.0.1";
        public const string NAME = "MSU Example Mod";


        internal static ExampleMain instance { get; private set; }

        private void Awake()
        {
            instance = this;
            new ExampleLogger(Logger);
            new ExampleConfig(this);
            new ExampleContent();

            LanguageFileLoader.AddLanguageFilesFromMod(this, "ExampleLanguage");
        }
    }
}