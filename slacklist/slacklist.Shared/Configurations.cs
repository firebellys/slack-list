using System;
using System.Collections.Generic;
using System.Text;

namespace slacklist
{
    /// <summary>
    /// Simple constant storage.
    /// </summary>
    class Configurations
    {
        // TODO: Move these to a configuration file. If the API changes, it will be a harder to change this.
        // See documentation for this api key.
        public static string UserListUrl = "https://api.slack.com/api/users.list?token={0}";
        public static string AuthUrl = "https://slack.com/oauth/authorize";
        public static string TokenUrl = "https://slack.com/api/oauth.access";
        public static string PresenceUrl = "https://slack.com/api/users.getPresence?token={0}&user={1}";
        public static string ApiKey = "xoxp-4698769766-4698769768-4898023905-7a1afa";
        public static string LocalStorageFileName = "AppData.json";
    }
}
