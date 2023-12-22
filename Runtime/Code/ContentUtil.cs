using BepInEx;
using R2API.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public static class ContentUtil
    {
        public static IContentPieceProvider<T> AnalyzeForContentPieces<T>(BaseUnityPlugin baseUnityPlugin, R2APISerializableContentPack contentPack) where T : UnityEngine.Object
        {
            var assembly = baseUnityPlugin.GetType().Assembly;

            IEnumerable<IContentPiece<T>> contentPieces = assembly.GetTypes()
                .Where(PassesFilter<T>)
                .Select(t => (IContentPiece<T>)Activator.CreateInstance(t));

            return new GenericContentPieceProvider<T>(contentPieces, contentPack);
        }

        private static bool PassesFilter<T>(Type t) where T : UnityEngine.Object
        {
            bool notAbstract = !t.IsAbstract;
            bool implementsInterface = t.GetInterfaces().Contains(typeof(IContentPiece<T>));
            return notAbstract && implementsInterface;
        }
        private class GenericContentPieceProvider<T> : IContentPieceProvider<T> where T : UnityEngine.Object
        {
            public R2APISerializableContentPack ContentPack => _contentPack;

            private R2APISerializableContentPack _contentPack;
            private IContentPiece<T>[] _contentPieces;
            public IContentPiece<T>[] GetContents()
            {
                return _contentPieces;
            }

            IContentPiece[] IContentPieceProvider.GetContents()
            {
                return _contentPieces;
            }
            public GenericContentPieceProvider(IEnumerable<IContentPiece<T>> contentPieces, R2APISerializableContentPack contentPack)
            {
                _contentPieces = contentPieces.ToArray();
                _contentPack = contentPack;
            }
        }
    }
}
