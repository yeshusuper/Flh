using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public interface IUser
    {
        long Uid { get; }
        string Name { get; }
        string Mobile { get; }
    }

    internal class User : IUser
    {
        private readonly Lazy<Data.User> _LazyUser;
        public long Uid { get; private set; }
        public string Name { get { return _LazyUser.Value.name; } }
        public User(Data.User entity)
        {
            _LazyUser = new Lazy<Data.User>(() => entity);
            Uid = entity.uid;
        }


        public string Mobile
        {
            get { return _LazyUser.Value.mobile; }
        }
    }
}
