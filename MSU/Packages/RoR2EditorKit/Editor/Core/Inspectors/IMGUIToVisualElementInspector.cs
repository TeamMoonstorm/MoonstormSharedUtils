using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using RoR2;
using RoR2.Skills;
using RoR2EditorKit.Utilities;
using UnityEngine;

namespace RoR2EditorKit.Core.Inspectors
{
    /// <summary>
    /// Inherit from this inspector to make an Editor that looks exactly like the default inspector, but uses UIElements.
    /// <para>Perfect for later on creating a property drawer for a specific property in said inspector, so that you dont have to rewrite the original inspector's functionality.</para>
    /// <para>Unlike the other Editor wrappers from RoR2EditorKit, this editor cannot be enabled or disabled.</para>
    /// <para>The child elements that get added to this RootVisualElement can be identifier by their name</para>
    /// <para>The m_Script property is an ObjectField, it's name is "m_Script"</para>
    /// <para>All other first level serializedProperties are drawn with PropertyFields, their names are the same as their property names.</para>
    /// </summary>
    /// <typeparam name="T">The type of object being inspected</typeparam>
    public abstract class IMGUIToVisualElementInspector<T> : Editor where T : UnityEngine.Object
    {
        /// <summary>
        /// The Editor's RootVisualElement.
        /// <para>It's name is a combination of the inheriting type's name plus "_RootElement"</para>
        /// <para>Example: MyInspector_RootElement</para>
        /// </summary>
        protected VisualElement RootVisualElement
        {
            get
            {
                if(_visualElement == null)
                {
                    _visualElement = new VisualElement();
                    _visualElement.name = $"{GetType().Name}_RootElement";
                }
                return _visualElement;
            }
        }
        private VisualElement _visualElement;

        /// <summary>
        /// Direct access to the object that's being inspected as its type
        /// </summary>
        protected T TargetType => target as T;

        /// <summary>
        /// Cannot be overwritten, creates the inspector gui using the serialized object's visible children and property fields
        /// <para>If you want to draw extra visual elements, write them in the <see cref="FinishGUI"/> method</para>
        /// </summary>
        /// <returns>The <see cref="RootVisualElement"/> property</returns>
        public sealed override VisualElement CreateInspectorGUI()
        {
            var children = serializedObject.GetVisibleChildren();
            foreach(var child in children)
            {
                if(child.name == "m_Script")
                {
                    ObjectField objField = new ObjectField();
                    objField.SetObjectType<MonoScript>();
                    objField.value = child.objectReferenceValue;
                    objField.label = child.displayName;
                    objField.name = child.name;
                    objField.bindingPath = child.propertyPath;
                    objField.SetEnabled(false);
                    RootVisualElement.Add(objField);
                    continue;
                }

                PropertyField propField = new PropertyField(child);
                propField.name = child.name;
                RootVisualElement.Add(new PropertyField(child));
            }
            FinishGUI();
            return RootVisualElement;
        }

        /// <summary>
        /// Override this method to finish the implementation of your GUI by modifying the RootVisualElement
        /// </summary>
        protected virtual void FinishGUI() { }
    }
}