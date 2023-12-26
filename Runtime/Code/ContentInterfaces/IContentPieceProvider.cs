﻿using R2API.ScriptableObjects;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public interface IContentPieceProvider
    {
        ContentPack ContentPack { get; }
        IContentPiece[] GetContents();
    }

    public interface IContentPieceProvider<T> : IContentPieceProvider where T : UnityEngine.Object
    {
        new IContentPiece<T>[] GetContents();
    }
}