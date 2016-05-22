﻿using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;

namespace Mystifier.Activation
{
    internal class MystifierActivation
    {
        private string _activationStatusFile = "_acx";
        public string LicenseHolder { get; set; }

        public bool CheckActivation()
        {
            if (LoadActivationStatus() == null)
                return false;
            var data = LoadActivationStatus().Split(' ');
            var acKey = data[0];
            var email = data[1];
            LicenseHolder = email;
            var requiresWebActivation = CheckCacheDateIsWebActivationRequired();
            return !requiresWebActivation || AttemptActivation(acKey, email);
        }

        /// <summary>
        /// Returns false if the cache is valid, returns true if the cache has expired
        /// </summary>
        /// <returns></returns>
        private bool CheckCacheDateIsWebActivationRequired()
        {
            IsolatedStorageFile isoStore =
                IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(_activationStatusFile))
            {
                var saveDate = isoStore.GetLastWriteTime(_activationStatusFile).DateTime;
                var rightNow = DateTime.Now;
                TimeSpan duration = rightNow.Subtract(saveDate);
                return duration.Days > 7;
            }
            else
            {
                return true;
            }
        }

        private string LoadActivationStatus()
        {
            string activationStatusInfo;
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(_activationStatusFile))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(_activationStatusFile, FileMode.Open, isoStore))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
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
                if (response != "1")
                {
                    throw new ApplicationException("Invalid license details.");
                }
                activationResult = true;
                SaveActivationStatus(activationKey, email);
                LicenseHolder = email;
            }
            catch (Exception)
            {
                activationResult = false;
            }
            return activationResult;
        }

        public void SaveActivationStatus(string licKey, string userDetails)
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            using (var isoStream = new IsolatedStorageFileStream(_activationStatusFile, FileMode.CreateNew, isoStore))
            {
                using (var writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine(licKey + " " + userDetails);
                }
            }
        }

        public void RemoveSavedActivationStatus()
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(_activationStatusFile))
                isoStore.DeleteFile(_activationStatusFile);
        }
    }
}