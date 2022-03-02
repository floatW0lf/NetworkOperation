using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace UpmPackages.Tools
{
    public static class SetupPackagesDependencies
    {
        [MenuItem("Packages/RebuildDependencies")]
        public static void RebuildAll()
        {
            var packages = Directory.EnumerateFiles(Path.Combine(Application.dataPath, "..", "Packages"), "package.json", SearchOption.AllDirectories)
                .Select(s =>
                {
                    var assemblyDef = Directory.GetFiles(Path.GetDirectoryName(s)!, "*.asmdef", SearchOption.AllDirectories)[0];
                    return new PackageToAssembly(s, assemblyDef);
                }).ToArray();

            var lookup = packages.ToDictionary(p => p.AssemblyFileName);
            var nameLookUp = packages.ToDictionary(a => a.AssemblyName, a => new Dependency(a.PackageName, a.Version));
            var depsTree = packages.ToDictionary(a => a.PackageName, a => a.GetPackagesDependencies(nameLookUp,lookup));
            
            OptimizeDependencyTree(depsTree);
            
            foreach (var package in packages)
            {
                package.UpdateDependencies(depsTree);
                package.Save();
            }
            
        }

        private static IEnumerable<Dependency> Flat(this Dictionary<string,Dependency[]> tree, string package, bool rootSkip = true)
        {
            var deps = tree[package];
            return (rootSkip ? Array.Empty<Dependency>() : deps).Concat(deps.SelectMany(x => tree.Flat(x.Name, false)));
        }
         
        

        private static void OptimizeDependencyTree(Dictionary<string,Dependency[]> tree)
        {
            foreach (var element in tree.Where(x => x.Value.Length > 1).ToArray())
            {
                var flatten = tree.Flat(element.Key).Distinct();
                var excepted = element.Value.Except(flatten).ToArray();
                tree[element.Key] = excepted;
            }
        }
    }
    public struct Dependency : IEquatable<Dependency>
    {   public Dependency(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; }
        public string Version { get; }
        public bool Equals(Dependency other)
        {
            return Name == other.Name && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            return obj is Dependency other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Version);
        }
    }

    public class PackageToAssembly
    {
        private string _packageFile;
        public JObject Package { get; set; }

        public PackageToAssembly(string packageFile, string assemblyFile)
        {
            _packageFile = packageFile;
            ReadJson(packageFile, assemblyFile);
        }

        private void ReadJson(string packageFile, string assemblyFile)
        {
            using var packageJson = new StreamReader(packageFile);
            using var assemblyDefinition = new StreamReader(assemblyFile);
            AssemblyFileName = Path.GetFileName(assemblyFile);
            Package = JObject.Load(new JsonTextReader(packageJson));
            var assembly = JObject.Load(new JsonTextReader(assemblyDefinition));
            PackageName = Package["name"].Value<string>();
            AssemblyName = assembly["name"].Value<string>();
            Version = Package["version"].Value<string>();
            References = assembly["references"].Values<string>().ToArray();
        }

        public string[] References { get; private set; }
        public string Version { get; private set; }
        public string AssemblyFileName { get; private set; }
        public string AssemblyName { get; private set; }
        public string PackageName { get; private set; }

        public Dependency[] GetPackagesDependencies(Dictionary<string,Dependency> assemblyToPackagesNames, Dictionary<string, PackageToAssembly> assemblyFileNameToPackageInfoLookUp)
        {
            return References.Select(s =>
            {
                if (s.StartsWith("GUID:"))
                {
                    var packageInfo =
                        assemblyFileNameToPackageInfoLookUp[Path.GetFileName(AssetDatabase.GUIDToAssetPath(s.TrimStart("GUID:")))];
                    return new Dependency(packageInfo.PackageName, packageInfo.Version);
                }
                return assemblyToPackagesNames[s];
            }).ToArray();
        }

        public void UpdateDependencies(Dictionary<string,Dependency[]> dependencyTree)
        {
            var obj = new JObject();
            foreach (var item in dependencyTree[PackageName])
            {
                obj.Add(item.Name, item.Version);
            }
            Package["dependencies"] = obj;
        }

        public void Save()
        {
            using var writer = new StreamWriter(_packageFile);
            Package.WriteTo(new JsonTextWriter(writer){Formatting = Formatting.Indented});
            writer.Flush();
        }
    }
}

