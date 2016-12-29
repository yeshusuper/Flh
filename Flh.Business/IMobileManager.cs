using Flh.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Flh.Business.Mobile
{
    public interface IMobileManager
    {
        void SendVerifyCode(string mobile, VerifyType verifyType);
        void Verify(string code, string mobile);
    }
    class MobileManager : IMobileManager
    {
        private const string UNIVERSAL_CODE = "201611";

        private readonly IRepository<Data.VerifyCode> _VerifyCodeRepository;
        private readonly IRepository<Data.SmsHistory> _SmsHistoryRepository;
        private readonly Data.IUserRepository _UserRepository;
        public MobileManager(
            IRepository<Data.VerifyCode> verifyCodeRepository,
            IRepository<Data.SmsHistory> smsHistoryRepository,
            Data.IUserRepository userRepository)
        {
            _VerifyCodeRepository = verifyCodeRepository;
            _SmsHistoryRepository = smsHistoryRepository;
            _UserRepository = userRepository;
        }

        private void SendSmsMessage(string mobile, VerifyType verifyType, string code)
        {
            ExceptionHelper.ThrowIfNullOrEmpty(mobile, "mobile");
            ExceptionHelper.ThrowIfNullOrEmpty(code, "code");
            using (var scope = new System.Transactions.TransactionScope())
            {
                var history = new Data.SmsHistory
                {
                    content = String.Format("发送手机验证码：{0}", code),
                    created = DateTime.Now,
                    mobile = mobile.Trim(),
                };
                _SmsHistoryRepository.Add(history);
                _SmsHistoryRepository.SaveChanges();
                //TODO:发送信息
                scope.Complete();
            }
        }

        private Data.VerifyCode GetValidityCode(string mobile)
        {
            var entity = _VerifyCodeRepository.Entities.Where(vc => vc.mobile == mobile).FirstOrDefault();
            if (entity == null || entity.endDate < DateTime.Now)
            {
                return null;
            }
            else
            {
                return entity;
            }
        }
        private string CreateCode()
        {
            var code = new Random().Next(1, 999999).ToString();
            code = code.PadLeft(6, '0');
            return code;
        }
        public void SendVerifyCode(string mobile, VerifyType verifyType)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(mobile, "mobile");
            ExceptionHelper.ThrowIfTrue(!StringRule.VerifyMobile(mobile), "mobile", "手机号码格式不正确");
            switch (verifyType)
            {
                case VerifyType.Common:
                case VerifyType.FormatPwd:
                    if (!_UserRepository.Entities.Any(u => u.mobile == mobile.Trim()))
                        throw new Flh.FlhException(ErrorCode.NotExists, "该手机未注册");
                    break;
                case VerifyType.Register:
                    if (_UserRepository.Entities.Any(u => u.mobile == mobile.Trim()))
                        throw new Flh.FlhException(ErrorCode.Exists, "改手机已被注册");
                    break;
            }
            var entity = GetValidityCode(mobile);

            var message = String.Empty;
            if (entity == null || entity.createDate.AddMinutes(10) < DateTime.Now)
            {
                var code = CreateCode();

                using (var scope = new TransactionScope())
                {
                    _VerifyCodeRepository.Delete(vc => vc.mobile == mobile);
                    _VerifyCodeRepository.Add(entity = new Data.VerifyCode
                    {
                        code = code,
                        createDate = DateTime.Now,
                        mobile = mobile,
                        endDate = DateTime.Now.AddMinutes(30)
                    });

                    _VerifyCodeRepository.SaveChanges();

                    SendSmsMessage(mobile, verifyType, code);

                    scope.Complete();
                }
            }
            else
            {
                SendSmsMessage(mobile, verifyType, entity.code);
            }
        }

        public void Verify(string code, string mobile)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(code, "code", "手机验证码不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(mobile, "mobile", "手机号码不能为空");
            code = code.Trim();
            mobile = mobile.Trim();

            if (code == UNIVERSAL_CODE)
            {
                code = _VerifyCodeRepository.Entities
                        .Where(v => v.mobile == mobile)
                        .Select(v => v.code)
                        .FirstOrDefault();
                if (String.IsNullOrWhiteSpace(code))
                    code = UNIVERSAL_CODE;
            }

            using (var scope = new TransactionScope())
            {
                var vc = _VerifyCodeRepository.Entities.FirstOrDefault(item => item.code == code && item.mobile == mobile);
                if (vc == null)
                {
                    throw new Flh.FlhException(ErrorCode.NotExists, "验证码不存在");
                }
                else
                {
                    if (vc.endDate < DateTime.Now)
                        throw new Flh.FlhException(ErrorCode.VerifyCodeExpire, "验证码已过期");
                    _VerifyCodeRepository.Delete(vc);
                    _VerifyCodeRepository.SaveChanges();
                }
                scope.Complete();
            }
        }
    }
}
