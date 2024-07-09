using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DvrService.Infrastructure.Classes
{

    internal record Root
    {
        public string FFmpegPath { get; set; }= String.Empty;
        public double CheckOfRecordFilesTimeMin { get; set; }
        public List<Camera> Cameras { get; set; } = [];

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
        public double CheckOfRecordFilesTimeMin { get; set; }

        public Config(string configPath, StreamWriter errorFile)
        {
            try
            {
                if (configPath == String.Empty)
                    ConfigPath = Environment.CurrentDirectory + @"\Infrastructure\Config\ServiceConfig.json";
                else
                    ConfigPath = AppDomain.CurrentDomain.BaseDirectory+"\\"+configPath;
                var root = JsonSerializer.Deserialize<Root>(File.ReadAllText(ConfigPath), SourceGenerationContext.Default.Root);
                Cameras = root!.Cameras;
                FFmpegPath = root.FFmpegPath;
                CheckOfRecordFilesTimeMin=root.CheckOfRecordFilesTimeMin;
            }
            catch (Exception ex)
            {
                errorFile.WriteLine($"Error:\n{ex.Message}");
                errorFile.Close();
                Environment.Exit(1);
            }
        }
    }
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Root))]
    internal partial class SourceGenerationContext  : JsonSerializerContext    
    {}

}





