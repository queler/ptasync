/*
 * Created by SharpDevelop.
 * User: aqueler
 * Date: 3/1/2013
 * Time: 12:27 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Util;
using MSHTML;

namespace CalSync2
{
	/// <summary>
	/// Description of frmAuth.
	/// </summary>
	public partial class frmAuth : Form
	{
		public frmAuth()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			if (!refreshToken.Directory.Exists)
			{
				refreshToken.Directory.Create();
			}
			
		}
		public CalendarService GetService()
		{
			// Register the authenticator. The Client ID and secret have to be copied from the API Access
			// tab on the Google APIs Console.
			var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description);
			provider.ClientIdentifier = "684359767455-v0thml7452r01npi7eer68es531vmc42.apps.googleusercontent.com";
			provider.ClientSecret = "a9nwPwbud9vlxPjxSxFwIWxA";
			var auth=new OAuth2Authenticator<NativeApplicationClient>(provider,GetAuthentication);

			// Create the service. This will automatically call the previously registered authenticator.
			var service = new CalendarService(auth);
			return service;
		}
		string authCode=null;

		private  IAuthorizationState GetAuthentication(NativeApplicationClient arg)
		{
			// Get the auth URL:
			IAuthorizationState state = new AuthorizationState(new[] { CalendarService.Scopes.Calendar.GetStringValue() });
			state.Callback = new Uri(NativeApplicationClient.OutOfBandCallbackUrl);
			
			string refreshToken = LoadRefreshToken();
			if (!String.IsNullOrEmpty(refreshToken))
			{
				state.RefreshToken = refreshToken;

				if (arg.RefreshToken(state,null))
					return state;
			}

			Uri authUri = arg.RequestUserAuthorization(state);
			web.DocumentCompleted+= new WebBrowserDocumentCompletedEventHandler(web_DocumentCompleted);
			
			web.Navigate(authUri);
			this.Show();
			
			while (true) {
				try {
					
					if (web.Document.Title.StartsWith("Success code")) {
						authCode=web.Document.Title.Substring(13);
						break;
					}
					else
					{
						Application.DoEvents();
					}
				} catch (Exception) {
					Application.DoEvents();
				}
				
			}
			//authCode=web.Document.GetElementById("code").Value;
			//Console.WriteLine();
			
			// Retrieve the access token by using the authorization code:
			this.Hide();
			web.Dispose();
			
			IAuthorizationState result= arg.ProcessUserAuthorization(authCode, state);
			StoreRefreshToken(result);
			return result;
		}
		FileInfo refreshToken=new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+@"\PTASync\refreshtoken");
		
		private  byte[] aditionalEntropy = { 1, 2, 3, 4, 5 };
		private  string LoadRefreshToken()
		{
			if (refreshToken.Exists) {
				
				
				return Encoding.Unicode.GetString(
					ProtectedData.Unprotect(
						Convert.FromBase64String(
							File.ReadAllText(
								refreshToken.FullName
							)
						),
						aditionalEntropy, DataProtectionScope.CurrentUser
					)
					
				);
			}
			else
			{
				return "";
			}
		}

		private  void StoreRefreshToken(IAuthorizationState state)
		{
			File.WriteAllText(refreshToken.FullName,Convert.ToBase64String(ProtectedData.Protect(Encoding.Unicode.GetBytes(state.RefreshToken), aditionalEntropy, DataProtectionScope.CurrentUser))
			                 );
		}
		void web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			HtmlDocument doc=web.Document;

			if (doc.Title.Equals("Request for Permission")) {
				
				IHTMLElement3 button3= (IHTMLElement3)doc.GetElementById("submit_approve_access").DomElement;
				
				IHTMLElement button1=(IHTMLElement)button3;
				button1.click();
				
				
			}else if (doc.Title.StartsWith("Success Code")) {
				authCode=doc.GetElementById("code").InnerText;
				
			}
		}
		[STAThread]
		public static void Main()
		{
			CalendarService cs=new frmAuth().GetService();
			Console.WriteLine(			cs.Calendars.Get("queler@gmail.com").Fetch().Id);
			Console.ReadLine();
		}
	}
}
