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
