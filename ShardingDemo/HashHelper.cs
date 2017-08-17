using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShardingDemo
{
    public class HashHelper
    {
        public static string Hash(string str)
        {
            SHA512 mySHA256 = SHA512.Create();
            var byteVal = System.Text.Encoding.UTF8.GetBytes(str);
            var retVal = mySHA256.ComputeHash(byteVal);
            //mySHA256.

            //下面的方法和ToString("x2")效果一样
            return BitConverter.ToString(retVal).Replace("-", "");

        }

        //public static string HashMd5(string orginal, string salt)
        //{
        //    return HashHelper.Hash(salt + MD5Helper.MD5Value(orginal));
        //}
    }
}
