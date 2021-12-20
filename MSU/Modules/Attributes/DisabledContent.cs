using System;

namespace Moonstorm
{
    /// <summary>
    /// Attatch this attribute to a ContentBase inheriting class and MSU will ignore this class when initializing your content bases.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DisabledContent : Attribute
    {
    }
}
