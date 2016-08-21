using Flh.Business.Users;
using Flh.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public interface IUserManager
    {
        IUser Register(IRegisterInfo info);
        bool IsUsableMobile(string mobile);
        bool IsUsableEmail(string email);
        IUser Login(string mobileOrEmail, string password, string ip);
    }

    internal class UserManager : IUserManager
    {
        private readonly Data.IUserRepository _UserRepository;
        private readonly IRepository<Data.LoginHistory> _LoginHistoryRepository;

        public UserManager(Data.IUserRepository userRepository,
            IRepository<Data.LoginHistory> loginHistoryRepository)
        {
            _UserRepository = userRepository;
            _LoginHistoryRepository = loginHistoryRepository;
        }

        public IUser Register(IRegisterInfo info)
        {
            ExceptionHelper.ThrowIfNull(info, "info");
            ExceptionHelper.ThrowIfTrue(!StringRule.VerifyMobile(info.Mobile), "mobile", "手机格式不正确");
            ExceptionHelper.ThrowIfTrue(!StringRule.VerifyEmail(info.Email), "email", "邮箱格式不正确");
            ExceptionHelper.ThrowIfTrue(!StringRule.VerifyPassword(info.Password), "password", "密码格式不正确，密码长度为6-20位");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(info.Name, "name", "名称不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(info.Company, "company", "公司不能为空");
            //ExceptionHelper.ThrowIfNullOrWhiteSpace(info.AreaNo, "areaNo", "没有选择地区");
            //ExceptionHelper.ThrowIfNullOrWhiteSpace(info.Address, "address", "地址不能为空");
            //ExceptionHelper.ThrowIfNullOrWhiteSpace(info.IndustryNo, "industryNo", "没有选择行业类别");

            ExceptionHelper.ThrowIfTrue(!IsUsableMobile(info.Mobile), "mobile", "此手机号已经被注册");
            ExceptionHelper.ThrowIfTrue(!IsUsableEmail(info.Email), "email", "此邮箱已经被注册");

            var entity = new Data.User
            {
                address = info.Address.SafeTrim(),
                area_no = info.AreaNo.SafeTrim(),
                industry_no = info.IndustryNo.SafeTrim(),
                company = info.Company.Trim(),
                email = info.Email.Trim(),
                employees_count_type = info.EmployeesCountRange,
                is_purchaser = info.IsPurchaser,
                mobile = info.Mobile.Trim(),
                name = info.Name.Trim(),
                neet_invoice = info.NeetInvoice,
                tel = info.Tel == null ? null : info.Tel.Trim(),
                pwd = new Security.MD5().Encrypt(info.Password.Trim()),
                last_login_date = DateTime.Now,
                register_date = DateTime.Now,
                enabled = true,
            };
            _UserRepository.Add(entity);
            _UserRepository.SaveChanges();

            return GetUser(entity);
        }

        public bool IsUsableMobile(string mobile)
        {
            ExceptionHelper.ThrowIfTrue(!StringRule.VerifyMobile(mobile), "mobile", "手机格式不正确");
            return !_UserRepository.Entities.Any(u => u.mobile == mobile.Trim());
        }
        public bool IsUsableEmail(string email)
        {
            ExceptionHelper.ThrowIfTrue(!StringRule.VerifyEmail(email), "email", "邮箱格式不正确");
            return !_UserRepository.Entities.Any(u => u.email == email.Trim());
        }


        public IUser Login(string mobileOrEmail, string password, string ip)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(mobileOrEmail, "mobileOrEmail", "账号不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(password, "password", "密码不能为空");
            var source = _UserRepository.Entities;
            if (StringRule.VerifyMobile(mobileOrEmail))
                source = source.Where(u => u.mobile == mobileOrEmail.Trim());
            else if (StringRule.VerifyEmail(mobileOrEmail))
                source = source.Where(u => u.email == mobileOrEmail.Trim());
            else
                ExceptionHelper.ThrowIfTrue(true, "mobileOrEmail", "请输入手机或者邮箱进行登陆");

            var user = source.FirstOrDefault();
            if(user == null)
                throw new FlhException(ErrorCode.NotExists, "账号不存在");
            if(!user.enabled)
                throw new FlhException(ErrorCode.Locked, "账号已被锁定");
            if(!new Security.MD5().Verify(password.Trim(), user.pwd))
                throw new FlhException(ErrorCode.ErrorUserNoOrPwd, "账号或密码错误");

            ip = (ip ?? String.Empty).Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? String.Empty;

            user.last_login_date = DateTime.Now;
            _UserRepository.SaveChanges();

            var history = new Data.LoginHistory
            {
                ip = ip,
                login_date = DateTime.Now,
                uid = user.uid
            };
            _LoginHistoryRepository.Add(history);
            _LoginHistoryRepository.SaveChanges();

            return GetUser(user);
        }

        private IUser GetUser(Data.User entity)
        {
            return new User(entity);
        }
    }

}
