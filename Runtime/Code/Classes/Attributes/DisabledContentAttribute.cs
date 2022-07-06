using System;

namespace Moonstorm
{
    /// <summary>
    /// When <see cref="ModuleBase{T}.GetContentClasses{T}(Type)"/> gets called, the ModuleBase will not create any instances of ContentBases that have this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DisabledContentAttribute : Attribute
    {
    }
}
