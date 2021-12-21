using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HKTool.ProjectManager
{
    public static class ModProjectFactory
    {
        public static ModProjectManager CreateModProject(string name, string path)
        {
            ProjectData projectData = new ProjectData();
            projectData.Guid = Guid.NewGuid().ToString();
            projectData.ProjectName = name;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, "ModProject.json"), JsonConvert.SerializeObject(projectData, Formatting.Indented));
            return new ModProjectManager(projectData, path);
        }
        public static ModProjectManager OpenModProject(string path)
        {
            string p = Path.Combine(path, "ModProject.json");
            if (!File.Exists(p)) return null;
            ProjectData projectData = JsonConvert.DeserializeObject<ProjectData>(File.ReadAllText(p));
            return new ModProjectManager(projectData, path);
        }
    }
}
