using System.IO;
using System.IO.IsolatedStorage;

namespace Mystifier.GitHub
{
    internal class MystifierGitHubAccess
    {
        public string GitHubAuthToken;
        public bool IsAuthenticated;
        public bool IsAvailable;
        private readonly string authTokenFile = ".gittoken";

        public MystifierGitHubAccess()
        {
            GitHubAuthToken = LoadAuthToken();
            IsAuthenticated = GitHubAuthToken != null;
        }

        private string LoadAuthToken()
        {
            string authToken = "";
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null,
                null);
            if (isoStore.FileExists(authTokenFile))
            {
                using (var isoStream = new IsolatedStorageFileStream(authTokenFile, FileMode.Open, isoStore))
                {
                    using (var reader = new StreamReader(isoStream))
                    {
                        authToken = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                return null;
            }
            return authToken;
        }

        public void SaveAuthToken(string authToken)
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null,
                null);
            using (var isoStream = new IsolatedStorageFileStream(authTokenFile, FileMode.Create, isoStore))
            {
                using (var writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine(authToken);
                }
            }
        }
    }
}