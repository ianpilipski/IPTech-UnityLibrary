
namespace IPTech.PackageCreator.Editor {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEditor.PackageManager.UI;
    using UnityEngine;

    [Serializable]
    public class PackageInfo {
        public string AssemblyDefName { get; set; }
        public string name;
        public string displayName;
        public string version = "0.0.1";
        public string unity;
        public string unityRelease;
        public List<string> keywords;
        public string category;
        public string description;
        public Dictionary<string, Dependency> dependencies;
        public List<Sample> samples;
        public Author author;
    }

    [Serializable]
    public class Dependencies : List<Dependency> {

    }

    [Serializable]
    public class Dependency {

    }

    [Serializable]
    public class Sample {
        public string displayName;
        public string description;
        public string path;
	}

    [Serializable]
    public class Author {
        public string name;
        public string url;
    }

    [Serializable]
    public class AssemblyDef {
        public string name;
        public List<string> references;
        public List<string> optionalUnityReferences;
        public List<string> includePlatforms;
        public List<string> excludePlatforms;
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public List<string> precompiledReferences;
        public bool autoReferenced = true;
        public List<string> defineConstraints;
        public List<string> versionDefines;
        public bool noEngineReferences = false;
	}

    public class PackageGenerator {
        static public void CreatePackage(PackageInfo packageInfo) {
            new PackageGenerator().Create(packageInfo);
        }

        internal PackageGenerator() {

        }

        internal void Create(PackageInfo packageInfo) {
            ValidatePackageInfo(packageInfo);
            CreatePackageFolders(packageInfo);
            CreatePackageJson(packageInfo);
            CreateRuntimeAssembly(packageInfo);
            CreateRuntimeAssemblyInfo(packageInfo);
            CreateRuntimeAssemblyTempScriptFile(packageInfo);
            CreateEditorAssembly(packageInfo);
            CreateEditorAssemblyInfo(packageInfo);
            CreateEditorAssemblyTempScriptFile(packageInfo);
            CreateEditorTestsAssembly(packageInfo);
            CreateEditorTestsAssemblyTempScriptFile(packageInfo);
            CreateRuntimeTestsAssembly(packageInfo);
            CreateRuntimeTestsAssemblyTempScriptFile(packageInfo);
            CreateChangeLog(packageInfo);
            CreateDocumentation(packageInfo);
            CreateLicense(packageInfo);
            UnityEditor.PackageManager.Client.Resolve();
        }

        void ValidatePackageInfo(PackageInfo packageInfo) {
            if (string.IsNullOrEmpty(packageInfo.AssemblyDefName)) throw new InvalidAssemblyDefNameException();
            if (!new Regex("^([a-z]+\\.)*[a-z]+$").IsMatch(packageInfo.name)) {
                throw new InvalidPackageNameException();
            }
        }

        void CreatePackageFolders(PackageInfo packageInfo) {
            List<string> directories = new List<string>() {
                "Runtime",
                "Editor",
                Path.Combine("Tests", "Editor"),
                Path.Combine("Tests", "Runtime"),
                "Documentation~",
                "Samples~"
            };

            foreach (var dir in directories) {
                Directory.CreateDirectory(Path.Combine(GetPackageDir(packageInfo), dir));
            }
        }

        void CreatePackageJson(PackageInfo packageInfo) {
            string p = Path.Combine(GetPackageDir(packageInfo), "package.json");
            string json = JsonUtility.ToJson(packageInfo, true);
            File.WriteAllText(
                p,
                json
            );
		}

        void CreateRuntimeAssembly(PackageInfo packageInfo) {
            CreateAssemblyDef(
                Path.Combine(GetPackageDir(packageInfo), "Runtime", string.Format("{0}.Runtime.asmdef", packageInfo.AssemblyDefName)),
                new AssemblyDef() { name = string.Format("{0}.Runtime", packageInfo.AssemblyDefName) }
            );
		}

        void CreateRuntimeAssemblyInfo(PackageInfo packageInfo) {
            File.WriteAllText(
                Path.Combine(GetPackageDir(packageInfo), "Runtime", "AssemblyInfo.cs"),
                string.Format(
                    "using System.Runtime.CompilerServices;\n\n" +
                    "[assembly: InternalsVisibleTo(\"{0}.Editor\")]\n" +
                    "[assembly: InternalsVisibleTo(\"{0}.Runtime.Tests\")]\n",
                    packageInfo.AssemblyDefName
                )
            );
		}

        void CreateRuntimeAssemblyTempScriptFile(PackageInfo packageInfo) {
            File.WriteAllText(
                Path.Combine(GetPackageDir(packageInfo), "Runtime", "TempClass.cs"),
                "public class TempClass { }"
            );
		}

