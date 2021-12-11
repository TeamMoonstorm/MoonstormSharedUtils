namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing an Elite Equipment
    /// <para>Inherits from EquipmentBase</para>
    /// </summary>
    public abstract class EliteEquipmentBase : EquipmentBase
    {
        /// <summary>
        /// Your Elite's MSEliteDef
        /// </summary>
        public abstract MSEliteDef EliteDef { get; set; }

        /// <summary>
        /// Initialize your Elite Equipment.
        /// <para>calling base.Initialize() heavily reccomended.</para>
        /// </summary>
        public override void Initialize()
        {
        }
    }
}
