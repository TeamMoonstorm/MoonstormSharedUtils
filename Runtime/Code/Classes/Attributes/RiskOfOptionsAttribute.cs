using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using BepInEx;

namespace Moonstorm
{
    public class RiskOfOptionsAttribute : Attribute
    {
        public Type OptionType { get; }
        public string[] Parameters { get; }
        private BepInPlugin tiedPlugin; 
        public RiskOfOptionsAttribute(Type optionType, params string[] constructorParams)
        {
            if(!optionType.IsSubclassOf(typeof(BaseOption)))
            {
                throw new ArgumentException($"Given option type ({optionType.Name}) does not inherit from BaseOption.");
            }
            OptionType = optionType;
            Parameters = constructorParams;
        }

        internal void ImplementOption<T>(ConfigEntry<T> configEntry, FieldInfo tiedfield)
        {
            GetTiedPlugin(tiedfield.DeclaringType.Assembly);

            ModSettingsManager.AddOption(null, tiedPlugin.GUID, tiedPlugin.Name);
        }

        private void GetTiedPlugin(Assembly assembly)
        {
            foreach(Type t in assembly.GetTypesSafe())
            {
                BepInPlugin bepInPlugin = t.GetCustomAttribute<BepInPlugin>();

                if(bepInPlugin == null)
                {
                    continue;
                }

                tiedPlugin = bepInPlugin;
                return;
            }
        }
    }
}
