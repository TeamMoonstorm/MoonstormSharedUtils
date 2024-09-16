using RoR2;
using System.Collections;

namespace MSU
{
    public interface IVanillaSurvivorContentPiece : IContentPiece
    {
        SurvivorDef survivorDef { get; set; }
        string survivorDefAddress { get; }

        IEnumerator InitializeAsync();
    }
}