#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace IPTech.DebugConsoleService
{
    public class TestTextureMemoryAnalyzer {

        [Test, Ignore("needs fixing")]
        [TestCase("texture2d-truecolor", 2097153, 2097153)]
        [TestCase("texture2d-truecolor-16bit", 1048576, 1048576)]
        [TestCase("texture2d-truecolor-16bit-withmips", 2097153, 2097153)]
        [TestCase("texture2d-truecolor-crunched", 262144, 524288)]
        [TestCase("texture2d-truecolor-crunched-withmips", 349544, 699064)]
        [TestCase("texture2d-truecolor-withmips-readable", 4194306, 4194306)]
        [TestCase("texture2d-truecolor-withmips", 2097153, 2097153)]
        [TestCase("texture2d-truecolor-nomips", 1572864, 1572864)]
        [TestCase("texture2d-truecolor-dxt1-withmips", 349544, 699064)]
        [TestCase("texture2d-truecolor-dxt5-withmips", 699088, 1398128)]
        [TestCase("texture2d-truecolor-sprite-NPOT", 3145728, 3145728)]
        [TestCase("texture2d-truecolor-sprite-NPOT-withmips", 4194303, 4194303)]
        public void Calculated_Memory_Texture2D(string resourceName, long expectedSizeAndroid, long expectedSizeIOS) {
            Texture2D tex = Resources.Load<Texture2D>("TestTextures/" + resourceName);

            Assert.IsNotNull(tex);

            MemoryData md = TextureMemoryAnalyzer.CalculateMemory(tex);

            long profilerSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex);
            int rawSize = tex.GetRawTextureData().Length;

            System.Console.Write("Profiler = " + profilerSize);
            System.Console.Write("Raw = " + rawSize);
            System.Console.Write("Calc = " + md.SizeInBytes);

            if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS) {
                Assert.AreEqual(expectedSizeIOS, md.SizeInBytes);
            } else {
                Assert.AreEqual(expectedSizeAndroid, md.SizeInBytes);
            }
        }
    }
}

#endif
