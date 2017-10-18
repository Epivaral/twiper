using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;
using System.Threading;
using System.Globalization;
using System.IO;
using NLog;

namespace limpiaTL
{
    public class statusOperations
    {
        //Logger to record deleted tweets
        private static Logger logger = LogManager.GetLogger("limpiaTL");

        //DateTime before which tweets will be deleted
        private static DateTime horaLimite;

        /// <summary>
        /// Gets user input in hours and calculates DateTime before which tweets will be deleted
        /// e.g. 1 = tweets from the last 1 hour won't be deleted
        /// </summary
        private static void calcularHoraLimite()
        {
            Console.WriteLine("\n\nDelete tweets before how many hours ago?");
            string s = Console.ReadLine();
            int horas = 0;
            if (s != null)
                horas = Convert.ToInt16(s);

            TimeSpan horasDeGracia = new TimeSpan(horas, 0, 0);
            horaLimite = DateTime.UtcNow - horasDeGracia;
            logger.Info("Deleting tweets before " + horaLimite);
        }

        /// <summary>
        /// Method to delete tweets
        /// </summary>
        /// <param name="twitterCtx"></param>
        /// <param name="argumentos"></param>
        /// <param name="screenName"></param>
        public static async Task getTweets(TwitterContext twitterCtx, Dictionary<string, string> argumentos, string screenName)
        {
            ulong sinceID = Convert.ToUInt64(argumentos["sinceID"]);
            ulong maxID = Convert.ToUInt64(argumentos["roofID"]);

            ulong newMaxID = maxID;
            ulong newSinceID = sinceID;

            calcularHoraLimite();

            do
            {
                //Get Tweet list
                logger.Info("Querying tweets between " + newSinceID + " - " + (newMaxID - 1));
                var tweets =
                    await
                    (from tweet in twitterCtx.Status
                     where tweet.Type == StatusType.User &&
                           tweet.ScreenName == screenName &&
                           tweet.Count == 200 &&
                           tweet.MaxID == newMaxID - 1 &&
                           tweet.SinceID == newSinceID &&
                           tweet.CreatedAt <= horaLimite
                     select tweet)
                    .ToListAsync();

                //Waits 15 minutes to continue if Twitter API rate limit exceeded
                if (twitterCtx.RateLimitRemaining == 0)
                {
                    TimeSpan interval = new TimeSpan(0, 15, 0);
                    logger.Info("Suspending 15 minutos until " + helper.UnixTimeStampToDateTime(twitterCtx.RateLimitReset));
                    Thread.Sleep(interval);
                }

                if (tweets.Count == 0)
                {
                    logger.Info("Empty response");
                    break;
                }


                //Gets white listed tweet IDs that will be skipped
                List<ulong> keep = Read(argumentos["workingDir"] + "keep.txt");

                if (keep == null)
                {
                    logger.Info("White list empty, requesting confirmation...");
                    Console.WriteLine("\n\nWhite list is empty, all tweets will be deleted. Continue? Y/N");
                    bool continueDeleting = Console.ReadLine().Equals("s", StringComparison.OrdinalIgnoreCase);

                    if (continueDeleting == false)
                    {
                        logger.Info("Deletion cancelled, terminating application");
                        Environment.Exit(0);
                    }

                    logger.Info("Confirmed, deleting all tweets");
                }


                foreach (var item in tweets)
                {
                    if (keep.IndexOf(item.StatusID) == -1)
                    {
                        string tweetText = item.Text.Replace("\n", " ").Replace("\r", " ");
                        CultureInfo ci = new CultureInfo("en-US");
                        string UTCTime = item.CreatedAt.ToString("G", ci);

                        logger.Info(item.StatusID.ToString() + "\t" +
                            UTCTime + "\t" +
                            tweetText + "\t" +
                            item.User.UserIDResponse.ToString() + "\t" +
                            item.User.ScreenNameResponse.ToString() + "\t" +
                            item.RetweetCount.ToString() + "\t" +
                            item.FavoriteCount.ToString() + "\t" +
                            item.Coordinates.Latitude.ToString() + "," + item.Coordinates.Longitude.ToString());

                        Task deleteOperation = deleteStatus(twitterCtx, item.StatusID);
                        deleteOperation.Wait(-1);
                    }

                    if (twitterCtx.RateLimitRemaining == 0)
                    {
                        TimeSpan interval = new TimeSpan(0, 15, 0);
                        logger.Info("Suspending 15 minutos until " + helper.UnixTimeStampToDateTime(twitterCtx.RateLimitReset));
                        Thread.Sleep(interval);
                    }
                }
                newMaxID = tweets.Last().StatusID;
            }
            while (1 == 1);
        }

        /// <summary>
        /// Borra un estatus
        /// </summary>
        /// <param name="twitterCtx"></param>
        /// <param name="statusID"></param>
        /// <returns></returns>
        public static async Task deleteStatus(TwitterContext twitterCtx, ulong statusID)
        {
            try
            {
                Status status = await twitterCtx.DeleteTweetAsync(statusID);
                logger.Info(statusID.ToString() + " deleted.");
            }
            catch (Exception ex)
            {
                logger.Error("Error deleting " + statusID.ToString() + ": " + ex.ToString());
            }
        }

        /// <summary>
        /// Read status IDs from a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static List<ulong> Read(string file)
        {
            var result = new List<ulong>();

            using (StreamReader reader = new StreamReader(file))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    result.Add(Convert.ToUInt64(line));
            }
            return result;
        }

        /// <summary>
        /// Writes list of IDs to a file
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="file"></param>
        static void Write(List<ulong> IDs, string file)
        {
            using (StreamWriter writer = new StreamWriter(file))
            {
                foreach (ulong item in IDs)
                {
                    writer.WriteLine(item.ToString());
                }
            }
        }
    }
}
