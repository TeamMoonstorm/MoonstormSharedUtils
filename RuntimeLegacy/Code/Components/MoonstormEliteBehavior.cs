using RoR2;
using UnityEngine;

namespace Moonstorm.Components
{
    /// <summary>
    /// The EliteBehaviour used for managing the <see cref="MSEliteDef"/> for a body
    /// </summary>
    [RequireComponent(typeof(CharacterBody))]
    public class MoonstormEliteBehavior : MonoBehaviour
    {
        /// <summary>
        /// Shorthand for Shader.PropertyToID("_EliteRamp");
        /// </summary>
        public static int EliteRampPropertyID
        {
            get
            {
                return Shader.PropertyToID("_EliteRamp");
            }
        }

        [Tooltip("The body that's tied to this EliteBehaviour")]
        public CharacterBody body;
        [Tooltip("The CharacterModel that's tied to this EliteBehaviour")]
        public CharacterModel characterModel;

        private GameObject effectInstance;
        private MSEliteDef elite;

        /// <summary>
        /// Sets a new EliteDef, changing the visual effect of the elite
        /// </summary>
        /// <param name="eliteDef">The new EliteDef, can be null</param>
        public void SetNewElite(MSEliteDef eliteDef)
        {
            if (eliteDef != elite)
            {
                elite = eliteDef;
                //this only gets executed if an elite def has already been loaded into the behavior
                if (!elite)
                {
                    if (effectInstance)
                        Destroy(effectInstance);
                }
                if (elite)
                {
                    if (elite.effect)
                        effectInstance = Instantiate(elite.effect, body.aimOriginTransform, false);
                }
            }
        }
    }
}