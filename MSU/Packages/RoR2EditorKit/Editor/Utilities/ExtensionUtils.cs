using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RoR2EditorKit.Utilities
{
    /// <summary>
    /// Class holding a multitude of extension methods.
    /// </summary>
    public static class ExtensionUtils
    {
        #region String Extensions
        /// <summary>
        /// Ensures that the string object is not Null, Empty or WhiteSpace.
        /// </summary>
        /// <param name="text">The string object to check</param>
        /// <returns>True if the string object is not Null, Empty or Whitespace, false otherwise.</returns>
        public static bool IsNullOrEmptyOrWhitespace(this string text)
        {
            return (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text));
        }
        #endregion

        #region SerializedProperties/Objects  Extensions
        /// <summary>
        /// Returns the serialized property that's bound to this ObjectField.
        /// </summary>
        /// <param name="objField">The objectField that has a bounded property</param>
        /// <param name="objectBound">The SerializedObject that has the objectField's property binding path.</param>
        /// <returns>The serialized property</returns>
        /// <exception cref="NullReferenceException">when the objField does not have a bindingPath set.</exception>
        public static SerializedProperty GetBindedProperty(this ObjectField objField, SerializedObject objectBound)
        {
            if (objField.bindingPath.IsNullOrEmptyOrWhitespace())
                throw new NullReferenceException($"{objField} does not have a bindingPath set");

            return objectBound.FindProperty(objField.bindingPath);
        }

        /// <summary>
        /// Obtains a List of all the top layer serialized properties from a serialized object.
        /// </summary>
        /// <param name="serializedObject">The serialized object to get the children</param>
        /// <returns>A List of all the top layer serialized properties</returns>
        public static List<SerializedProperty> GetVisibleChildren(this SerializedObject serializedObject)
        {
            List<SerializedProperty> list = new List<SerializedProperty>();
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                {
                    list.Add(serializedObject.FindProperty(iterator.propertyPath));
                }

                enterChildren = false;
            }
            return list;
        }
        #endregion

        #region Visual Element Extensions
        /// <summary>
        /// Quick method to set the ObjectField's object type
        /// </summary>
        /// <typeparam name="TObj">The type of object to set</typeparam>
        /// <param name="objField">The object field</param>
        public static void SetObjectType<T>(this ObjectField objField) where T : UnityEngine.Object
        {
            objField.objectType = typeof(T);
        }

        /// <summary>
        /// Quick method to Clear a visual element's USS Class List, Hierarchy, and Unbind it from a serializedObject
        /// </summary>
        public static void Wipe(this VisualElement visualElement)
        {
            visualElement.Clear();
            visualElement.ClearClassList();
            visualElement.Unbind();
        }
        #endregion
    }
}
