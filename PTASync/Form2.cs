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

using CalSync2;
using Google.Apis.Authentication;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using PTASync;

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
		CalendarService cs;
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

			State = StateEnum.Starting;
			//notifyIcon1.ContextMenu = notificationMenu;
			 auth=new frmAuth();
			 cs=auth.GetService();
			 Debug.Print(cs.Calendars.Get("primary").Fetch().Id + " " + "to trigger auth");
			//cs=(CalendarService)auth.Invoke(new CalServiceInvoker(auth.GetService));
			
			backgroundWorker1.RunWorkerAsync();
		}
		frmAuth auth;
		const string TITLE_CONFIRM_REPLACE = "Confirm File Replace";
		const string TITLE_COPYING = "Copying...";
		delegate CalendarService CalServiceInvoker();
		int EventComparison (Event x,Event y){		
			/*
			 * Less than 0  x is less than y.
				0  x equals y.
				Greater than 0 x is greater than y.*/
			int r=x.ICalUID.CompareTo(y.ICalUID);
			return r;
		}
		void BackgroundWorker1DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{

			backgroundWorker1.ReportProgress(0, "Initializing");
			string calId=GetId();
			Events gAppts=cs.Events.List(calId).Fetch();
			List<Event> sortedGAppts= new List<Event>(gAppts.Items);
			sortedGAppts.Sort(new Comparison<Event>(EventComparison));
			using (StreamWriter sw=File.CreateText(Path.GetTempFileName()))
			{
				for (int i=0; i<sortedGAppts.Count;i++ ) {
					Event ev=sortedGAppts[i];
					sw.WriteLine(string.Format("{0}\t{1}\t{2}",ev.ICalUID,ev.Start.ToString(),ev.Summary));
				}
			}
//			FileInfo icalFile = new FileInfo(Environment.GetEnvironmentVariable("temp") + "\\newcal.ics");
//			string httpHost = @"home.comcast.net";
//			string ftpHost = @"upload.comcast.net";
//			string userDir = @"/~queler12";
//			string folderPath = @"/calendar";
//			string user = "queler12";
//			string pass = GetPassword();
//			UriBuilder ftpUri = new UriBuilder("ftp", ftpHost);
//			ftpUri.Path = folderPath;
//			ftpUri.UserName = user;
//			ftpUri.Password = pass;
//			UriBuilder httpUri = new UriBuilder("http", httpHost);
//            httpUri.Path = userDir + folderPath + '/' + icalFile.Name;
            
            
/*            
            backgroundWorker1.ReportProgress(0, "Saving");
            this.State = StateEnum.Working;
            
            
            Core.SaveCalendarToDisk(icalFile.ToString());
            
            
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
					Settings.Default.PASS_SET = false;
					throw new NullReferenceException("No password set");
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
			textBox1.Text += DateTime.Now.ToString() + ":" + text + "\r\n";
			notifyIcon1.Text=text;
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
			Application.Exit();
		}
		
		void Form2Shown(object sender, EventArgs e)
		{
			this.Hide();
		}
	}
}
