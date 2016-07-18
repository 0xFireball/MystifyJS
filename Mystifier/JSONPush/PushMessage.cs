using Java.Lang;

namespace JSONPush
{
    public class PushMessage
    {
        public string Title { get; set; }
        public string Message { get; set; }

        public string MessageHash
        {
            get
            {
                return HashMessage(Title + Message);
            }
        }

        private static string HashMessage(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}