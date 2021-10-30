using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace HKTool.Core
{
    public class FakeLibrary
    {
        public static void TypeConverter(TypeDefinition type)
        {
            if (type.HasGenericParameters && !type.IsPublic)
            {
                //type.IsPublic = false;
                return;
            }

            //TypeReference s = type.Module.ImportReference(typeof(string));

            if (!type.IsPublic)
            {
                type.Name = "fake_" + type.Name;
                type.IsSealed = true;
            }
            type.IsPublic = true;

            foreach (var v in type.Methods)
            {
                if (!v.IsPublic && !v.IsConstructor)
                {
                    v.Name = "fake_" + v.Name;
                }
                v.IsPublic = true;
                v.Body = null;
                v.IsNative = true;
            }
            foreach (var v in type.Fields)
            {

                if (!v.IsPublic)
                {
                    v.Name = "fake_" + v.Name;
                }
                v.IsPublic = true;
            }
            foreach(var v in type.NestedTypes)
            {
                if (!v.IsPublic || type.Name.StartsWith("fake_"))
                {
                    v.IsPublic = false;
                }
                TypeConverter(v);
            }
        }
        public static AssemblyDefinition MakeFakeLibrary(AssemblyDefinition src)
        {
            var s = new MemoryStream();
            src.Write(s);
            s.Position = 0;
            AssemblyDefinition ad = AssemblyDefinition.ReadAssembly(s);
            ad.Name.Name = "fake_" + ad.Name.Name;
            ad.Name.PublicKey = null;
            foreach (var v in ad.MainModule.Types) TypeConverter(v);
            return ad;
        }
        public static void MakeFakeLibrary(string src, string dst)
        {
            MakeFakeLibrary(AssemblyDefinition.ReadAssembly(src)).Write(dst);

        }
    }
}
