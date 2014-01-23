/*
 * Created by SharpDevelop.
 * User: aqueler
 * Date: 12/6/10
 * Time: 4:27 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace PTASync
{
    public sealed class Program
    {
        #region Main - Program entry point
        /// <summary>Program entry point.</summary>
        /// <param name="args">Command Line Arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            FormMain();

        }

        [STAThread]
        private static void FormMain()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form2());
        }

    }

        #endregion


    public enum StateEnum
    {
        Starting, Working, Up, Done, Error
    }
}
//

