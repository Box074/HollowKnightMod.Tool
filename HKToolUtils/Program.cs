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
            if (args.Length < 2) return;
            var cmd = args[0];
            if (cmd.Equals("NewProject", StringComparison.OrdinalIgnoreCase) && args.Length == 3)
            {
                using (var gitignoreS = new StreamReader(
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("HKToolUtils.gitignoreTemplate.txt")))
                {
                    File.WriteAllText(".gitignore", gitignoreS.ReadToEnd());
                }
                var name = args[2];
                var p = ModProjectFactory.CreateModProject(name, Path.GetFullPath(args[1]));
                p.CreateMSProject();
            }else if(cmd.Equals("RefreshMSProject", StringComparison.OrdinalIgnoreCase))
            {
                var p = ModProjectFactory.OpenModProject(Path.GetFullPath(args[1]));
                p.CreateMSProject();
            }else if(cmd.Equals("Build", StringComparison.OrdinalIgnoreCase))
            {
                var p = ModProjectFactory.OpenModProject(Path.GetFullPath(args[1]));
                p.Build();
            }
            else if (cmd.Equals("DownloadDependencies"))
            {
                var p = ModProjectFactory.OpenModProject(Path.GetFullPath(args[1]));
                p.DownloadDependenciesDefault(true);
                p.DownloadModdingAPI(true);
            }
        }
    }
}
