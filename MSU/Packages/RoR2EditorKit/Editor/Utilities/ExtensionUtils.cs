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
                throw new NullReferenceException($"{objField} doesnot have a bindingPath set");

            return objectBound.FindProperty(objField.bindingPath);
        }

        /// <summary>
        /// Returns an IEnumerable of all the visible serialized properties based off the SerializedObject's Iterator.
        /// </summary>
        /// <param name="serializedProperty">The serialized property obtained from ScriptableObject.GetIterator()</param>
        /// <returns>An IEnumerable of all the visible children</returns>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty;
                }
                while (currentProperty.NextVisible(false));
            }
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
        #endregion
    }
}
