using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleCMSForCore2.Helper
{
    public class ShortGuid
    {
        public static string ToShortGuid(Guid newGuid)
        {
            var modifiedBase64 = Convert.ToBase64String(newGuid.ToByteArray())
                .Replace('+', '-').Replace('/', '_') // avoid invalid URL characters
                .Substring(0, 22);
            return modifiedBase64;
        }

        public static Guid ParseShortGuid(string shortGuid)
        {
            var base64 = shortGuid.Replace('-', '+').Replace('_', '/') + "==";
            var bytes = Convert.FromBase64String(base64);
            return new Guid(bytes);
        }

    }
}
