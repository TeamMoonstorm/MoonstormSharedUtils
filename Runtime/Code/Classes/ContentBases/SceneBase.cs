using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents a <see cref="RoR2.SceneDef"/> for the game, the Scene is represented via the <see cref="SceneDef"/>
    /// <para>Its tied ModuleBase is the <see cref="SceneModuleBase"/></para>
    /// <para>Should also work with Rain of Stages' SceneDefinition</para>
    /// </summary>
    public abstract class SceneBase : ContentBase
    {
        /// <summary>
        /// The SceneDef associated with this SceneBase
        /// <para>Can also be a Rain of Stages SceneDefinition</para>
        /// </summary>
        public abstract SceneDef SceneDef { get; }
    }
}
