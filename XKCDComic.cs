using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace XKCD {
    public class XKCDComic : Comic {

        // Constructor
        public XKCDComic () : base ("http://xkcd.com/info.0.json") {
            SetBaseComic ();
        }

        #region Properties
        // Inherited Properties
        public override string MainTitle { get { return BaseContent.XKCDComDetails["title"].ToString (); } }
        public override string MainImgURL { get { return BaseContent.XKCDComDetails["img"].ToString (); } }
        public override string RandImgURL { get { return RandomComicContent.XKCDComDetails["img"].ToString (); } }
        public override string RandIssue { get { return RandomComicContent.XKCDComDetails["num"].ToString (); } }

        // Local Properties
        string ModJSONurl { get; set; }
        public string RandText { get { return ScrambleSentence (RandomComicContent.XKCDComDetails["alt"].ToString ()); } }
        int MaxNum { get; set; }

        // JSON object data
        XKCDContent BaseContent;
        XKCDContent RandomComicContent;
        #endregion

        public override void GenerateRandComic () {
            Random rnd = new Random ();
            ModJSONurl = DefaultJSON.Insert (15, "/" + rnd.Next (MaxNum).ToString ());
            RandomComicContent = GenerateComic (ModJSONurl);
        }

        public override void SetBaseComic () {
            BaseContent = GenerateComic (DefaultJSON);
            MaxNum = Convert.ToInt32 (BaseContent.XKCDComDetails["num"]);
        }

        XKCDContent GenerateComic (string originalJson) {
            try {
                using (WebClient wc = new WebClient ()) {
                    var json = wc.DownloadString (originalJson);
                    Dictionary<string, object> deserializedComic = JsonConvert.DeserializeObject<Dictionary<string, object>> (json, new JsonSerializerSettings {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    XKCDContent result = new XKCDContent ();
                    result.XKCDComDetails = deserializedComic;
                    return result;
                }
            } catch (Exception e) {
                Console.WriteLine (e.Message);
                return new XKCDContent ();
            }

        }

        // XKCD specific function
        // just scrambling the text
        public static string ScrambleSentence (string sentence) {
            var random = new Random ();
            return string.Join (" ", sentence.Split (' ').OrderBy (x => random.Next ()).ToArray ());
        }

    }

    class XKCDContent : Content {
        public Dictionary<string, object> XKCDComDetails { get; set; }

    }

    class XKCDComDetails : ComDetails {
        [JsonProperty ("month")]
        string Month { get; set; }

        [JsonProperty ("num")]
        int Num { get; set; }

        [JsonProperty ("year")]
        int Year { get; set; }

        [JsonProperty ("link")]
        string Link { get; set; }

        [JsonProperty ("news")]
        string News { get; set; }

        [JsonProperty ("safe_title")]
        string SafeTitle { get; set; }

        [JsonProperty ("transcript")]
        string Transcript { get; set; }

        [JsonProperty ("alt")]
        string Alt { get; set; }

        [JsonProperty ("img")]
        Uri Img { get; set; }

        [JsonProperty ("title")]
        string Title { get; set; }

        [JsonProperty ("day")]
        int Day { get; set; }
    }
}