using RoR2;
using System;
using System.Collections.Generic;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for managing Characterbodies
    /// </summary>
    public abstract class CharacterModuleBase : ModuleBase
    {
        /// <summary>
        /// List of all the Characters loaded by Moonstorm Shared Utils
        /// </summary>
        public static List<CharacterBase> MoonstormCharacters = new List<CharacterBase>();

        [SystemInitializer(new Type[] { typeof(BodyCatalog), typeof(MasterCatalog) })]
        private static void HookInit()
        {
            MSULog.LogI("Subscribing to delegates related to survivors & monsters.");
        }

        #region Characters
        /// <summary>
        /// Finds all the CharacterBase inherited classes in your assembly and creates instances for each found.
        /// <para>Ignores all classes with the "DisabledContent" attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's CharacterBases</returns>
        public virtual IEnumerable<CharacterBase> InitializeCharacters()
        {
            MSULog.LogD($"Getting the Characters found inside {GetType().Assembly}...");
            return GetContentClasses<CharacterBase>();
        }

        /// <summary>
        /// Initializes a Character
        /// </summary>
        /// <param name="character">The CharacterBase class</param>
        /// <param name="characterList">Optinal, a List for storing the CharacterBases.</param>
        public void AddCharacter(CharacterBase character, List<CharacterBase> characterList = null)
        {
            character.Initialize();
            MoonstormCharacters.Add(character);
            if (characterList != null)
                characterList.Add(character);
            MSULog.LogD($"Character {character} added");
        }
        #endregion
    }
}
