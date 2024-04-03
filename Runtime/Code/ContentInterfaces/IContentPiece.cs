using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using RoR2.ContentManagement;

namespace MSU
{
    public interface IContentPiece
    {
        IEnumerator LoadContentAsync();
        bool IsAvailable(ContentPack contentPack);
        void Initialize();
    }

    public interface IContentPiece<T> : IContentPiece where T : UnityEngine.Object
    {
        T Asset { get; }
    }
}