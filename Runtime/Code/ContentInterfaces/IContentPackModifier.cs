using R2API.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public interface IContentPackModifier
    {
        void ModifyContentPack(R2APISerializableContentPack contentPack);
    }
}
