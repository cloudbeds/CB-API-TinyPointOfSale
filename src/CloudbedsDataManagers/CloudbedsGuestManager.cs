using System;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Manages retreiving lists of Guests from Cloudbeds
/// </summary>
class CloudbedsGuestManager
{
    private readonly ICloudbedsServerInfo _cbServerInfo;
    private readonly ICloudbedsAuthSessionId _authSession;
    private readonly TaskStatusLogs _statusLog;

    private class CachedData
    {
        public readonly ICollection<CloudbedsGuest> Guests;
        public readonly DateTime LastUpdated;

        public CachedData(ICollection<CloudbedsGuest> guests, DateTime updated)
        {
            this.Guests = guests;
            this.LastUpdated = updated;
        }
    }

    CachedData _cachedData;
    public ICollection<CloudbedsGuest>? Guests
    {
        get 
        { 
            if(_cachedData == null)
            {
                return null;
            }
            return _cachedData.Guests; 
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="authSession"></param>
    /// <param name="statusLog"></param>
    public CloudbedsGuestManager(
        ICloudbedsServerInfo cbServerInfo,
        ICloudbedsAuthSessionId authSession,
        TaskStatusLogs statusLog)
    {
        _cbServerInfo = cbServerInfo;
        _authSession = authSession;
        _statusLog = statusLog;      
    }

    /// <summary>
    /// Queries for guests, if needed
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void EnsureCachedData()
    {
        //Success
        if(_cachedData != null)
        {
            return;
        }

        ForceRefreshOfCachedData();
    }

    /// <summary>
    /// Get the latest data in the cache
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ForceRefreshOfCachedData()
    {
        var cbQueryGuests = new CloudbedsRequestCurrentGuests(_cbServerInfo, _authSession, _statusLog);
        var queryTime = DateTime.Now;
        var querySuccess = cbQueryGuests.ExecuteRequest();
        if (!querySuccess)
        {
            throw new Exception("1021-825: CloudbedsGuestManager, query failure");
        }

        var queriedGuests = cbQueryGuests.CommandResults_Guests;
        IwsDiagnostics.Assert(queriedGuests != null, "1021-826: Expected query results");

        //Store the cached results
        _cachedData = new CachedData(queriedGuests, queryTime);

    }

    /// <summary>
}
