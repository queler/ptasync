using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTASync
{
    public interface IEvent
    {
        DateTime Start { get; set; }
        DateTime End { get; set; }
        TimeSpan Duration { get; set; }
        String TitleRaw{ get; set; }
        String TitleOut { get; set; }
        String Location { get; set; }
        String Trainer { get; set; }

    }
}
