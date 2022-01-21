using RoR2;

namespace Moonstorm
{
    /// <summary>
    /// A content class for initializing and handling scenes
    /// </summary>
    public abstract class SceneBase : ContentBase
    {
        /// <summary>
        /// Your Scene's SceneDef. It also works for SceneDefinitions from ROS.
        /// </summary>
        public abstract SceneDef SceneDef { get; set; }
    }
}
