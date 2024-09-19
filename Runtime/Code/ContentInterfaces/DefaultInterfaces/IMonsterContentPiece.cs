namespace MSU
{
    /// <summary>
    /// See <see cref="IGameObjectContentPiece{T}"/>, <see cref="IContentPiece"/> and <see cref="IContentPiece{T}"/> for more information regarding Content Pieces
    /// <br></br>
    /// <br>A version of <see cref="ICharacterContentPiece"/> used to represent a Monster for the Game</br>
    /// <br>It's module is the <see cref="CharacterModule"/></br>
    /// <br>Contains properties to specify the monster's <see cref="MonsterCardProvider"/> and the monster's DissonanceCard used when the Artifact of Dissonance is enabled.</br>
    /// </summary>
    public interface IMonsterContentPiece : ICharacterContentPiece
    {
        /// <summary>
        /// The <see cref="MonsterCardProvider"/> for this Monster. Can be Null.
        /// </summary>
        NullableRef<MonsterCardProvider> cardProvider { get; }

        /// <summary>
        /// The DirectorCard used for this Monster when <see cref="RoR2.RoR2Content.Artifacts.mixEnemyArtifactDef"/> is enabled. Can be Null
        /// </summary>
        NullableRef<DirectorCardHolderExtended> dissonanceCard { get; }
    }
}
