using RiskOfOptions.Components.Panel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using SearchableAttribute = HG.Reflection.SearchableAttribute;
using System.Xml.Linq;
using MSU.Config;

namespace MSU
{
    internal static class FormatTokenManager
    {
        private static Dictionary<string, FormatTokenAttribute[]> _cachedFormattingArray = null;

        [RoR2.SystemInitializer(typeof(ConfigSystem))]
        private static void Init()
        {
            MSULog.Info("Initializing FormatTokenManager");
            On.RoR2.Language.LoadStrings += (orig, self) =>
            {
                orig(self);
                FormatTokensInLanguage(self);
            };
            RoR2.RoR2Application.onLoad += () => FormatTokensInLanguage(null);

            ModOptionPanelController.OnModOptionsExit += () =>
            {
                string langName = RoR2.Language.currentLanguageName;
                RoR2.Language.currentLanguage?.UnloadStrings();
                RoR2.Language.SetCurrentLanguage(langName);
            };
        }

        private static void FormatTokensInLanguage(RoR2.Language lang)
        {
            lang = lang ?? RoR2.Language.currentLanguage;

            if (_cachedFormattingArray == null)
                CreateFormattingArray(lang);

            FormatTokens(lang);
        }

        private static void CreateFormattingArray(RoR2.Language lang)
        {
            GetFormatTokenLists(out var propertyFormatTokens, out var fieldFormatTokens);

            var formattingDictionaryFromFields = CreateFormattingDictionary(fieldFormatTokens);
            var formattingDictionaryFromProperties = CreateFormattingDictionary(propertyFormatTokens);

            _cachedFormattingArray = new Dictionary<string, FormatTokenAttribute[]>();

            foreach(var (token, formattingArray) in formattingDictionaryFromFields)
            {
                //Add token from dictionary, this replaces the array, but that's ok as this dictionary is currently empty
                _cachedFormattingArray[token] = Array.Empty<FormatTokenAttribute>();
                var arrayFromCache = _cachedFormattingArray[token];
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
                _cachedFormattingArray[token] = arrayFromCache;
            }
            foreach (var (token, formattingArray) in formattingDictionaryFromProperties)
            {
                //We do not overwrite the array if the token is already in the dictionary.
                //This is due to the fact that the kye may already be in the dictionary due to being created from fields with the token modifiers

                if (!_cachedFormattingArray.ContainsKey(token))
                {
                    _cachedFormattingArray[token] = Array.Empty<FormatTokenAttribute>();
                }
                var arrayFromCache = _cachedFormattingArray[token];
                for (int i = 0; i < formattingArray.Length; i++)
                {
                    if (arrayFromCache.Length < i + 1)
                    {
                        Array.Resize(ref arrayFromCache, i + 1);
                    }
                    //only set value if the value in the cache is not null 
                    if (arrayFromCache[i] == null)
                        arrayFromCache[i] = formattingArray[i];
                }
                _cachedFormattingArray[token] = arrayFromCache;
            }
        }

        private static void GetFormatTokenLists(out List<FormatTokenAttribute> propertyFormatTokens, out List<FormatTokenAttribute> fieldFormatTokens)
        {
            propertyFormatTokens = new List<FormatTokenAttribute>();
            fieldFormatTokens = new List<FormatTokenAttribute>();
            var allTokenModifiers = SearchableAttribute.GetInstances<FormatTokenAttribute>() ?? new List<SearchableAttribute>();
            foreach (FormatTokenAttribute formatToken in allTokenModifiers)
            {
                if (formatToken.target is FieldInfo)
                {
                    fieldFormatTokens.Add(formatToken);
                }
                else
                {
                    propertyFormatTokens.Add(formatToken);
                }
            }
        }

        private static Dictionary<string, FormatTokenAttribute[]> CreateFormattingDictionary(List<FormatTokenAttribute> formatTokens)
        {
            var dictionary = new Dictionary<string, FormatTokenAttribute[]>();
            if (formatTokens.Count == 0)
                return dictionary;

            foreach (FormatTokenAttribute formatToken in formatTokens)
            {
                try
                {
                    var token = formatToken.LanguageToken;
                    var formattingIndex = formatToken.FormattingIndex;
                    //If the token is not in the dictionary, add it and initialize an empty array.
                    if (!dictionary.ContainsKey(token))
                    {
                        dictionary[token] = Array.Empty<FormatTokenAttribute>();
                    }

                    var dictArray = dictionary[token];
                    //Ensure array is big enough for the new modifier
                    if (dictArray.Length < formattingIndex + 1)
                    {
                        Array.Resize(ref dictArray, formattingIndex + 1);
                    }

                    //We should only set the modifier if there is no modifier already
                    if (dictArray[formattingIndex] == null)
                    {
                        dictArray[formattingIndex] = formatToken;
                    }
                    dictionary[token] = dictArray;
                }
                catch (Exception ex)
                {
                    MSULog.Error(ex);
                }
            }
            return dictionary;
        }

        private static void FormatTokens(RoR2.Language lang)
        {
            if (_cachedFormattingArray.Count == 0)
                return;

            MSULog.Info($"Formatting a total of {_cachedFormattingArray.Count} tokens.");
            foreach(var (token, attributes) in _cachedFormattingArray)
            {
                try
                {
                    if (!lang.stringsByToken.ContainsKey(token))
                    {
#if DEBUG
                        MSULog.Error($"Token {token} could not be found in the tokenToModifiers dictionary in {lang.name}! Either the mod that implements the token doesnt support the language {lang.name} or theyre adding their tokens via R2Api's LanguageAPI");
#endif
                        continue;
                    }
#if DEBUG
                    MSULog.Debug($"Modifying {token}");
#endif
                    FormatToken(lang, token, attributes);
                }
                catch(Exception e)
                {
                    MSULog.Error($"{e}\n(Token={token})");
                }
            }
        }

        private static void FormatToken(RoR2.Language lang, string token, FormatTokenAttribute[] formattingArray)
        {
            var tokenValue = lang.stringsByToken[token];
            object[] format = formattingArray.Select(att => att.GetFormattingValue()).ToArray();
            var formatted = string.Format(tokenValue, format);
            lang.stringsByToken[token] = formatted;
        }
    }
}