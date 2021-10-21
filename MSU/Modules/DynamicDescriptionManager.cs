using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moonstorm
{
    public static class DynamicDescriptionManager
    {
        private static List<Type> types = new List<Type>();

        [SystemInitializer()]
        private static void Init()
        {
            //This needs to be a hook, otherwise when the language changes it wont update with teh correct values.
            On.RoR2.Language.LoadStrings += (orig, self) =>
            {
                orig(self);
                CreateDynamicDescriptions();
            };
        }

        public static void AddMod(Assembly assembly)
        {
            var ts = assembly.GetTypes().Where(type => type.GetCustomAttribute<DynamicDescription>() != null);

            foreach(Type type in ts)
            {
                try
                {
                    if(!types.Contains(type))
                    {
                        if(type.IsSubclassOf(typeof(ItemBase)))
                        {
                            types.Add(type);
                        }
                        else if(type.IsSubclassOf(typeof(EquipmentBase)) && !type.IsSubclassOf(typeof(EliteEquipmentBase)))
                        {
                            types.Add(type);
                        }
                        else if(type.IsSubclassOf(typeof(EliteEquipmentBase)))
                        {
                            types.Add(type);
                        }
                        else
                        {
                            MSULog.LogI($"Type {type} does not inherit from ItemBase, EquipmentBase or EliteEquipmentBase.");
                        }
                    }
                }
                catch (Exception e)
                {
                    MSULog.LogE($"An Exception has Ocurred: {e}");
                }
            }
        }


        private static void CreateDynamicDescriptions()
        {
            MSULog.LogI($"Checking if there's need for creating Dynamic Item/Equip Descriptions.");
            if(types.Count > 0)
            {
                MSULog.LogI($"Creating Dynamic Descriptions for {types.Count} types.");
                foreach(Type type in types)
                {
                    if (type.IsSubclassOf(typeof(ItemBase)))
                    {
                        foreach (var kvp in PickupModuleBase.MoonstormItems)
                        {
                            var itemDef = kvp.Key;
                            var itemBase = kvp.Value;
                            var dynamicDesc = type.GetCustomAttribute<DynamicDescription>();

                            if (itemBase.GetType() == type)
                            {
                                DynamicItemDesc(type, dynamicDesc, itemDef);
                            }
                        }
                    }
                    else if (type.IsSubclassOf(typeof(EquipmentBase)) && !type.IsSubclassOf(typeof(EliteEquipmentBase)))
                    {
                        foreach (var kvp in PickupModuleBase.MoonstormNonEliteEquipments)
                        {
                            var eqpDef = kvp.Key;
                            var eqpBase = kvp.Value;
                            var dynamicDesc = type.GetCustomAttribute<DynamicDescription>();

                            if (eqpBase.GetType() == type)
                            {
                                DynamicEqpDesc(type, dynamicDesc, eqpDef);
                            }
                        }
                    }
                    else if (type.IsSubclassOf(typeof(EliteEquipmentBase)))
                    {
                        foreach (var kvp in PickupModuleBase.MoonstormNonEliteEquipments)
                        {
                            var eqpDef = kvp.Key;
                            var eliteEqpBase = kvp.Value;
                            var dynamicDesc = type.GetCustomAttribute<DynamicDescription>();

                            if (eliteEqpBase.GetType() == type)
                            {
                                DynamicEqpDesc(type, dynamicDesc, eqpDef);
                            }
                        }
                    }
                }
            }
        }

        private static void DynamicItemDesc(Type type, DynamicDescription dynamicDesc, ItemDef itemDef)
        {
            MSULog.LogD($"Creating Description token for {itemDef.descriptionToken}");
            string key = itemDef.descriptionToken;
            foreach (Language lang in Language.GetAllLanguages())
            {
                if(lang.stringsByToken.ContainsKey(key))
                {
                    if(lang.stringsByToken.TryGetValue(key, out string value))
                    {
                        object[] formatting = dynamicDesc.GetFormatting(type);

                        MSULog.LogE($"Original value: {value}");

                        var formatted = string.Format(value, formatting);

                        MSULog.LogE($"New Value: {formatted}");

                        lang.stringsByToken[key] = formatted;
                    }
                }
            }
        }

        private static void DynamicEqpDesc(Type type, DynamicDescription dynamicDesc, EquipmentDef eqpDef)
        {
            MSULog.LogD($"Creating Description token for {eqpDef.descriptionToken}");
            string key = eqpDef.descriptionToken;
            foreach(Language lang in Language.GetAllLanguages())
            {
                if(lang.stringsByToken.ContainsKey(key))
                {
                    if(lang.stringsByToken.TryGetValue(key, out string value))
                    {
                        object[] formatting = dynamicDesc.GetFormatting(type);

                        lang.stringsByToken[key] = string.Format(value, formatting);
                    }
                }
            }
        }
    }
}
