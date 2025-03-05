using System.Globalization;
using System.Text;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using Nekonya.WebDanmakuStarter.Configuration;

namespace Nekonya.WebDanmakuStarter;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    const string DefaultScriptUrl = "";
    
    public Plugin(
        IApplicationPaths applicationPaths, 
        IXmlSerializer xmlSerializer,
        IServerConfigurationManager configurationManager,
        ILogger<Plugin> logger
        ) 
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;


        if (string.IsNullOrWhiteSpace(applicationPaths.WebPath))
            return;

        var indexFile = Path.Combine(applicationPaths.WebPath, "index.html");
        if(!File.Exists(indexFile))
            return;

        string? scriptUrl = Configuration.ScriptUrl;
        // 简单的检查Script Url是否有效，如果是明显的无效，则设为null，后续方法使用默认值。主要是避免一些低级配置错误把index.html搞炸了
        // if (scriptUrl?.Trim() == "")
        //     scriptUrl = null;
        
        // if (!scriptUrl?.StartsWith("https://") ?? false)
        //     scriptUrl = null;

        string htmlText = File.ReadAllText(indexFile, Encoding.UTF8);
        var changed = false;
        if (!this.Configuration.EnableDanmaku)
        {
            // 不启用，如果有脚本则移除
            if (TryRemoveScriptElementIfExist(ref htmlText, scriptUrl))
            {
                logger.LogInformation("DanmakuStarter: Removed script element from index.html ({scriptUrl})", scriptUrl);
                changed = true;
            }

            if(!changed && scriptUrl != null)
            {
                // 刚刚使用用户配置的url进行检查，但没有要处理的。但以防万一，使用默认值再检查一次
                if(TryRemoveScriptElementIfExist(ref htmlText))
                {
                    logger.LogInformation("DanmakuStarter: Removed script element from index.html (use defaule scripturl: {DefaultScriptUrl})", DefaultScriptUrl);
                    changed = true;
                }
            }
        }
        else
        {
            // 需要启用
            // 如果用户配置了脚本地址，但不是默认地址，则先尝试移除默认地址
            if (scriptUrl != null && scriptUrl != DefaultScriptUrl)
            {
                if (TryRemoveScriptElementIfExist(ref htmlText))
                {
                    logger.LogInformation("DanmakuStarter: Removed default script element from index.html");
                    changed = true;
                }
            }

            // 添加脚本
            if (AddScriptElementIfNotExist(ref htmlText, scriptUrl))
            {
                logger.LogInformation("DanmakuStarter: Added script element to index.html ({scriptUrl})", scriptUrl);
                changed = true;
            }
        }

        if (changed)
        {
            File.WriteAllText(indexFile, htmlText, Encoding.UTF8);
            logger.LogInformation("DanmakuStarter: index.html has been modified");
        }
    }

    public override string Name => "DanmakuStarter";

    public override Guid Id => Guid.Parse("d70666cf-0db6-4074-9f3e-aea9959d956d");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = "WebDanmakuStarter",
                EmbeddedResourcePath = string.Format(
                    CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", "WebDanmakuStarter"
                )
            }
        ];
    }


    // private bool CheckScriptElement(string htmlText, string? scriptUrl = null)
    // {
    //     // 只使用字符串方式操作，不作为HTML解析，这样可以减少依赖和性能开销
    //     string scriptUrlToCheck = scriptUrl ?? DefaultScriptUrl;
    //     string scriptElement = $"<script src=\"{scriptUrlToCheck}\" defer></script>";
    //     return htmlText.Contains(scriptElement);
    // }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="htmlText"></param>
    /// <param name="scriptUrl"></param>
    /// <returns>如果有修改</returns>
    private bool TryRemoveScriptElementIfExist(ref string htmlText, string? scriptUrl = null)
    {
        string scriptUrlToCheck = scriptUrl ?? DefaultScriptUrl;
        string scriptElement = $"<script src=\"{scriptUrlToCheck}\" defer></script>";
        if (htmlText.Contains(scriptElement))
        {
            htmlText = htmlText.Replace(scriptUrl, "");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="htmlText"></param>
    /// <param name="scriptUrl"></param>
    /// <returns>如果有修改</returns>
    private bool AddScriptElementIfNotExist(ref string htmlText, string? scriptUrl = null)
    {
        string finalScriptUrl = scriptUrl ?? DefaultScriptUrl;
        if (!htmlText.Contains(scriptElement))
        {
            htmlText = htmlText.Replace("</head>", $"{scriptElement}</head>");
            return true;
        }
        return false;
    }

}
