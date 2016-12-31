using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Security
{
   public class DES
    {
       private readonly string _EncryptKey;
       byte[] byKey = null;
       byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
       public DES(string encryptKey)
       {
           if (String.IsNullOrWhiteSpace(encryptKey) || encryptKey.Trim().Length<8)
               throw new ArgumentNullException("参数encryptKey必须为8位");
           _EncryptKey = encryptKey.Trim();
           byKey = System.Text.Encoding.UTF8.GetBytes(_EncryptKey.Substring(0, 8));
       }
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="input">明码</param>
        /// <returns>加密后的密码</returns>
        public string DesEncryptFixKey(string input)
        {
            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(_EncryptKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(input);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (System.Exception ex)
            {
                throw new Exception("加密失败");
            }
        }
        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="this.inputString">加了密的字符串</param>
        /// <param name="input">密钥</param>
        public  string DesDecryptFixKey(string input)
        {
            byte[] inputByteArray = new Byte[input.Length];

            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(_EncryptKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                inputByteArray = Convert.FromBase64String(input);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = new System.Text.UTF8Encoding();
                return encoding.GetString(ms.ToArray());
            }
            catch (System.Exception ex)
            {
                throw new Exception("解密失败");
            }
        }
    }
}
