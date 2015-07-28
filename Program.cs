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
		private static readonly string WAITER_NAME = @"Global\FlaToSwfWaiter";

		[STAThread]
		static int Main(string[] args)
		{
			if (args.Length > 0)
			{
				if (args[0] == "-complete")
				{
					try
					{
						var waiter = EventWaitHandle.OpenExisting(WAITER_NAME);
						waiter.Set();
					}
					catch (WaitHandleCannotBeOpenedException) {}
				}
				else
				{
					var users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
					var rule = new EventWaitHandleAccessRule(users, EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify, AccessControlType.Allow);
					var security = new EventWaitHandleSecurity();
					security.AddAccessRule(rule);

					bool created;
					var waiter = new EventWaitHandle(false, EventResetMode.AutoReset, WAITER_NAME, out created, security);
					var jsflFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".jsfl";
					
					publish(args[0], jsflFilePath);
					
					waiter.WaitOne();
					try { File.Delete(jsflFilePath); } catch { }
				}
				
				return 0;
			}

			return 1;
		}

		static void publish(string flaPath, string tempJSFLfilepath)
		{
			var fileuri = "file:///" + flaPath.Replace("\\", "/");
			var template = Encoding.Default.GetString(FlaToSwf.Properties.Resources.TemplateJsfl)
				.Replace("{FILE_PATH_TO_OPEN}", HttpUtility.JavaScriptStringEncode(fileuri))
				.Replace("{COMPLETE_COMMAND}", HttpUtility.JavaScriptStringEncode("\"" + Environment.GetCommandLineArgs()[0] + "\"" + " -complete"));
			File.WriteAllText(tempJSFLfilepath, template);
			Process.Start(tempJSFLfilepath);
		}
	}
}