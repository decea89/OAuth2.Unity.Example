
using System.Text;
using System.Security.Cryptography;

public static class OAuthUtils
{

        public static string GetSecureUUID(string s){

            return StringToBase64(SHA256HashStringForUTF8String(s));
            
        }

                /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns>String of hex digits</returns>
        private static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        private static string StringToBase64(byte[] s){
            
            char[] padding = { '=' };

            string secureStr = System.Convert.ToBase64String(s).TrimEnd(padding).Replace('+', '-').Replace('/', '_');

            // secureStr += secureStr;

            return secureStr;

        }

                /// <summary>
        /// Compute hash for string encoded as UTF8
        /// </summary>
        /// <param name="s">String to be hashed</param>
        /// <returns>40-character hex string</returns>
        private static byte[] SHA256HashStringForUTF8String(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);

            var sha1 = SHA256.Create();
            byte[] hashBytes = sha1.ComputeHash(bytes);
 
            // return HexStringFromBytes(hashBytes);
            return hashBytes;
        }


    
}
