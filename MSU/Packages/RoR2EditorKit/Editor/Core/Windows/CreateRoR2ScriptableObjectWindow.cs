using System.Linq;
using UnityEngine;

namespace RoR2EditorKit.Core.Windows
{
    /// <summary>
    /// Class that's used for creating ScriptableObjects for the editor
    /// </summary>
    /// <typeparam name="T">The type of scriptable object to create</typeparam>
    public abstract class CreateRoR2ScriptableObjectWindow<T> : ExtendedEditorWindow where T : ScriptableObject
    {
        public T ScriptableObject { get; private set; }

        protected string nameField;
        protected string actualName;
        protected override void OnWindowOpened()
        {
            ScriptableObject = UnityEngine.ScriptableObject.CreateInstance<T>();
            nameField = string.Empty;
            actualName = string.Empty;
        }

        /// <summary>
        /// Turns the given string into DoubleCamelCase
        /// </summary>
        /// <param name="name">the string to modify</param>
        /// <returns>The string in DoubleCamelCase</returns>
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

        /// <summary>
        /// Attempts to close the window
        /// </summary>
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
