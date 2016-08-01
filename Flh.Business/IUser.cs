using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public interface IUser
    {
    }

    internal class User : IUser
    {
        public User(Data.User entity)
        {

        }
    }
}
