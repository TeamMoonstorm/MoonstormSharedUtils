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
        public CharacterModel model;

        private GameObject effectInstance;
        private MSEliteDef elite;
        private Texture oldRamp;
        public void Start()
        {
            model = body.modelLocator.modelTransform.GetComponent<CharacterModel>();
        }

        public void SetNewElite(MSEliteDef eliteDef)
        {
            if (model && eliteDef != elite)
            {
                oldRamp = elite?.eliteRamp;
                elite = eliteDef;
                //this only gets executed if an elite def has already been loaded into the behavior
                if (!elite)
                {
                    if (model && model.propertyStorage != null)
                    {
                        model.propertyStorage.SetTexture(EliteRampPropertyID, Shader.GetGlobalTexture(EliteRampPropertyID));
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
            if (model && elite)
            {
                model.propertyStorage.SetTexture(EliteRampPropertyID, elite.eliteRamp);
            }
            else if (model)
            {
                if (!oldRamp)
                    return;

                if (model.propertyStorage.GetTexture(EliteRampPropertyID) == oldRamp)
                {
                    model.propertyStorage.Clear();
                }
            }
        }
    }
}