﻿using System;

namespace Moonstorm
{
    /// <summary>
    /// When all the defs are registered, these ones are ignored and aren't added
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DisabledContent : Attribute
    {
    }
}
