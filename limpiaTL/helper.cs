using System;
using System.Collections.Generic;
using System.Configuration;
using NLog;

namespace limpiaTL
{
    public class helper
    {
        //Initializing tweet logger
        private static Logger logger = LogManager.GetLogger("limpiaTL");

        /// <summary>
        /// Parses command line arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Dictionary object with arguments</returns>
        public static Dictionary<string, string> parseArgs(string[] args)
        {
            var argumentos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i].ToString().StartsWith("-") || args[i].ToString().StartsWith("/")) && args[i].ToString() != "-debug")
                {
                    logger.Info("Argumento[" + i + "] " + args[i].ToString().Substring(1) + "," + args[i + 1].ToString());
                    argumentos.Add(args[i].ToString().Substring(1), args[i + 1].ToString());
                    i++;
                }
                if (args[i].ToString() == "-debug")
                {
                    logger.Debug("RUNNING DEBUG MODE");
                    argumentos.Add(args[i].ToString().Substring(1), null);
                }
            }
            return argumentos;
        }

        /// <summary>
        /// Initialize arguments from config file
        /// </summary>
        /// <returns>Dictionary object with arguments</returns>
        public static Dictionary<string, string> initArgs()
        {
            var argumentos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            argumentos["cuenta"] = ""; //default twitter screen name 
            argumentos["roofID"] = ConfigurationManager.AppSettings["roofID"];
            argumentos["sinceID"] = ConfigurationManager.AppSettings["sinceID"];
            argumentos["workingDir"] = ConfigurationManager.AppSettings["workingDir"];
            argumentos["debugDir"] = ConfigurationManager.AppSettings["debugDir"];
            argumentos["bookmarksFile"] = ConfigurationManager.AppSettings["bookmarksFile"];
            return argumentos;
        }

        /// <summary>
        /// Convert Unix miliseconds to DateTime
        /// </summary>
        /// <param name="unixTimeStamp">int double / miliseconds</param>
        /// <returns>DateTime object</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
