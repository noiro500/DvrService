namespace DvrService.Infrastructure.Classes
{
    public static class Properties
    {
        //public static ConcurrentBag<Process> FFmpegProcessBag { get; set; }
        public static StreamWriter errorFiles;
        static Properties()
        {
            //FFmpegProcessBag = new();
            errorFiles = new StreamWriter($@"{AppDomain.CurrentDomain.BaseDirectory}\Error.txt", false);
        }
        public static async Task WriteErrorsAsync(string message)
        {
            await errorFiles.WriteLineAsync(message);
            await errorFiles.FlushAsync();
        }
        public static void WriteErrors(string message)
        {
            errorFiles.WriteLine(message);
            errorFiles.Flush();
        }
    }
}
