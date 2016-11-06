using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flh.IO
{
    public class ZoomProcessArguments : IImageProcessorArguments
    {
        public enum EdgeEnum
        {
            Long = 0,
            Short = 1,
        }

        private static Regex m_rLarge = new Regex("(?:^|_)([0-1])l(?:$|_|\\.)", RegexOptions.Compiled | RegexOptions.RightToLeft);
        private static Regex m_rWidth = new Regex("(?:^|_)(\\d+)w(?:$|_|\\.)", RegexOptions.Compiled | RegexOptions.RightToLeft);
        private static Regex m_rHeigth = new Regex("(?:^|_)(\\d+)h(?:$|_|\\.)", RegexOptions.Compiled | RegexOptions.RightToLeft);
        private static Regex m_rMultiple = new Regex("(?:^|_)(\\d+)m(?:$|_|\\.)", RegexOptions.Compiled | RegexOptions.RightToLeft);
        private static Regex m_rQuality = new Regex("(?:^|_)(\\d+)q(?:$|_|\\.)", RegexOptions.Compiled | RegexOptions.RightToLeft);
        private static Regex m_rAbsoluteQuality = new Regex("(?:^|_)(\\d+)Q(?:$|_|\\.)", RegexOptions.Compiled | RegexOptions.RightToLeft);
        private static Regex m_rEdge = new Regex("(?:^|_)(\\d)e(?:$|_|\\.)", RegexOptions.Compiled | RegexOptions.RightToLeft);
        private static Regex m_rFormat = new Regex("(\\.\\w+)$", RegexOptions.Compiled | RegexOptions.RightToLeft);

        /// <summary>
        /// l
        /// </summary>
        public bool? Large { get; set; }
        /// <summary>
        /// w
        /// </summary>
        public int? Width { get; set; }
        /// <summary>
        /// h
        /// </summary>
        public int? Height { get; set; }
        /// <summary>
        /// 尺寸倍数(m)
        /// </summary>
        public int? Multiple { get; set; }
        /// <summary>
        /// q
        /// </summary>
        public int? Quality { get; set; }
        /// <summary>
        /// Q
        /// </summary>
        public int? AbsoluteQuality { get; set; }
        /// <summary>
        /// 长短边优先(e)
        /// </summary>
        public EdgeEnum Edge { get; set; }
        public string Format { get; set; }

        public bool IsNeetProcess()
        {
            return Width.HasValue || Height.HasValue || Multiple.HasValue || Quality.HasValue || AbsoluteQuality.HasValue;
        }

        public string ToQuery()
        {
            var strs = new List<string>();
            if (Width.HasValue)
                strs.Add(Width.Value + "w");
            if (Height.HasValue)
                strs.Add(Height.Value + "h");
            if (Large.HasValue)
                strs.Add((Large.Value ? "1" : "0") + "l");
            if (Multiple.HasValue)
                strs.Add(Multiple.Value + "m");
            if (Quality.HasValue)
                strs.Add(Quality.Value + "q");
            if (AbsoluteQuality.HasValue)
                strs.Add(AbsoluteQuality.Value + "Q");
            if (Edge != EdgeEnum.Long)
                strs.Add(Edge.To<byte>() + "e");
            var format = !String.IsNullOrWhiteSpace(Format) ? Format : String.Empty;
            return String.Join("_", strs) + format;
        }

        public static ZoomProcessArguments Parse(string query)
        {
            query = (query ?? String.Empty).Trim();

            var result = new ZoomProcessArguments();
            var large = MatchNullable<int>(m_rLarge, query);
            result.Large = large.HasValue && large.Value == 1;
            result.Width = MatchNullable<int>(m_rWidth, query);
            result.Height = MatchNullable<int>(m_rHeigth, query);
            result.Multiple = MatchNullable<int>(m_rMultiple, query);
            result.Quality = MatchNullable<int>(m_rQuality, query);
            result.AbsoluteQuality = MatchNullable<int>(m_rAbsoluteQuality, query);
            result.Edge = Match<EdgeEnum>(m_rEdge, query);
            result.Format = Match<string>(m_rFormat, query);
            return result;
        }

        private static Nullable<T> MatchNullable<T>(Regex regex, string query)
            where T : struct, IConvertible
        {
            var match = regex.Match(query);
            if (match.Success)
            {
                return match.Groups[1].Value.To<T>();
            }
            else
            {
                return null;
            }
        }

        private static T Match<T>(Regex regex, string query)
            where T : IConvertible
        {
            var match = regex.Match(query);
            if (match.Success)
            {
                return match.Groups[1].Value.To<T>();
            }
            else
            {
                return default(T);
            }
        }
    }
}
