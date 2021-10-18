using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils
{
    [InitializeOnLoad]
    public class MakeShadersReadonly
    {
        static MakeShadersReadonly()
        {
            TurnShadersReadonly();
        }

        public static void TurnShadersReadonly()
        {
            List<FileInfo> files = Util.FindAssetsByType<Shader>()
                                       .Select(shader => AssetDatabase.GetAssetPath(shader))
                                       .Where(path => path.Contains(".asset"))
                                       .Select(path => new FileInfo(path))
                                       .ToList();

            int count = 0;
            foreach (FileInfo file in files)
            {
                try
                {
                    if (!file.IsReadOnly)
                    {
                        file.IsReadOnly = true;
                        count++;
                    }

                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            if(count != 0)
            {
                Debug.Log($"Found {count} Shader .Assets that where not ReadOnly, turning ReadOnly.");
            }
        }
    }
}
