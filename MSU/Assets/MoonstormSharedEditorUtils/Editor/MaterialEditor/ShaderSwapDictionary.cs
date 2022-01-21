using RoR2EditorKit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils
{
    [InitializeOnLoad]
    public static class ShaderSwapDictionary
    {
        public static Dictionary<Shader, Shader> realToStubbed = new Dictionary<Shader, Shader>();
        public static Dictionary<Shader, Shader> stubbedToReal = new Dictionary<Shader, Shader>();

        static ShaderSwapDictionary()
        {
            PopulateDictionary();
            if (realToStubbed.Count == 0 || stubbedToReal.Count == 0)
                Debug.Log($"There was an error while trying to populate the Shaders dictionary.");
            else
                Debug.Log("Populated Shader dictionary.");
        }

        public static void PopulateDictionary()
        {
            var allShadersInAssets = (List<Shader>)Util.FindAssetsByType<Shader>("hg");

            for (int i = 0; i < allShadersInAssets.Count; i++)
            {
                var current = allShadersInAssets[i];
                Shader real;
                string realFileName;

                Shader stubbed;
                if (current.name.StartsWith("Hopoo Games"))
                {
                    real = current;
                    realFileName = Path.GetFileName(AssetDatabase.GetAssetPath(real)).Replace(".asset", string.Empty);

                    stubbed = allShadersInAssets.Where(shader => shader.name != real.name)
                                                       .Select(shader => AssetDatabase.GetAssetPath(shader))
                                                       .Where(path => path.Contains(".shader"))
                                                       .Where(path => path.Contains(realFileName))
                                                       .Select(path => AssetDatabase.LoadAssetAtPath<Shader>(path))
                                                       .First();

                    if (real && stubbed)
                    {
                        stubbedToReal.Add(stubbed, real);
                        realToStubbed.Add(real, stubbed);
                    }
                }
            }

            var allCalmWaterShaders = (List<Shader>)Util.FindAssetsByType<Shader>("CalmWater");
            for (int i = 0; i < allCalmWaterShaders.Count; i++)
            {
                var current = allCalmWaterShaders[i];

                Shader real;
                string realFileName;

                Shader stubbed;
                if (current.name.StartsWith("CalmWater/"))
                {
                    real = current;
                    realFileName = Path.GetFileName(AssetDatabase.GetAssetPath(real)).Replace(".asset", string.Empty);

                    stubbed = allCalmWaterShaders.Where(shader => shader.name != real.name)
                                                 .Select(shader => AssetDatabase.GetAssetPath(shader))
                                                 .Where(path => path.Contains(".shader"))
                                                 .Where(path => path.Contains(realFileName))
                                                 .Select(path => AssetDatabase.LoadAssetAtPath<Shader>(path))
                                                 .First();

                    if (real && stubbed)
                    {
                        stubbedToReal.Add(stubbed, real);
                        realToStubbed.Add(real, stubbed);
                    }
                }
            }
            var allDecaliciousShaders = (List<Shader>)Util.FindAssetsByType<Shader>("Decalicious");
            for (int i = 0; i < allDecaliciousShaders.Count; i++)
            {
                var current = allDecaliciousShaders[i];

                Shader real;
                string realFileName;

                Shader stubbed;

                if (current.name.StartsWith("Decalicious/"))
                {
                    real = current;
                    realFileName = Path.GetFileName(AssetDatabase.GetAssetPath(real)).Replace(".asset", string.Empty);

                    stubbed = allDecaliciousShaders.Where(shader => shader.name != real.name)
                                                   .Select(shader => AssetDatabase.GetAssetPath(shader))
                                                   .Where(path => path.Contains(".shader"))
                                                   .Where(path => path.Contains(realFileName))
                                                   .Select(path => AssetDatabase.LoadAssetAtPath<Shader>(path))
                                                   .First();

                    if (real && stubbed)
                    {
                        stubbedToReal.Add(stubbed, real);
                        realToStubbed.Add(real, stubbed);
                    }
                }
            }
        }
    }
}

