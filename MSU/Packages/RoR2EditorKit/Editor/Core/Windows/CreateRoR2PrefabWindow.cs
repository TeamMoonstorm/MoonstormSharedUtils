using RoR2EditorKit.Settings;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.Windows
{
    public abstract class CreateRoR2PrefabWindow<T> : ExtendedEditorWindow where T : MonoBehaviour
    {
        public T MainComponent { get; private set; }
        protected SerializedObject serializedComponent;

        public RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }

        protected GameObject mainPrefab { get; private set; }

        protected string nameField;
        protected string actualName;

        protected override void OnWindowOpened()
        {
            mainPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            MainComponent = mainPrefab.AddComponent<T>();
            serializedComponent = new SerializedObject(MainComponent);
            nameField = string.Empty;
            actualName = string.Empty;
        }

        protected string GetCorrectAssetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            if (name.Contains(' '))
            {
                string[] strings = name.Split(' ');

                for (int i = 0; i < strings.Length; i++)
                {
                    strings[i] = char.ToUpper(strings[i][0]) + strings[i].Substring(1);
                }
                name = string.Join("", strings);
            }
            if (char.IsLower(name[0]))
            {
                name = char.ToUpper(name[0]) + name.Substring(1);
            }
            return name;
        }

        protected void TryToClose()
        {
            if (Settings.CloseWindowWhenAssetIsCreated)
                Close();
            else
            {
                OnWindowOpened();
            }
        }
    }
}