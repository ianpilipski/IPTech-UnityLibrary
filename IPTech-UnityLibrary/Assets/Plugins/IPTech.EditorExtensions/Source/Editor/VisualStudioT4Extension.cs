using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;


namespace IPTech.EditorExtensions {
	public class VisualStudioT4Extension : UnityEditor.AssetPostprocessor {

		public override int GetPostprocessOrder() {
            return int.MaxValue;
		}

        public static string OnGeneratedCSProject(string path, string content) {
            using (var sw = new StringWriter()) {
                using (var sr = new StringReader(content)) {
                    while (true) {
                        string line = sr.ReadLine();
                        if (line == null) break;

                        sw.WriteLine(ProcessLine(line));
                    }
                }
                return sw.ToString();
            }
        }

        static string ProcessLine(string line) {
            var assetsPath = Application.dataPath;
            var parentPath = Directory.GetParent(assetsPath);

            var toTrimAmount = parentPath.FullName.Length + 1;

            var toReturn = new List<string>();

            Regex regex = new Regex(".+<None Include=\"(.+\\.tt)\" \\/>");
            Match match = regex.Match(line);
			if(match.Success) {
				if(match.Groups.Count>1) {
                    string ttFile = match.Groups[1].Value;
                    string ttFileSystemPath = ttFile.Replace('\\', Path.DirectorySeparatorChar);
                    AssetImporter importer = AssetImporter.GetAtPath(ttFile);
                    T4Importer t4importer = importer as T4Importer;

					if(t4importer) {
                        Debug.Log("Processed t4 templat: " + ttFile);
                        string generator = t4importer.CustomTool == T4Importer.ProcessingType.Generator ? "TextTemplatingFileGenerator" : "TextTemplatingFilePreprocessor";
                        string customNamespace = t4importer.CustomToolNamespace!=null ? t4importer.CustomToolNamespace : "";

                        return string.Format(
@"  <None Include=""{0}"">
	    <Generator>{1}</Generator>
        <LastGenOutput>{2}.cs</LastGenOutput>
        <CustomToolNamespace>{3}</CustomToolNamespace>
    </None>",
							ttFile, generator, Path.GetFileNameWithoutExtension(ttFileSystemPath), customNamespace
                        );
                    } else {
                        Debug.Log("not t4");
					}
                }
			}
            return line;
        }

    }
}
