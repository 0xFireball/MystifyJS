using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Mystifier.Activation
{
    internal class MystifierActivation
    {
        private static readonly string SecretHashSalt =
            "cuDFGVwg4J0oTSXxg3nUFQL3y3okNMA5Fs72bALhMdXm0VU0pu2Ju5lAmhErD3SkCCAKbosJXcccaFz9azsJ7LMT3pT16QflrGRJo06sHlQ6aT68SKcinFPX5dCwn5";

        private readonly string _activationStatusFile = "_acx";
        public string LicenseHolder { get; set; }
        public string LicenseKey { get; set; }

        public bool CheckActivation()
        {
            var existingActivationInfo = LoadActivationStatus();
            if (existingActivationInfo == null)
                return false;
            var data = existingActivationInfo.Split(' ');
            var acKey = data[0];
            var email = data[1];
            LicenseHolder = email;
            LicenseKey = acKey;
            var requiresWebActivation = CheckCacheDateIsWebActivationRequired() || IsInternetConnectionAvailable();
            return !requiresWebActivation || AttemptActivation(acKey, email);
        }

        private static bool IsInternetConnectionAvailable()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("https://www.google.com")) //Check Google
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Returns false if the cache is valid, returns true if the cache has expired
        /// </summary>
        /// <returns></returns>
        private bool CheckCacheDateIsWebActivationRequired()
        {
            var isoStore =
                IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(_activationStatusFile))
            {
                var saveDate = isoStore.GetLastWriteTime(_activationStatusFile).DateTime;
                var rightNow = DateTime.Now;
                var duration = rightNow.Subtract(saveDate);
                return duration.Days > 7;
            }
            return true;
        }

        private string LoadActivationStatus()
        {
            string activationStatusInfo;
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null,
                null);
            if (isoStore.FileExists(_activationStatusFile))
            {
                using (var isoStream = new IsolatedStorageFileStream(_activationStatusFile, FileMode.Open, isoStore))
                {
                    using (var reader = new StreamReader(isoStream))
                    {
                        activationStatusInfo = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                return null;
            }
            return activationStatusInfo;
        }

        public bool AttemptActivation(string activationKey, string email)
        {
            var activationResult = false;
            var deviceUid = FingerprintGenerator.GetFingerprint();
            var productId = 3826; //Mystifier Product ID
            string activationUrl =
                $"https://apps.zetaphase.io/licensing/verify.php?serial_key={activationKey}&product_id={productId}&unique1={email}&unique2={deviceUid}";
            try
            {
                var wc = new WebClient();
                var response = wc.DownloadString(activationUrl);
                var validResponse = SHA256(SecretHashSalt + "1");
                if (response != validResponse)
                {
                    throw new ApplicationException("Invalid license details.");
                }
                activationResult = true;
                SaveActivationStatus(activationKey, email);
                LicenseHolder = email;
                LicenseKey = activationKey;
            }
            catch (Exception)
            {
                activationResult = false;
            }
            return activationResult;
        }

        public void SaveActivationStatus(string licKey, string userDetails)
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null,
                null);
            using (var isoStream = new IsolatedStorageFileStream(_activationStatusFile, FileMode.Create, isoStore))
            {
                using (var writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine(licKey + " " + userDetails);
                }
            }
        }

        public void RemoveSavedActivationStatus()
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null,
                null);
            if (isoStore.FileExists(_activationStatusFile))
                isoStore.DeleteFile(_activationStatusFile);
        }

        private static string SHA256(string password)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}