
using System;
using System.Text;
using System.Web;

/// <summary>
/// Common singletons we want to use across the application
/// </summary>
static internal partial class CloudbedsSingletons
{
    private static ICloudbedsAuthSessionBase s_currentAuthSession;
    private static ICloudbedsServerInfo s_currentServerInfo;
    private static PosItemManager s_posItemManager;
    private static CloudbedsGuestManager s_guestManager;

    /// <summary>
    /// Common status logging for the application
    /// </summary>
    public static TaskStatusLogs StatusLogs
    {
        get { return TaskStatusLogsSingleton.Singleton; }
    }

    /// <summary>
    /// The authentication session to Cloudbeds.com
    /// </summary>
    public static ICloudbedsAuthSessionBase CloudbedsAuthSession
    {
        get
        {
            EnsureAuthSessionAndSeverInfo();
            return s_currentAuthSession;
        }
    }

    /// <summary>
    /// The server-connection configuration/url/etc
    /// </summary>
    public static ICloudbedsServerInfo CloudbedsServerInfo
    {
        get
        {
            EnsureAuthSessionAndSeverInfo();
            return s_currentServerInfo;
        }
    }


    /// <summary>
    /// The cached list of guests...
    /// </summary>
    public static CloudbedsGuestManager CloudbedsGuestManager
    {
        get
        {
            return EnsureGuestManager();
        }
    }

    /// <summary>
    /// Manages the list of items we have for sale and can be added to customers' bills
    /// </summary>
    public static PosItemManager PointOfSaleItemManager
    {
        get
        {
            EnsurePointOfSale_ItemManager();
            return s_posItemManager;
        }
    }


    /// <summary>
    /// Loads a token from persisted storage
    /// </summary>
    /// <param name="statusLogs"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void EnsureAuthSessionAndSeverInfo()
    {
        //===================================================================
        //If the auth-session and server info are already loaded we can 
        //just exit
        //===================================================================
        if ((s_currentAuthSession != null) && (s_currentServerInfo != null))
        {
            return;
        }

        TaskStatusLogs statusLogs = CloudbedsSingletons.StatusLogs;

        statusLogs.AddStatusHeader("Load Auth Tokens from storage");

        //=================================================================================
        //If we are using simlated data -- create it here...
        //=================================================================================
        if (AppSettings.UseSimulatedGuestData)
        {
            helper_EnsureAuthSessionAndSeverInfo_CreateSimulated();
            return;
        }

        //=================================================================================
        //Create the LIVE session
        //=================================================================================
        helper_EnsureAuthSessionAndSeverInfo_CreateLive(statusLogs);
    }

    /// <summary>
    /// Creates the simulated session (not a real csession)
    /// </summary>
    private static void helper_EnsureAuthSessionAndSeverInfo_CreateSimulated()
    {
        s_currentAuthSession = new CloudbedsAuthSession_OAuth(
            new OAuth_RefreshToken("FAKE REFRESH TOKEN:xxxxxxxxx"),
            new OAuth_AccessToken("FAKE ACCESS TOKEN:yyyyyyyyy"),
            DateTime.Today.AddYears(2),
            TaskStatusLogsSingleton.Singleton);

        s_currentServerInfo = CloudbedsAppConfig.TESTING_CreateSimulatedAppConfig();
        return;
    }

    /// <summary>
    /// Creates the LIVE session 
    /// </summary>
    private static void helper_EnsureAuthSessionAndSeverInfo_CreateLive(TaskStatusLogs statusLogs)
    {
        //=================================================================================
        //Load our secrets and create an authentication session...
        //=================================================================================

        var filePathToAppSecrets = AppSettings.LoadPreference_PathAppSecretsConfig();
        if (!System.IO.File.Exists(filePathToAppSecrets))
        {
            statusLogs.AddError("220825-814: App secrets file does not exist: " + filePathToAppSecrets);
            throw new Exception("220825-814805: App secrets file does not exist: " + filePathToAppSecrets);
        }
        //----------------------------------------------------------
        //Load the application-global configuration from storage
        //(we need to to refresh the token)
        //----------------------------------------------------------
        var appConfigAndSecrets = CloudbedsAppConfig.FromFile(filePathToAppSecrets);
        s_currentServerInfo = appConfigAndSecrets;

        //================================================================================
        //Load the authentication secrets from storage
        //================================================================================
        var filePathToPersistedToken = AppSettings.LoadPreference_PathUserAccessTokens();
        if (!System.IO.File.Exists(filePathToPersistedToken))
        {
            statusLogs.AddError("220825-805: Auth token file does not exist: " + filePathToPersistedToken);
            throw new Exception("220825-805: Auth token file does not exist: " + filePathToPersistedToken);
        }
        CloudbedsTransientSecretStorageManager authSecretsStorageManager =
            CloudbedsTransientSecretStorageManager.LoadAuthTokensFromFile(
                filePathToPersistedToken, 
                appConfigAndSecrets, 
                true, 
                statusLogs);

        var authSession = authSecretsStorageManager.AuthSession;
        //Sanity test
        if (authSession == null)
        {
            statusLogs.AddError("220725-229: No auth session returned");
        }
        else
        {
            statusLogs.AddStatus("Successfully loaded auth token from storage");
        }

        //--------------------------------------------------------------------
        //Store this at the class' level, so that it can be used by other calls
        //--------------------------------------------------------------------
        s_currentAuthSession = authSession;
    }


    /// <summary>
    /// Create/Load an item manager, if needed
    /// </summary>
    /// <returns></returns>
    private static PosItemManager EnsurePointOfSale_ItemManager()
    {
        if (s_posItemManager == null)
        {
            s_posItemManager = new PosItemManager();
        }

        return s_posItemManager;
    }

    /// <summary>
    /// Creates a guest manager object if necessary
    /// </summary>
    /// <returns></returns>
    private static CloudbedsGuestManager EnsureGuestManager()
    {
        var guestManager = s_guestManager;
        if (guestManager != null)
        {
            return guestManager;
        }

        var taskStatus = TaskStatusLogsSingleton.Singleton;

        //Make sure we are logged in
        EnsureAuthSessionAndSeverInfo();

        var authSession = CloudbedsSingletons.CloudbedsAuthSession;
        var serverInfo = CloudbedsSingletons.CloudbedsServerInfo;
        if ((authSession == null) || (serverInfo == null))
        {
            taskStatus.AddError("1021-835: No auth/server session");
            throw new Exception("1021-835: No auth/server session");
        }

        guestManager = new CloudbedsGuestManager(serverInfo, authSession, taskStatus);
        s_guestManager = guestManager;
        return guestManager;
    }

    /// <summary>
    /// If necessary, starts common queries for data our application uses.
    /// 
    /// These queries will run asynchronously, and will allow user facing UI
    /// to come up. 
    /// 
    /// NOTE: If the UI needs the data in question, it will cause a
    /// synchronous call that will then wait for the query to complete
    /// (This is done by putting thread-lock/critical-sections in the specific
    ///  query code)
    /// </summary>
    public static void WarmUpCloudbedsDataCachesIfNeeded_Async()
    {
        //See if the Guest Manager needs to query for its data async
        EnsureGuestManager().EnsureCachedData_Async();
    }
}
