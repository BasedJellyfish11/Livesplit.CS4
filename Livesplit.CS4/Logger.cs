using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Livesplit.CS4
{

    // Logs to a file in Temp rather than being forced to constantly use Debug.Print as that's annoying
    
    public static class Logger
    {
        private const string FILE_NAME = "ToCS4_Livesplit_Log";
        private static readonly string path = Path.Combine(Path.GetTempPath(), FILE_NAME, ".txt");
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        static Logger()
        {
            Debug.Print("Creating Logger");
            File.Create(path);
            File.SetCreationTimeUtc(path, DateTime.UtcNow);
        }
        
        public static async void Log(string message)
        {
            await _semaphore.WaitAsync();
            using(StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write)))
            {
                await writer.WriteLineAsync($"[{DateTime.Now}] {message}");
            }

            _semaphore.Release();
        }
        
    }
}