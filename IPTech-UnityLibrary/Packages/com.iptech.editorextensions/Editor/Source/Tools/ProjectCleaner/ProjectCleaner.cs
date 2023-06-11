
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace IPTech.EditorTools {
    public static class ProjectCleaner {
        public static List<string> DeleteEmptyDirectories() {
            var retVal = GetEmptyDirectories();
            DeleteDirectories(retVal);
            return retVal;
        }

        static void DeleteDirectories(List<string> dirs) {
            foreach(var d in dirs) {
                UnityEditor.FileUtil.DeleteFileOrDirectory(d);
                string metaFile = d + ".meta";
                if(File.Exists(metaFile)) {
                    File.Delete(metaFile);
                }
            }
        }

        public static List<string> GetEmptyDirectories() {
            var retVal = new List<string>();
            var stack = new Stack<string>();
            stack.Push(Application.dataPath);

            while(stack.TryPop(out string dir)) {
                if(IsDirEmpty(dir)) {
                    retVal.Add(dir);
                } else {
                    var dirs = Directory.GetDirectories(dir);
                    foreach(var d in dirs) {
                        stack.Push(d);
                    }
                }
            }

            for(int i=0;i<retVal.Count;i++) {
                string parentDir = Path.GetDirectoryName(retVal[i]);
                Debug.Log(retVal[i] + " parent:" + parentDir);
                if(IsDirEmpty(parentDir)) {
                    if(!retVal.Contains(parentDir)) {
                        retVal.Add(parentDir);
                    }
                }
            }

            return retVal;

            bool IsDirEmpty(string dir) {
                var files = Directory.GetFiles(dir);
                bool hasFiles = files.Any(f => !f.EndsWith(".meta"));
                if(!hasFiles) {
                    var dirs = Directory.GetDirectories(dir);
                    if(dirs.Length==0) {
                        return true;
                    } 

                    foreach(var d in dirs) {
                        if(!retVal.Contains(d)) {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
        }


    }
}