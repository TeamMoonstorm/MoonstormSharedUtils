using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public struct CharacterBodyIndexEqualityComparer : IEqualityComparer<CharacterBody>
    {
        public bool Equals(CharacterBody x, CharacterBody y)
        {
            if (!x || !y)
                return false;

            if (x.bodyIndex == BodyIndex.None || y.bodyIndex == BodyIndex.None)
                return false;

            return x.bodyIndex == y.bodyIndex;
        }

        public int GetHashCode(CharacterBody obj)
        {
            return obj.GetHashCode();
        }
    }
}