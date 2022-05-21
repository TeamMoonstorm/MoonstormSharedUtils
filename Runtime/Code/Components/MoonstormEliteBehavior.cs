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
        private Texture oldRamp;

        public void SetNewElite(MSEliteDef eliteDef)
        {
            if (eliteDef != elite)
            {
                oldRamp = elite?.eliteRamp;
                elite = eliteDef;
                //this only gets executed if an elite def has already been loaded into the behavior
                if (!elite)
                {
                    if (characterModel && characterModel.propertyStorage != null)
                    {
                        characterModel.propertyStorage.SetTexture(EliteRampPropertyID, Shader.GetGlobalTexture(EliteRampPropertyID));
                    }
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

        public void UpdateShaderRamp()
        {
            if (characterModel && elite)
            {
                characterModel.propertyStorage.SetTexture(EliteRampPropertyID, elite.eliteRamp);
            }
            else if (characterModel)
            {
                if (!oldRamp)
                    return;

                if (characterModel.propertyStorage.GetTexture(EliteRampPropertyID) == oldRamp)
                {
                    characterModel.propertyStorage.Clear();
                }
            }
        }
    }
}