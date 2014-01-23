using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTASync
{
    class PTEvent : IEvent
    {
        DateTime start;

        public DateTime Start
        {
            get { return start; }
            set
            {
                if (AllDay)
                {

                    start = value.Date;
                }
                else
                {
                    start = value;
                }
            }
        }
        DateTime end;

        public DateTime End
        {
            get { return end; }
            set {
                if (AllDay)
                {
                    end = value.Date;
                }
                else{
                    end = value; 
                }
            }
        }
        
        public TimeSpan Duration
        {
            get { return End-Start; }
            set { End=Start+value; }
        }
        String titleRaw="";

        public String TitleRaw
        {
            get { return titleRaw; }
            set { titleRaw = value; }
        }

        public String TitleOut
        {
            get { 
        		string prefix="";
        		if (Trainer.ToLower().Contains("trainer")) {
        			prefix="$";
        		}
        		if (Trainer.ToLower().Contains("leslie") || Trainer.ToLower().Contains("gary")){
        			prefix="#";
        		}
        		return prefix+ TitleRaw;
        	}
            set { throw new NotImplementedException();}
        }
        String location="";

        public String Location
        {
            get { return location; }
            set { location = value; }
        }
        bool allDay;

        public bool AllDay
        {
            get { return allDay; }
            set { 
                allDay = value;
                Start = Start.Date;
                End = End.Date;

            }
        }
        string trainer="";

        public string Trainer
        {
            get { return trainer; }
            set { trainer = value; }
        }
        public override string ToString()
        {
            return Start+";"+End+";"+TitleOut+";"+Trainer+";"+Location;
        }
    }
}
