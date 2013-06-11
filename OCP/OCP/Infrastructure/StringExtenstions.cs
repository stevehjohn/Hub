using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace OCP.Infrastructure
{
    public static class StringExtenstions
    {
        public static string SplitLongString(this string instance, int charsPerChunk, string breakToInsert)
        {
            var ret = new StringBuilder();

            var taken = 0;
            while (taken < instance.Length)
            {
                ret.Append(instance.Skip(taken).Take(charsPerChunk).ToArray());
                taken += charsPerChunk;
                
                if (taken < instance.Length)
                {
                    ret.Append(breakToInsert);
                }
            }

            return ret.ToString();
        }
    }
}