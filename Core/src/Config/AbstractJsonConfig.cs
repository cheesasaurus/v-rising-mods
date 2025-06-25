using System;
using System.IO;
using System.Reflection;

namespace VRisingMods.Core.Config;

public abstract class AbstractJsonConfig
{
    protected string FilePath;

    protected AbstractJsonConfig(string filepath)
    {
        FilePath = filepath;
    }

    public abstract string ToJson();

    protected abstract void InitDefaults();

    protected abstract void InitFromJson(string json);

    public static T Init<T>(string pluginGUID, string filename) where T : AbstractJsonConfig
    {
        var filepath = Filepath(pluginGUID, filename);
        var config = (T)Activator.CreateInstance(typeof(T), filepath);
        if (File.Exists(filepath))
        {
            var json = File.ReadAllText(filepath);
            config.InitFromJson(json);
        }
        else
        {
            config.InitDefaults();
            File.WriteAllText(filepath, config.ToJson());
        }
        return config;
    }

    public static string Filepath(string pluginGUID, string filename)
    {
        var dir = Path.Combine(BepInEx.Paths.ConfigPath, pluginGUID);
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, filename);
    }

    public static void CopyExampleIfNotExists(string pluginGUID, string outputFilename, string inputResourceName)
    {
        var outputFilepath = Filepath(pluginGUID, outputFilename);
        if (File.Exists(outputFilepath))
        {
            return;
        }

        var assembly = Assembly.GetExecutingAssembly();
        using (Stream inputStream = assembly.GetManifestResourceStream(inputResourceName))
        {
            using (FileStream outputStream = File.Create(outputFilepath))
            {
                inputStream.CopyTo(outputStream);
            }
        }
    }

}
