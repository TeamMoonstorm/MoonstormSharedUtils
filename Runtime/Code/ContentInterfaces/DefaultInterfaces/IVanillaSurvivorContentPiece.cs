using RoR2;

namespace MSU
{
    public interface IVanillaSurvivorContentPiece : IContentPiece
    {
        SurvivorDef survivorDef { get; set; }
        string survivorDefAddress { get; }
    }
}