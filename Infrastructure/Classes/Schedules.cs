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
    }
}
