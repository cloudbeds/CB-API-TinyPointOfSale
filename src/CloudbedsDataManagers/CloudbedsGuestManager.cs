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
    /// If we do not have cached data, set up an aync job to request it
    /// </summary>
    public void EnsureCachedData_Async()
    {
        //If we have cached data, there is nothing to do...
        if (_cachedData != null)
        {
            return;
        }

        CloudbedsSingletons.StatusLogs.AddStatus("1030-1146: Starting Async request(s) to warm up Cloudbeds query data cache");

        //Run the job async to request we fill the cache
        System.Threading.Tasks.Task.Run(() => this.EnsureCachedData());
    }

    /// <summary>
    /// Thread synchronization lick.  We use this to create a critical section
    /// that prevents is from performing a cache-query to fill the Guests cache
    /// if that query is already underway
    /// </summary>
    object _syncLockForGuestCacheQuery = new object();


    /// <summary>
    /// Queries for guests, if needed
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void EnsureCachedData()
    {
        //If we have cached data, there is nothing to do...
        if(_cachedData != null)
        {
            return;
        }

        //==========================================================================
        //If another thread is already running this code, let it finish first
        //because it is ALREDY getting the data we want
        //==========================================================================
        lock (_syncLockForGuestCacheQuery)
        {
            //======================================================================
            //Since another thread may have just completed this call - check againg
            //when we are in the lock, it ensure we dont re-query the data unncessarily
            if (_cachedData != null)
            {
                return;
            }

            //We don't have the data yet... so go ahead and re-query
            ForceRefreshOfCachedData();
        }//end: Lock
    }


    /// <summary>
    /// Get the latest data in the cache
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ForceRefreshOfCachedData()
    {
        ICollection<CloudbedsGuest> queriedGuests = null;

        var queryTime = DateTime.Now;

        if (AppSettings.UseSimulatedGuestData)
        {
            //Create simulated data
            queriedGuests = Testing_CreateSimulatedGuestData();
            _cachedData = new CachedData(queriedGuests, queryTime);
            return;
        }

        queriedGuests = helper_SynchronouEnsureGuestData();

        //Store the cached results
        _cachedData = new CachedData(queriedGuests, queryTime);
    }

    /// <summary>
    /// Query for the guest data.  Ensure that only one query can occur at a time
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private ICollection<CloudbedsGuest> helper_SynchronouEnsureGuestData()
    {
        var cbQueryGuests = new CloudbedsRequestCurrentGuests(_cbServerInfo, _authSession, _statusLog);
        var querySuccess = cbQueryGuests.ExecuteRequest();
        if (!querySuccess)
        {
            throw new Exception("1021-825: CloudbedsGuestManager, query failure");
        }

        var queriedGuests = cbQueryGuests.CommandResults_Guests;
        IwsDiagnostics.Assert(queriedGuests != null, "1021-826: Expected query results");
        return queriedGuests;
    }


    /// <summary>
    /// Create test data
    /// </summary>
    /// <returns></returns>
    private static ICollection<CloudbedsGuest> Testing_CreateSimulatedGuestData()
    {
        DateTime reservationDate = DateTime.Today.AddDays(-4);
        DateTime reservationDateEnd = DateTime.Today.AddDays(14);

        var testData = new List<CloudbedsGuest>();
        testData.Add(Testing_CreateSimulatedGuest("Dopy Sevendwarves", "dopy.sevendwarves@cloudbeds.com", "111.123.1111", "Rm 201"));
        testData.Add(Testing_CreateSimulatedGuest("Bashful Sevendwarves", "bashful.sevendwarves@cloudbeds.com", "111.123.1112", "Rm 202"));
        testData.Add(Testing_CreateSimulatedGuest("Sleepy Sevendwarves", "bashful.sevendwarves@cloudbeds.com", "111.123.1112", "Rm 203"));
        testData.Add(Testing_CreateSimulatedGuest("Sneezy Sevendwarves", "sneezy.sevendwarves@cloudbeds.com", "111.123.1113", "Rm 204"));
        testData.Add(Testing_CreateSimulatedGuest("Grumpy Sevendwarves", "grumpy.sevendwarves@cloudbeds.com", "111.123.1114", "Rm 205"));
        testData.Add(Testing_CreateSimulatedGuest("Doc Sevendwarves", "doc.sevendwarves@cloudbeds.com", "111.123.1115", "Rm 206"));
        testData.Add(Testing_CreateSimulatedGuest("Sneezy Sevendwarves", "sneezy.sevendwarves@cloudbeds.com", "111.123.1116", "Rm 212"));
        testData.Add(Testing_CreateSimulatedGuest("Happy Sevendwarves", "happy.sevendwarves@cloudbeds.com", "111.123.1117", "Rm 214"));
        testData.Add(Testing_CreateSimulatedGuest("Snow White", "snow.white@cloudbeds.com", "111.123.1130", "Rm 107"));
        testData.Add(Testing_CreateSimulatedGuest("Evil Queen", "evil.queen@cloudbeds.com", "111.123.1140", "Rm 321"));

        return testData;
    }


    /// <summary>
    /// Create a similated guest
    /// </summary>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <param name="phone"></param>
    /// <param name="roomName"></param>
    /// <returns></returns>
    private static CloudbedsGuest Testing_CreateSimulatedGuest(string name, string email, string phone, string roomName)
    {
        var random = new Random();

        DateTime reservationDate = DateTime.Today.AddDays(-2 - random.Next(8));
        DateTime reservationDateEnd = DateTime.Today.AddDays(14 + random.Next(32));
        Guid fakeId1 = Guid.NewGuid();
        Guid fakeId2 = Guid.NewGuid();
        Guid fakeId3 = Guid.NewGuid();

        return new CloudbedsGuest(
                "fake_guestID_" + fakeId1.ToString(),
                name,
                email,
                phone,
                "fake_reservationID_" + fakeId2,
                reservationDate.ToShortDateString(),
                reservationDateEnd.ToShortDateString(),
                "fake_roomID_" + fakeId3,
                roomName);

    }

    /// <summary>
}
