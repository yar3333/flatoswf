using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;

namespace FlaToSwf
{
	static class Program
	{
		const string WAITER_NAME = @"Global\FlaToSwfWaiter";

		[STAThread]
		static int Main(string[] args)
		{
			if (args.Length > 0)
			{
				if (args[0] == "-complete")
				{
					try { EventWaitHandle.OpenExisting(WAITER_NAME).Set(); }
					catch (WaitHandleCannotBeOpenedException) {}
				}
				else if (args[0] == "-start")
				{
					runJsfl("");
				}
				else if (args[0] == "-quit")
				{
					runJsfl("fl.quit();");
				}
				else if (args[0].StartsWith("-"))
				{
					Console.WriteLine("Unknow switch '" + args[0] + "'.");
					return 1;
				}
				else
				{
					runJsfl
					(
						"var doc = fl.openDocument('file:///" + Path.GetFullPath(args[0].Replace("/", "\\")).Replace("\\", "/") + "');\n" +
						"doc.publish();\n" +
						"doc.close();\n"
					);
				}
				
				return 0;
			}
			else
			{
				Console.WriteLine("Usage: FlaToSwf <pathToSourceFla> | -start | -quit");
				Console.WriteLine("\t<pathToSourceFla> Fla document to publish. This can be also *.xfl document.");
				Console.WriteLine("\t-start            Just run the Flash (don't need in regular).");
				Console.WriteLine("\t-quit             Send 'quit' command to the Flash (don't need in regular).");
			}

			return 1;
		}

		static void runJsfl(string jsfl)
		{
			jsfl += "FLfile.runCommandLine('" + HttpUtility.JavaScriptStringEncode("\"" + Environment.GetCommandLineArgs()[0] + "\"" + " -complete") + "');";

			var users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
			var rule = new EventWaitHandleAccessRule(users, EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify, AccessControlType.Allow);
			var security = new EventWaitHandleSecurity();
			security.AddAccessRule(rule);

			bool created;
			var waiter = new EventWaitHandle(false, EventResetMode.AutoReset, WAITER_NAME, out created, security);
			var jsflFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".jsfl";

			File.WriteAllText(jsflFilePath, jsfl);
			Process.Start(jsflFilePath);
					
			waiter.WaitOne();
			try { File.Delete(jsflFilePath); } catch { }
		}
	}
}