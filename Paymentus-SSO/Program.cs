using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PaymentusSSO
{
    class Program
    {
        static readonly string SSOURL = "";
        static readonly string MakePaymentURL = "";
        static readonly string PaymentusKey = "";

        public static void Main()
        {
            try
            {
                var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                //string inputString =
                //    $"iframe=false;loginId=cdtestloan@yopmail.com;ignoreCase=true;firstName=Cel;lastName=Test;email=cdtestloan@yopmail.com;address.zipCode=28173;timestamp={timestamp};"
                //    //+ "accounts=1600702|LOAN,1606200028|LOAN,1702339|LOAN,1703845|LOAN,575079462|LOAN,1600688|LOAN;step=2;"
                //    ;
                string inputString = $"timestamp={timestamp};loginId=jmccullough@paymentus.com;ignoreCase=true;lang=en;accounts=5091700004|LOAN;iframe=true";

                byte[] key = GetBytes(PaymentusKey);
                //byte[] key = ToByteArray(PaymentusKey);

                var paddedInputString = PadString(inputString);

                byte[] encrypted = EncryptStringToBytes(paddedInputString, key);

                var hexString = BitConverter.ToString(encrypted).Replace("-", ""); //Error: Token doesn't have required fields

                var url = $"{SSOURL}{hexString}";

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }

        static byte[] EncryptStringToBytes(string plainText, byte[] Key)
        {
            byte[] encrypted;
            byte[] plainTextBytes = GetBytes(plainText);

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Padding = PaddingMode.None;
                rijAlg.Mode = CipherMode.ECB;
                rijAlg.Key = Key; //can only be 16, 24 or 32 bytes

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, null);

                using MemoryStream msEncrypt = new MemoryStream();

                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                //csEncrypt.Write(plainTextBytes, 0, plainTextBytes.Length);
                //csEncrypt.FlushFinalBlock();

                encrypted = msEncrypt.ToArray();
            }

            return encrypted;
        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key)
        {
            string plaintext = null;

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Padding = PaddingMode.None;
                rijAlg.Mode = CipherMode.ECB;
                rijAlg.Key = Key;

                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, null);

                using MemoryStream msDecrypt = new MemoryStream(cipherText);

                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

                using StreamReader srDecrypt = new StreamReader(csDecrypt);

                plaintext = srDecrypt.ReadToEnd();
            }

            return plaintext;
        }

        static string PadString(string inputString)
        {
            var len = inputString.Length;
            var lenMod = len % 32;
            return lenMod > 0 ? inputString.PadRight(len + (32 - lenMod)) : inputString;
        }

        static byte[] GetBytes(string input)
        {
            return Encoding.Default.GetBytes(input);
        }

        public static byte[] ToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
