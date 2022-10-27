
using System;
using System.Text;
using System.Web;

static partial class CloudbedsUris
{
    const string TemplateUrl_RequestOAuthAccess =
        "{{iwsServerUrl}}/api/v1.1/oauth?client_id={{iwsClientId}}&redirect_uri={{iwsOAuthResponseUri}}&response_type=code&scope=read:dashboard";

    const string TemplateUrl_RequestOAuthRefreshToken =
        "{{iwsServerUrl}}/api/v1.1/access_token";

    const string TemplateUrl_CustomItemToReservation =
        "{{iwsServerUrl}}/api/v1.1/postCustomItem";

    const string TemplateUrl_CustomItemToReservation_PostContents =
        "reservationID={{iwsReservationId}}" +
        "&guestID={{iwsGuestId}}" +
        "&referenceID={{iwsReferenceId}}" +
        "{{iwsOrderItems}}" +
        "";



    const string TemplateUrl_PostAdjustmentToReservation =
        "{{iwsServerUrl}}/api/v1.1/postAdjustment";

    const string Template_PostAdjustmentToReservation_PostContents =
        "reservationID={{iwsReservationId}}&type={{iwsAdjustmentType}}&amount={{iwsAdjustmentAmount}}&notes={{iwsNotes}}";

    const string TemplateUrl_RequestOAuthRefreshToken_PostContents =
        "grant_type=refresh_token&client_id={{iwsClientId}}&client_secret={{iwsClientSecret}}&refresh_token={{iwsOAuthRefreshToken}}";


    const string TemplateUrl_RequestOAuthAccessToken =
        "{{iwsServerUrl}}/api/v1.1/access_token";

    const string TemplateUrl_RequestOAuthAccessToken_PostContents =
       "grant_type=authorization_code&client_id={{iwsClientId}}&client_secret={{iwsClientSecret}}&redirect_uri={{iwsOAuthResponseUri}}&code={{iwsOAuthCode}}";


    const string TemplateUrl_HotelDashboard = "{{iwsServerUrl}}/api/v1.1/getDashboard";
    const string TemplateUrl_RequestAuthUserInfo = "{{iwsServerUrl}}/api/v1.1/userinfo";
    const string TemplateUrl_GetCurrentGuestsList = "{{iwsServerUrl}}/api/v1.1/getGuestsByStatus?status=in_house&pageNumber={{iwsPageNumber}}&pageSize={{iwsPageSize}}";

