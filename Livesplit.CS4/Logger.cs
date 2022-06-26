using System;
using System.Diagnostics;
using System.IO;

namespace Livesplit.CS4
{

    // Logs to a file in Temp rather than being forced to constantly use Debug.Print as that's annoying
    
    public static class Logger
    {
        private const string FILE_NAME = "ToCS4_Livesplit_Log.log";
        private static readonly string path = Path.Combine(Path.GetTempPath(), FILE_NAME);

        static Logger()
        {
            Debug.Print("Creating Logger");
            Debug.Print(path);
            File.Create(path).Close();
            File.SetCreationTimeUtc(path, DateTime.UtcNow); 
        }
        
        public static void Log(string message)
        {

            Debug.Print(message);
            using(StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"[{DateTime.Now}] {message}");
            }

        }
        
    }
}