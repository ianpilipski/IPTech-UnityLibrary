using System;
using IPTech.BuildTool.Processors;

namespace IPTech.BuildTool
{

    [Obsolete("ConfigModifierExportAsGradleProject is obsolete. Use SetExportAsGradleProject instead.")]
    public class ConfigModifierExportAsGradleProject : ConfigModifier {
        public bool ExportAsGradleProject;

        public override BuildProcessor ConvertToBuildProcessor() {
            var bp = CreateInstance<SetExportAsGradleProject>();
            bp.name = nameof(SetExportAsGradleProject);
            bp.ExportAsGradleProject = ExportAsGradleProject;
            return bp;
        }
    }
}