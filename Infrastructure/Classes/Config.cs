using System.Text.Json;
using System.Text.Json.Serialization;

namespace DvrService.Infrastructure.Classes
{

    internal record Root
    {
        public string FFmpegPath { get; set; } = String.Empty;
        public int CheckOfRecordFilesTimeMin { get; set; }
        public List<Camera> Cameras { get; set; } = [];
        public int RestartRecordAfterHours { get; set; }

    }

    internal record Camera
    {
        public string CameraName { get; set; } = String.Empty;
        public string CameraUrl { get; set; } = String.Empty;
        public string PathRecord { get; set; } = String.Empty;
        public bool EncodeRecord { get; set; }
        public string EncodeQuality { get; set; } = String.Empty;
        public int RecordTimeMin { get; set; }
        public int NumberFilesInFolder { get; set; }
        public int RemoveOldFilesAfterMin { get; set; }
        //public int RestartRecordAfterHours { get; set; }

    }

    internal record Config
    {
        public List<Camera> Cameras { get; set; }
        public string FFmpegPath { get; set; }
        private string ConfigPath { get; set; }
        public int CheckOfRecordFilesTimeMin { get; set; }
        public int RestartRecordAfterHours { get; set; }

        public Config(string configPath)
        {
            try
            {
                if (configPath == String.Empty)
                    ConfigPath = Environment.CurrentDirectory + @"\Infrastructure\Config\ServiceConfig.json";
                else
                    ConfigPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + configPath;
                var root = JsonSerializer.Deserialize<Root>(File.ReadAllText(ConfigPath), SourceGenerationContext.Default.Root);
                Cameras = root!.Cameras;
                FFmpegPath = root.FFmpegPath;
                CheckOfRecordFilesTimeMin = int.Abs(root.CheckOfRecordFilesTimeMin);
                RestartRecordAfterHours = int.Abs(root.RestartRecordAfterHours);
            }
            catch (Exception ex)
            {
                Properties.errorFiles.WriteLine($"Error:\n{ex.Message}\nОшибка в конфигурационном файле ServiceConfig.json");
                Properties.errorFiles.Close();
                Environment.Exit(1);
            }
        }
    }
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Root))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    { }

}





