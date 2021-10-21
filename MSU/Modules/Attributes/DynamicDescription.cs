using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    /// <summary>
    /// Attribute attached to a class that contains a field of type ItemDef or EquipmentDef, used for creating a dynamic description token.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DynamicDescription : Attribute
    {
        /// <summary>
        /// The Arguments to use in the string formatting, must be the name of the fields that are public & static.
        /// </summary>
        public string[] Arguments { get; set; }

        /// <summary>
        /// Get the formatting for the description
        /// </summary>
        /// <returns>An array with the objects ready for formatting</returns>
        internal object[] GetFormatting(Type type)
        {
            List<object> objList = new List<object>();

            if(Arguments != null)
            {
                foreach(string arg in Arguments)
                {
                    objList.Add(type.GetField(arg).GetValue(null));
                }
            }

            return objList.ToArray();
        }
    }
}
