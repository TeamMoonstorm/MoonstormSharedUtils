using R2API.ScriptableObjects;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    /// <summary>
    /// See also <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information on how ContentInterfaces work.
    /// <br>An Interface Addon that can be added to a ContentClass, during module initialization for said content, the Module will call <see cref="ModifyContentPack(ContentPack)"/>, which can be used for adding extra assets to the ContentPack.</br>
    /// <para>An example is a <see cref="ICharacterContentPiece"/>, which can be decorated with this interface and then in the method implementation it can add stuff like SkillDefs, SkillFamilies, EntityStates, etc.</para>
    /// <br>See also <see cref="IUnlockableContent"/></br>
    /// </summary>
    public interface IContentPackModifier
    {
        /// <summary>
        /// Method thats called during ModuleInitialization which can be used to modify your ContentPack to add, or remove assets.
        /// </summary>
        /// <param name="contentPack">Your content pack.</param>
        void ModifyContentPack(ContentPack contentPack);
    }
}
