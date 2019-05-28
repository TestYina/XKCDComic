using System;
using System.Diagnostics;
using System.Threading;

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

        static void Main (string[] args) {
            try {
                do {
                    var com = new XKCDComic ();
                    openTwitter ();
                    Console.WriteLine ("Press enter to spam my Twitter with an out of context comic \n\t\tor press 'zz' to exit.");

                    while (Console.ReadKey (true).Key == ConsoleKey.Enter) {
                        com.GenerateRandComic ();
                        Console.WriteLine ($"Tweeting comic #{com.RandIssue} ... ");

                        client.PostTweet (com.RandText, com.RandImgURL);

                        Thread.Sleep (10000); //please don't ban me b/c spam
                        Console.WriteLine ("Attempt Complete!");
                    }

                    // clean up
                    closeTwitter (client);
                } while (Console.ReadKey (true).Key != ConsoleKey.Z);
            } catch (Exception e) {
                PrintErrorMessage (e.ToString ());
            }
        }

        public static void openTwitter () {
            try {
                Console.WriteLine ("Opening the best twitter of all time. I hope you have chrome!");
                process.StartInfo.FileName = Environment.GetEnvironmentVariable ("ProgramFiles(x86)") + "\\Google\\Chrome\\Application\\chrome.exe";
                process.StartInfo.Arguments = JunkTweets + " --new-window";
                process.Start ();
            } catch (System.ComponentModel.Win32Exception e) {
                PrintErrorMessage (e.ToString () + "\nWhere is Chrome, you heathen...");
            }
        }

        // clean up processes and memory
        public static void closeTwitter (TwitterClient client) {
            process.CloseMainWindow ();
            process.Close ();
            client.DeleteImagePath ();
        }

        // highlight errors from expecting Console Print
        public static void PrintErrorMessage (string message) {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine (message);
        }
    }
}
