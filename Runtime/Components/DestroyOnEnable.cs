using UnityEngine;

namespace Moonstorm.Components
{
    /// <summary>
    /// Destroys the game object attatched when enabled
    /// </summary>
    public class DestroyOnEnable : MonoBehaviour
    {
        private void OnEnable()
        {
            Destroy(gameObject);
        }
    }
}
