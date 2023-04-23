using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine.Serialization;

namespace IPTech.BuildTool.Processors {
    [Tooltip("Adds shell scripts configured to build the xcode project artifact.")]
    public class IOSBuildScripts : BuildProcessor {
        const string DATAPATH = "iptechbuild/data";

        [FormerlySerializedAs("Builds")]
        public List<IOSBuildConfig> XcodeBuilds;
        
        public override void PostProcessBuild(BuildReport report) {
            if(report.summary.platform == BuildTarget.iOS) {
                CopyXCodeLibsToDest(report, "iptechbuild");
                CopyDataToDest(report, DATAPATH);
                CreateBuildScripts(report, "iptechbuild");
                CopyLauncher(report);
            }
        }

        void CopyLauncher(BuildReport report) {
            string source = BuildToolsSettings.GetFullPathToRelativePackagePath("Data~/xcode/iptechbuild.sh");
            string dest = Path.GetFullPath(Path.Combine(report.summary.outputPath, "iptechbuild.sh"));
            FileUtil.CopyFileOrDirectory(source, dest);
        }
        
        void CopyXCodeLibsToDest(BuildReport report, string destPath) {
            string sourceXCodeLibs = BuildToolsSettings.GetFullPathToRelativePackagePath("Data~/xcode/iptechbuild");
            string destXCodeLibs = Path.GetFullPath(Path.Combine(report.summary.outputPath, destPath));
            Directory.CreateDirectory(Path.GetDirectoryName(destXCodeLibs));
            FileUtil.CopyFileOrDirectory(sourceXCodeLibs, destXCodeLibs);
        }

        void CopyDataToDest(BuildReport report, string destPath) {
            string dest = Path.GetFullPath(Path.Combine(report.summary.outputPath, destPath));
            Directory.CreateDirectory(dest);
            var configEval = new ConfigEvaluator(this);

            foreach(var encFile in configEval.GetUniqueEncryptedFiles()) {
                if(BuildToolsSettings.instance.EncryptedStorage.TryGetEncryptedValue(encFile.Name, out string encData)) {
                    File.WriteAllText(Path.Combine(dest, encFile.Name + ".encrypted"), encData);    
                } else {
                    throw new Exception("could not get the encrypted data for " + encFile.Name);
                }
            }
        }

