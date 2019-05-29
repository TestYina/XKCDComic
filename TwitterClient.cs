// Note: definitely relied heavily on google for guidance for the bot
// but it works and was a cool learning experience

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

/// <summary>
/// The <c>Twitter Client</c> class
/// Contains all methods for handling posts to Twitter
/// </summary>
namespace XKCD {
    public class TwitterClient {

        // OAuth 
        readonly string _consumerKey, _consumerKeySecret, _accessToken, _accessTokenSecret;
        readonly HMACSHA1 _sigHasher;
        readonly DateTime _epochUtc = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // endpoints
        readonly string _TwitterTextAPI, _TwitterImageAPI;

        // helper properties
        readonly int _limit;
        string tempDirectory;
        string _errorMessage;

        /// <summary>
        /// Constructor that takes in API keys
        /// Sets security hash
        /// </summary>
        public TwitterClient (string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret, int limit = 280) {
            _TwitterTextAPI = "https://api.twitter.com/1.1/statuses/update.json";
            _TwitterImageAPI = "https://upload.twitter.com/1.1/media/upload.json";

            _consumerKey = consumerKey;
            _consumerKeySecret = consumerKeySecret;
            _accessToken = accessToken;
            _accessTokenSecret = accessTokenSecret;
            _limit = limit;

            _sigHasher = new HMACSHA1 (new ASCIIEncoding ().GetBytes ($"{_consumerKeySecret}&{_accessTokenSecret}"));
        }

        /// <summary>
        /// Creates post to twitter by breaking up the post by image and text
        /// </summary>
        /// <param name="post">text portion/param>
        /// <param name="imageUri">image uri to be downloaded</param>           
        public string PostTweet (string post, string imageUri) {
            try {
                // first, download the image into a temp folder
                string tempPath = GetImagePath (imageUri);

                // now upload the image to Twitter
                // wait for response about image vs trying to evaluate variable
                var resultImg = Task.Run (async () => {
                    var response = await TweetImage (tempPath);
                    return response;
                });

                // grab string w/ JSON
                var resultImgJSON = JObject.Parse (resultImg.Result.Item2);
                CheckErrorCode (resultImg, resultImgJSON);

                // get necessary media_id for post upload
                string mediaID = resultImgJSON["media_id_string"].Value<string> ();

                // second, send the text with the uploaded image
                var resultText = Task.Run (async () => {
                    var response = await TweetText (MatchTextLimit (post), mediaID);
                    return response;
                });
                var resultTextJSON = JObject.Parse (resultText.Result.Item2);

                CheckErrorCode (resultText, resultTextJSON);

                return (string.IsNullOrEmpty (_errorMessage) ? "OK" : _errorMessage);

            } catch (Exception e) {
                Console.WriteLine ($"Errors up until now: {_errorMessage} \nbut also with this: {e.ToString()}");
                return "Did not publish tweet.";
            }
        }

        /// <summary>
        /// Checks for success code in HTTPResponse
        /// </summary>
        void CheckErrorCode (Task<Tuple<int, string>> resultTask, JObject resultJson) {
            if (resultTask.Result.Item1 != 200) {
                _errorMessage += $"Error uploading image portion to Twitter: {resultJson.Value<String>()}  \n";
            }
        }

        /// <summary>
        /// Prepares text and image for posting along with destination endpoint
        /// </summary>
        public Task<Tuple<int, string>> TweetText (string text, string mediaID) {
            var textData = new Dictionary<string, string> { { "status", text },
                    { "trim_user", "1" },
                    { "media_ids", mediaID }
                };

            return SendText (_TwitterTextAPI, textData);
        }

        /// <summary>
        /// Upload initial image to twitter
        /// </summary>
        public Task<Tuple<int, string>> TweetImage (string pathToImage) {
            byte[] imgdata = System.IO.File.ReadAllBytes (pathToImage);
            var imageContent = new ByteArrayContent (imgdata);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue ("multipart/form-data");

            var multipartContent = new MultipartFormDataContent ();
            multipartContent.Add (imageContent, "media");

            return SendImage (_TwitterImageAPI, multipartContent);
        }

        /// <summary>
        /// Sends text with image information to Twitter
        /// </summary>
        /// <returns> 
        /// Status code and response
        /// </returns>
        async Task<Tuple<int, string>> SendText (string URL, Dictionary<string, string> textData) {
            using (var httpClient = new HttpClient ()) {
                httpClient.DefaultRequestHeaders.Add ("Authorization", PrepareOAuth (URL, textData));

                var httpResponse = await httpClient.PostAsync (URL, new FormUrlEncodedContent (textData));
                var httpContent = await httpResponse.Content.ReadAsStringAsync ();

                return new Tuple<int, string> (
                    (int) httpResponse.StatusCode,
                    httpContent
                );
            }
        }

