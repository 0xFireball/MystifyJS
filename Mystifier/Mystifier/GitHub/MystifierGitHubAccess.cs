using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Octokit;
using FileMode = System.IO.FileMode;

namespace Mystifier.GitHub
{
    internal class MystifierGitHubAccess
    {
        public GitHubClient ApiClient { get; set; }
        public string GitHubAuthInfo { get; }
        public Credentials GitHubCredentials { get; set; }
        public bool HasCredentials { get; }
        public bool IsAvailable { get; }
        public bool GitHubAuthenticationValid { get; set; }
        public bool ConnectionAvailable { get; set; }
        public User CurrentlyAuthenticatedUser { get; set; }

        private const string AuthInfoFile = ".gittoken";

        public MystifierGitHubAccess()
        {
            GitHubAuthInfo = LoadAuthInfo();
            HasCredentials = GitHubAuthInfo != null;
            if (HasCredentials)
            {
                AttemptLoadingCredentials();
            }
            IsAvailable = MystifierUtil.IsInternetConnectionAvailable();
        }

        private void AttemptLoadingCredentials()
        {
            if (GitHubAuthInfo.Contains(" "))
            {
                //It's un/pw set
                int splitIndex = GitHubAuthInfo.IndexOf(" ", StringComparison.Ordinal);
                string un = GitHubAuthInfo.Substring(0, splitIndex);
                string pw = GitHubAuthInfo.Substring(splitIndex + 1);
                GitHubCredentials = new Credentials(un, pw);
            }
            else
            {
                GitHubCredentials = new Credentials(GitHubAuthInfo);
            }
        }

        public void LoadCredentials(Credentials gitHubCreds)
        {
            GitHubCredentials = gitHubCreds;
            if (gitHubCreds.AuthenticationType == AuthenticationType.Oauth)
            {
                SaveAuthInfo(gitHubCreds.GetToken());
            }
            else if (gitHubCreds.AuthenticationType == AuthenticationType.Basic)
            {
                SaveAuthInfo(gitHubCreds.Login + " " + gitHubCreds.Password);
            }
        }

        public async Task<bool> CheckIfAuthenticationIsValid()
        {
            if (!MystifierUtil.IsInternetConnectionAvailable())
            {
                GitHubAuthenticationValid = false;
                ConnectionAvailable = false;
            }
            else
            {
                ConnectionAvailable = true;
                if (GitHubCredentials == null)
                    return false;
                ApiClient = new GitHubClient(new ProductHeaderValue("Mystifier-Studio"))
                {
                    Credentials = GitHubCredentials
                };
                try
                {
                    CurrentlyAuthenticatedUser = await ApiClient.User.Current();
                    GitHubAuthenticationValid = true;
                }
                catch (AuthorizationException)
                {
                    return false;
                }

            }
            return GitHubAuthenticationValid;
        }

        private string LoadAuthInfo()
        {
            string AuthInfo = "";
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null,
                null);
            if (isoStore.FileExists(AuthInfoFile))
            {
                using (var isoStream = new IsolatedStorageFileStream(AuthInfoFile, FileMode.Open, isoStore))
                {
                    using (var reader = new StreamReader(isoStream))
                    {
                        AuthInfo = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                return null;
            }
            return AuthInfo;
        }

        public void SaveAuthInfo(string AuthInfo)
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null,
                null);
            using (var isoStream = new IsolatedStorageFileStream(AuthInfoFile, FileMode.Create, isoStore))
            {
                using (var writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine(AuthInfo);
                }
            }
        }

        public void PurgeAuthInfo()
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null,
                null);
            if (isoStore.FileExists(AuthInfoFile))
                isoStore.DeleteFile(AuthInfoFile);
        }

        public void DiscardSavedCredentials()
        {
            PurgeAuthInfo();
        }
    }
}