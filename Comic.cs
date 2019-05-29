using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XKCD {
    /// <summary>
    /// The main abstract comic class.
    /// Contains basic and necessary information for all comics.
    /// </summary>
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
    
    /// <summary>
    /// Data structure for each comic and information
    /// </summary>
    public abstract class Content {
        public Dictionary<string, object> ComDetails { get; set; }
    }

    /// <summary>
    /// Class to store JSON properties per comic
    /// </summary>
    /// <remarks>
    /// Also if we need to have something to unify them in the JSON
    /// </remarks>
    public abstract class ComDetails {
       
    }

    
    /// <summary>
    /// Specialized JsonConverter for specific formats
    /// </summary>
    /// <remarks>
    /// For this example, XKCD comics are straightforward and this did not need to be implemented
    /// inserting in case we will need to.
    /// </remarks>
    
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
