using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FileManagerSdk
{
    /// <summary>
    /// oms系统api
    /// </summary>
    public partial interface IFileClient
    {
        ///// <summary>
        ///// Sets a basic authentication header.
        ///// </summary>
        ///// <param name="userName">Name of the user.</param>
        ///// <param name="password">The password.</param>
        //void SetBasicAuthentication(string userName, string password);
        ///// <summary>
        ///// Sets a basic authentication header for RFC6749 client authentication.
        ///// </summary>
        ///// <param name="userName">Name of the user.</param>
        ///// <param name="password">The password.</param>
        //void SetBasicAuthenticationOAuth(string userName, string password);
        ///// <summary>
        ///// Sets an authorization header with a given scheme and value.
        ///// </summary>
        ///// <param name="scheme">The scheme.</param>
        ///// <param name="token">The token.</param>
        //void SetToken(string scheme, string token);

        ///// <summary>
        ///// Sets an authorization header with a bearer token.
        ///// </summary>
        ///// <param name="token">The token.</param>
        //void SetBearerToken(string token);
        ///// <summary>
        ///// Sets an authorization header with a Basic token.
        ///// </summary>
        ///// <param name="token">The token.</param>
        //void SetBasicToken(string token);
    }
    /// <summary>
    /// oms系统api
    /// </summary>
    public partial class FileClient
    {
        public FileClient() : this(new HttpClient())
        {
        }
        ///// <summary>
        ///// Sets a basic authentication header.
        ///// </summary>
        ///// <param name="userName">Name of the user.</param>
        ///// <param name="password">The password.</param>
        //public void SetBasicAuthentication(string userName, string password)
        //{
        //    _httpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(userName, password);
        //}

        ///// <summary>
        ///// Sets a basic authentication header for RFC6749 client authentication.
        ///// </summary>
        ///// <param name="userName">Name of the user.</param>
        ///// <param name="password">The password.</param>
        //public void SetBasicAuthenticationOAuth(string userName, string password)
        //{
        //    _httpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationOAuthHeaderValue(userName, password);
        //}

        ///// <summary>
        ///// Sets an authorization header with a given scheme and value.
        ///// </summary>
        ///// <param name="scheme">The scheme.</param>
        ///// <param name="token">The token.</param>
        //public void SetToken(string scheme, string token)
        //{
        //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
        //}

        ///// <summary>
        ///// Sets an authorization header with a bearer token.
        ///// </summary>
        ///// <param name="token">The token.</param>
        //public void SetBearerToken(string token)
        //{
        //    this.SetToken("Bearer", token);
        //}/// <summary>
        // /// Sets an authorization header with a Basic token.
        // /// </summary>
        // /// <param name="token">The token.</param>
        //public void SetBasicToken(string token)
        //{
        //    this.SetToken("Basic", token);
        //}

        ///// <summary>
        ///// HTTP Basic Authentication authorization header for RFC6749 client authentication
        ///// </summary>
        ///// <seealso cref="System.Net.Http.Headers.AuthenticationHeaderValue" />
        //internal class BasicAuthenticationOAuthHeaderValue : AuthenticationHeaderValue
        //{
        //    /// <summary>
        //    /// Initializes a new instance of the <see cref="BasicAuthenticationOAuthHeaderValue"/> class.
        //    /// </summary>
        //    /// <param name="userName">Name of the user.</param>
        //    /// <param name="password">The password.</param>
        //    public BasicAuthenticationOAuthHeaderValue(string userName, string password)
        //        : base("Basic", EncodeCredential(userName, password))
        //    { }

        //    /// <summary>
        //    /// Encodes the credential.
        //    /// </summary>
        //    /// <param name="userName">Name of the user.</param>
        //    /// <param name="password">The password.</param>
        //    /// <returns></returns>
        //    /// <exception cref="ArgumentNullException">userName</exception>
        //    public static string EncodeCredential(string userName, string password)
        //    {
        //        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));
        //        if (password == null) password = "";

        //        var encoding = System.Text.Encoding.UTF8;
        //        string credential = $"{UrlEncode(userName)}:{UrlEncode(password)}";

        //        return Convert.ToBase64String(encoding.GetBytes(credential));
        //    }

        //    private static string UrlEncode(string value)
        //    {
        //        if (String.IsNullOrEmpty(value))
        //        {
        //            return String.Empty;
        //        }

        //        return Uri.EscapeDataString(value).Replace("%20", "+");
        //    }
        //}
        ///// <summary>
        ///// HTTP Basic Authentication authorization header
        ///// </summary>
        ///// <seealso cref="System.Net.Http.Headers.AuthenticationHeaderValue" />
        //internal class BasicAuthenticationHeaderValue : AuthenticationHeaderValue
        //{
        //    /// <summary>
        //    /// Initializes a new instance of the <see cref="BasicAuthenticationHeaderValue"/> class.
        //    /// </summary>
        //    /// <param name="userName">Name of the user.</param>
        //    /// <param name="password">The password.</param>
        //    public BasicAuthenticationHeaderValue(string userName, string password)
        //        : base("Basic", EncodeCredential(userName, password))
        //    { }

        //    /// <summary>
        //    /// Encodes the credential.
        //    /// </summary>
        //    /// <param name="userName">Name of the user.</param>
        //    /// <param name="password">The password.</param>
        //    /// <returns></returns>
        //    /// <exception cref="ArgumentNullException">userName</exception>
        //    public static string EncodeCredential(string userName, string password)
        //    {
        //        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));
        //        if (password == null) password = "";

        //        var encoding = System.Text.Encoding.UTF8;
        //        string credential = String.Format("{0}:{1}", userName, password);

        //        return Convert.ToBase64String(encoding.GetBytes(credential));
        //    }
        //}
    }
}