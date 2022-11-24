using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SearchableAttribute = HG.Reflection.SearchableAttribute;

namespace Moonstorm
{
    /// <summary>
    /// The TokenModifierManager is a class that handles the usage of <see cref="TokenModifierAttribute"/> in mods.
    /// </summary>
    public static class TokenModifierManager
    {
        //This is the finalized dictionary that gets created from the two above.
        private static Dictionary<string, object[]> cachedFormattingArray = null;
        [SystemInitializer]
        private static void Init()
        {
            MSULog.Info($"Initializing TokenModifierManager");
            On.RoR2.Language.LoadStrings += (orig, self) =>
            {
                orig(self);
                ModifyTokensInLanguage(self);
            };
            RoR2Application.onLoad += () => ModifyTokensInLanguage(null);
        }

        /// <summary>
        /// Adds the calling assembly to the TokenModifierManager.
        /// <para>When added, the manager will look for types with public static fields that implement the <see cref="TokenModifierAttribute"/></para>
        /// </summary>
        [Obsolete("Apply the following assembly attribute to your assembly: \"[assembly: HG.Reflection.SearchableAttribute.OptIn]\"")]
        public static void AddToManager()
        {
        }

        private static void ModifyTokensInLanguage(Language lang)
        {
            lang = lang == null ? Language.currentLanguage : lang;

            if (cachedFormattingArray == null)
                CreateFinalizedFormattingArray(lang);

            //Do formatting with cached array
            FormatTokens(lang);
        }

        private static void CreateFinalizedFormattingArray(Language lang)
        {
            GetTokenModifierLists(out var propertyTokenModifiers, out var fieldTokenModifiers);
            var formattingDictionaryFromFields = CreateFormattingDictionary(fieldTokenModifiers);
            var formattingDictionaryFromProperties = CreateFormattingDictionary(propertyTokenModifiers);

            cachedFormattingArray = new Dictionary<string, object[]>();
            foreach (var kvp in formattingDictionaryFromFields)
            {
                var token = kvp.Key;
                var formattingArray = kvp.Value;

                //Add token from dictionary, this replaces the array, but thats ok as this dictionary is completely empty.
                //Empty array
                cachedFormattingArray[token] = Array.Empty<object>();
                var arrayFromCache = cachedFormattingArray[token];
                for (int i = 0; i < formattingArray.Length; i++)
                {
                    //Resize if needed
                    if (arrayFromCache.Length < i + 1)
                    {
                        Array.Resize(ref arrayFromCache, i + 1);
                    }
                    //only set value if the value in the cache is not null 
                    if (arrayFromCache[i] == null)
                        arrayFromCache[i] = formattingArray[i];
                }
                cachedFormattingArray[token] = arrayFromCache;
            }
            foreach (var kvp in formattingDictionaryFromProperties)
            {
                var token = kvp.Key;
                var formattingArray = kvp.Value;

                //We do not overwrite the array if the token is already in the dictionary.
                //this is due to the fact that key may already be in the dictionary due to being created from field token modifiers.
                if(!cachedFormattingArray.ContainsKey(token))
                {
                    cachedFormattingArray[token] = Array.Empty<object>();
                }
                var arrayFromCache = cachedFormattingArray[token];
                for (int i = 0; i < formattingArray.Length; i++)
                {
                    //Resize if needed
                    if (arrayFromCache.Length < i + 1)
                    {
                        Array.Resize(ref arrayFromCache, i + 1);
                    }
                    //only set value if the value in the cache is not null 
                    if (arrayFromCache[i] == null)
                        arrayFromCache[i] = formattingArray[i];
                }
                cachedFormattingArray[token] = arrayFromCache;
            }
        }

        private static void GetTokenModifierLists(out List<TokenModifierAttribute> propertyTokenModifiers, out List<TokenModifierAttribute> fieldTokenModifiers)
        {
            propertyTokenModifiers = new List<TokenModifierAttribute>();
            fieldTokenModifiers = new List<TokenModifierAttribute>();
            var allTokenModifiers = SearchableAttribute.GetInstances<TokenModifierAttribute>() ?? new List<SearchableAttribute>();
            foreach(TokenModifierAttribute tokenModifier in allTokenModifiers)
            {
                if(tokenModifier.target is FieldInfo)
                {
                    fieldTokenModifiers.Add(tokenModifier);
                }
                else
                {
                    propertyTokenModifiers.Add(tokenModifier);
                }
            }
        }

        private static Dictionary<string, object[]> CreateFormattingDictionary(List<TokenModifierAttribute> tokenModifiers)
        {
            var dictionary = new Dictionary<string, object[]>();
            if (tokenModifiers.Count == 0)
                return dictionary;

            foreach(TokenModifierAttribute tokenModifier in tokenModifiers)
            {
                try
                {
                    var token = tokenModifier.langToken;
                    var formattingIndex = tokenModifier.formatIndex;
                    var formattingValue = tokenModifier.GetFormattingValue();
                    if(!dictionary.ContainsKey(token)) //If the token is not in the dictionary, add it with an empty array.
                    {
                        dictionary[token] = Array.Empty<object>();
                    }

                    var dictArray = dictionary[token];
                    if(dictArray.Length < formattingIndex + 1) //Resize array if needed
                    {
                        Array.Resize(ref dictArray, formattingIndex + 1);
                    }

                    if (dictArray[formattingIndex] == null) //Only set value if the current value is null
                    {
                        dictArray[formattingIndex] = formattingValue;
                    }
                    dictionary[token] = dictArray;
                }
                catch(Exception e)
                {
                    MSULog.Error(e);
                }
            }

            return dictionary;
        }
        private static void FormatTokens(Language lang)
        {
            if (cachedFormattingArray.Count == 0)
                return;

            MSULog.Info($"Modifying a total of {cachedFormattingArray.Count} tokens.");
            foreach (var kvp in cachedFormattingArray)
            {
                try
                {
                    var token = kvp.Key;
                    var formattingArray = kvp.Value;

                    if (!lang.stringsByToken.ContainsKey(token))
                    {
                        MSULog.Error($"Token {token} could not be found in the tokenToModifiers dictionary in {lang.name}! Either the mod that implements the token doesnt support the language {lang.name} or theyre adding their tokens via R2Api's LanguageAPI");
                        continue;
                    }
                    MSULog.Debug($"Modifying {token}");
                    FormatToken(lang, token, formattingArray);
                }
                catch (Exception e) { MSULog.Error(e); }
            }
        }

        private static void FormatToken(Language lang, string token, object[] formattingArray)
        {
            var tokenValue = lang.stringsByToken[token];
            var formatted = string.Format(tokenValue, formattingArray);
            lang.stringsByToken[token] = formatted;
        }
    }
}
        