using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    public interface IContentPiece
    {
        IEnumerator LoadContentAsync();
        bool IsAvailable();
    }

    public interface IContentPiece<T> : IContentPiece where T : UnityEngine.Object
    {
        T Asset { get; }
    }
}