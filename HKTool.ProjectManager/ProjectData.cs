using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.ProjectManager
{
    [Serializable]
    public class WebDependenciesInfo
    {
        public Dictionary<string, string> Files { get; set; } = new Dictionary<string, string>();
    }
    [Serializable]
    public class ProjectData
    {
        public string ProjectName { get; set; } = "UNKNOW";
        public string ModVersion { get; set; } = "0.0.0.0";
        public string CSharpVersion { get; set; } = "latest";
        public string CodeDir { get; set; } = @".\Scripts\";
        public string EmbeddedResourceDir { get; set; } = @".\EmbeddedResource\";
        public Dictionary<string, string> EmbeddedResource { get; set; } = new Dictionary<string, string>();
        public string Guid { get; set; }
        public string DefaultNamespace { get; set; } = "";
        public string DependenciesDir { get; set; } = @".\Dependencies\";
        public string ModdingAPIVersion { get; set; } = "1.5.78.11833-67";
        public List<string> WebDependencies { get; set; } = new List<string>();
    }
}
