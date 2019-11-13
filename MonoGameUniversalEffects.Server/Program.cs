using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using MonoGameUniversalEffects.Pipeline.Processors;

namespace MonoGameUniversalEffects.RemoteEffectServer
{
	internal static class MainClass
	{
		public static void Main()
		{
			var listener = new HttpListener();
			listener.Prefixes.Add($"http://*:60321/api/Effect/");
			listener.Start();
			ThreadPool.QueueUserWorkItem(o =>
			{
				try
				{
					Console.WriteLine("Web server running...");
					while (listener.IsListening)
					{
						ThreadPool.QueueUserWorkItem(c =>
						{
							var ctx = (HttpListenerContext) c;
							try
							{
								var contentLength = (int) ctx.Request.ContentLength64;
								var buffer = new byte[contentLength];
								ctx.Request.InputStream.Read(buffer, 0, contentLength);

								var obj = Encoding.ASCII.GetString(buffer);
								var d = (Data) JsonConvert.DeserializeObject(obj, typeof(Data));
								var buf = RunMgcb(d.Code, d.Platform, out var error);

								var result = JsonConvert.SerializeObject(new Result {Compiled = buf, Error = error});
								ctx.Response.ContentLength64 = result.Length;
								ctx.Response.OutputStream.Write(Encoding.UTF8.GetBytes(result), 0, result.Length);
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.ToString());
							}
							finally
							{
								// always close the stream
								ctx.Response.OutputStream.Close();
							}
						}, listener.GetContext());
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($"Exception : {e.Message}");
				}
			});

			try
			{
				while (true)
				{
					//Do nothing
					Thread.Sleep(1000); //Fix for #3
				}
			}
			finally
			{
				listener.Stop();
				listener.Close();
			}
		}


		private static byte[] RunMgcb(string code, string platform, out string error)
		{
			string[] platforms =
			{
				"DesktopGL",
				"MacOSX",
				"Android",
				"iOS",
				"tvOS",
				"OUYA",
			};
			error = string.Empty;
			var tempPath = Path.GetFileName(Path.ChangeExtension(Path.GetTempFileName(), ".fx"));
			var xnb = Path.ChangeExtension(tempPath, ".mgfx");
			var tempOutput = Path.GetTempPath();
			File.WriteAllText(Path.Combine(tempOutput, tempPath), code);
			var startInfo = new ProcessStartInfo();
			var programFilesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			startInfo.FileName =
				Path.Combine(programFilesDirectory, "MSBuild", "MonoGame", "v3.0", "Tools", "2MGFX.exe");
			startInfo.WorkingDirectory = tempOutput;
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardError = true;
			var profile = platforms.Contains(platform) ? "OpenGL" : "DirectX_11";
			startInfo.Arguments = $"./{tempPath} ./{xnb} /Profile:{profile}";
			var process = new Process {StartInfo = startInfo};
			process.Start();
			try
			{
				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					error = process.StandardError.ReadToEnd();
					Console.WriteLine(error);
				} 
				else if (File.Exists(Path.Combine(tempOutput, xnb)))
				{
					return File.ReadAllBytes(Path.Combine(tempOutput, xnb));
				}
			}
			catch (Exception ex)
			{
				error = ex.ToString();
				Console.WriteLine(ex.ToString());
			}
			finally
			{
				File.Delete(Path.Combine(tempOutput, tempPath));
				File.Delete(Path.Combine(tempOutput, xnb));
			}

			return new byte[0];
		}
	}
}

namespace MonoGameUniversalEffects.Pipeline.Processors {
	public class Data {
		public string Platform { get; set; }
		public string Code { get; set; }
	}

	public class Result
	{
		public byte[] Compiled { get; set;  }
		public string Error { get; set;  }
	}
}
