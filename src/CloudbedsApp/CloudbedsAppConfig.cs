using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


/// <summary>
/// Config For Cloudbeds Application API access
/// </summary>
internal partial class CloudbedsAppConfig : ICloudbedsServerInfo
{


    //ID/Password to sign into the Online Site
    public readonly string CloudbedsServerUrl;
    public readonly string CloudbedsAppOAuthRedirectUri;
    public readonly string CloudbedsAppClientId;
    public readonly string CloudbedsAppClientSecret;
    
    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="filePathConfig"></param>
    public CloudbedsAppConfig(string filePathConfig)
    {
        if(!System.IO.File.Exists(filePathConfig))
        {
            throw new Exception("722-153: File does not exist: " + filePathConfig);
        }

        //==================================================================================
        //Load values from the TARGET SITE config file
        //==================================================================================
        var xmlConfigTargetSite = new System.Xml.XmlDocument();
        xmlConfigTargetSite.Load(filePathConfig);

        var xNode = xmlConfigTargetSite.SelectSingleNode("//Configuration/CloudbedsApp");
        this.CloudbedsServerUrl = xNode.Attributes["serverUrl"].Value;
        this.CloudbedsAppClientId = xNode.Attributes["clientId"].Value;
        this.CloudbedsAppClientSecret = xNode.Attributes["secret"].Value;
        this.CloudbedsAppOAuthRedirectUri = xNode.Attributes["oAuthRedirectUri"].Value;
    }

    string ICloudbedsServerInfo.ServerUrl
    {
        get { return this.CloudbedsServerUrl; } 
    }
}

    