/*
 * Created by SharpDevelop.
 * User: aqueler
 * Date: 11/14/2013
 * Time: 2:57 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Google.Apis.Calendar.v3.Data;
using Microsoft.Office.Interop.Excel;

namespace PTASync
{
	/// <summary>
	/// Description of Schedule.
	/// </summary>
	public class Schedule
	{
		object missing=Type.Missing;
		public IDictionary<DateTime, Event>Events {
			get { return events; }
		}
		
		
		private SortedList<DateTime, Event> events=new SortedList<DateTime, Event>(16*20);
		public Schedule()
		{
			init();
		}
		private void init()
		{
			
		}
		public void ReadFromXL(string file)
		{
			
			
			
			Application xl=new ApplicationClass();
			try {
				try {
					xl.Visible=true;
					Workbook book=xl.Workbooks.Open(file,
					                                0, true, missing, missing, missing, missing, missing,
					                                missing, missing, missing, missing, missing,missing, missing);
					
					
				}
				finally {
//					foreach(Workbook w in xl.Workbooks)
//					{
//						object opt=Type.Missing;
//						w.Close(false,opt,opt);
//
//					}
					xl.DisplayAlerts=false;
					xl.Quit();
					Marshal.ReleaseComObject(xl);
				}
			} catch (Exception ) {
				throw;
			}
		}
		static void Main(){
			new Schedule().ReadFromXL(@"\\w-pattr-002\AcademyDocs\USPTA_schedules\Nov_4th_Entry-Level-Phase 1_Training _Schedule 10-21-13.xls");
			
		}
	}

}
