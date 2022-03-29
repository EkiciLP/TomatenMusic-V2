using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TomatenMusic.Util
{
    class RandomUtil
    {

        public static string GenerateGuid()
        {
            return String.Concat(Guid.NewGuid().ToString("N").Select(c => (char)(c + 17))).ToLower();
        }
    }
}
