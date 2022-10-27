using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.IO;


/// <summary>
/// Base class on top of which requests to Cloudbeds are based
/// </summary>
abstract class CloudbedsRequestBase
{

    readonly TaskStatusLogs _statusLogs;
    public TaskStatusLogs StatusLog
    {
        get
        {
            return _statusLogs;
        }
    }

    protected CloudbedsRequestBase(TaskStatusLogs statusLogs)
    {
        _statusLogs = statusLogs;
    }


   protected virtual HttpRequestMessage Internal_CreateHttpRequest(string httpMethod, string uri)
    {
        var httpRequest = new HttpRequestMessage(new HttpMethod(httpMethod), uri);
        return httpRequest;
    }

    /// <summary>
    /// Creates a HTTP request
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="bodyText"></param>
    /// <returns></returns>
    protected HttpRequestMessage CreateHttpRequest_Post_WithUriEncodedForm(string uri, string bodyText)
    {
        var httpRequest = Internal_CreateHttpRequest("POST", uri);

        httpRequest.Content = new StringContent(bodyText, System.Text.UTF8Encoding.UTF8, "application/x-www-form-urlencoded");
        return httpRequest;
    }

    /// <summary>
    /// Creates a HTTP request
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="bodyText"></param>
    /// <returns></returns>
    protected HttpRequestMessage CreateHttpRequest_Get(string uri)
    {
        var httpRequest = Internal_CreateHttpRequest("GET", uri);

        //httpRequest.Content = new StringContent(bodyText, System.Text.UTF8Encoding.UTF8, "application/x-www-form-urlencoded");
        return httpRequest;
    }


    /// <summary>
    /// Debugging function: Allows us to test that all of our content was replaced
    /// </summary>
    /// <param name="text"></param>
    protected void AssertTemplateFullyReplaced(string text)
    {
        if (text.Contains("{{iws"))
        {
            System.Diagnostics.Debug.Assert(false, "Text still contains template fragments");
        }
    }

    /// <summary>
    /// Gets the web response as a Json document
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    protected static JsonDocument GetWebResponseAsJson(HttpResponseMessage response)
    {
        
        string streamText = "";
        var responseStream = response.Content.ReadAsStream();
        using (responseStream)
        {
            var streamReader = new StreamReader(responseStream);
            using (streamReader)
            {
                streamText = streamReader.ReadToEnd();
                streamReader.Close();
            }
            responseStream.Close();
        }

        var jsonDoc = JsonDocument.Parse(streamText);
        return jsonDoc;
    }

    /// <summary>
    /// Gets the web response as a XML document
    /// </summary>
    protected static System.Xml.XmlDocument GetWebResponseAsXml(HttpResponseMessage response)
    {
        string streamText = "";
        var responseStream = response.Content.ReadAsStream();
        using (responseStream)
        {
            var streamReader = new StreamReader(responseStream);
            using (streamReader)
            {
                streamText = streamReader.ReadToEnd();
                streamReader.Close();
            }
            responseStream.Close();
        }

        var xmlDoc = new System.Xml.XmlDocument();
        xmlDoc.LoadXml(streamText);
        return xmlDoc;
    }

    /// <summary>
    /// Gets the web response as a XML document
    /// </summary>
    protected static string GetWebResponseAsText(HttpResponseMessage response)
    {
        string streamText = "";
        var responseStream = response.Content.ReadAsStream();
        using (responseStream)
        {
            var streamReader = new StreamReader(responseStream);
            using (streamReader)
            {
                streamText = streamReader.ReadToEnd();
                streamReader.Close();
            }
            responseStream.Close();
        }

        return streamText;
    }


    /// <summary>
    /// Get the web response; log any error codes that occur and rethrow the exception.
    /// This allows us to get error log data with detailed information
    /// </summary>
    /// <param name="webRequest"></param>
    /// <returns></returns>
    protected HttpResponseMessage GetWebReponseLogErrors(HttpRequestMessage webRequest, string description)
    {
        string requestUri = webRequest.RequestUri.ToString();
        try
        {
            return HttpRequestsSingleton.Client.Send(webRequest);
        }
        catch (HttpRequestException webException)
        {
            AttemptToLogWebException(webException, description + " (" + requestUri + ") ", this.StatusLog);
            throw webException;
        }
    }


    /// <summary>
    /// Attempt to log any detailed information we find about the failed web request
    /// </summary>
    /// <param name="webException"></param>
    /// <param name="onlineStatusLog"></param>
    private static void AttemptToLogWebException(HttpRequestException webException, string description, TaskStatusLogs onlineStatusLog)
    {
        if (onlineStatusLog == null) return; //No logger? nothing to do

        try
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                description = "web request failed";
            }
            string responseText = "";
            /*
            //NOTE: In some cases (e.g. time-out) the response may be NULL
            var response = webException.;
            if (response != null)
            {
                responseText = GetWebResponseAsText(response);
                response.Close();
            }

            //Cannonicalize a blank result...
            if (string.IsNullOrEmpty(responseText))
            {
                responseText = "";
            }*/

            onlineStatusLog.AddError(description + ": " + webException.Message + "\r\n" + responseText + "\r\n");
        }
        catch (Exception ex)
        {
            onlineStatusLog.AddError("811-830: Error in web request exception: " + ex.Message);
            return;
        }
    }

}
