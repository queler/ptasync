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
            set { start = value; }
        }
        DateTime end;

        public DateTime End
        {
            get { return end; }
            set { end = value; }
        }
        
        public TimeSpan Duration
        {
            get { return Start-End; }
            set { End=Start+value; }
        }
        String titleRaw;

        public String TitleRaw
        {
            get { return titleRaw; }
            set { titleRaw = value; }
        }
        String titleOut;

        public String TitleOut
        {
            get { return titleOut; }
            set { titleOut = value; }
        }
        String location;

        public String Location
        {
            get { return location; }
            set { location = value; }
        }
        bool allDay;

        public bool AllDay
        {
            get { return allDay; }
            set { allDay = value; }
        }
        string trainer;

        public string Trainer
        {
            get { return trainer; }
            set { trainer = value; }
        }
        public override string ToString()
        {
            return Start+":"+TitleOut+":"+End;
        }
    }
}
