using UnityEngine;

namespace Moonstorm.Components
{
    public class HGControllerFinder : MonoBehaviour
    {
        public Renderer Renderer;
        public Material material;

        public void OnEnable()
        {
            if (Renderer && material)
            {
                Renderer.material = material;
                Renderer.sharedMaterials[0] = material;
                MaterialControllerComponents.MaterialController materialController = null;
                switch (material.shader.name)
                {
                    case "Hopoo Games/Deferred/Standard":
                        materialController = gameObject.AddComponent<MaterialControllerComponents.HGStandardController>();
                        break;
                    case "Hopoo Games/Deferred/Snow Topped":
                        materialController = gameObject.AddComponent<MaterialControllerComponents.HGSnowToppedController>();
                        break;
                    case "Hopoo Games/Deferred/Triplanar Terrain Blend":
                        materialController = gameObject.AddComponent<MaterialControllerComponents.HGTriplanarController>();
                        break;
                    case "Hopoo Games/FX/Cloud Remap":
                        materialController = gameObject.AddComponent<MaterialControllerComponents.HGCloudRemapController>();
                        break;
                    case "Hopoo Games/FX/Cloud Intersection Remap":
                        materialController = gameObject.AddComponent<MaterialControllerComponents.HGIntersectionController>();
                        break;
                    case "Hopoo Games/FX/Solid Parallax":
                        materialController = gameObject.AddComponent<MaterialControllerComponents.HGSolidParallaxController>();
                        break;
                    case "Hopoo Games/Deferred/Wavy Cloth":
                        materialController = gameObject.AddComponent<MaterialControllerComponents.HGWavyClothController>();
                        break;
                    case "Hopoo Games/FX/Opaque Cloud Remap":
                        materialController = gameObject.AddComponent<MaterialControllerComponents.HGOpaqueCloudRemap>();
                        break;
                        
                    //not a hg shader but still applies
                    case "CalmWater/Calm Water [DX11] [Double Sided]":
                        materialController = gameObject.AddComponent<MaterialControllerComponents.CW_DX11_DoubleSidedController>();
                        break;
                }
                if (materialController)
                {
                    materialController.material = material;
                    materialController.renderer = Renderer;
                    materialController.MaterialName = material.name;
                    Destroy(this);
                }
                else
                    enabled = false;
            }
            else
                enabled = false;
        }
    }
}
