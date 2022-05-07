using RoR2EditorKit.Common;
using RoR2EditorKit.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.EditorWindows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;
    /// <summary>
    /// Base window for creating an EditorWindow with visual elements.
    /// </summary>
    public abstract class ExtendedEditorWindow : EditorWindow
    {
        /// <summary>
        /// RoR2EK's main settings file
        /// </summary>
        public static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }
        /// <summary>
        /// The serialized object of this EditorWindow
        /// </summary>
        protected SerializedObject SerializedObject { get; private set; }

        /// <summary>
        /// Called when the Editor Window is enabled, always keep the original implementation unless you know what youre doing
        /// </summary>
        protected virtual void OnEnable()
        {
            base.rootVisualElement.Clear();
            GetTemplateInstance(GetType().Name, rootVisualElement, ValidateUXMLPath);
            SerializedObject = new SerializedObject(this);
            rootVisualElement.Bind(SerializedObject);
        }

        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making a window that isnt in the same assembly as RoR2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this editor window, false otherwise</returns>
        protected virtual bool ValidateUXMLPath(string path)
        {
            return path.StartsWith(Constants.AssetFolderPath) || path.StartsWith(Constants.AssetFolderPath);
        }

        private void CreateGUI()
        {
            DrawGUI();
        }

        /// <summary>
        /// Create or finalize your VisualElement UI here.
        /// </summary>
        protected abstract void DrawGUI();

        #region Util Methods
        /// <summary>
        /// Shorthand for finding a visual element. the element you're requesting will be queried on the rootVisualElement.
        /// </summary>
        /// <typeparam name="TElement">The type of visual element.</typeparam>
        /// <param name="name">Optional parameter to find the element</param>
        /// <param name="ussClass">Optional parameter to find the element</param>
        /// <returns>The VisualElement specified</returns>
        protected TElement Find<TElement>(string name = null, string ussClass = null) where TElement : VisualElement
        {
            return rootVisualElement.Q<TElement>(name, ussClass);
        }

        /// <summary>
        /// Shorthand for finding a visual element. the element you're requesting will be queried on the "elementToSearch"
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement</typeparam>
        /// <param name="elementToSearch">The VisualElement where the Quering process will be done.</param>
        /// <param name="name">Optional parameter to find the element</param>
        /// <param name="ussClass">Optional parameter to find the element</param>
        /// <returns>The VisualElement specified</returns>
        protected TElement Find<TElement>(VisualElement elementToSearch, string name = null, string ussClass = null) where TElement : VisualElement
        {
            return elementToSearch.Q<TElement>(name, ussClass);
        }
        /// <summary>
        /// Queries a visual element of type T from the rootVisualElement, and binds it to a property on the serialized object.
        /// <para>Property is found by using the Element's name as the binding path</para>
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement, must inherit IBindable</typeparam>
        /// <param name="name">Optional parameter to find the Element, used in the Quering</param>
        /// <param name="ussClass">Optional parameter of the name of a USSClass the element youre finding uses</param>
        /// <returns>The VisualElement specified, with a binding to the property</returns>
        protected TElement FindAndBind<TElement>(string name = null, string ussClass = null) where TElement : VisualElement, IBindable
        {
            var bindableElement = rootVisualElement.Q<TElement>(name, ussClass);
            if (bindableElement == null)
                throw new NullReferenceException($"Could not find element of type {typeof(TElement)} inside the DrawInspectorElement.");

            bindableElement.bindingPath = bindableElement.name;
            bindableElement.BindProperty(SerializedObject);

            return bindableElement;
        }

        /// <summary>
        /// Queries a visual element of type T from the rootVisualElement, and binds it to a property on the serialized object.
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement, must inherit IBindable</typeparam>
        /// <param name="prop">The property which is used in the Binding process</param>
        /// <param name="name">Optional parameter to find the Element, used in the Quering</param>
        /// <param name="ussClass">Optional parameter of the name of a USSClass the element youre finding uses</param>
        /// <returns>The VisualElement specified, with a binding to the property</returns>
        protected TElement FindAndBind<TElement>(SerializedProperty prop, string name = null, string ussClass = null) where TElement : VisualElement, IBindable
        {
            var bindableElement = rootVisualElement.Q<TElement>(name, ussClass);
            if (bindableElement == null)
                throw new NullReferenceException($"Could not find element of type {typeof(TElement)} inside the DrawInspectorElement.");

            bindableElement.BindProperty(prop);

            return bindableElement;
        }

        /// <summary>
        /// Queries a visual element of type T from the elementToSearch argument, and binds it to a property on the serialized object.
        /// <para>Property is found by using the Element's name as the binding path</para>
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement, must inherit IBindable</typeparam>
        /// <param name="elementToSearch">The VisualElement where the Quering process will be done.</param>
        /// <param name="name">Optional parameter to find the Element, used in the Quering</param>
        /// <param name="ussClass">The name of a USSClass the element youre finding uses</param>
        /// <returns>The VisualElement specified, with a binding to the property</returns>
        protected TElement FindAndBind<TElement>(VisualElement elementToSearch, string name = null, string ussClass = null) where TElement : VisualElement, IBindable
        {
            var bindableElement = elementToSearch.Q<TElement>(name, ussClass);
            if (bindableElement == null)
                throw new NullReferenceException($"Could not find element of type {typeof(TElement)} inside element {elementToSearch.name}.");

            bindableElement.bindingPath = bindableElement.name;
            bindableElement.BindProperty(SerializedObject);

            return bindableElement;
        }

        /// <summary>
        /// Queries a visual element of type T from the elementToSearch argument, and binds it to a property on the serialized object.
        /// <para>Property is found by using the Element's name as the binding path</para>
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement, must inherit IBindable</typeparam>
        /// <param name="elementToSearch">The VisualElement where the Quering process will be done.</param>
        /// <param name="name">Optional parameter to find the Element, used in the Quering</param>
        /// <param name="ussClass">The name of a USSClass the element youre finding uses</param>
        /// <returns>The VisualElement specified, with a binding to the property</returns>
        protected TElement FindAndBind<TElement>(VisualElement elementToSearch, SerializedProperty prop, string name = null, string ussClass = null) where TElement : VisualElement, IBindable
        {
            var bindableElement = elementToSearch.Q<TElement>(name, ussClass);
            if (bindableElement == null)
                throw new NullReferenceException($"Could not find element of type {typeof(TElement)} inside element {elementToSearch.name}.");

            bindableElement.BindProperty(prop);

            return bindableElement;
        }

        /// <summary>
        /// Creates a HelpBox and attatches it to a visualElement using IMGUIContainer
        /// </summary>
        /// <param name="message">The message that'll appear on the help box</param>
        /// <param name="messageType">The type of message</param>
        /// <param name="attachToRootIfElementToAttachIsNull">If left true, and the elementToAttach is not null, the IMGUIContainer is added to the RootVisualElement.</param>
        /// <param name="elementToAttach">Optional, if specified, the Container will be added to this element, otherwise if the "attachToRootIfElementToAttachIsNull" is true, it'll attach it to the RootVisualElement, otherwise if both those conditions fail, it returns the IMGUIContainer unattached.</param>
        /// <returns>An IMGUIContainer that's either not attached to anything, attached to the RootElement, or attached to the elementToAttach argument.</returns>
        protected IMGUIContainer CreateHelpBox(string message, MessageType messageType)
        {
            IMGUIContainer container = new IMGUIContainer();
            container.name = $"EditorWindow_HelpBox";
            container.onGUIHandler = () =>
            {
                EditorGUILayout.HelpBox(message, messageType);
            };

            return container;
        }
        #endregion
    }
}