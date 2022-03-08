using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Moonstorm
{
    /// <summary>
    /// Class for managing TokenModifier attributes
    /// <para>TokenModifierManager runs after the ConfigurableFieldManager finishes configuring fields</para>
    /// </summary>
    public static class TokenModifierManager
    {
        private static bool initialized = false;

        //A single language token can be modified from multiple fields, and each field may contian multiple token modifiers.
        private static Dictionary<string, List<(FieldInfo, TokenModifier[])>> tokenToModifiers = new Dictionary<string, List<(FieldInfo, TokenModifier[])>>();

        [SystemInitializer()]
        private static void Init()
        {
            initialized = true;
            MSULog.Info($"Initializing TokenModifierManager");
            On.RoR2.Language.SetStringByToken += (orig, self, token, localizedString) =>
            {
                orig(self, token, localizedString);
                ModifyLocalizedString(self, token, localizedString);
            };
        }

        /// <summary>
        /// Adds a mod to the TokenModifier manager
        /// <para>Will automatically look for Types that have fields with TokenModifier attribute and prepare them for formatting</para>
        /// </summary>
        public static void AddToManager()
        {
            Assembly assembly = Assembly.GetCallingAssembly();

            if(initialized)
            {
                MSULog.Warning($"Cannot add {assembly.GetName().Name} to the Dictionary as the token modifier manager has already been initialized.");
                return;
            }

            MSULog.Info($"Adding mod {assembly.GetName().Name} to the token modifier manager");
            List<(FieldInfo, TokenModifier[])> allFieldsWithAttributes = new List<(FieldInfo, TokenModifier[])>();
            foreach (Type type in assembly.GetTypes().Where(type => type.GetCustomAttribute<DisabledContent>() == null))
            {
                try
                {
                    List<FieldInfo> fields = type.GetFields()
                                                 .Where(f => f.GetCustomAttributes<TokenModifier>().Count() != 0) //A field can have multiple modifiers.
                                                 .ToList();

                    foreach(FieldInfo field in fields)
                    {
                        try
                        {
                            var modifiers = field.GetCustomAttributes<TokenModifier>().ToArray();
                            if(modifiers.Length > 0)
                            {
                                allFieldsWithAttributes.Add((field, modifiers));
                            }

                        }
                        catch(Exception e) { MSULog.Error(e); }
                    }
                }
                catch (Exception e) { MSULog.Error(e); }
            }

            if(allFieldsWithAttributes.Count == 0)
            {
                MSULog.Warning($"Found no fields with the {nameof(TokenModifier)} attribute within {assembly.GetName().Name}");
                return;
            }

            MSULog.Debug($"Found a total of {allFieldsWithAttributes.Count} fields with the {nameof(TokenModifier)} attribute within {assembly.GetName().Name}.");
            foreach (var (field, attributes) in allFieldsWithAttributes)
            {
                foreach(TokenModifier modifier in attributes)
                {
                    var token = modifier.langToken;
                    if(!tokenToModifiers.ContainsKey(token))
                    {
                        tokenToModifiers.Add(token, new List<(FieldInfo, TokenModifier[])>());
                    }

                    var attributeMatch = attributes.Where(att => att.langToken == token).ToArray(); //A field can have an multiple attributes that modifies different tokens.

                    if (attributeMatch.Length == 0)
                        continue;

                    //If the value doesnt contain the field & their attributes, add them.
                    if(!tokenToModifiers[token].Contains((field, attributeMatch.ToArray())))
                    {
                        tokenToModifiers[token].Add((field, attributeMatch));
                    }
                }
            }
        }

        private static void ModifyLocalizedString(Language lang, string token, string localizedString)
        {
            if (!tokenToModifiers.ContainsKey(token))
                return;

            MSULog.Message($"Modifying token {token}");
            ModifyToken(lang, token, localizedString, tokenToModifiers[token]);
        }

        private static void ModifyToken(Language lang, string token, string localizedString, List<(FieldInfo, TokenModifier[])> modifiers)
        {
            try
            {
                object[] formatting = GetFormattingFromList(modifiers);
                lang.stringsByToken[token] = string.Format(localizedString, formatting);
            }
            catch (Exception e) { MSULog.Error(e); }
        }

        private static object[] GetFormattingFromList(List<(FieldInfo, TokenModifier[])> fieldAndModifiers)
        {
            object[] objectArray = new object[0];
            foreach (var (field, modifiers) in fieldAndModifiers)
            {
                try
                {
                    foreach (var modifier in modifiers)
                    {
                        try
                        {
                            (object value, int index) formattingTuple = modifier.GetFormatting(field);
                            if (formattingTuple.value != null)
                            {
                                if (objectArray.Length < formattingTuple.index + 1)
                                {
                                    Array.Resize(ref objectArray, formattingTuple.index + 1);
                                }
                                objectArray[formattingTuple.index] = formattingTuple.value;
                            }
                        }
                        catch(Exception e) { MSULog.Error(e); }
                    }
                }
                catch(Exception e) { MSULog.Error(e); }
            }
            return objectArray;
        }
    }
}
