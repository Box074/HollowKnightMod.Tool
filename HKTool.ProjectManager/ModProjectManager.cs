using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft;
using Microsoft.CSharp;
using Microsoft;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace HKTool.ProjectManager
{
    public class ModProjectManager
    {
        private static string _slnTemplate = null;
        public static string SlnTemplate
        {
            get
            {
                if (_slnTemplate == null)
                {
                    using (TextReader tr = new StreamReader(
                        Assembly.GetExecutingAssembly().GetManifestResourceStream("HKTool.ProjectManager.template.sln.txt"))
                        )
                    {
                        _slnTemplate = tr.ReadToEnd();
                    }
                }
                return _slnTemplate;
            }
        }
        public ProjectData ProjectData { get; private set; } = null;
        public string ProjectBaseDirectory { get; private set; } = "";
        public string ModdingAPIPath
        {
            get
            {
                string p = Path.Combine(LibraryPath, "ModdingAPI");
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
                return p;
            }
        }
        public string OutputPath
        {
            get
            {
                string p = Path.Combine(ProjectBaseDirectory, "Output");
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
                return p;
            }
        }
        public string EmbeddedResourcePath
        {
            get
            {
                string p = Path.Combine(ProjectBaseDirectory, ProjectData.EmbeddedResourceDir);
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
                return p;
            }
        }

        public string LibraryPath
        {
            get
            {
                string p = Path.Combine(ProjectBaseDirectory, "Library");
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
                return p;
            }
        }
        public string WebDependenciesPath
        {
            get
            {
                string p = Path.Combine(LibraryPath, "WebDependenciesCache");
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
                return p;
            }
        }
        public WebDependenciesInfo WebDependenciesInfo
        {
            get
            {
                string p = Path.Combine(ProjectBaseDirectory, "WebDependenciesCache.json");
                WebDependenciesInfo info;
                if (!File.Exists(p))
                {
                    info = new WebDependenciesInfo();
                    WebDependenciesInfo = info;
                    return info;
                }
                info = JsonConvert.DeserializeObject<WebDependenciesInfo>(File.ReadAllText(p));
                return info;
            }
            set
            {
                string p = Path.Combine(ProjectBaseDirectory, "WebDependenciesCache.json");
                File.WriteAllText(p, JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.Indented));
            }
        }
        public string DependenciesPath
        {
            get
            {
                string p = Path.Combine(ProjectBaseDirectory, ProjectData.DependenciesDir);
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
                return p;
            }
        }
        public string CodePath
        {
            get
            {
                string p = Path.Combine(ProjectBaseDirectory, ProjectData.CodeDir);
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
                return p;
            }
        }
        public ModProjectManager(ProjectData project, string directory)
        {
            if (project is null) throw new ArgumentNullException(nameof(project));
            if (directory is null) throw new ArgumentNullException(nameof(directory));
            ProjectData = project;
            ProjectBaseDirectory = directory;
        }
        public bool DownloadDependency(Uri uri, string orig, WebDependenciesInfo info)
        {
            var name = Path.GetFileName(uri.LocalPath);

            var dp = Path.Combine(WebDependenciesPath, name);
            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {

                using (var client = new WebClient())
                {
                    var data = client.DownloadData(uri);

                    if (Path.GetExtension(dp).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        dp = Path.GetTempFileName();
                        File.WriteAllBytes(dp, data);
                        using (var zip = ZipFile.OpenRead(dp))
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (var v in zip.Entries)
                            {
                                string p = Path.Combine(WebDependenciesPath, v.Name);
                                sb.Append(v.FullName);
                                sb.Append(";");
                                v.ExtractToFile(p, true);
                            }

                            info.Files[orig] = sb.ToString();
                        }
                        File.Delete(dp);
                    }
                    else
                    {
                        File.WriteAllBytes(dp, data);
                        info.Files[orig] = name;
                    }

                }
                return true;
            }
            return true;
        }
        public bool DownloadModdingAPI(bool forceDownload = false)
        {
            if (File.Exists(Path.Combine(ModdingAPIPath, "Assembly-CSharp.dll")) && !forceDownload) return true;
            if (forceDownload)
            {
                if (Directory.Exists(ModdingAPIPath)) Directory.Delete(ModdingAPIPath, true);
            }
            try
            {
                using (WebClient client = new WebClient())
                {
                    Console.WriteLine("Downloading HKTool Libraries");
                    //UnityEngine
                    var uetmp = Path.GetTempFileName();
                    client.DownloadFile(@"https://github.com/HKLab/HKToolLibraries/archive/refs/heads/master.zip", uetmp);
                    ZipFile.ExtractToDirectory(uetmp, ModdingAPIPath);
                    Console.WriteLine("Finished!");
                    //ModdingAPI
                    Console.WriteLine("Downloading Modding API");
                    var matmp = Path.GetTempFileName();
                    client.DownloadFile(
                        $@"https://github.com/hk-modding/api/releases/download/{ProjectData.ModdingAPIVersion}/ModdingApiWin.zip", matmp);
                    ZipFile.ExtractToDirectory(matmp, ModdingAPIPath);
                    Console.WriteLine("Finished!");
                    return true;
                }
            } catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Console.WriteLine("Failed!");
                return false;
            }
        }
        public bool DownloadDependencies(bool forceDownload = false)
        {
            WebDependenciesInfo info = forceDownload ? new WebDependenciesInfo() : WebDependenciesInfo;
            bool s = true;
            if (forceDownload)
            {
                ClearWebDependencies();
            }
            foreach (var v in ProjectData.WebDependencies)
            {
                try
                {
                    if (info.Files.TryGetValue(v, out var p))
                    {
                        if (p.Split(';').All(x => string.IsNullOrWhiteSpace(x) || File.Exists(Path.Combine(WebDependenciesPath, x)))) continue;
                    }
                    Uri uri = new Uri(v);
                    Console.WriteLine($"Downloading dependency from '{uri}'");
                    s = DownloadDependency(uri, v, info) && s;
                    Console.WriteLine("Finished");
                } catch (Exception e)
                {
                    s = false;
                    Console.Error.WriteLine(e);
                }
            }
            WebDependenciesInfo = info;
            return s;
        }
        public void ClearWebDependencies()
        {
            Directory.Delete(WebDependenciesPath, true);
        }
        public void CreateMSProject()
        {
            MSProjectData msproj = new MSProjectData();
            string msprojGuid = $"{{{ProjectData.Guid}}}";
            string msprojName = $"{ProjectData.ProjectName}.csproj";
            string msprojPath = Path.Combine(ProjectBaseDirectory, msprojName);
            StringBuilder msprojBuilder = new StringBuilder();
            msprojBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<Project ToolsVersion=\"15.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n" +
                "<Import Project=\"$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props\" Condition=\"Exists('$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props')\" />\n" +
                "<PropertyGroup>\n" +
                "<Configuration Condition=\" '$(Configuration)' == '' \">Debug</Configuration>\n" +
                "<Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform>\n" +
                $"<ProjectGuid>{msprojGuid}</ProjectGuid>\n" +
                "<OutputType>Library</OutputType>\n" +
                "<AppDesignerFolder>Properties</AppDesignerFolder>\n" +
                $"<RootNamespace>{ProjectData.DefaultNamespace}</RootNamespace>\n" +
                $"<AssemblyName>{ProjectData.ProjectName}</AssemblyName>\n" +
                $"<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>\n" +
                $"<FileAlignment>512</FileAlignment>\n" +
                $"<LangVersion>{ProjectData.CSharpVersion}</LangVersion>\n" +
                $"<Deterministic>true</Deterministic>\n" +
                $"</PropertyGroup>\n" +
                $"<PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">\n" +
                $"<DebugSymbols>true</DebugSymbols>\n" +
                $"<DebugType>full</DebugType>\n" +
                $"<Optimize>false</Optimize>\n" +
                $"<OutputPath>bin\\Debug\\</OutputPath>\n" +
                $"<DefineConstants>DEBUG;TRACE</DefineConstants>\n" +
                $"<ErrorReport>prompt</ErrorReport>\n" +
                $"<WarningLevel>4</WarningLevel>\n" +
                $"</PropertyGroup>\n" +
                $"<ItemGroup>\n");
            #region Reference
            DownloadModdingAPI();
            foreach (var v in Directory.EnumerateFiles(ModdingAPIPath, "*.dll", SearchOption.AllDirectories))
            {
                msprojBuilder.AppendLine($"<Reference Include=\"{Path.GetFileNameWithoutExtension(v)}\">");
                msprojBuilder.AppendLine($"<HintPath>{v}</HintPath>");
                msprojBuilder.AppendLine("<Private>False</Private>");
                msprojBuilder.AppendLine("</Reference>");
            }
            DownloadDependencies();
            foreach (var v in Directory.EnumerateFiles(WebDependenciesPath, "*.dll", SearchOption.AllDirectories))
            {
                msprojBuilder.AppendLine($"<Reference Include=\"{Path.GetFileNameWithoutExtension(v)}\">");
                msprojBuilder.AppendLine($"<HintPath>{v}</HintPath>");
                msprojBuilder.AppendLine("</Reference>");
            }
            foreach (var v in Directory.GetFiles(DependenciesPath, "*.dll", SearchOption.AllDirectories)) ;
            #endregion
            #region CompileFiles
            var codeDir = new DirectoryInfo(CodePath);
            if (codeDir.Exists)
            {
                foreach (var v in codeDir.EnumerateFiles(
                "*.cs", SearchOption.AllDirectories)
                )
                {
                    msprojBuilder.AppendLine($"<Compile Include=\"{v.FullName}\" />");
                }
            }
            #endregion
            #region EmbeddedResource
            foreach (var v in ProjectData.EmbeddedResource)
            {
                var rp = Path.Combine(EmbeddedResourcePath, v.Key);
                if (File.Exists(rp))
                {
                    msprojBuilder.AppendLine($"<EmbeddedResource Include=\"{rp}\" />");
                }
            }
            #endregion
            msprojBuilder.AppendLine("</ItemGroup>");
            msprojBuilder.AppendLine("<Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />");
            msprojBuilder.AppendLine("</Project>");

            File.WriteAllText(msprojPath, msprojBuilder.ToString(), Encoding.UTF8);

            string slnName = ProjectData.ProjectName + ".sln";
            string slnPath = Path.Combine(ProjectBaseDirectory, slnName);
            if (!File.Exists(slnPath))
            {
                File.WriteAllText(slnPath, string.Format(SlnTemplate, ProjectData.ProjectName, ProjectData.Guid));
            }
        }
        public bool Build()
        {
            if (!DownloadDependencies() || !DownloadModdingAPI())
            {
                return false;
            }

            var metadataReference = new List<MetadataReference>();
            foreach (var v in Directory.EnumerateFiles(WebDependenciesPath, "*.dll", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(DependenciesPath, "*.dll", SearchOption.AllDirectories))
                )
            {
                try
                {
                    metadataReference.Add(MetadataReference.CreateFromFile(v));
                    File.Copy(v, Path.Combine(OutputPath, Path.GetFileName(v)), true);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            }
            foreach (var v in Directory.EnumerateFiles(ModdingAPIPath, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    metadataReference.Add(MetadataReference.CreateFromFile(v));
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            }
            var syntaxTree = new List<SyntaxTree>();
            syntaxTree.Add(CSharpSyntaxTree.ParseText($"[assembly: System.Runtime.InteropServices.Guid(\"{ProjectData.Guid}\")]\n" +
                $"[assembly: System.Reflection.AssemblyVersion(\"{ProjectData.ModVersion}\")]\n"));
            foreach (var v in Directory.EnumerateFiles(CodePath, "*.cs", SearchOption.AllDirectories))
            {
                try
                {
                    syntaxTree.Add(CSharpSyntaxTree.ParseText(
                        File.ReadAllText(v), CSharpParseOptions.Default, v, Encoding.UTF8, default
                        ));
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            }

            List<ResourceDescription> ers = new List<ResourceDescription>();
            foreach (var v in ProjectData.EmbeddedResource)
            {
                var fp = Path.Combine(EmbeddedResourcePath, v.Key);
                if (File.Exists(fp))
                {
                    var r0 = new ResourceDescription(v.Value, () => File.OpenRead(fp), true);
                    ers.Add(r0);
                }
            }
            var r = CSharpCompilation.Create(ProjectData.ProjectName)
                .WithOptions(new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                true, ProjectData.ProjectName, null, null,
                null, OptimizationLevel.Release, true, true, null, null, default, null,
                Platform.AnyCpu, ReportDiagnostic.Default, 4, null, true, false, null,
                null
                )
                ).AddSyntaxTrees(syntaxTree)
                .AddReferences(metadataReference)
                .Emit(Path.Combine(OutputPath, $"{ProjectData.ProjectName}.dll"),
                Path.Combine(OutputPath, $"{ProjectData.ProjectName}.pdb"),
                Path.Combine(OutputPath, $"{ProjectData.ProjectName}.xml"),
                null, ers, default);
            if (!r.Success)
            {
                foreach (var v in r.Diagnostics)
                {
                    switch (v.WarningLevel)
                    {
                        case (int)DiagnosticSeverity.Error:
                            Console.Error.WriteLine(v.ToString());
                            break;
                        case (int)DiagnosticSeverity.Warning:
                        case (int)DiagnosticSeverity.Info:
                            Console.WriteLine(v.ToString());
                            break;
                    }
                }
                Console.Error.WriteLine("Failed!");
                return false;
            }
            else
            {
                foreach (var v in r.Diagnostics)
                {
                    switch (v.WarningLevel)
                    {
                        case (int)DiagnosticSeverity.Error:
                            Console.Error.WriteLine(v.ToString());
                            break;
                        case (int)DiagnosticSeverity.Warning:
                        case (int)DiagnosticSeverity.Info:
                        default:
                            Console.WriteLine(v.ToString());
                            break;
                    }
                }
                Console.WriteLine("Success!");
                return true;
            }
        }
    }
}
