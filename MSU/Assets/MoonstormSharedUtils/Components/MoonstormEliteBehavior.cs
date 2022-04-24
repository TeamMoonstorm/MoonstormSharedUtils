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

        public CharacterBody Body { get; internal set; }
        public CharacterModel CharacterModel { get; internal set; }

        private GameObject effectInstance;
        private MSEliteDef elite;
        private Texture oldRamp;

        public void Start()
        {
            MSULog.Info($"GameObject {gameObject} (MoonstormEliteBehavior):" +
                $"\nBody: {Body}" +
                $"\nEliteBehavior: {CharacterModel}");
        }
        public void SetNewElite(MSEliteDef eliteDef)
        {
            if (eliteDef != elite)
            {
                oldRamp = elite?.eliteRamp;
                elite = eliteDef;
                //this only gets executed if an elite def has already been loaded into the behavior
                if (!elite)
                {
                    if (CharacterModel && CharacterModel.propertyStorage != null)
                    {
                        CharacterModel.propertyStorage.SetTexture(EliteRampPropertyID, Shader.GetGlobalTexture(EliteRampPropertyID));
                    }
                    if (effectInstance)
                        Destroy(effectInstance);
                }
                if (elite)
                {
                    if (elite.effect)
                        effectInstance = Instantiate(elite.effect, Body.aimOriginTransform, false);
                }
            }
        }

        public void UpdateShaderRamp()
        {
            if (CharacterModel && elite)
            {
                CharacterModel.propertyStorage.SetTexture(EliteRampPropertyID, elite.eliteRamp);
            }
            else if (CharacterModel)
            {
                if (!oldRamp)
                    return;

                if (CharacterModel.propertyStorage.GetTexture(EliteRampPropertyID) == oldRamp)
                {
                    CharacterModel.propertyStorage.Clear();
                }
            }
        }
    }
}