        void CreateBuildScripts(BuildReport report, string destPath) {
            string dest = Path.GetFullPath(Path.Combine(report.summary.outputPath, destPath));
            string templateFile = BuildToolsSettings.GetFullPathToRelativePackagePath("Data~/xcode/build-template.sh");
            var configEval = new ConfigEvaluator(this);

            string buildScript = File.ReadAllText(templateFile);
            buildScript = buildScript.Replace("# IPTECH_SUB: SIGNING_CERTS", GenerateSigningCertsSnippet());
            buildScript = buildScript.Replace("# IPTECH_SUB: PROVISIONING_PROFILES", GenerateProvisioningProfilesSnippet());
            buildScript = buildScript.Replace("# IPTECH_SUB: BUILDS", GenerateTheBulidAndExportSnippet());

            var destFileName = Path.Combine(dest, "build.sh");
            File.WriteAllText(destFileName, buildScript);

            string GenerateSigningCertsSnippet() {
                StringBuilder sb = new StringBuilder();
                foreach(var cert in configEval.SigningCerts) {
                    sb.AppendLine(
$@"
readValueFromDecryptedFile -d -k {nameof(EncryptedItemSigningCert.SigningCert)} -f ""{DATAPATH}/{cert.Name}"" -o ""{DATAPATH}/{cert.Name}-cert.p12""
installSigningCert \
    -k $KEYCHAIN_NAME \
    -p ""$IPTECH_BUILDTOOL_PASSWORD"" \
    -c ""{DATAPATH}/{cert.Name}-cert.p12"" \
    -a ""$(readValueFromDecryptedFile -k Password -f {DATAPATH}/{cert.Name} -d)""
");
                }    
                return sb.ToString();
            }

            string GenerateProvisioningProfilesSnippet() {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                foreach(var pp in configEval.ProvisioningProfiles) {
                    sb.AppendLine($"readValueFromDecryptedFile -d -k {nameof(EncryptedItemMobileProvision.MobileProvision)} -f \"{DATAPATH}/{pp.Name}\" -o \"{DATAPATH}/{pp.Name}-mobileprovision.mobileprovision\"");
                    sb.AppendLine($"installProvisioningProfile \"{DATAPATH}/{pp.Name}-mobileprovision.mobileprovision\"");
                }
                return sb.ToString();
            }
        }

        string GenerateTheBulidAndExportSnippet() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            foreach(var b in XcodeBuilds) {
                string outputPath = $"build/{b.Name}";
                sb.AppendLine($"# build {b.Name}");
                sb.AppendLine(GenerateTheBuildSnippet(b, outputPath));
                foreach(var export in b.ExportArchives) {
                    sb.AppendLine(GenerateExportArchiveSnippet(export, outputPath));
                }
            }
            return sb.ToString();

            string GenerateTheBuildSnippet(IOSBuildConfig b, string outputPath) {
                return $@"
ARCHIVE_PATH=""{outputPath}/Unity-iPhone.xcarchive""

[ -f Unity-iPhone.xcworkspace ] && xcodebuildarg=(-workspace Unity-iPhone.xcworkspace) || xcodebuildarg=(-project Unity-iPhone.xcodeproj)

xcodebuild \
    $xcodebuildarg \
    -scheme Unity-iPhone \
    -configuration {b.BuildConfiguration} \
    -sdk iphoneos \
    -derivedDataPath DerivedData \
    -archivePath ""$ARCHIVE_PATH"" \
    CODESIGNING_ALLOWED=NO \
    CODESIGNING_REQUIRED=NO \
    DEVELOPMENT_TEAM= \
    PROVISIONING_PROFILE_SPECIFIER= \
    CODE_SIGN_IDENTITY= \
    clean archive 

# export the archives
";
            }

            string GenerateExportArchiveSnippet(Export export, string outputPath) {
                StringBuilder sb = new StringBuilder();

                var plist = $"{DATAPATH}/{export.ExportOptionPlist.Name}";
                var entitlements = string.IsNullOrEmpty(export.EntitlementsFile.Name) ? "" : $"{DATAPATH}/{export.EntitlementsFile.Name}";
                
                sb.AppendLine($"readValueFromDecryptedFile -d -k {nameof(EncryptedItemPList.PList)} -f \"{plist}\" -o \"{plist}-plist.plist\"");                
                if(!string.IsNullOrEmpty(entitlements)) {
                    sb.AppendLine($"readValueFromDecryptedFile -d -k {nameof(EncryptedItemPList.PList)} -f \"{entitlements}\" -o \"{entitlements}-plist.plist\"");
                    sb.AppendLine($@"resignArchive ""$ARCHIVE_PATH"" ""{plist}-plist.plist"" ""{entitlements}-plist.plist""");
                } else {
                    sb.AppendLine($@"resignArchive ""$ARCHIVE_PATH"" ""{plist}-plist.plist""");
                }
                
                sb.AppendLine($@"
xcodebuild \
    -exportArchive \
    -archivePath ""$ARCHIVE_PATH"" \
    -exportPath ""{outputPath}/{export.Name}"" \
    -exportOptionsPlist ""{plist}-plist.plist""
");
                return sb.ToString();
            }
        }

        
        

        

        [Serializable]
        public class IOSBuildConfig {
            public string Name;

            [FormerlySerializedAs("Configuration")]
            public EBuildConfiguration BuildConfiguration;
            public List<Export> ExportArchives;
        }

        [Serializable]
        public class Export {
            public string Name;
            public EncryptedItemMobileProvision.Reference ProvisionigProfile;
            public EncryptedItemPList.Reference ExportOptionPlist;
            public EncryptedItemPList.Reference EntitlementsFile;
            public EncryptedItemSigningCert.Reference SigningCert;
        }

        public enum EBuildConfiguration {
            Release,
            Debug,
            ReleaseForProfiling,
            ReleaseForRunning
        }

        class ConfigEvaluator {
            public readonly List<EncryptedItemInfo> ProvisioningProfiles;
            public readonly List<EncryptedItemInfo> SigningCerts;
            public readonly List<EncryptedItemInfo> ExportOptionsPlists;
            public readonly List<EncryptedItemInfo> EntitlementsFiles;

            public ConfigEvaluator(IOSBuildScripts iosBuildScripts) {
                ProvisioningProfiles = new List<EncryptedItemInfo>();
                SigningCerts = new List<EncryptedItemInfo>();
                ExportOptionsPlists = new List<EncryptedItemInfo>();
                EntitlementsFiles = new List<EncryptedItemInfo>();

                foreach(var b in iosBuildScripts.XcodeBuilds) {
                    foreach(var export in b.ExportArchives) {
                        ConditionallyAddToList(export.ProvisionigProfile, ProvisioningProfiles);
                        ConditionallyAddToList(export.ExportOptionPlist, ExportOptionsPlists);
                        ConditionallyAddToList(export.SigningCert, SigningCerts);
                        ConditionallyAddToList(export.EntitlementsFile, EntitlementsFiles);
                    }
                }
            }

            void ConditionallyAddToList(EncryptedItemInfo encFile, List<EncryptedItemInfo> knownList) {
                if(!string.IsNullOrEmpty(encFile.Name)) {
                    if(!knownList.Contains(encFile)) {
                        knownList.Add(encFile);
                    }
                }
            }

            public IEnumerable<EncryptedItemInfo> GetUniqueEncryptedFiles() {
                return ProvisioningProfiles.Concat(SigningCerts).Concat(ExportOptionsPlists).Concat(EntitlementsFiles);
            }
        }
    }
}
