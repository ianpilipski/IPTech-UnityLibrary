#if !UNITY_6000_0_OR_NEWER
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace IPTech.DebugConsoleService
{
    public static class TextureMemoryAnalyzer {

        public static MemoryData CalculateMemory(Texture texture)
        {
            var texture2D = texture as Texture2D;
            if(texture2D != null)
                return new MemoryDataImpl(texture.name, CalculateRuntimeSizeInBytes(texture2D), MemoryDataStatus.CALCULATED);
            return new MemoryDataImpl(texture.name, 0, MemoryDataStatus.INDETERMINANT);
        }

        public static MemoryDataCollection CalculateMemory(Texture[] textures) {
            List<MemoryData> mdList = new List<MemoryData>();
            for(int i=0; i< textures.Length; i++) {
                mdList.Add(CalculateMemory(textures[i])); 
            }
            return new MemoryDataCollectionImpl("Textures", mdList);
        }

        public static MemoryDataCollection CalculateMemoryForAllTextures() 
        {
            Texture[] textures = RuntimeInspector.FindAllTextures(true);
            return CalculateMemory(textures);
        }

        private static long CalculateRuntimeSizeInBytes(Texture2D tex2d)
        {
            TextureDataCalculator tdc = new TextureDataCalculator(tex2d);
            return tdc.RuntimeMemoryInBytes;
        }

        private class MemoryDataImpl : MemoryData {
            private const float BYTES_IN_MEGABYTE = 1024.0f * 1024.0f;

            private string name;
            private MemoryDataStatus status;
            private long sizeInBytes;

            public MemoryDataImpl(string name, long sizeInBytes, MemoryDataStatus status) {
                this.name = name;
                this.sizeInBytes = sizeInBytes;
                this.status = status;
            }

            public override string Name {
                get {
                    return name;
                }
            }

            public override MemoryDataStatus Status {
                get {
                    return status;
                }
            }

            public override long SizeInBytes {
                get {
                    return sizeInBytes;
                }
            }

            public override float SizeInMegabytes {
                get {
                    return SizeInBytes / BYTES_IN_MEGABYTE;
                }
            }
        }

        public class MemoryDataCollectionImpl : MemoryDataCollection {

            private string name;
            private IEnumerable<MemoryData> memoryData;

            public MemoryDataCollectionImpl(string name, IEnumerable<MemoryData> memoryData) {
                this.name = name;
                this.memoryData = memoryData;
            }

            public override string Name { get { return name; } }
           
            public override IEnumerator<MemoryData> GetEnumerator() {
                return memoryData.GetEnumerator();
            }
        }
    }
}

#endif
#endif