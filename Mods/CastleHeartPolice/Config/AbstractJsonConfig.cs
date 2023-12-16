using System;
using System.IO;

namespace CastleHeartPolice.Config;

public abstract class AbstractJsonConfig {
    protected string FilePath;

    protected AbstractJsonConfig(string filepath) {
        FilePath = filepath;
    }

    public abstract string ToJson();

    protected abstract void InitDefaults();

    protected abstract void InitFromJson(string json);

    public static T Init<T>(string filename) where T : AbstractJsonConfig {
        var dir = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_GUID);
        Directory.CreateDirectory(dir);
        var filepath = Path.Combine(dir, filename);

        var config = (T)Activator.CreateInstance(typeof(T), filepath);
        if (File.Exists(filepath)) {
            var json = File.ReadAllText(filepath);
            config.InitFromJson(json);
        }
        else {
            config.InitDefaults();
            File.WriteAllText(filepath, config.ToJson());
        }
        return config;
    }
}
