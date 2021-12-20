using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.ProjectManager
{
    public class MSProjectData
    {
        public Guid Guid { get; set; }
        public List<string> References { get; set; } = new List<string>();
        public List<string> EmbeddedResources { get; set; } = new List<string>();
    }
}
