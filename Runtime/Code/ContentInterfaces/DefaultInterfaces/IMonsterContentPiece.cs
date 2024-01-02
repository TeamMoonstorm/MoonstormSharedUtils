using R2API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public interface IMonsterContentPiece : ICharacterContentPiece
    {
        NullableRef<MonsterCardProvider> CardProvider { get; }

        NullableRef<DirectorAPI.DirectorCardHolder> DissonanceCard { get; }
    }
}
