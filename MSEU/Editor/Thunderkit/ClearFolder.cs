using System.IO;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;

namespace Moonstorm.EditorUtils.Pipelines
{
    [PipelineSupport(typeof(Pipeline))]
    public class ClearFolder : PipelineJob
    {
        [PathReferenceResolver]
        public string input;
        public override void Execute(Pipeline pipeline)
        {
            string source = input.Resolve(pipeline, this);
            if (!Directory.Exists(source))
            {
                return;
            }

            ClearDirectory(source);
        }

        private void ClearDirectory(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }

            foreach (var directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }
        }

        private void DeleteDirectory(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }

            foreach (var directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            Directory.Delete(path);
        }
    }

}
