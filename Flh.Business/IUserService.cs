using Flh.Business.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public interface IUserService
    {
        long Uid { get; }
        string Name { get; }
        string Mobile { get; }
        void UpdateInfo(IUserInfo info);
        void ChangePassword(string oldPassword, string newPasswrod);
        void ChangeMobile(string mobile);
    }

    internal class UserService : IUserService
    {
        private readonly Data.IUserRepository _UserRepository;
        private readonly IUserManager _UserManager;
        private readonly Lazy<Data.User> _LazyUser;
        public long Uid { get; private set; }
        public string Name { get { return _LazyUser.Value.name; } }
        public UserService(Data.User entity, Data.IUserRepository userRepository, IUserManager userManager)
        {
            if (entity == null)
                throw new FlhException(ErrorCode.NotExists, "用户不存在");
            _LazyUser = new Lazy<Data.User>(() => entity);
            Uid = entity.uid;
            _UserRepository = userRepository;
            _UserManager = userManager;
        }
        public UserService(long uid, Data.IUserRepository userRepository, IUserManager userManager)
        {
            ExceptionHelper.ThrowIfNotId(uid, "uid");
            Uid = uid;
            _UserRepository = userRepository;
            _UserManager = userManager;
            _LazyUser = new Lazy<Data.User>(() =>
            {
                var entity = _UserRepository.Entities.FirstOrDefault(u => u.uid == uid);
                if (entity == null)
                    throw new FlhException(ErrorCode.NotExists, "用户不存在");
                return entity;
            });
        }

        public string Mobile
        {
            get { return _LazyUser.Value.mobile; }
        }

        public void UpdateInfo(IUserInfo info)
        {
            if (!String.IsNullOrWhiteSpace(info.Address))
                _LazyUser.Value.address = info.Address.Trim();
            if (!String.IsNullOrWhiteSpace(info.AreaNo))
                _LazyUser.Value.area_no = info.AreaNo.Trim();
            if (!String.IsNullOrWhiteSpace(info.IndustryNo))
                _LazyUser.Value.industry_no = info.IndustryNo.Trim();
            if (!String.IsNullOrWhiteSpace(info.Company))
                _LazyUser.Value.company = info.Company.Trim();
            if (!String.IsNullOrWhiteSpace(info.Email) && info.Email != _LazyUser.Value.email)
            {
                ExceptionHelper.ThrowIfTrue(!_UserManager.IsUsableEmail(info.Email), "email", "此邮箱已经被注册");
                _LazyUser.Value.email = info.Email.Trim();
            }
            if (info.EmployeesCountRange != null)
                _LazyUser.Value.employees_count_type = info.EmployeesCountRange;
            if (info.IsPurchaser != null)
                _LazyUser.Value.is_purchaser = info.IsPurchaser.Value;
            if (!String.IsNullOrWhiteSpace(info.Name))
                _LazyUser.Value.name = info.Name.Trim();
            if (info.NeetInvoice != null)
                _LazyUser.Value.neet_invoice = info.NeetInvoice.Value;
            if (!String.IsNullOrWhiteSpace(info.Tel))
                _LazyUser.Value.tel = info.Tel.Trim();
            _UserRepository.SaveChanges();
        }
        public void ChangePassword(string oldPassword, string newPasswrod)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(newPasswrod, "newPasswrod", "新密码");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(oldPassword, "oldPassword", "旧密码");
            newPasswrod = newPasswrod.Trim();
            ExceptionHelper.ThrowIfTrue(!StringRule.VerifyPassword(newPasswrod), "password", "新密码格式不正确，密码长度为6-20位");
            if (!new Security.MD5().Verify(oldPassword.Trim(), _LazyUser.Value.pwd))
                throw new FlhException(ErrorCode.ErrorUserNoOrPwd, "账号或密码错误");
            _LazyUser.Value.pwd = new Security.MD5().Encrypt(newPasswrod);
            _UserRepository.SaveChanges();
        }
        public void ChangeMobile(string mobile)
        {
            ExceptionHelper.ThrowIfNullOrEmpty(mobile, "mobile");
            ExceptionHelper.ThrowIfTrue(!StringRule.VerifyMobile(mobile), "mobile", "手机号码格式不正确");
            ExceptionHelper.ThrowIfTrue(!_UserManager.IsUsableMobile(mobile), "mobile", "此手机已经被注册");
            _LazyUser.Value.mobile = mobile.Trim();
            _UserRepository.SaveChanges();
        }
    }
}
