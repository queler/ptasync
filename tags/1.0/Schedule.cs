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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Excel;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;

namespace PTASync
{
	/// <summary>
	/// Description of Schedule.
	/// </summary>
	public class Schedule
	{
		object missing = Type.Missing;
		public IList<IEvent> Events
		{
			get { return  events; }
		}

		
		private IList<IEvent> events = new List<IEvent>(16 * 20);
		public Schedule()
		{
			init();
		}
		private void init()
		{

		}
		public void ReadFromXL(string file)
		{



			Application xl = new ApplicationClass();
			try
			{
				try
				{
//			xl.Visible = true;
			Workbook book = xl.Workbooks.Open(file,
			                                  0, true, missing, missing, missing, missing, missing,
			                                  missing, missing, missing, missing, missing, missing, missing);
			DateTime curDay = DateTime.MinValue;
			DateTime tempDate;
			foreach (Worksheet sheet in book.Sheets)
			{
				Debug.Print(sheet.Name);
				Range row = GetFirstDayRange(sheet);
				
				if (row != null)
				{

					IEvent toAdd;
					
					while (!(FirstNonBlankColStr(row)=="" && FirstNonBlankColStr(row.get_Offset(1,0))==""))
					{
						
						if (row!=null) {
							
							string rowAddress=row.get_Address(false, false, Microsoft.Office.Interop.Excel.XlReferenceStyle.xlA1, false, false);
						}
						tempDate = GetDate(row);
						if (tempDate!=DateTime.MinValue)
						{// set the date and advance
							curDay = tempDate;
							{
								
							}
						}
						else
						{//it's an allday or the next appt
							toAdd = new PTEvent();
							DateTime tempTime;
							if (GetTime(row.Value2,out tempTime))
							{
								toAdd.Start = curDay.Date+ tempTime.TimeOfDay;
							
								toAdd.End = curDay + GetTime(row.get_Offset(0, 1).Value2).TimeOfDay;
								toAdd.TitleRaw = (row.get_Offset(0, 2).Value2??"").ToString();
								toAdd.Trainer = (row.get_Offset(0, 4).Value2?? "").ToString() ;

								toAdd.Location = (row.get_Offset(0, 5).Value2??"").ToString();

							}
							
							else
							{
								toAdd.TitleRaw=FirstNonBlankColStr(row);
								toAdd.Start=curDay.Date;
								toAdd.End=curDay.Date;
								toAdd.AllDay=true;
								
							}
							if (toAdd.TitleRaw!="")
							{
								this.events.Add(toAdd);
								
							}
						}



						row = row.get_Offset(1, 0);
					}
				}
			}

				}
				finally
				{
			//					foreach(Workbook w in xl.Workbooks)
			//					{
			//						object opt=Type.Missing;
			//						w.Close(false,opt,opt);
			//
			//					}
			////					xl.DisplayAlerts = false;
					xl.Quit();
					Marshal.ReleaseComObject(xl);
				}
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		DateTime GetTime(object value2)
		{
			DateTime temp;
			if (GetTime(value2,out temp)) {
				return temp	;
			}
			else
			{
				throw new ArgumentException("not a valid time");
			}
		}
		
		/// <summary>
		/// Takes Value2 from a Range and returns true if it is a double or if a time can be parsed., only the timepart will be set
		/// </summary>
		/// <param name="value2"></param>
		/// <param name="tempTime"></param>
		/// <returns></returns>
		bool GetTime(object value2,out DateTime tempTime)
		{
			tempTime=DateTime.MinValue;
			if (value2 == null) {
				
				return false;
			}
			else if (value2 is double) {
				tempTime=DateTime.FromOADate((Double)value2);
				return true;
			}
			else if (value2.ToString().Trim()=="") {
				return false;
			}
			else
			{
				return DateTime.TryParse(value2.ToString(),out tempTime);
			}
				
			
			
		}
		
		String FirstNonBlankColStr(Range row)
		{
			string toAddTitleRaw = "";
			for (int i = 0; i < 5; i++)
			{
				
				string val=(row.get_Offset(0, i).Value2??"").ToString();
				if (!val.Equals(""))
				{
					toAddTitleRaw = val;
					break;
				}
			}
			return toAddTitleRaw;
			
		}
		private static DateTime combineDateAndXLTime(DateTime curDay, double timePart)
		{
			DateTime time = DateTime.FromOADate(timePart);
			DateTime start = curDay.Date.Add(time.TimeOfDay);


			return start;
		}
		Regex date = new Regex(@"((?:Jan|Feb|Mar|Jun|Jul|Aug|Sep|Oct|Nov|Dec).*?\d+.*?\d\d\d\d)");

		private DateTime GetDate(Range row)
		{
			DateTime result = DateTime.MinValue;
			Match m = date.Match(FirstNonBlankColStr(row));
			DateTime.TryParse(m.Value, out result);
			return result;
		}

		private Range GetFirstDayRange(Worksheet sheet)
		{
			Range cell=(Range)sheet.Cells[6, 1];
			if (GetDate(cell)==DateTime.MinValue)
			{
				return null;
			}
			else
			{
				return cell;
			}
		}
		internal void Dump(string filename)
		{
			using(StreamWriter sw=new StreamWriter(filename))
			{
				foreach(IEvent ev in this.Events)
				{
					sw.WriteLine(ev.ToString());
				}
			}
		}
        public void ToIcalFile(string filename)
        {
            iCalendar ical = new iCalendar();
            ical.AddLocalTimeZone();
            //ical.Name="PTAcadamy";
            foreach (IEvent evt in this.Events)
            {
                Event icalEvent = ical.Create<Event>();
                icalEvent.Start = new iCalDateTime( evt.Start);
                icalEvent.Summary = evt.TitleOut;
                if (evt.AllDay)
                {
                    icalEvent.IsAllDay = true;
                }
                else
                {
                    icalEvent.End = new iCalDateTime( evt.End);
                }
                icalEvent.Description = "Trainer:" + evt.Trainer;
                icalEvent.Location = evt.Location;
            }
         
            iCalendarSerializer writer = new iCalendarSerializer();
            writer.Serialize(ical,filename);
        }
		static void Main()
		{
			Schedule s=new Schedule();
			s.ReadFromXL(@"\\w-pattr-002\AcademyDocs\USPTA_schedules\Nov_4th_Entry-Level-Phase 1_Training _Schedule 10-21-13.xls");
			//s.ReadFromXL(@"\\quelertime\shareddocs\tashed.xlsb");
            s.ToIcalFile(Path.GetTempPath() + Path.DirectorySeparatorChar + @"sked.ics");
			
		}
	}
	
}