        /// <summary>
        /// Sends image information to Twitter
        /// </summary>
        /// <returns> 
        /// Status code and response
        /// </returns>
        async Task<Tuple<int, string>> SendImage (string URL, MultipartFormDataContent multipartContent) {
            using (var httpClient = new HttpClient ()) {
                httpClient.DefaultRequestHeaders.Add ("Authorization", PrepareOAuth (URL, null));

                var httpResponse = await httpClient.PostAsync (URL, multipartContent);
                var httpContent = await httpResponse.Content.ReadAsStringAsync ();

                return new Tuple<int, string> (
                    (int) httpResponse.StatusCode,
                    httpContent
                );
            }
        }

        #region Helper Functions
        /// <summary>
        /// Shortens text to twitter character limit
        /// </summary>
        /// <returns> 
        /// Shortened text
        /// </returns>
        string MatchTextLimit (string text) {
            while (text.Length >= _limit) {
                text = text.Substring (0, text.LastIndexOf (" "));
            }

            return text;
        }

        /// <summary>
        /// Creates temporary image directory for all images needed to download
        /// </summary>
        /// <returns> 
        /// Image paths from comic
        /// </returns>
        string GetImagePath (string imageUri) {
            try {
                // check if tempDirectory was already made
                if (string.IsNullOrEmpty (tempDirectory)) {
                    string pathStart = string.IsNullOrEmpty (Path.GetTempPath ()) ? @"C:\Windows\TEMP" : Path.GetTempPath ();
                    tempDirectory = Path.Combine (pathStart, "Tweet_Test_Bot_XKCD");
                    Directory.CreateDirectory (tempDirectory);
                }

                // creates file name for specific image
                string tempFileName = Path.Combine (tempDirectory, imageUri.Split ('/').Last ());

                // download file
                // since this is one image, we will use the synchronous version vs async
                using (var client = new WebClient ()) {
                    client.DownloadFile (imageUri, tempFileName);
                }

                return tempFileName;
            } catch (Exception e) {
                Console.WriteLine ("An error occurred downloading file: " + e.ToString ());
                return string.Empty;
            }
        }

        /// <summary>
        /// Deletes temp folder created for this exercise
        /// </summary>
        public void DeleteImagePath () {
            try {
                // there may be permission errors
                Directory.Delete (tempDirectory, true);
            } catch (Exception e) {
                Console.WriteLine ($"Error deleting directory: {e.ToString()}");
            }
        }
        #endregion

        #region OAuth Hocus Pocus
        // the standard way
        string PrepareOAuth (string URL, Dictionary<string, string> data) {
            // seconds passed since 1/1/1970
            var timestamp = (int) ((DateTime.UtcNow - _epochUtc).TotalSeconds);

            // Add all the OAuth headers we'll need to use when constructing the hash
            Dictionary<string, string> oAuthData = new Dictionary<string, string> ();
            oAuthData.Add ("oauth_consumer_key", _consumerKey);
            oAuthData.Add ("oauth_signature_method", "HMAC-SHA1");
            oAuthData.Add ("oauth_timestamp", timestamp.ToString ());
            oAuthData.Add ("oauth_nonce", Guid.NewGuid ().ToString ());
            oAuthData.Add ("oauth_token", _accessToken);
            oAuthData.Add ("oauth_version", "1.0");

            if (data != null) // add text data too, because it is a part of the signature
            {
                foreach (var item in data) {
                    oAuthData.Add (item.Key, item.Value);
                }
            }

            // Generate the OAuth signature and add it to our payload
            oAuthData.Add ("oauth_signature", GenerateSignature (URL, oAuthData));

            // Build the OAuth HTTP Header from the data
            return GenerateOAuthHeader (oAuthData);
        }

        /// <summary>
        /// Generate an OAuth signature from OAuth header values
        /// </summary>
        string GenerateSignature (string url, Dictionary<string, string> data) {
            var sigString = string.Join (
                "&",
                data
                .Union (data)
                .Select (kvp => string.Format ("{0}={1}", Uri.EscapeDataString (kvp.Key), Uri.EscapeDataString (kvp.Value)))
                .OrderBy (s => s)
            );

            var fullSigData = string.Format ("{0}&{1}&{2}",
                "POST",
                Uri.EscapeDataString (url),
                Uri.EscapeDataString (sigString.ToString ())
            );

            return Convert.ToBase64String (
                _sigHasher.ComputeHash (
                    new ASCIIEncoding ().GetBytes (fullSigData.ToString ())
                )
            );
        }

        /// <summary>
        /// Generate the raw OAuth HTML header from the values (including signature)
        /// </summary>
        string GenerateOAuthHeader (Dictionary<string, string> data) {
            return string.Format (
                "OAuth {0}",
                string.Join (
                    ", ",
                    data
                    .Where (kvp => kvp.Key.StartsWith ("oauth_"))
                    .Select (
                        kvp => string.Format ("{0}=\"{1}\"",
                            Uri.EscapeDataString (kvp.Key),
                            Uri.EscapeDataString (kvp.Value)
                        )
                    ).OrderBy (s => s)
                )
            );
        }
        #endregion

    }

}
