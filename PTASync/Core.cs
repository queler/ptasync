/*
 * Created by SharpDevelop.
 * User: aqueler
 * Date: 3/4/2013
 * Time: 11:01 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace PTASync
{
	/// <summary>
	/// Description of Core.
	/// </summary>
	public static class Core
	{
		static Outlook.Application oApp = new Outlook.ApplicationClass();
		static Core()
		{

		}

		
		public static DateTime lastModNetFtp(Uri ftpfile)
		{
			WebRequest ftp = WebRequest.Create(ftpfile);
			WebException e=null;
			ftp.Method = WebRequestMethods.Ftp.GetDateTimestamp;
			FtpWebResponse res;
			try
			{
				res = (FtpWebResponse)ftp.GetResponse();

			}
			catch (WebException webExcpetion)
			{
				res=(FtpWebResponse)webExcpetion.Response;
				e=webExcpetion;
			}
			switch (res.StatusCode){
				case FtpStatusCode.ActionNotTakenFileUnavailable:
					return DateTime.MinValue;
					
				case FtpStatusCode.FileStatus:
					return res.LastModified;
				case FtpStatusCode.NeedLoginAccount:
				case FtpStatusCode.AccountNeeded:
				case FtpStatusCode.NotLoggedIn:
					if (System.Windows.Forms.MessageBox.Show("Login issues\nDelete credentials and exit?", "FTP", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
					{
						Settings.Default.Reset();
						MessageBox.Show("Cleared. Exitting.");
						throw new ApplicationException("Pass reset");
					}
					else
					{
						throw new ApplicationException("bad creds");
					}
				default:
					throw e??new WebException("Unexpected code and no exception");;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://msdn.microsoft.com/en-us/library/office/bb647583%28v=office.14%29.aspx">
		/// <param name="calendarFileName"></param>
		public static void SaveCalendarToDisk(string calendarFileName)
		{
			if (string.IsNullOrEmpty(calendarFileName))
				throw new ArgumentException("calendarFileName",
				                            "Parameter must contain a value.");

			
			Outlook.Folder calendar = oApp.Session.GetDefaultFolder(
				Outlook.OlDefaultFolders.olFolderCalendar) as Outlook.Folder;
			AppointmentItem updated=(AppointmentItem)oApp.CreateItem(OlItemType.olAppointmentItem);
				updated.Start=DateTime.Now;
				updated.Duration=0;
				updated.AllDayEvent=false;
				updated.BusyStatus= OlBusyStatus.olFree;
				updated.ReminderSet=false;
				updated.Subject="***UPDATED***";
					
				updated.Save();
			
			Outlook.CalendarSharing exporter = calendar.GetCalendarExporter();

			// Set the properties for the export
			exporter.CalendarDetail = Outlook.OlCalendarDetail.olFullDetails;
			exporter.IncludeAttachments = true;
			exporter.IncludePrivateDetails = true;
			exporter.RestrictToWorkingHours = false;
			exporter.IncludeWholeCalendar = true;


			// Save the calendar to disk
			exporter.SaveAsICal(calendarFileName);
			//updated.Delete();
			permDel(updated);
				Marshal.ReleaseComObject(updated);
		}
		public static void permDel(AppointmentItem item)
		{
			MAPIFolder calItems=(MAPIFolder)item.Parent;
			for (int i=1;i<=calItems.Items.Count ;i++ ) {
				string testuid=item.GetType().InvokeMember("EntryID",BindingFlags.GetProperty,null,calItems.Items[i],null).ToString();
				if (testuid==item.EntryID) {
					calItems.Items.Remove(i);
					return;
				}
			}
			throw new ApplicationException("Adam can't program");
		}
		public static void delTest()
		{
			Outlook.Folder calendar = oApp.Session.GetDefaultFolder(
				Outlook.OlDefaultFolders.olFolderCalendar) as Outlook.Folder;
			AppointmentItem updated=(AppointmentItem)oApp.CreateItem(OlItemType.olAppointmentItem);
				updated.Start=DateTime.Now;
				updated.Duration=0;
				updated.AllDayEvent=false;
				updated.BusyStatus= OlBusyStatus.olFree;
				updated.ReminderSet=false;
				updated.Subject="***Calendar Updated***";
					
				updated.Save();
				permDel(updated);
		}
		public static void UploadToFtp(Uri ftpUrl, Uri httpUri, FileInfo f, BackgroundWorker bw)
		{
			Shell32.ShellClass shell = new Shell32.ShellClass();
			//Shell32.Folder ftp=shell.NameSpace(url.ToString());
			Shell32.Folder ftp = (Shell32.Folder)shell.GetType().InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell,
			                                                                  new object[] { ftpUrl.ToString() });

			//SystemWindow[] windows;
			//while (true)
			//{
			//    windows = SystemWindow.FilterToplevelWindows(IsCopyingWindow);
			//    if (windows.Length > 0)
			//    {
			//        if (bw != null) bw.ReportProgress(0, "Sleep while other files are copying (" + windows.Length + ")");
			//    }
			//    else
			//    {
			//        if (bw != null) bw.ReportProgress(0, "No other copying, prepare to copy");
			//        break;
			//    }

			//}
			DateTime copied=lastMod(httpUri,ftpUrl,f);
			ftp.CopyHere(f.FullName, 20);
			
			bw.ReportProgress(0, "Upload Started: " + copied.ToShortTimeString());
			for (int loops = 0; loops < 12; loops++)
			{
				DateTime lm = Core.lastMod(httpUri,ftpUrl,f);
				
				if (lm>=copied)
				{
					bw.ReportProgress(0, "timestamps matchish");
					return;
				}
				else
				{
					bw.ReportProgress(0,String.Format("server time={0},loops={1}",lm.ToShortTimeString(),loops));
					Thread.Sleep(10000);
				}
			}
			throw new TimeoutException("file not verified");
			

		}
		const string TITLE_CONFIRM_REPLACE = "Confirm File Replace";
		const string TITLE_COPYING = "Copying...";
		//        public static bool IsCopyingWindow(SystemWindow sw)
		//        {
		//            return sw.Title == TITLE_CONFIRM_REPLACE || sw.Title == TITLE_COPYING;
		//        }
		public static void DeleteFtpShell(Uri ftpUrl, FileInfo f)
		{
			Shell32.ShellClass shell = new Shell32.ShellClass();
			//Shell32.Folder ftp=shell.NameSpace(url.ToString());
			Shell32.Folder ftp = (Shell32.Folder)shell.GetType().InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell,
			                                                                  new object[] { ftpUrl.ToString() });
			foreach (Shell32.FolderItem2 file in ftp.Items())
			{
				if (file.Name == f.Name)
				{
					bool verbFound = false;
					foreach (Shell32.FolderItemVerb item in file.Verbs())
					{
						if (item.Name.Replace("&", "").Equals("Delete", StringComparison.CurrentCultureIgnoreCase))
						{
							item.DoIt();
							verbFound = true;
							break;
						}
					}
					if (!verbFound)
					{
						throw new ApplicationException("Delete verb not found, that's not right");
					}
				}

			}
		}
		public static DateTime lastMod(Uri http,Uri ftp,FileInfo f)
		{

			return lastModNetFtp(AddFileToFtpUrl(ftp,f));
		}
		public static DateTime lastModNet (Uri url)
		{
			HttpWebRequest req=(HttpWebRequest)WebRequest.Create(url);
			req.Method=WebRequestMethods.Http.Head;
			req.UseDefaultCredentials=true;
			req.Proxy=WebRequest.GetSystemWebProxy();
			req.AllowAutoRedirect=true;
			req.MaximumAutomaticRedirections=200;
			req.UserAgent="Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3)";
			WebResponse res=			req.GetResponse();
			DateTime lm=DateTime.Parse(res.Headers[HttpResponseHeader.LastModified]);
			return lm;
		}

		static DateTime lastModWinHttp(Uri url)
		{
			WinHttp.WinHttpRequest http = new WinHttp.WinHttpRequestClass();
			http.Open("get", url.ToString(), false);
			http.SetAutoLogonPolicy(WinHttp.WinHttpRequestAutoLogonPolicy.AutoLogonPolicy_Always);
			object nothing = System.Type.Missing;
			http.Send(nothing);
			DateTime res = DateTime.Parse(http.GetResponseHeader("Last-Modified"));
			return res;
		}

		internal static string DeleteNet(Uri ftpUri, FileInfo icalFile)
		{
			Uri ftpfile =
				AddFileToFtpUrl(ftpUri, icalFile);
			WebRequest ftp = WebRequest.Create(ftpfile);

			ftp.Method = WebRequestMethods.Ftp.DeleteFile;

			try
			{
				FtpWebResponse res = (FtpWebResponse)ftp.GetResponse();
				if (res.StatusCode != FtpStatusCode.FileActionOK)
				{
					throw new WebException("not ok on ftp delete");
				}
				return "File Deleted";
			}
			catch (WebException e)
			{
				FtpWebResponse res = (FtpWebResponse)e.Response;
				switch (res.StatusCode)
				{
					case FtpStatusCode.ActionNotTakenFileUnavailable:
						return "File not found to delete";

						
					case FtpStatusCode.NeedLoginAccount:
					case FtpStatusCode.AccountNeeded:
					case FtpStatusCode.NotLoggedIn:
						if (System.Windows.Forms.MessageBox.Show("Login issues\nDelete credentials and exit?", "FTP", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
						{
							Settings.Default.Reset();
							MessageBox.Show("Cleared. Exitting.");
							throw new ApplicationException("Pass reset");
						}
						else
						{
							throw new ApplicationException("bad creds");
						}
						
					default:
						
						throw new WebException("Something else went wrong on the delete", e);
				}

			}
		}

		static Uri AddFileToFtpUrl(Uri baseFtp, FileInfo icalFile)
		{
			return new Uri(baseFtp + "/" + icalFile.Name);
		}
		internal static void TzFix(string file)
		{
			StringBuilder strFile=new StringBuilder(File.ReadAllText(file));
			strFile.Replace("TZID=:","TZID=ET:");
			const string badTZ="BEGIN:VTIMEZONE\r\nTZID:\r\n";
			const string goodTZ="BEGIN:VTIMEZONE\r\nTZID:ET\r\n";
			strFile.Replace(badTZ,goodTZ);
			strFile.Replace('\t',' ');
			File.WriteAllText(file,strFile.ToString());

		}
	}
}
