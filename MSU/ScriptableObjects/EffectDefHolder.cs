using RoR2;
using UnityEngine;

namespace Moonstorm
{
    //EffectDefs aren't scriptable Objects atm so this is a workaround
    [CreateAssetMenu(fileName = "New EffectDef Holder", menuName = "Moonstorm/EffectDef Holder", order = 2)]
    public class EffectDefHolder : ScriptableObject
    {
        public GameObject[] effectPrefabs;

        public static EffectDef ToEffectDef(GameObject effect)
        {
            return new EffectDef(effect);
        }

    }
}