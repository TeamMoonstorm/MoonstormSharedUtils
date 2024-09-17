using RoR2.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Pipelines.Jobs;
using UnityEditor;

namespace MSU.Editor
{
    [PipelineSupport(typeof(Pipeline))]
    public sealed class ChangeAssemblyBuildMode : PipelineJob
    {
        public AssemblyBuildMode buildMode = AssemblyBuildMode.Release;

        public override Task Execute(Pipeline pipeline)
        {
            var result = new ChangeAssemblyBuildModeResult();
            FindAndSetBuildMode(pipeline.Jobs.ToArray(), result);
            pipeline.Log(LogLevel.Information, $"Finished changing a total of {result.totalStageAssemblyJobs} stage assemblies jobs with a total of {result.totalRecursions} recursions.", result.context.ToArray());
            return Task.CompletedTask;
        }

        private void FindAndSetBuildMode(PipelineJob[] jobs, ChangeAssemblyBuildModeResult result)
        {
            StageAssemblies[] stageAssembliesJobs = jobs.OfType<StageAssemblies>().ToArray();
            for (int i = 0; i < stageAssembliesJobs.Length; i++)
            {
                StageAssemblies stageAssemblies = stageAssembliesJobs[i];
                AssemblyBuildMode previousBuildMode = stageAssemblies.releaseBuild ? AssemblyBuildMode.Release : AssemblyBuildMode.Debug;
                result.totalStageAssemblyJobs++;
                stageAssemblies.releaseBuild = buildMode == AssemblyBuildMode.Release;
                EditorUtility.SetDirty(stageAssemblies);
                result.context.Add($"Modified {MarkdownUtils.GenerateAssetLink(stageAssemblies)}'s StageAssemblies pipeline job (previous mode: {previousBuildMode}, new mode: {buildMode})");
            }

            ExecutePipeline[] executePipelineJobs = jobs.OfType<ExecutePipeline>().ToArray();
            PipelineJob[] executePipelinesPipelineJobs = executePipelineJobs.SelectMany(executePipeline => executePipeline.targetpipeline.Jobs).ToArray();
            if (executePipelinesPipelineJobs.Length > 0)
            {
                result.totalRecursions++;
                FindAndSetBuildMode(executePipelinesPipelineJobs, result);
            }
        }
        public enum AssemblyBuildMode
        {
            Debug,
            Release
        }

        private class ChangeAssemblyBuildModeResult
        {
            public int totalStageAssemblyJobs = 0;
            public int totalRecursions = 0;
            public int totalPipelinesAffected = 0;
            public List<string> context = new List<string>();
        }
    }
}
