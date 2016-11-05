using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Security
{
    public abstract class Digest
    {
        public string Encrypt(string input, Encoding encoding)
        {
            if (input == null) return null;

            return Encrypt(encoding.GetBytes(input));
        }

        public string Encrypt(string input)
        {
            return Encrypt(input, Encoding.UTF8);
        }

        public string Encrypt(byte[] input)
        {
            if (input == null) return null;

            var data = ComputeHash(input);

            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public bool Verify(string input, string hash, Encoding encoding)
        {
            string hashOfInput = Encrypt(input, encoding);
            return 0 == StringComparer.Compare(hashOfInput, hash);
        }

        public bool Verify(string input, string hash)
        {
            string hashOfInput = Encrypt(input);
            return 0 == StringComparer.Compare(hashOfInput, hash);
        }

        protected abstract byte[] ComputeHash(byte[] input);
        protected virtual StringComparer StringComparer { get { return StringComparer.OrdinalIgnoreCase; } }
    }
}
