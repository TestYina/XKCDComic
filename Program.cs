using System;
using System.Diagnostics;
using System.Threading;

/// <summary>
/// The main class.
/// Console application.
/// </summary>
namespace XKCD {
    class Program {

        #region Properties
        static string JunkTweets = "https://twitter.com/junk_us";
        static Process process = new Process ();


        // PLACE API KEYS HERE
        static string conKey = "";
        static string conKeySecret = "";
        static string accToken = "";
        static string accTokenSecret = "";

        static TwitterClient client = new TwitterClient (conKey, conKeySecret, accToken, accTokenSecret);

        #endregion

        /// <summary>
        /// The main <c>Main</c> method
        /// Contains all methods for performing basic math functions
        /// </summary>
        static void Main (string[] args) {
            try {
                do {
                    var com = new XKCDComic ();
                    OpenTwitter ();
                    Console.WriteLine ("Press enter to spam my Twitter with an out of context comic \n\t\tor press 'zz' to exit.");

                    while (Console.ReadKey (true).Key == ConsoleKey.Enter) {
                        com.GenerateRandComic ();
                        Console.WriteLine ($"Tweeting comic #{com.RandIssue} ... ");

                        client.PostTweet (com.RandText, com.RandImgURL);

                        Thread.Sleep (10000); //please don't ban me b/c spam
                        Console.WriteLine ("Attempt Complete!");
                    }

                    // clean up
                    CloseTwitter (client);
                } while (Console.ReadKey (true).Key != ConsoleKey.Z);
            } catch (Exception e) {
                PrintErrorMessage (e.ToString ());
            }
        }

        /// <summary>
        /// Opens a new chrome window for Twitter - junk_us
        /// </summary>
        public static void OpenTwitter () {
            try {
                Console.WriteLine ("Opening the best twitter of all time. I hope you have chrome!");
                process.StartInfo.FileName = Environment.GetEnvironmentVariable ("ProgramFiles(x86)") + "\\Google\\Chrome\\Application\\chrome.exe";
                process.StartInfo.Arguments = JunkTweets + " --new-window";
                process.Start ();
            } catch (System.ComponentModel.Win32Exception e) {
                PrintErrorMessage (e.ToString () + "\nWhere is Chrome, you heathen...");
            }
        }

        /// <summary>
        /// Cleans up process-specific properties
        /// Deletes path created for downloaded images
        /// </summary>
        public static void CloseTwitter (TwitterClient client) {
            process.CloseMainWindow ();
            process.Close ();
            client.DeleteImagePath ();
        }

        /// <summary>
        /// Customizes console messages to differentiate between expected and errors
        /// Highlights error messages with red background and white text
        /// </summary>
        public static void PrintErrorMessage (string message) {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine (message);
        }
    }
}
