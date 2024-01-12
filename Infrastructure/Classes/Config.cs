using System.Reflection;
using System.Text.Json;

namespace DvrService.Infrastructure.Classes
{

    internal record Root
    {
        public string FFmpegPath { get; set; }= String.Empty;
        public List<Camera> Cameras { get; set; } = new();

    }

    internal record Camera
    {
        public string CameraName { get; set; } = String.Empty;
        public string CameraUrl { get; set; } = String.Empty;
        public string PathRecord { get; set; } = String.Empty;
        public double RecordTimeMin { get; set; }
        public int NumberFilesInFolder { get; set; }
        public double RemoveOldFilesAfterMin { get; set; }
        public double RestartRecordAfterHours { get; set; } 
    }

    internal record Config
    {
        public List<Camera> Cameras { get; set; }
        public string FFmpegPath { get; set; }
        private string ConfigPath { get; set; }

        public Config(string configPath)
        {
            if (configPath == String.Empty)
                ConfigPath = Environment.CurrentDirectory + @"\Infrastructure\Config\ServiceConfig.json";
            else
                ConfigPath = configPath;
            var root=JsonSerializer.Deserialize<Root>(File.ReadAllText(ConfigPath));
            Cameras=root!.Cameras;
            FFmpegPath=root.FFmpegPath;
        }
    }

}





