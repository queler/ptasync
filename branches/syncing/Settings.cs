using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace PTASync
{
    /// <summary>
    /// Description of Settings.
    /// </summary>
    public class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            [System.Diagnostics.DebuggerStepThrough()]
            get
            {
                return defaultInstance;
            }
        }

        public Settings()
            : base()
        {
        }
        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("false")]
        public bool PASS_SET
        {
            [System.Diagnostics.DebuggerStepThrough()]
            get { return (bool)this["PASS_SET"]; }
            [System.Diagnostics.DebuggerStepThrough()]
        set { this["PASS_SET"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        /// <summary>
        /// <returns> null if !PASS_SET</returns>
        /// </summary>
        public string Password
        {
            [System.Diagnostics.DebuggerStepThrough()]
            get
            {
                return (PASS_SET) ? (DecryptString(this["Password"].ToString())) : null;
            }
            [System.Diagnostics.DebuggerStepThrough()]
            set { this["Password"] = EncryptString((value)); PASS_SET = true; }
        }
       [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public string CalId
        {
            [System.Diagnostics.DebuggerStepThrough()]
            get
            {
            	return (string) this["CalId"];
            }
            [System.Diagnostics.DebuggerStepThrough()]
            set { this["Password"] = value;}
        }
        static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("Bumblebee Tuna, I can see your balls!");

        public static string EncryptString(string input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
             System.Text.Encoding.Unicode.GetBytes((input)),
             entropy,
             System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static String DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                 Convert.FromBase64String(encryptedData),
                 entropy,
                 System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return (System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return null;
            }
        }
    }
}
