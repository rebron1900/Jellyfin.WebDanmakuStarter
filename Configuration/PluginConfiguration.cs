using MediaBrowser.Model.Plugins;

namespace Nekonya.WebDanmakuStarter.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// 是否启用弹幕
    /// </summary>
    public bool EnableDanmaku { get; set; } = true;

    /// <summary>
    /// 脚本地址
    /// </summary>
    public string ScriptUrl { get; set; } = "https://jellyfin-danmaku.pages.dev/ede.user.js";
}
