using RoR2;
using System.Collections;

namespace MSU
{
    public interface IVanillaSurvivorContentPiece : IContentPiece
    {
        SurvivorDef survivorDef { get; }

        IEnumerator InitializeAsync();
    }
}