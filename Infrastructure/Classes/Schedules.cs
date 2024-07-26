using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentScheduler;

namespace DvrService.Infrastructure.Classes
{
    internal class Schedules:Registry
    {
        public Schedules()
        {
            test();
        }

        public void test()
        {
            Schedule(() => { Console.WriteLine("test"); }).ToRunEvery(10).Seconds();
        }
    }
    public static class Properties
    {
        public static ConcurrentBag<Process> FFmpegProcessBag { get; set; }
        public static StreamWriter errorFiles;
        static Properties()
        {
            FFmpegProcessBag = new();
errorFiles=new StreamWriter($@"{AppDomain.CurrentDomain.BaseDirectory}\Error.txt", false);
        }
    }
}
