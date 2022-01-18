#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IPTech.DebugConsoleService
{
    public class TextureDataCalculator {

        public int RuntimeWidth { get; private set; }
        public int RuntimeHeight { get; private set; }
        public int BitsPerPixel { get; private set; }
        public int TexelWidthInPixels { get; private set; }
        public TextureFormat RuntimeFormat { get; private set; }
        public bool IsTextureReadable { get; private set; }
        public long RuntimeMemoryInBytes { get; private set; }

        public TextureDataCalculator(Texture2D texture) {
            CalculateRuntimeDimensions(texture);
            CalculateRuntimeFormat(texture);
            CalculateRuntimeBitsPerPixel();
            CalculateTexelWidthInPixels();
            CalculateIsTextureReadable(texture);
            CalculateRuntimeMemoryInBytes(texture);
        }

        private void CalculateRuntimeDimensions(Texture2D texture) {
            RuntimeWidth = ConvertToPowerOfTwo(texture.width);
            RuntimeHeight = ConvertToPowerOfTwo(texture.height);
            EnsureSquareTextureIfUsingPVRTCCompression();
        }

        private void EnsureSquareTextureIfUsingPVRTCCompression() {
            if(RuntimeWidth != RuntimeHeight && IsPVCRTCompressed()) {
                RuntimeWidth = Mathf.Max(RuntimeWidth, RuntimeHeight);
                RuntimeHeight = Mathf.Max(RuntimeWidth, RuntimeHeight);
            }
        }

        private int ConvertToPowerOfTwo(int value) {
            if((value & (value-1)) != 0) {
                for(int i=0; i<5; i++){ value |= (value >> (1 << i)); }
                value++;
            }
            return value;
        }

        private bool IsPVCRTCompressed() {
            switch(RuntimeFormat) {
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGBA4:
                    return true;
                default:
                    return false;
            }
        }

        private void CalculateRuntimeFormat(Texture2D texture) {
            RuntimeFormat = texture.format;
            #if UNITY_EDITOR
            RuntimeFormat = GetTextureImporterFormat(texture);
            #endif
        }

        private void CalculateRuntimeBitsPerPixel() {
            BitsPerPixel = GetBitsPerPixelForFormat(RuntimeFormat);
        }

        private void CalculateRuntimeMemoryInBytes(Texture2D texture) {
            RuntimeMemoryInBytes = CalculateMemoryInBytes(
                RuntimeWidth, 
                RuntimeHeight,
                BitsPerPixel,
                texture.mipmapCount,
                TexelWidthInPixels
            );
            if(IsTextureReadable) {
                RuntimeMemoryInBytes *= 2;
            }
        }

        public static long CalculateMemoryInBytes(long textureWidthPixels, long textureHeightPixels, int bitsPerPixel, int mipmapCount, int minTexelPixels) {
            long size = textureWidthPixels * textureHeightPixels * bitsPerPixel / 8;
            for(int j = 1 ; j < mipmapCount; j++) {
                textureWidthPixels = (long)Mathf.Max(Mathf.Ceil(textureWidthPixels / 2.0f), minTexelPixels);
                textureHeightPixels = (long)Mathf.Max(Mathf.Ceil(textureHeightPixels / 2.0f), minTexelPixels);
                size += textureWidthPixels * textureHeightPixels * bitsPerPixel / 8L;
            }
            return size;
        }

        #if UNITY_EDITOR
        private TextureFormat GetTextureImporterFormat(Texture2D texture) {
            string assetPath = AssetDatabase.GetAssetPath(texture);
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);

            TextureImporter ti = assetImporter as TextureImporter;
			BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;

			if(ti!=null) {
				ti.GetPlatformTextureSettings(currentBuildTarget.ToString(), out int maxTextureSize, out TextureImporterFormat textureFormat);
				switch(textureFormat) {

                    case TextureImporterFormat.Alpha8:                      return TextureFormat.Alpha8;
                    case TextureImporterFormat.ARGB16:                      return TextureFormat.ARGB4444;
                    case TextureImporterFormat.ARGB32:                      return TextureFormat.ARGB32;
                    case TextureImporterFormat.ASTC_RGBA_10x10:             return TextureFormat.ASTC_RGB_10x10;
                    case TextureImporterFormat.ASTC_RGBA_12x12:             return TextureFormat.ASTC_RGBA_12x12;
                    case TextureImporterFormat.ASTC_RGB_4x4:                return TextureFormat.ASTC_RGB_4x4;
                    case TextureImporterFormat.ASTC_RGB_5x5:                return TextureFormat.ASTC_RGB_5x5;
                    case TextureImporterFormat.ASTC_RGB_6x6:                return TextureFormat.ASTC_RGB_6x6;
                    case TextureImporterFormat.ASTC_RGB_8x8:                return TextureFormat.ASTC_RGB_8x8;
                    case TextureImporterFormat.DXT1:                        return TextureFormat.DXT1;
                    case TextureImporterFormat.DXT1Crunched:                return TextureFormat.DXT1Crunched;
                    case TextureImporterFormat.DXT5:                        return TextureFormat.DXT5;
                    case TextureImporterFormat.DXT5Crunched:                return TextureFormat.DXT5Crunched;
                    case TextureImporterFormat.EAC_R:                       return TextureFormat.EAC_R;
                    case TextureImporterFormat.EAC_RG:                      return TextureFormat.EAC_RG;
                    case TextureImporterFormat.EAC_RG_SIGNED:               return TextureFormat.EAC_RG_SIGNED;
                    case TextureImporterFormat.EAC_R_SIGNED:                return TextureFormat.EAC_R_SIGNED;
                    case TextureImporterFormat.ETC2_RGB4:                   return TextureFormat.ETC_RGB4;
                    case TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA: return TextureFormat.ETC_RGB4;
                    case TextureImporterFormat.ETC2_RGBA8:                  return TextureFormat.ETC2_RGBA8;
                    case TextureImporterFormat.ETC_RGB4:                    return TextureFormat.ETC_RGB4;
                    case TextureImporterFormat.PVRTC_RGB2:                  return TextureFormat.PVRTC_RGB2;
                    case TextureImporterFormat.PVRTC_RGB4:                  return TextureFormat.PVRTC_RGB4;
                    case TextureImporterFormat.PVRTC_RGBA2:                 return TextureFormat.PVRTC_RGBA2;
                    case TextureImporterFormat.PVRTC_RGBA4:                 return TextureFormat.PVRTC_RGBA4;
                    case TextureImporterFormat.RGB16:                       return TextureFormat.RGB565;
                    case TextureImporterFormat.RGB24:                       return TextureFormat.RGB24;
                    case TextureImporterFormat.RGBA16:                      return TextureFormat.ARGB4444;
                    case TextureImporterFormat.RGBA32:                      return TextureFormat.RGBA32;
#pragma warning disable 618
					//case TextureImporterFormat.ATC_RGB4:                    return TextureFormat.ATC_RGB4;
					//case TextureImporterFormat.ATC_RGBA8:                   return TextureFormat.ATC_RGBA8;

                    case TextureImporterFormat.Automatic16bit:
                    case TextureImporterFormat.AutomaticCompressed:
                    case TextureImporterFormat.AutomaticCrunched:
                    case TextureImporterFormat.AutomaticTruecolor:
#pragma warning restore
                        return GetBestGuessAutomaticTextureFormat(texture, textureFormat);
                    default:
                        if(textureFormat!=0) {
                            Debug.LogWarning("unknown format " + textureFormat);
                        }
                        break;
                }
            }
            return texture.format;
        }

		private TextureFormat GetBestGuessAutomaticTextureFormat(Texture2D texture, TextureImporterFormat importerFormat) {
#pragma warning disable 618
            switch (importerFormat) {
                case TextureImporterFormat.Automatic16bit:
                    if(IsAlphaFormat(texture.format)) {
                        if(IsAlphaEncodedFirst(texture.format)) 
                            return TextureFormat.ARGB4444;
                        return TextureFormat.RGBA4444;
                    }
                    return TextureFormat.RGB565;   
                case TextureImporterFormat.AutomaticCrunched:
                    if(IsAlphaFormat(texture.format)) {
                        return TextureFormat.DXT5Crunched;
                    }     
                    return TextureFormat.DXT1Crunched;
                case TextureImporterFormat.AutomaticTruecolor:
                    int bpp = GetBitsPerPixelForFormat(texture.format);
                    if(bpp == 32) {
                        if(IsAlphaFormat(texture.format)) {
                            if(IsAlphaEncodedFirst(texture.format))
                                return TextureFormat.ARGB32;
                        }
                        return TextureFormat.RGBA32;
                    }
                    if(bpp == 8) {
                        return TextureFormat.Alpha8;
                    }
                    return TextureFormat.RGB24;
                case TextureImporterFormat.AutomaticCompressed:
                    if(IsAlphaFormat(texture.format)) {
                        return TextureFormat.DXT5;
                    }
                    return TextureFormat.DXT1;
                default:
                    Debug.LogWarning("unknown format " + importerFormat);
                    return texture.format;
            }
#pragma warning restore
        }

        private bool IsAlphaFormat(TextureFormat textureFormat) {
            switch(textureFormat) {
                case TextureFormat.Alpha8:
                case TextureFormat.ARGB32:
                case TextureFormat.ARGB4444:
                case TextureFormat.ASTC_RGBA_10x10:
                case TextureFormat.ASTC_RGBA_12x12:
                case TextureFormat.ASTC_RGBA_4x4:
                case TextureFormat.ASTC_RGBA_5x5:
                case TextureFormat.ASTC_RGBA_6x6:
                case TextureFormat.ASTC_RGBA_8x8:
                case TextureFormat.BGRA32:
                case TextureFormat.DXT5:
                case TextureFormat.DXT5Crunched:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.RGBA32:
                case TextureFormat.RGBA4444:
                case TextureFormat.RGBAFloat:
                case TextureFormat.RGBAHalf:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsAlphaEncodedFirst(TextureFormat textureFormat) {
            switch(textureFormat) {
                case TextureFormat.ARGB32:
                case TextureFormat.ARGB4444:
                    return true;
                default:
                    return false;
            }
        }

        #endif

        private int GetBitsPerPixelForFormat(TextureFormat textureFormat) 
        {
            switch(textureFormat) {
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                    return 2;
                case TextureFormat.DXT1:
                #if UNITY_IOS
                case (TextureFormat)28:
                #else
                case TextureFormat.DXT1Crunched:
                #endif
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                //case TextureFormat.ATC_RGB4:
                    return 4;
                case TextureFormat.Alpha8:
                case TextureFormat.DXT5:
                #if UNITY_IOS
                case (TextureFormat)29:
                #else
                case TextureFormat.DXT5Crunched:
                #endif
                case TextureFormat.ETC2_RGBA8:
                //case TextureFormat.ATC_RGBA8:
                    return 8;
                case TextureFormat.ARGB4444:
                case TextureFormat.RGBA4444:
                case TextureFormat.RGB565:
                case (TextureFormat)9:
                    return 16;
                case TextureFormat.RGB24:
                    return 24;
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                    return 32;
                default:
                    #if UNITY_EDITOR
                    Debug.LogWarning("unknown format " + RuntimeFormat);
                    #endif
                    break;
            }
            return 32;
        }

        private void CalculateTexelWidthInPixels() {
            
            switch(RuntimeFormat) {
                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                #if UNITY_IOS
                case (TextureFormat)28:
                case (TextureFormat)29:
                #else
                case TextureFormat.DXT1Crunched:
                case TextureFormat.DXT5Crunched:
                #endif
                    TexelWidthInPixels = 4;
                    break;
                default:
                    TexelWidthInPixels = 1;
                    break;
            }
        }

        private void CalculateIsTextureReadable(Texture2D texture) {
            IsTextureReadable = false;
            try {
                // If we can read a pixel color than the texture is marked read/write
                texture.GetPixel(0,0);
                IsTextureReadable = true;
            } catch {}
        }
    }
}

#endif

