using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Moonstorm
{
    /// <summary>
    /// The TokenModifierManager is a class that handles the usage of <see cref="TokenModifierAttribute"/> in mods.
    /// </summary>
    public static class TokenModifierManager
    {
        private static bool initialized = false;

        //A single language token can be modified from multiple fields, and each field may contian multiple token modifiers.
        private static Dictionary<string, List<(FieldInfo, TokenModifierAttribute[])>> tokenToModifiersFields = new Dictionary<string, List<(FieldInfo, TokenModifierAttribute[])>>();
        //Token modifiers can now use properties
        private static Dictionary<string, List<(PropertyInfo, TokenModifierAttribute[])>> tokenToModifiersProperties = new Dictionary<string, List<(PropertyInfo, TokenModifierAttribute[])>>();
        //This is the finalized dictionary that gets created from the two above.
        private static Dictionary<string, object[]> cachedFormattingArray = null;
        [SystemInitializer]
        private static void Init()
        {
            initialized = true;
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
        public static void AddToManager()
        {
            Assembly assembly = Assembly.GetCallingAssembly();

            if (initialized)
            {
                MSULog.Warning($"Cannot add {assembly.GetName().Name} to the Dictionary as the token modifier manager has already been initialized.");
                return;
            }

            MSULog.Info($"Adding mod {assembly.GetName().Name} to the token modifier manager");
            GetFieldsWithAttribute(assembly);
            GetPropertiesWithAttribute(assembly);
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
            var formattingDictionaryFromFields = GetFieldModifiers(lang);
            var formattingDictionaryFromProperties = GetPropertyModifiers(lang);

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
            tokenToModifiersFields.Clear();
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
            tokenToModifiersProperties.Clear();
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
        #region Methods using FieldInfos
        private static void GetFieldsWithAttribute(Assembly assembly)
        {
            List<(FieldInfo, TokenModifierAttribute[])> allFieldsWithAttributes = new List<(FieldInfo, TokenModifierAttribute[])>();
            foreach (Type type in assembly.GetTypesSafe().Where(type => type.GetCustomAttribute<DisabledContentAttribute>() == null))
            {
                try
                {
                    List<FieldInfo> fields = type.GetFields()
                                                 .Where(f => f.GetCustomAttributes<TokenModifierAttribute>().Count() != 0) //A field can have multiple modifiers.
                                                 .ToList();

                    foreach (FieldInfo field in fields)
                    {
                        try
                        {
                            var modifiers = field.GetCustomAttributes<TokenModifierAttribute>().ToArray();
                            if (modifiers.Length > 0)
                            {
                                allFieldsWithAttributes.Add((field, modifiers));
                            }

                        }
                        catch (Exception e) { MSULog.Error(e); }
                    }
                }
                catch (Exception e) { MSULog.Error(e); }
            }

            if (allFieldsWithAttributes.Count == 0)
            {
                MSULog.Warning($"Found no fields with the {nameof(TokenModifierAttribute)} attribute within {assembly.GetName().Name}");
                return;
            }

            MSULog.Debug($"Found a total of {allFieldsWithAttributes.Count} fields with the {nameof(TokenModifierAttribute)} attribute within {assembly.GetName().Name}.");
            foreach (var (field, attributes) in allFieldsWithAttributes)
            {
                foreach (TokenModifierAttribute modifier in attributes)
                {
                    var token = modifier.langToken;
                    if (!tokenToModifiersFields.ContainsKey(token))
                    {
                        tokenToModifiersFields.Add(token, new List<(FieldInfo, TokenModifierAttribute[])>());
                    }

                    var attributeMatch = attributes.Where(att => att.langToken == token).ToArray(); //A field can have an multiple attributes that modifies different tokens.

                    if (attributeMatch.Length == 0)
                        continue;

                    //If the value doesnt contain the field & their attributes, add them.
                    if (!tokenToModifiersFields[token].Contains((field, attributeMatch.ToArray())))
                    {
                        tokenToModifiersFields[token].Add((field, attributeMatch));
                    }
                }
            }
        }

        private static Dictionary<string, object[]> GetFieldModifiers(Language lang)
        {
            var dictionary = new Dictionary<string, object[]>();
            if (tokenToModifiersFields.Count == 0)
                return dictionary;

            MSULog.Info($"Getting a total of {tokenToModifiersFields.Count} token modifiers using fields...");
            foreach (var kvp in tokenToModifiersFields)
            {
                try
                {
                    var token = kvp.Key;
                    var modifiers = kvp.Value;

                    if (!lang.stringsByToken.ContainsKey(token))
                    {
                        MSULog.Error($"Token {token} could not be found in the tokenToModifiers dictionary in {lang.name}! Either the mod that implements the token doesnt support the language {lang.name} or theyre adding their tokens via R2Api's LanguageAPI");
                        continue;
                    }
                    MSULog.Debug($"Getting the field modifiers for token {token}.");

                    //If the token is not in the dictionary, add with with an empty array.
                    if (!dictionary.ContainsKey(token))
                    {
                        dictionary.Add(token, Array.Empty<object>());
                    }

                    //This gets the formatting, some entries may be null, but thats ok as we're going to get the other entries from properties.
                    var formattingArrayWhichMayHaveNullEntries = GetFormattingFromField(modifiers);
                    //Get the key's value array
                    var dictionaryArray = dictionary[token];
                    //Iterate thru the formatting array
                    for (int i = 0; i < formattingArrayWhichMayHaveNullEntries.Length; i++)
                    {
                        //Resize if needed
                        if (dictionaryArray.Length < i + 1)
                        {
                            Array.Resize(ref dictionaryArray, i + 1);
                        }
                        //Only set value if the current value is null.
                        if (dictionaryArray[i] == null)
                        {
                            dictionaryArray[i] = formattingArrayWhichMayHaveNullEntries[i];
                        }
                    }
                    //Update dictionary.
                    dictionary[token] = dictionaryArray;
                }
                catch (Exception e) { MSULog.Error(e); }
            }
            return dictionary;
        }

        private static object[] GetFormattingFromField(List<(FieldInfo, TokenModifierAttribute[])> fieldAndModifiers)
        {
            object[] objectArray = Array.Empty<object>();
            foreach (var (field, modifiers) in fieldAndModifiers)
            {
                try
                {
                    foreach (var modifier in modifiers)
                    {
                        try
                        {
                            (object value, int index) formattingTuple = modifier.GetFormatting(field);
                            if (formattingTuple.value == null)
                                throw new NullReferenceException($"Formatting tuple contains a null value, Field full name: {field.DeclaringType.FullName}.{field.Name}");

                            if (objectArray.Length < formattingTuple.index + 1)
                            {
                                Array.Resize(ref objectArray, formattingTuple.index + 1);
                            }
                            objectArray[formattingTuple.index] = formattingTuple.value;
                        }
                        catch (Exception e) { MSULog.Error(e); }
                    }
                }
                catch (Exception e) { MSULog.Error(e); }
            }
            return objectArray;
        }
        #endregion

        #region Methods using PropertyInfos
        private static Dictionary<string, object[]> GetPropertyModifiers(Language lang)
        {
            var dictionary = new Dictionary<string, object[]>();
            if (tokenToModifiersProperties.Count == 0)
                return dictionary;

            MSULog.Info($"Getting a total of {tokenToModifiersProperties.Count} token modifiers using properties...");
            foreach (var kvp in tokenToModifiersProperties)
            {
                try
                {
                    var token = kvp.Key;
                    var modifiers = kvp.Value;

                    if (!lang.stringsByToken.ContainsKey(token))
                    {
                        MSULog.Error($"Token {token} could not be found in the tokenToModifiers dictionary in {lang.name}! Either the mod that implements the token doesnt support the language {lang.name} or theyre adding their tokens via R2Api's LanguageAPI");
                        continue;
                    }
                    MSULog.Debug($"Getting the property modifiers for token {token}.");

                    //If the token is not in the dictionary, add with with an empty array.
                    if (!dictionary.ContainsKey(token))
                    {
                        dictionary.Add(token, Array.Empty<object>());
                    }

                    //This gets the formatting, some entries may be null, but thats ok as we're going to get the other entries from properties.
                    var formattingArrayWhichMayHaveNullEntries = GetFormattingFromProperty(modifiers);
                    //Get the key's value array
                    var dictionaryArray = dictionary[token];
                    //Iterate thru the formatting array
                    for (int i = 0; i < formattingArrayWhichMayHaveNullEntries.Length; i++)
                    {
                        //Resize if needed
                        if (dictionaryArray.Length < i + 1)
                        {
                            Array.Resize(ref dictionaryArray, i + 1);
                        }
                        //Only set value if the current value is null.
                        if (dictionaryArray[i] == null)
                        {
                            dictionaryArray[i] = formattingArrayWhichMayHaveNullEntries[i];
                        }
                    }
                    //Update dictionary.
                    dictionary[token] = dictionaryArray;
                }
                catch (Exception e) { MSULog.Error(e); }
            }
            return dictionary;
        }
        private static void GetPropertiesWithAttribute(Assembly assembly)
        {
            List<(PropertyInfo, TokenModifierAttribute[])> allPropertiesWithAttributes = new List<(PropertyInfo, TokenModifierAttribute[])>();
            foreach (Type type in assembly.GetTypesSafe().Where(t => t.GetCustomAttribute<DisabledContentAttribute>() == null))
            {
                try
                {
                    List<PropertyInfo> properties = type.GetProperties()
                        .Where(p => p.GetCustomAttributes<TokenModifierAttribute>().Count() != 0) //A property can have multiple modifiers
                        .ToList();

                    foreach (PropertyInfo info in properties)
                    {
                        try
                        {
                            var modifiers = info.GetCustomAttributes<TokenModifierAttribute>().ToArray();
                            if (modifiers.Length > 0)
                            {
                                allPropertiesWithAttributes.Add((info, modifiers));
                            }
                        }
                        catch (Exception e) { MSULog.Error(e); }
                    }
                }
                catch (Exception e) { MSULog.Error(e); }
            }

            if (allPropertiesWithAttributes.Count == 0)
            {
                MSULog.Warning($"Found no properties with the {nameof(TokenModifierAttribute)} attribute within {assembly.GetName().Name}");
                return;
            }

            MSULog.Debug($"Found a total of {allPropertiesWithAttributes.Count} properties with the {nameof(TokenModifierAttribute)} attribute within {assembly.GetName().Name}.");
            foreach (var (prop, attributes) in allPropertiesWithAttributes)
            {
                foreach (TokenModifierAttribute modifier in attributes)
                {
                    var token = modifier.langToken;
                    if (!tokenToModifiersProperties.ContainsKey(token))
                    {
                        tokenToModifiersProperties.Add(token, new List<(PropertyInfo, TokenModifierAttribute[])>());
                    }

                    var attributeMatch = attributes.Where(att => att.langToken == token).ToArray(); //a properti can have multiple attributes that modifies different tokens.

                    if (attributeMatch.Length == 0)
                        continue;

                    //If the value doesnt contain the field & their attributes, add them
                    if (!tokenToModifiersProperties[token].Contains((prop, attributeMatch.ToArray())))
                    {
                        tokenToModifiersProperties[token].Add((prop, attributeMatch));

                    }
                }
            }
        }
        private static object[] GetFormattingFromProperty(List<(PropertyInfo, TokenModifierAttribute[])> propAndModifiers)
        {
            object[] objectArray = Array.Empty<object>();
            foreach (var (prop, modifiers) in propAndModifiers)
            {
                try
                {
                    foreach (var modifier in modifiers)
                    {
                        try
                        {
                            (object value, int index) formattingTuple = modifier.GetFormatting(prop);
                            if (formattingTuple.value == null)
                                throw new NullReferenceException($"Formatting tuple contains a null value, Field full name: {prop.DeclaringType.FullName}.{prop.Name}");

                            if (objectArray.Length < formattingTuple.index + 1)
                            {
                                Array.Resize(ref objectArray, formattingTuple.index + 1);
                            }
                            objectArray[formattingTuple.index] = formattingTuple.value;
                        }
                        catch (Exception e) { MSULog.Error(e); }
                    }
                }
                catch (Exception e) { MSULog.Error(e); }
            }
            return objectArray;
        }
        #endregion
    }
}
        