using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Spreadsheets;
using System.Diagnostics;
using System.Xml.Serialization;

namespace GoogleSync
{
    public class GoogleAuth
    {
        public string googleLogin { get; set; }

        public string googlePassword { get; set; }

        [XmlIgnore]
        public string client_id 
        { 
            get { return parameters.ClientId; } 
            set {parameters.ClientId = value;}
        }
        [XmlIgnore]
        public string client_secret 
        { 
            get { return parameters.ClientSecret; } 
            set { parameters.ClientSecret = value; }
        }
        [XmlIgnore]
        public string scope
        {
            get { return parameters.Scope; }
            set
            {
                if (value == null)
                    parameters.Scope = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds";
                parameters.Scope = value;
            }
        }
        [XmlIgnore]
        public string redirect_uri 
        {
            get { return parameters.RedirectUri; }
            set { parameters.RedirectUri = value; } 
        }

        public OAuth2Parameters parameters { get; set; }

        public GoogleAuth()
        {
            parameters = new OAuth2Parameters();
            scope = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds";
        }

        public GoogleAuth(string client_id,
                        string client_secret,
                        string redirect_uri,
                        string scope,
                        string googleLogin,
                        string googlePassword)
        {
            parameters = new OAuth2Parameters();
            this.googleLogin = googleLogin;
            this.googlePassword = googlePassword;
            this.client_id = client_id;
            this.client_secret = client_secret;
            this.redirect_uri = redirect_uri;
            this.scope = scope;
        }

        public string GetAuthorizationUrl()
        {
            string authorizationUrl = OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
            return authorizationUrl;
        }

        public void StartAuthInBrowser(string authorizationUrl)
        {
            ProcessStartInfo prs = new ProcessStartInfo();
            prs.Arguments = authorizationUrl;
            try
            {
                prs.FileName = "chrome.exe";
                Process.Start(prs);
            }
            catch (System.IO.FileNotFoundException ex)          
            {                
                prs.FileName = "iexplore.exe";
                Process.Start(prs);
            }    
        }

        public void GetAccess(string AccessCode)
        {             
            parameters.AccessCode = AccessCode;
            OAuthUtil.GetAccessToken(parameters);
        }

        public bool RefreshAccess()
        {
            try
            {
                OAuthUtil.RefreshAccessToken(parameters);
            }
            catch (System.Net.WebException ex)
            {
                if (ex.Status == System.Net.WebExceptionStatus.ProtocolError)
                {
                    return false;
                }
            }
            catch (System.ArgumentNullException ex)
            {                
                if (ex.ParamName == "refresh_token")
                {
                    return false;
                }
            }
            return true;
        }

        public GOAuth2RequestFactory GetRequestFactory()
        {       
            return new GOAuth2RequestFactory(null, "Default", parameters); 
        }

        public void SetUserCred(SpreadsheetsService srv)
        {
            srv.setUserCredentials(googleLogin, googlePassword);
        }

    }
}
