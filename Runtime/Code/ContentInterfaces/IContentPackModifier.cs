using R2API.ScriptableObjects;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public interface IContentPackModifier
    {
        void ModifyContentPack(ContentPack contentPack);
    }
}
