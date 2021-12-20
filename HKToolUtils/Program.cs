using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft;
using HKTool.ProjectManager;

namespace HKToolUtils
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1) return;
            var cmd = args[0];
            if (cmd.Equals("NewProject", StringComparison.OrdinalIgnoreCase))
            {
                using (var gitignoreS = new StreamReader(
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("HKToolUtils.gitignoreTemplate.txt")))
                {
                    File.WriteAllText(".gitignore", gitignoreS.ReadToEnd());
                }
                var pd = new ProjectData();
                pd.Guid = Guid.NewGuid().ToString();
                File.WriteAllText("ModProject.json", JsonConvert.SerializeObject(pd, Formatting.Indented));
                Main(new string[] { "RefreshMSProject" });
            }else if(cmd.Equals("RefreshMSProject", StringComparison.OrdinalIgnoreCase))
            {
                ProjectData pd = JsonConvert.DeserializeObject<ProjectData>(File.ReadAllText("ModProject.json"));
                var mg = new ModProjectManager(pd, Path.GetDirectoryName(Path.GetFullPath("ModProject.json")));
                mg.CreateMSProject();
            }else if(cmd.Equals("Build", StringComparison.OrdinalIgnoreCase))
            {
                ProjectData pd = JsonConvert.DeserializeObject<ProjectData>(File.ReadAllText("ModProject.json"));
                var mg = new ModProjectManager(pd, Path.GetDirectoryName(Path.GetFullPath("ModProject.json")));
                mg.Build();
            }else if (cmd.Equals("DownloadDependencies"))
            {
                ProjectData pd = JsonConvert.DeserializeObject<ProjectData>(File.ReadAllText("ModProject.json"));
                var mg = new ModProjectManager(pd, Path.GetDirectoryName(Path.GetFullPath("ModProject.json")));
                mg.DownloadDependencies(true);
                mg.DownloadModdingAPI(true);
            }
        }
    }
}
