using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using OneNightWerewolf.Models;
using DotNetOpenAuth.AspNet.Clients;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth.Messages;
using DotNetOpenAuth.OAuth.ChannelElements;
using Microsoft.Owin.Host.SystemWeb;

namespace OneNightWerewolf
{
    public static class AuthConfig
    {
        public const string TW_CONSUMER_KEY = "hVPsAe1BPYEE98kGtgXxYA";
        public const string TW_CONSUMER_SECRET = "n4lX8MSOLA6kIkhhdLrznFg7tn5hTFi4UDalEBrevE";

        public static void RegisterAuth()
        {
            // このサイトのユーザーが、Microsoft、Facebook、および Twitter などの他のサイトのアカウントを使用してログインできるようにするには、
            // このサイトを更新する必要があります。詳細については、http://go.microsoft.com/fwlink/?LinkID=252166 を参照してください

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            OAuthWebSecurity.RegisterClient(new TwitterClient(
                  consumerKey: TW_CONSUMER_KEY,
                  consumerSecret: TW_CONSUMER_SECRET), "Twitter", null);

            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: "",
            //    appSecret: "");

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }

    public class TwitterClient : OAuthClient
    {
        /// <summary>
        /// The description of Twitter's OAuth protocol URIs for use with their "Sign in with Twitter" feature.
        /// </summary>
        public static readonly ServiceProviderDescription TwitterServiceDescription = new ServiceProviderDescription
        {
            RequestTokenEndpoint =
                new MessageReceivingEndpoint(
                    "https://api.twitter.com/oauth/request_token",
                    HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            UserAuthorizationEndpoint =
                new MessageReceivingEndpoint(
                    "https://api.twitter.com/oauth/authenticate",
                    HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            AccessTokenEndpoint =
                new MessageReceivingEndpoint(
                    "https://api.twitter.com/oauth/access_token",
                    HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
        };

        public TwitterClient(string consumerKey, string consumerSecret) :
            base("twitter", TwitterServiceDescription, consumerKey, consumerSecret) { }

        /// Check if authentication succeeded after user is redirected back from the service provider.
        /// The response token returned from service provider authentication result. 
        protected override DotNetOpenAuth.AspNet.AuthenticationResult VerifyAuthenticationCore(AuthorizedTokenResponse response)
        {
            string accessToken = response.AccessToken;
            string accessSecret = (response as ITokenSecretContainingMessage).TokenSecret;
            string userId = response.ExtraData["user_id"];
            string userName = response.ExtraData["screen_name"];

            var extraData = new Dictionary<string, string>()
                            {
                                {"accesstoken", accessToken},
                                {"accesssecret", accessSecret}
                            };
            return new DotNetOpenAuth.AspNet.AuthenticationResult(
                isSuccessful: true,
                provider: ProviderName,
                providerUserId: userId,
                userName: userName,
                extraData: extraData);
        }
    }
}
