using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh
{
    public class FlhException : Exception
    {
        public ErrorCode ErrorCode { get; private set; }

        public FlhException(ErrorCode core, string message) : base(message) { ErrorCode = core; }
        public FlhException(ErrorCode core, string message, Exception internalException) : base(message, internalException) { ErrorCode = core; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("错误码:" + Convert.ToInt64(ErrorCode));
            sb.AppendLine(base.ToString());
            return base.ToString();
        }
    }

    public class FlhArgumentException : FlhException
    {
        public string ParamName { get; private set; }
        public FlhArgumentException(string message, string paramName) : base(ErrorCode.ArgError, message) { ParamName = paramName; }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("参数名称:" + (ParamName ?? String.Empty));
            sb.AppendLine(base.ToString());
            return sb.ToString();
        }
    }



    public enum ErrorCode
    {
        None = 0x00000000,
        /// <summary>
        /// 未登录
        /// </summary>
        UnLogin = 0x00000001,
        /// <summary>
        /// 参数错误
        /// </summary>
        ArgError = 0x00000002,
        /// <summary>
        /// 不存在
        /// </summary>
        NotExists = 0x00000003,
        /// <summary>
        /// 被锁定
        /// </summary>
        Locked = 0x00000004,
        /// <summary>
        /// 错误的账号或者密码
        /// </summary>
        ErrorUserNoOrPwd = 0x00000005,
        /// <summary>
        /// 禁止的操作
        /// </summary>
        NotAllow = 0x00000006,
        /// <summary>
        /// 已存在
        /// </summary>
        Exists = 0x00000007,
        /// <summary>
        /// 系统错误
        /// </summary>
        ServerError = 0x00000010,
    }
}
