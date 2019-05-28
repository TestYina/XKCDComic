// abstract class
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XKCD {
    public abstract class Comic {

        protected Comic (string setURL) {
            DefaultJSON = setURL;
        }

        public string DefaultJSON { get; set; }
        public abstract string MainTitle { get; }
        public abstract string MainImgURL { get; }

        public abstract string RandImgURL { get; }
        public abstract string RandIssue { get; }

        public abstract void GenerateRandComic ();
        public abstract void SetBaseComic ();

    }

    public abstract class Content {
        public Dictionary<string, object> ComDetails { get; set; }
    }

    public abstract class ComDetails {
        // For JSON properties per comic
        // Also if we need to have something to unify them in the JSON
    }

    // Ideally we might need to have our own converter with specific formats and to also handle null values
    // Luckily XKCD comics are pretty straightforward - leaving this commented out in case it should need to be built out
    /*  public class DetailedComicConverter : JsonConverter {
            public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
               throw new JsonSerializationException ("Unexpected format in DetailedComicConverter: " + token.ToString ());
            }

            public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer) {
            }

            public override bool CanConvert (Type objectType) {
                return if (something?);
            }
        }
    */

}