

import org.gcaldaemon.api.SyncEngine;
import java.io.File;
import java.net.URL;

public class IcalRemoteUserPass {

    public static void main(String[] args) {
        try {

            // This directory is used by the SyncEngine
            // for temporary file storage
            //File workDir = new File(null);

            // Path to local iCalendar file
            File localCalendar = new File(args[0]);

            // Private iCal URL of a Google Calendar
            // Use the SyncEngine.listCalendars() method to get URLs
            URL remoteCalendar = new URL(args[1]);

            // Gmail user
            String username = args[2];

            // Gmail password
            String password = args[3];

            // Creates a synchronizer engine
            SyncEngine engine = new SyncEngine();

            // Do the synchronization
            engine.synchronize(localCalendar,
                    remoteCalendar,
                    username,
                    password);
			//System.exit(0);
			
        } catch (Exception e) {
           
			e.printStackTrace();
			System.exit(1);
        }
    }
}

   