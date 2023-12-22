using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public interface IContentPiece
    {
        IEnumerator LoadContentAsync();
        bool IsAvailable();
        void Initialize();
    }

    public interface IContentPiece<T> : IContentPiece where T : UnityEngine.Object
    {
        T Asset { get; }
    }
}