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


namespace PTASync
{
	/// <summary>
	/// Description of Core.
	/// </summary>
	public static class Core
	{
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
