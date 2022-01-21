using UnityEngine;

namespace Moonstorm.Components
{
    /// <summary>
    /// Attach this component to a gameObject and pass a meshrenderer in. It'll attempt to find the correct shader controller from the meshrenderer material, attach it if it finds it, and destroy itself.
    /// </summary>
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
