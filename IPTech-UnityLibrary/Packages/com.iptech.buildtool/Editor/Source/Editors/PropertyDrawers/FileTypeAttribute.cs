
using UnityEngine;

namespace IPTech.BuildTool {
    public class FileTypeAttribute : PropertyAttribute {
        public string Extension;

        public FileTypeAttribute() {}

        public FileTypeAttribute(string extension) {
            this.Extension = extension;
        }
    }
}