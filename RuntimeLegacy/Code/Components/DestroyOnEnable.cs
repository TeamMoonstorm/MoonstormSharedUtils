using System;
using UnityEngine;

namespace Moonstorm.Components
{
    [Obsolete]
    public class DestroyOnEnable : MonoBehaviour
    {
        private void OnEnable()
        {
            Destroy(gameObject);
        }
    }
}
