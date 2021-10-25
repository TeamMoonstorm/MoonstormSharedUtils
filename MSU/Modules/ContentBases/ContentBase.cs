using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    /// <summary>
    /// A Class where all the other Content Base classes inherit from.
    /// </summary>
    public abstract class ContentBase
    {
        /// <summary>
        /// Initialize your content here.
        /// </summary>
        public virtual void Initialize() { }
    }
}
