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
using System.Text.RegularExpressions;

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
                    xl.Visible = true;
                    Workbook book = xl.Workbooks.Open(file,
                                                    0, true, missing, missing, missing, missing, missing,
                                                    missing, missing, missing, missing, missing, missing, missing);
                    DateTime curDay = DateTime.MinValue;
                    DateTime tempDate;
                    foreach (Worksheet sheet in book.Sheets)
                    {
                        Range row = GetFirstDayRange(sheet);
                        if (row != null)
                        {

                            IEvent toAdd;
                            
                            while (!(row.Value2.ToString().Equals("") && row.get_Offset(1,0).Value2.ToString().Equals("")))
                            {
                                tempDate = GetDate(row);
                                if (tempDate!=DateTime.MinValue)
                                {// set the date and advance
                                    curDay = tempDate;
                                }
                                else
                                {//it's an allday or the next appt
                                    toAdd = new PTEvent();
                                    if (row.Value2 is double)
                                    {
                                        toAdd.Start = combineDateAndXLTime(curDay, (Double)row.Value2);
                                        toAdd.End = combineDateAndXLTime(curDay, (Double)row.get_Offset(0, 1).Value2);
                                        toAdd.TitleRaw = row.get_Offset(0, 2).Value2.ToString()??"";
                                        toAdd.Trainer = row.get_Offset(0, 4).Value2.ToString() ?? "";

                                        toAdd.Location = row.get_Offset(0, 5).Value2.ToString() ?? "";

                                    }
                                    else
                                    {
                                        toAdd.TitleRaw = "";
                                        for (int i = 0; i < 5; i++)
                                        {
                                            
                                            string val=row.get_Offset(0, i).Value2.ToString();
                                            if (!val.Equals("")) 
                                            {
                                                toAdd.TitleRaw = val;
                                                break;
                                            }
                                        }
                                        
                                    
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
                    xl.DisplayAlerts = false;
                    xl.Quit();
                    Marshal.ReleaseComObject(xl);
                }
            }
            catch (Exception)
            {
                throw;
            }
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
            Match m = date.Match(row.Value2.ToString());
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
        static void Main()
        {
            new Schedule().ReadFromXL(@"\\quelertime\shareddocs\tashed.xlsb");
            //@"\\w-pattr-002\AcademyDocs\USPTA_schedules\Nov_4th_Entry-Level-Phase 1_Training _Schedule 10-21-13.xls");

        }
    }

}
