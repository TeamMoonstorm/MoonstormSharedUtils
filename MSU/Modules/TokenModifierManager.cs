using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Moonstorm
{
    public static class TokenModifierManager
    {
        private static bool initialized = false;
        private static Dictionary<string, List<(FieldInfo, TokenModifier[])>> stringToModifiers = new Dictionary<string, List<(FieldInfo, TokenModifier[])>>();

        [SystemInitializer()]
        private static void Init()
        {
            initialized = true;
            MSULog.LogI($"Initializing TokenModifierManager");
            //This needs to be a hook, otherwise when the language changes it wont update with teh correct values.
            On.RoR2.Language.LoadStrings += (orig, self) =>
            {
                orig(self);
                ModifyTokensInLanguage(self);
            };

            RoR2Application.onLoad += () => ModifyTokensInLanguage(null);
        }

        public static void AddMod()
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            if (!initialized)
            {
                MSULog.LogI($"Adding mod {assembly.GetName().Name} to the token modifier manager");
                List<(FieldInfo, TokenModifier[])> allFieldsWithAttributes = new List<(FieldInfo, TokenModifier[])>();
                foreach (Type type in assembly.GetTypes())
                {
                    try
                    {
                        type.GetFields()
                            .Where(field => field.GetCustomAttributes<TokenModifier>().Count() != 0)
                            .ToList()
                            .ForEach(field =>
                            {
                                try
                                {
                                    //A field can have multiple token modifiers.
                                    var tokenModifiers = field.GetCustomAttributes<TokenModifier>().ToArray();
                                    if (tokenModifiers.Length > 0)
                                    {
                                        allFieldsWithAttributes.Add((field, tokenModifiers));
                                    }
                                }
                                catch (Exception e)
                                {
                                    MSULog.LogE($"An Exception has Ocurred: {e}");
                                }
                            });
                    }
                    catch (Exception e)
                    {
                        MSULog.LogE($"An Exception has Ocurred: {e}");
                    }
                }

                if (allFieldsWithAttributes.Count > 0)
                {
                    MSULog.LogD($"Found a total of {allFieldsWithAttributes.Count} fields with the {nameof(TokenModifier)} attribute within {assembly.GetName().Name}.");
                    foreach (var (field, attributes) in allFieldsWithAttributes)
                    {
                        foreach (TokenModifier attribute in attributes)
                        {

                            var currentToken = attribute.langToken;
                            //If the key doesnt exist, add a new one alongside empty list.
                            if (!stringToModifiers.ContainsKey(currentToken))
                            {
                                stringToModifiers.Add(currentToken, new List<(FieldInfo, TokenModifier[])>());
                            }

                            var attributesThatMatchToken = attributes.Where(tokenMod => tokenMod.langToken == currentToken);

                            //If the value doesnt contain the field & their attributes, add them.
                            if (attributesThatMatchToken.Count() > 0 && !stringToModifiers[attribute.langToken].Contains((field, attributesThatMatchToken.ToArray())))
                            {
                                stringToModifiers[attribute.langToken].Add((field, attributesThatMatchToken.ToArray()));
                            }
                        }
                    }
                }
                else
                {
                    MSULog.LogW($"Found no fields with the {nameof(TokenModifier)} attribute within {assembly.GetName().Name}");
                }
            }
            else
            {
                MSULog.LogW($"Cannot add {assembly.GetName().Name} to the Dictionary as the token modifier manager has already been initialized.");
            }
        }

        private static void ModifyTokensInLanguage(Language lang)
        {
            if (lang == null)
            {
                lang = Language.currentLanguage;
            }

            MSULog.LogI($"Checking if there's need for modifying tokens in language {lang.name}");
            if (stringToModifiers.Count > 0)
            {
                try
                {
                    MSULog.LogI($"Modifying a total of {stringToModifiers.Keys.Count} tokens.");
                    foreach (var kvp in stringToModifiers)
                    {
                        var key = kvp.Key;
                        var value = kvp.Value;

                        if (lang.stringsByToken.ContainsKey(key))
                        {
                            MSULog.LogD($"Modifying {key}");
                            ModifyToken(lang, key, value);
                        }
                        else
                        {
                            MSULog.LogW($"Token {key} could not be found in the stringsByToken dictionary in {lang.name}! Either the mod that implements the token doesnt support the language {lang.name} or theyre adding their tokens via R2Api's LanguageAPI");
                            continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    MSULog.LogE($"An Exception has Ocurred {e}");
                }
            }
            else
            {
                MSULog.LogI($"Dictionary Empty, no tokens are modified.");
            }
        }

        private static void ModifyToken(Language lang, string token, List<(FieldInfo, TokenModifier[])> modifiers)
        {
            try
            {
                if (lang.stringsByToken.TryGetValue(token, out string tokenValue))
                {
                    object[] formatting = GetFormattingFromList(modifiers);
                    if (formatting.Length != 0)
                    {
                        var formatted = string.Format(tokenValue, formatting);

                        lang.stringsByToken[token] = formatted;
                    }
                }
            }
            catch (Exception e)
            {
                MSULog.LogE($"An Exception has Ocurred {e}");
            }
        }

        private static object[] GetFormattingFromList(List<(FieldInfo, TokenModifier[])> fieldAndModifiers)
        {
            object[] objectArray = new object[0];
            foreach(var (field, modifiers) in fieldAndModifiers)
            {
                foreach(var modifier in modifiers)
                {
                    (object value, int index) formattingTuple = modifier.GetFormatting(field);
                    if(formattingTuple.value != null)
                    {
                        if(objectArray.Length < formattingTuple.index + 1)
                        {
                            Array.Resize(ref objectArray, formattingTuple.index + 1);
                        }
                        objectArray[formattingTuple.index] = formattingTuple.value;
                    }
                }
            }
            return objectArray;
        }
    }
}
