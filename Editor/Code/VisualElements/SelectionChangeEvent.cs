using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.VisualElements
{
    public class SelectionChangeEvent : EventBase<SelectionChangeEvent>
    {
        protected override void Init()
        {
            base.Init();
            PropertyInfo propagation = typeof(EventBase).GetProperty("propagation", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            propagation.GetSetMethod(true).Invoke(this, new object[] { 2 });
        }
    }
}
