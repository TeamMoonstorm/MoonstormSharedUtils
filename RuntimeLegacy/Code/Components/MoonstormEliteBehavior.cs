using RoR2;
using UnityEngine;

namespace Moonstorm.Components
{
    [RequireComponent(typeof(CharacterBody))]
    public class MoonstormEliteBehavior : MonoBehaviour
    {
        public static int EliteRampPropertyID
        {
            get
            {
                return Shader.PropertyToID("_EliteRamp");
            }
        }

        public CharacterBody body;

        public CharacterModel characterModel;

        private GameObject effectInstance;

        private MSEliteDef elite;

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