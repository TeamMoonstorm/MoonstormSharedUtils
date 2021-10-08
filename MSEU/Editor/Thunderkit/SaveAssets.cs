using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.Pipelines;
using UnityEditor;

namespace Moonstorm.EditorUtils.Pipelines
{
    [PipelineSupport(typeof(Pipeline))]
    public class SaveAssets : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            AssetDatabase.SaveAssets();
        }
    }
}
