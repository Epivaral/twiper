using System;
using System.Collections.Generic;
using System.Configuration;
using LinqToTwitter;
using NLog;
using System.Threading.Tasks;

namespace limpiaTL
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Usage");
            Console.WriteLine("twiper.exe \n\t-cuenta [cuenta] \n\t-source [bookmarks file path] (optional) " +
                "\n\t-roof [top tweet ID] (optional) \n\t-since [oldest tweet ID] (optional) " +
                "\n\t-dir [working dir] (optional) \n\t-debug (to run in debug mode)");

            Logger logger = LogManager.GetLogger("limpiaTL");
            logger.Info("Ejecutando aplicación");

            #region setup

            //Default arguments
            var argumentos = new Dictionary<string, string>(helper.initArgs());

            //Parse console arguments and override defaults
            var consoleArgs = new Dictionary<string, string>(helper.parseArgs(args));
            argumentos["cuenta"] = (consoleArgs.ContainsKey("cuenta") && consoleArgs["cuenta"] != null) ? consoleArgs["cuenta"] : argumentos["cuenta"];
            argumentos["bookmarksFile"] = (consoleArgs.ContainsKey("source") && consoleArgs["source"] != null) ? consoleArgs["source"] : argumentos["bookmarksFile"];
            argumentos["roofID"] = (consoleArgs.ContainsKey("roof") && consoleArgs["roof"] != null) ? consoleArgs["roof"] : argumentos["roofID"];
            argumentos["sinceID"] = (consoleArgs.ContainsKey("since") && consoleArgs["since"] != null) ? consoleArgs["since"] : argumentos["sinceID"];
            argumentos["workingDir"] = (consoleArgs.ContainsKey("dir") && consoleArgs["dir"] != null) ? consoleArgs["dir"] : argumentos["workingDir"];
            argumentos["workingDir"] = (consoleArgs.ContainsKey("debug")) ? argumentos["debugDir"] : argumentos["workingDir"];

            //OAuth
            var credential = new Dictionary<string, string>();

            var OAuthDataSection = ConfigurationManager.GetSection(OAuthConfigSection.SectionName) as OAuthConfigSection;

            //TODO - add error handling or default values if (OAuthDataSecton == null)
            foreach (OAuthElement element in OAuthDataSection.OAuthNames)
            {
                if (element.name == argumentos["cuenta"])
                {
                    credential["accessToken"] = element.accessToken;
                    credential["consumerSecret"] = element.consumerSecret;
                    credential["consumerKey"] = element.consumerKey;
                    credential["accessTokenSecret"] = element.accessTokenSecret;
                    credential["name"] = element.name;
                    break;
                }
            }

            logger.Info("Autenticando usuario \"" + credential["name"] + "\" en aplicación");
            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = credential["consumerKey"],
                    ConsumerSecret = credential["consumerSecret"],
                    AccessToken = credential["accessToken"],
                    AccessTokenSecret = credential["accessTokenSecret"]
                }
            };

            var twitterCtx = new TwitterContext(auth);
            #endregion

            //Delete all tweets from selected user
            Task deleteAllOperation = statusOperations.getTweets(twitterCtx, argumentos, argumentos["cuenta"]);
            deleteAllOperation.Wait(-1);

            logger.Info("Application exited");
        }
    }
}
