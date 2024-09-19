using RoR2.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MSU.Editor
{
    public class ShaderDictionaryProvider : SettingsProvider
    {
        private ShaderDictionary dictionary;
        private SerializedObject serializedObject;

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var keywords = new[] { "MSU" };
            VisualElementTemplateDictionary.instance.DoSave();
            var dictionary = ShaderDictionary.instance;
            dictionary.hideFlags = UnityEngine.HideFlags.DontSave | UnityEngine.HideFlags.HideInHierarchy;
            dictionary.DoSave();
            return new ShaderDictionaryProvider("Project/Shader Dictionary", SettingsScope.Project, keywords)
            {
                serializedObject = new SerializedObject(dictionary),
                dictionary = dictionary
            };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(ShaderDictionary), rootElement, p => p.ValidateUXMLPath());

            rootElement.Q<Button>("AddDefaultStubbedShaders").clicked += dictionary.AddDefaultStubbeds;
            rootElement.Q<Button>("AttemptToFindMissingKeys").clicked += dictionary.AttemptToFinishDictionaryAutomatically;
            rootElement.Q<Button>("ReloadInternalDictionary").clicked += dictionary.ReloadDictionaries;
            rootElement.Q<Button>("Save").clicked += Save;
            rootElement.Bind(serializedObject);
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            serializedObject?.ApplyModifiedProperties();
            Save();
        }

        private void Save()
        {
            serializedObject?.ApplyModifiedProperties();
            if (dictionary)
            {
                dictionary.DoSave();
            }
        }
        public ShaderDictionaryProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
    }
}