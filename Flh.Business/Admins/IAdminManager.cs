using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Admins
{
    public interface IAdminManager
    {
        IAdmin Login(string mobileOrEmail, string password, string ip);
    }

    internal class AdminManager : IAdminManager
    {
        private readonly Data.IAdminRepository _AdminRepository;
        private readonly IUserManager _UserManager;

        public AdminManager(Data.IAdminRepository adminRepository,
            IUserManager userManager)
        {
            _AdminRepository = adminRepository;
            _UserManager = userManager;
        }

        public IAdmin Login(string mobileOrEmail, string password, string ip)
        {
            var user = _UserManager.Login(mobileOrEmail, password, ip);
            var entity = _AdminRepository.EnabledAdmins.FirstOrDefault(a => a.uid == user.Uid);
            if (entity == null)
                throw new FlhException(ErrorCode.NotExists, "你不是管理员，不能登陆后台");
            return new Admin(entity, user);
        }
    }
}
