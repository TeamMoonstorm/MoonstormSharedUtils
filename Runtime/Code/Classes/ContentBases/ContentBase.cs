namespace Moonstorm
{
    /// <summary>
    /// A class which all content bases derive from
    /// <para>A content base is a representation of a miscelaneous content piece from Risk of Rain 2 or general content piece</para>
    /// <para>Content bases are only representation of content, to load the content or have it initialize you must use the corresponding ModuleBase associated with the ContentBase</para>
    /// </summary>
    public abstract class ContentBase
    {
        /// <summary>
        /// Implement the initialization of your Content here
        /// </summary>
        public virtual void Initialize() { }
    }
}
