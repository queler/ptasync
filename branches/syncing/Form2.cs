/*
 * Created by SharpDevelop.
 * User: aqueler
 * Date: 3/5/2013
 * Time: 3:49 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace PTASync
{
	/// <summary>
	/// Description of Form2.
	/// </summary>
	public partial class Form2 : Form
	{

		Dictionary<StateEnum, Icon> images;
		public Form2()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
//		CalendarService cs;
		void Form2Load(object sender, EventArgs e)
		{
			//ContextMenu notificationMenu = new ContextMenu(InitializeMenu());
			
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Program));
			images = new Dictionary<StateEnum, Icon>(3);
			images.Add(StateEnum.Working, Resources.work);
			images.Add(StateEnum.Done, Resources.done);
			images.Add(StateEnum.Error, Resources.error);
			images.Add(StateEnum.Starting, Resources.cal);
			images.Add(StateEnum.Up, Resources.upload);

//			State = StateEnum.Starting;
//			//notifyIcon1.ContextMenu = notificationMenu;
//			 auth=new frmAuth();
//			 cs=auth.GetService();
//			 Debug.Print(cs.Calendars.Get("primary").Fetch().Id + " " + "to trigger auth");
//			//cs=(CalendarService)auth.Invoke(new CalServiceInvoker(auth.GetService));
//
			backgroundWorker1.RunWorkerAsync();
		}
		//frmAuth auth;
		const string TITLE_CONFIRM_REPLACE = "Confirm File Replace";
		const string TITLE_COPYING = "Copying...";
		////		delegate CalendarService CalServiceInvoker();
//		int EventComparison (Event x,Event y){
//			/*
//			 * Less than 0  x is less than y.
//				0  x equals y.
//				Greater than 0 x is greater than y.*/
//			int r=x.ICalUID.CompareTo(y.ICalUID);
//			return r;
//		}
		void BackgroundWorker1DoWorkSync(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			FileInfo icalFile = createIcalFile();
			ProcessStartInfo psi=new ProcessStartInfo();
			//java -cp "lib\gdlib\*;." IcalRemoteUserPass C:\Users\aqueler\AppData\Local\Temp\newcal.ics https://www.google.com/calendar/ical/3c74a7m3h6dqvtb5mltalqfa24%40group.calendar.google.com/private-e83945b635f57e5536bfdb75414538bd/basic.ics queler@gmail.com killr0bb
			string installPath = GetJavaInstallationPath();
			string javaPath = System.IO.Path.Combine(installPath, "bin\\Java.exe");
			if (!System.IO.File.Exists(javaPath))
			{
				throw new ApplicationException("Java not found");
			}
			psi.FileName=javaPath;
			psi.UseShellExecute=false;
			psi.RedirectStandardOutput=true;
			psi.RedirectStandardError=true;
			psi.WorkingDirectory=Environment.CurrentDirectory;
			string execPath=Environment.CurrentDirectory;
			string calid=GetId();
			string user=GetGName();
			string pass=GetPassword();
			psi.Arguments=String.Format("-cp \"{0}\\*;.\" IcalRemoteUserPass {1} {2} {3} {4}",execPath,icalFile,calid,user,pass);
			Process p=new Process();
		
			p.OutputDataReceived+= new DataReceivedEventHandler(p_OutputDataReceived);
			p.ErrorDataReceived+= new DataReceivedEventHandler(p_OutputDataReceived);
			this.State=StateEnum.Up;
			
				p.StartInfo=psi;
			
				p.Start();
				p.BeginOutputReadLine();
				p.WaitForExit();
				if (p.ExitCode!=0) {
					throw new ApplicationException("Error in java program");
				}
			
			
		}

		void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			backgroundWorker1.ReportProgress(1,e.Data);
		}
		private string GetJavaInstallationPath()
		{
		    string environmentPath = Environment.GetEnvironmentVariable("JAVA_HOME");
		    if (!string.IsNullOrEmpty(environmentPath))
		    {
		       return environmentPath;
		    }
		
		    string javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment\\";
		    using (Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(javaKey))
		    {
		        string currentVersion = rk.GetValue("CurrentVersion").ToString();
		        using (Microsoft.Win32.RegistryKey key = rk.OpenSubKey(currentVersion))
		        {
		            return key.GetValue("JavaHome").ToString();
		        }
		    }
		}
		void BackgroundWorker1DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{

			throw new NotImplementedException();
			/*
			  FileInfo icalFile = createIcalFile();
			string httpHost = @"home.comcast.net";
			string ftpHost = @"upload.comcast.net";
			string userDir = @"/~queler12";
			string folderPath = @"/calendar";
			string user = "queler12";
			string pass = GetPassword();
			UriBuilder ftpUri = new UriBuilder("ftp", ftpHost);
			ftpUri.Path = folderPath;
			ftpUri.UserName = user;
			ftpUri.Password = pass;
			UriBuilder httpUri = new UriBuilder("http", httpHost);
			httpUri.Path = userDir + folderPath + '/' + icalFile.Name;
			this.State = StateEnum.Up;
			
			backgroundWorker1.ReportProgress(0, "deleting");
			
			
			string delRes = Core.DeleteNet(ftpUri.Uri, icalFile);
			backgroundWorker1.ReportProgress(0, delRes ?? "");
			backgroundWorker1.ReportProgress(0, "Uploading");

			Core.UploadToFtp(ftpUri.Uri,httpUri.Uri, icalFile, backgroundWorker1);
			
			/*
            
            backgroundWorker1.ReportProgress(0,"Fixing");
            Core.TzFix(icalFile.ToString());
            
            backgroundWorker1.ReportProgress(0,"fixed");
            this.State = StateEnum.Up;
            
            backgroundWorker1.ReportProgress(0, "deleting");
            
            
            string delRes = Core.DeleteNet(ftpUri.Uri, icalFile);
           backgroundWorker1.ReportProgress(0, delRes ?? "");
            backgroundWorker1.ReportProgress(0, "Uploading");

             Core.UploadToFtp(ftpUri.Uri,httpUri.Uri, icalFile, backgroundWorker1);*/
			
		}

		//s.ReadFromXL(@"\\quelertime\shareddocs\tashed.xlsb");
		
		FileInfo createIcalFile()
		{
			string xlFile=System.Configuration.ConfigurationManager.AppSettings["schedulePath"];
			string[] deb=Environment.GetCommandLineArgs();
			if (Environment.GetCommandLineArgs().Length>1)
			{
				string arg0=Environment.GetCommandLineArgs()[1];
				if(File.Exists(arg0)) {
					backgroundWorker1.ReportProgress(0,"using "+arg0);
					xlFile=arg0;
				}
				else
				{
					backgroundWorker1.ReportProgress(0,arg0+" doesn't exist/isn't a valid file, reading app.config for path");
					
				}
				
			}
			else
			{
				backgroundWorker1.ReportProgress(0,"No commandline, reading app.config for path");
			}
			
			backgroundWorker1.ReportProgress(0, "Initializing");
			state = StateEnum.Starting;
			Schedule s = new Schedule();
			backgroundWorker1.ReportProgress(10, "Reading XL");
			this.State = StateEnum.Working;
			s.ReadFromXL(xlFile);
			backgroundWorker1.ReportProgress(30, "Exporting to ics");
			FileInfo icalFile = new FileInfo(Environment.GetEnvironmentVariable("temp") + "\\newcal.ics");
			s.ToIcalFile(icalFile.FullName);
			return icalFile;
		}
		internal string GetId()
		{
			string c=Settings.Default.CalId;
			if ((c ?? "") != "")
			{
				return c;
			}
			else
			{
				Form frmPass = new Form();
				frmPass.Text = "Calendar ID?";
				TextBox tb = new TextBox();
				Button ok = new Button();
				ok.Text = "Ok";
				ok.Dock = DockStyle.Bottom;
				tb.Dock = DockStyle.Top;
				ok.DialogResult = DialogResult.OK;
				frmPass.AcceptButton = ok;
				tb.UseSystemPasswordChar = false;
				frmPass.Controls.Add(tb);
				frmPass.Controls.Add(ok);
				DialogResult res = frmPass.ShowDialog();
				if (res==DialogResult.OK)
				{
					Settings.Default.CalId = tb.Text;
					Settings.Default.Save();
					
					tb.Dispose();
					frmPass.Dispose();
					return tb.Text;
				}
				else
				{
					throw new NullReferenceException("No Calendar set");
				}
			}
		}
		internal string GetGName()
		{
			string c=Settings.Default.GName;
			if ((c ?? "") != "")
			{
				return c;
			}
			else
			{
				Form frmPass = new Form();
				frmPass.Text = "Google Id?";
				TextBox tb = new TextBox();
				Button ok = new Button();
				ok.Text = "Ok";
				ok.Dock = DockStyle.Bottom;
				tb.Dock = DockStyle.Top;
				ok.DialogResult = DialogResult.OK;
				frmPass.AcceptButton = ok;
				tb.UseSystemPasswordChar = false;
				frmPass.Controls.Add(tb);
				frmPass.Controls.Add(ok);
				DialogResult res = frmPass.ShowDialog();
				if (res==DialogResult.OK)
				{
					Settings.Default.GName = tb.Text;
					Settings.Default.Save();
					
					tb.Dispose();
					frmPass.Dispose();
					return tb.Text;
				}
				else
				{
					throw new NullReferenceException("No Gmail Name set");
				}
			}
		}
		internal static string GetPassword()
		{
			if (Settings.Default.PASS_SET)
			{
				return Settings.Default.Password;
			}
			else
			{
				Form frmPass = new Form();
				frmPass.Text = "Password?";
				TextBox tb = new TextBox();
				Button ok = new Button();
				ok.Text = "Ok";
				ok.Dock = DockStyle.Bottom;
				tb.Dock = DockStyle.Top;
				ok.DialogResult = DialogResult.OK;
				frmPass.AcceptButton = ok;
				tb.UseSystemPasswordChar = true;
				frmPass.Controls.Add(tb);
				frmPass.Controls.Add(ok);
				DialogResult res = frmPass.ShowDialog();
				if (res==DialogResult.OK)
				{
					Settings.Default.Password = tb.Text;
					Settings.Default.Save();
					
					tb.Dispose();
					frmPass.Dispose();
					return tb.Text;
				}
				else
				{
					Settings.Default.PASS_SET = false;
					throw new NullReferenceException("No password set");
				}
			}
		}


		private StateEnum state;
		public StateEnum State
		{
			get
			{
				return state;
			}
			set
			{
				state = value;
				notifyIcon1.Icon = images[value];
			}

		}

		public void Status(string text)
		{
			textBox1.AppendText( DateTime.Now.ToString() + ":" + text + "\r\n");
			
			notifyIcon1.Text=State.ToString();
		}

		void BackgroundWorker1ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
			Status((string)(e.UserState));
		}

		void NotifyIcon1Click(object sender, EventArgs e)
		{
			this.Show();
		}

		void Button1Click(object sender, EventArgs e)
		{
			if (State == StateEnum.Error || State == StateEnum.Done)
			{
				Application.Exit();
			}
			else
			{
				this.Hide();
			}
		}

		void BackgroundWorker1RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			
			if (!(null == e.Error))
			{
				this.Show();
				Debug.WriteLine(e.Error.ToString());

				State = StateEnum.Error;
				Status("Error: " + e.Error.Message);
				//backgroundWorker1.CancelAsync();
			}
			else
			{
				State = StateEnum.Done;
				Status("Done");
				timer1.Start();
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (this.Visible) {
				
			}
			else
			{Application.Exit();}
		}
		
		void Form2Shown(object sender, EventArgs e)
		{
			this.Hide();
		}
	}
}
