using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moonstorm.Components
{
    public class DestroyOnEnable : MonoBehaviour
    {
        private void OnEnable()
        {
            Destroy(gameObject);
        }
    }
}
