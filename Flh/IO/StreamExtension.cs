using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Flh.IO
{
    public static class StreamExtension
    {
        public static Stream Copy(this Stream stream)
        {
            if (stream == null) return null;
            var result = new System.IO.MemoryStream();
            stream.CopyTo(result);
            result.Position = 0;
            return result;
        }
    }
}
