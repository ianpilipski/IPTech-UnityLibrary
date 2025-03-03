using System;
using System.Diagnostics;
using UnityEngine;

namespace IPTech.BuildTool {
	public static class ShellCommand {
		public static int ExecBash(String commands, String workingDirectory) {
			string shellcmd = "/bin/bash";
			string args = PrepareArguments();
			Process p = new Process();
			p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			//p.StartInfo.CreateNoWindow = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.FileName = shellcmd;
			p.StartInfo.Arguments = args;
			p.StartInfo.WorkingDirectory = workingDirectory;
			p.EnableRaisingEvents = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardInput = true;
			p.Start();

			p.StandardInput.WriteLine(commands);
			p.StandardInput.WriteLine("exit");
			p.StandardInput.Flush();
			p.StandardInput.Close();

			string stdOut = p.StandardOutput.ReadToEnd();
			string stdErr = p.StandardError.ReadToEnd();
			p.WaitForExit();

			if(!string.IsNullOrEmpty(stdOut)) {
				BuildToolLogger.Log(stdOut);
			}
			if(!string.IsNullOrEmpty(stdErr)) {
				BuildToolLogger.LogError(stdErr);
			}

			return p.ExitCode;
		}

		static string PrepareArguments() {
			if(Application.platform == RuntimePlatform.WindowsEditor) {
				throw new NotImplementedException();
			} else {
				if(UnityEditorInternal.InternalEditorUtility.inBatchMode) {
					return "-l";
				}
				return string.Empty;
			}
		}
	}
}
