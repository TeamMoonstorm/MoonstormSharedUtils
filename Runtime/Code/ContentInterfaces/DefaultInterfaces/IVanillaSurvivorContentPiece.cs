using RoR2;

namespace MSU
{
    public interface IVanillaSurvivorContentPiece : IContentPiece
    {
        SurvivorDef SurvivorDef { get; set; }
        string SurvivorDefAddress { get; }
    }
}