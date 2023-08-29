using System;
using System.IO;


namespace IPTech.BuildTool.Internal {
    public class EncryptionUtil {
        public string OpenSSLEncrypt(byte[] bytes, string passphrase) {
            using(var tmpPath = new TempPath()) {
                var tmpFileName = Path.Combine(tmpPath.ThePath, Path.GetRandomFileName());
                File.WriteAllBytes(tmpFileName, bytes);
                ShellCommand.ExecBash($"openssl enc -aes-256-cbc -md sha256 -base64 -pass \"pass:{passphrase}\" -in \"{tmpFileName}\" -out \"{tmpFileName}.enc\"", tmpPath.ThePath);
                return File.ReadAllText($"{tmpFileName}.enc");
            }
        }

        public byte[] OpenSSLDecrypt(string encrypted, string passphrase) {
            using(var tmpPath = new TempPath()) {
                var tmpFileName = Path.Combine(tmpPath.ThePath, Path.GetRandomFileName());
                File.WriteAllText(tmpFileName, encrypted);
                ShellCommand.ExecBash($"openssl enc -aes-256-cbc -md sha256 -base64 -d -pass \"pass:{passphrase}\" -in \"{tmpFileName}\" -out \"{tmpFileName}.dec\"", tmpPath.ThePath);
                return File.ReadAllBytes($"{tmpFileName}.dec");
            }
        }

        class TempPath : IDisposable {
            public readonly string ThePath;
            bool alreadyDisposed;

            public TempPath() {
                var tp = Path.GetTempPath();
                ThePath = Path.Combine(tp, Path.GetRandomFileName());
                Directory.CreateDirectory(ThePath);
            }

            public void Dispose() {
                if(!alreadyDisposed) {
                    alreadyDisposed = true;
                    Directory.Delete(ThePath, true);
                }
            }
        }
    }
}