        void CreateEditorAssembly(PackageInfo packageInfo) {
            CreateAssemblyDef(
                Path.Combine(GetPackageDir(packageInfo), "Editor", string.Format("{0}.Editor.asmdef", packageInfo.AssemblyDefName)),
                new AssemblyDef() {
                    name = string.Format("{0}.Editor", packageInfo.AssemblyDefName),
                    includePlatforms = new List<string>() { "Editor" },
                    references = new List<string>() {
                        string.Format("{0}.Runtime", packageInfo.AssemblyDefName)
					}
                }
            );
		}

        void CreateEditorAssemblyInfo(PackageInfo packageInfo) {
            File.WriteAllText(
                Path.Combine(GetPackageDir(packageInfo), "Editor", "AssemblyInfo.cs"),
                string.Format(
                    "using System.Runtime.CompilerServices;\n\n" +
                    "[assembly: InternalsVisibleTo(\"{0}.Editor.Tests\")]",
                    packageInfo.AssemblyDefName
                )
            );
        }

        void CreateEditorAssemblyTempScriptFile(PackageInfo packageInfo) {
            File.WriteAllText(
                Path.Combine(GetPackageDir(packageInfo), "Editor", "TempClass.cs"),
                "public class TempClass { }"
            );
		}

        void CreateEditorTestsAssembly(PackageInfo packageInfo) {
            CreateAssemblyDef(
                Path.Combine(GetPackageDir(packageInfo), "Tests", "Editor", string.Format("{0}.Editor.Tests.asmdef", packageInfo.AssemblyDefName)),
                new AssemblyDef() {
                    name = string.Format("{0}.Editor.Tests", packageInfo.AssemblyDefName),
                    includePlatforms = new List<string>() { "Editor" },
                    references = new List<string>() {
                        string.Format("{0}.Runtime", packageInfo.AssemblyDefName),
                        string.Format("{0}.Editor", packageInfo.AssemblyDefName)
					},
                    optionalUnityReferences = new List<string>() { "TestAssemblies" }
				}
            );
		}

        void CreateEditorTestsAssemblyTempScriptFile(PackageInfo packageInfo) {
            File.WriteAllText(
                Path.Combine(GetPackageDir(packageInfo), "Tests", "Editor", "TempClass.cs"),
                "public class TempClass { }"
            );
		}

        void CreateRuntimeTestsAssembly(PackageInfo packageInfo) {
            CreateAssemblyDef(
                Path.Combine(GetPackageDir(packageInfo), "Tests", "Runtime", string.Format("{0}.Runtime.Tests.asmdef", packageInfo.AssemblyDefName)),
                new AssemblyDef() {
                    name = string.Format("{0}.Runtime.Tests", packageInfo.AssemblyDefName),
                    references = new List<string>() {
                        string.Format("{0}.Runtime", packageInfo.AssemblyDefName)
                    },
                    optionalUnityReferences = new List<string>() { "TestAssemblies" }
                }
            );
        }

        void CreateRuntimeTestsAssemblyTempScriptFile(PackageInfo packageInfo) {
            File.WriteAllText(
                Path.Combine(GetPackageDir(packageInfo), "Tests", "Runtime", "TempClass.cs"),
                "public class TempClass { }"
            );
		}

        void CreateAssemblyDef(string filePath, AssemblyDef assemblyDef) {
            File.WriteAllText(
                filePath,
                JsonUtility.ToJson(assemblyDef)
            );
		}

        void CreateChangeLog(PackageInfo packageInfo) {
            File.WriteAllText(
                Path.Combine(GetPackageDir(packageInfo), "CHANGELOG.md"),
                "# Changelog\n\n## X.X.X\n"
            );
		}

        void CreateDocumentation(PackageInfo packageInfo) {
            File.WriteAllText(
                Path.Combine(GetPackageDir(packageInfo), "Documentation~", "README.md"),
                string.Format("# {0}\n", packageInfo.displayName)
            );
        }

        void CreateLicense(PackageInfo packageInfo) {
            File.WriteAllText(
                Path.Combine(GetPackageDir(packageInfo), "LICENSE.md"),
                "# LICENSE\n"
            );
        }

        string GetPackageDir(PackageInfo packageInfo) {
            return Path.Combine(Application.dataPath, "..", "Packages", packageInfo.name);
        }
    }

    public class InvalidAssemblyDefNameException : Exception {
        public InvalidAssemblyDefNameException() : base("The AssemblyDefName is invalid") { }
	}

    public class InvalidPackageNameException : Exception {
        public InvalidPackageNameException() : base("The name of the package is invalid") { }
    }
}