    /// <summary>
    /// Get the Dashboard for the hotel
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <returns></returns>
    internal static string UriGenerate_RequestDashboard(ICloudbedsServerInfo cbServerInfo)
    {
        var sb = new StringBuilder(TemplateUrl_HotelDashboard);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);
        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Get the current set of guests
    /// https://hotels.cloudbeds.com/api/docs/#api-Guest-getGuestsByStatus
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    internal static string UriGenerate_GetCurrentGuestsList(
        ICloudbedsServerInfo cbServerInfo,
        int pageNumber,
        int pageSize)
    {
        var sb = new StringBuilder(TemplateUrl_GetCurrentGuestsList);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);
        sb.Replace("{{iwsPageNumber}}", pageNumber.ToString());
        sb.Replace("{{iwsPageSize}}", pageSize.ToString());
        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Post a custom item to a reservation
    /// https://hotels.cloudbeds.com/api/docs/#api-Item-postCustomItem
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    internal static string UriGenerate_PostCustomItemToReservation(
        ICloudbedsServerInfo cbServerInfo)
    {
        var sb = new StringBuilder(TemplateUrl_CustomItemToReservation);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);
        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Called to generate the post contents for a reservation adjustment (i.e. add fee to a reservation)
    /// https://hotels.cloudbeds.com/api/docs/#api-Item-postCustomItem    
    /// </summary>
    /// <returns></returns>
    public static string UriGenerate_PostCustomItemToReservation_PostContents(
        CloudbedsGuest guest, PosOrderManager posOrderManager)
    {
        var sb = new StringBuilder(TemplateUrl_CustomItemToReservation_PostContents);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsReservationId}}", HttpUtility.UrlEncode(guest.Reservation_Id));
        sb.Replace("{{iwsGuestId}}", HttpUtility.UrlEncode(guest.Guest_Id));
        //Store a unique reference ID; so if the order is submitted multiple times it is not duplicated
        sb.Replace("{{iwsReferenceId}}", HttpUtility.UrlEncode(posOrderManager.UniqueOrderReferenceId.ToString()));

        sb.Replace("{{iwsOrderItems}}", PointOfSaleItemsEncoder.GenerateChargeItemsSegment(posOrderManager));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Post an adjustment to a reservation
    /// https://hotels.cloudbeds.com/api/docs/#api-Adjustment-postAdjustment
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <returns></returns>
    internal static string UriGenerate_PostAdjustmentToReservation(
        ICloudbedsServerInfo cbServerInfo)
    {
        var sb = new StringBuilder(TemplateUrl_PostAdjustmentToReservation);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);
        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Called to generate the post contents for a reservation adjustment (i.e. add fee to a reservation)
    /// https://hotels.cloudbeds.com/api/docs/#api-Adjustment-postAdjustment
    /// </summary>
    /// <returns></returns>
    public static string UriGenerate_PostAdjustmentToReservation_PostContents(
        string reservationId, CloudbedsAdjustmentType adjustmentType, decimal amount, string notes)
    {
        //Cannonicalize
        if (string.IsNullOrWhiteSpace(notes))
        {
            notes = "";
        }
        var sb = new StringBuilder(Template_PostAdjustmentToReservation_PostContents);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsReservationId}}", HttpUtility.UrlEncode(reservationId));
        sb.Replace("{{iwsAdjustmentType}}", HttpUtility.UrlEncode(Helper_GenerateCloudbedsAdjustmentType(adjustmentType)));
        sb.Replace("{{iwsAdjustmentAmount}}", amount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        sb.Replace("{{iwsNotes}}", HttpUtility.UrlEncode(notes));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }



    /// <summary>
    /// Get the user info for the user who has authenticated us
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <returns></returns>
    internal static string UriGenerate_RequestAuthUserInfo(ICloudbedsServerInfo cbServerInfo)
    {
        var sb = new StringBuilder(TemplateUrl_RequestAuthUserInfo);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbServerInfo.ServerUrl);
        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;

    }


    /// <summary>
    /// Called to broker an access token
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthRefreshToken(CloudbedsAppConfig cbAppConfig)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthRefreshToken);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Called to broker an access token
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthRefreshToken_PostContents(CloudbedsAppConfig cbAppConfig, OAuth_RefreshToken oauthRefreshToken)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthRefreshToken_PostContents);

        //===================================================================
        //Perform the replacements
        //===================================================================
        //        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);
        sb.Replace("{{iwsClientId}}", cbAppConfig.CloudbedsAppClientId);
        sb.Replace("{{iwsClientSecret}}", cbAppConfig.CloudbedsAppClientSecret);
        sb.Replace("{{iwsOAuthRefreshToken}}", oauthRefreshToken.TokenValue);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }



    /// <summary>
    /// Turn the enumeration into text
    /// </summary>
    /// <param name="adjustment"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static string Helper_GenerateCloudbedsAdjustmentType(CloudbedsAdjustmentType adjustment)
    {
        switch (adjustment)
        {
            case CloudbedsAdjustmentType.Rate:
                return "rate";
            case CloudbedsAdjustmentType.Fee:
                return "fee";
            case CloudbedsAdjustmentType.Product:
                return "product";
            case CloudbedsAdjustmentType.Tax:
                return "tax";
            default:
                IwsDiagnostics.Assert(false, "1023-1202: Unknown Cloudbeds Adjustment type");
                throw new Exception("1023-1202: Unknown Cloudbeds Adjustment type");
        }
    }


    /// <summary>
    /// Called to broker an access token
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthAccessToken(CloudbedsAppConfig cbAppConfig)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthAccessToken);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /// <summary>
    /// Called to broker an access token
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthAccessToken_PostContents(CloudbedsAppConfig cbAppConfig, OAuth_BootstrapCode oAuthBootstrapCode)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthAccessToken_PostContents);

        //===================================================================
        //Perform the replacements
        //===================================================================
        //        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);
        sb.Replace("{{iwsClientId}}", cbAppConfig.CloudbedsAppClientId);
        sb.Replace("{{iwsClientSecret}}", cbAppConfig.CloudbedsAppClientSecret);
        sb.Replace(
            "{{iwsOAuthResponseUri}}",
            HttpUtility.UrlEncode(cbAppConfig.CloudbedsAppOAuthRedirectUri));
        sb.Replace("{{iwsOAuthCode}}", oAuthBootstrapCode.TokenValue);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }

    /*
    /// <summary>
    /// UNDONE: This does NOT work yet
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <param name="oauthCode"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthRefreshToken(CloudbedsAppConfig cbAppConfig, string oauthCode)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthRefreshToken);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);
        sb.Replace("{{iwsClientId}}", cbAppConfig.CloudbedsAppClientId);
        sb.Replace("{{iwsClientSecret}}", cbAppConfig.CloudbedsAppClientSecret);
        sb.Replace("{{iwsOAuthCode}}", oauthCode);

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }
    */

    /// <summary>
    /// URI to use for requesting an application key
    /// </summary>
    /// <param name="cbAppConfig"></param>
    /// <returns></returns>
    public static string UriGenerate_RequestOAuthAccess(CloudbedsAppConfig cbAppConfig)
    {
        var sb = new StringBuilder(TemplateUrl_RequestOAuthAccess);

        //===================================================================
        //Perform the replacements
        //===================================================================
        sb.Replace("{{iwsServerUrl}}", cbAppConfig.CloudbedsServerUrl);
        sb.Replace("{{iwsClientId}}", cbAppConfig.CloudbedsAppClientId);
        sb.Replace(
            "{{iwsOAuthResponseUri}}",
            HttpUtility.UrlEncode(cbAppConfig.CloudbedsAppOAuthRedirectUri));

        //Make sure we replaced all the tokens
        var outText = sb.ToString();
        AssertTemplateCompleted(outText);
        return outText;
    }


    /// <summary>
    /// Double check that we have taken care of the tokens
    /// </summary>
    /// <param name="text"></param>
    private static void AssertTemplateCompleted(string text)
    {
        if (text.Contains("{{"))
        {
            IwsDiagnostics.Assert(false, "722-204: Template still contains tokens");
        }
    }
}
