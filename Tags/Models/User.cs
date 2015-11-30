using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tags.Models
{
    public partial class User
    {
        private static string _rolesbyuser = @"
            select r.roleid, r.role, r.ADGroup 
            from role r
            join userrole u on r.roleid = u.roleid 
            where u.userid = @0
        ";

        private void copy(User u)
        {
            UserId = u.UserId;
            Login = u.Login;
            Identity = u.Identity;
        }

        public User() { }

        public User(int id)
        {
            if (id == 0) return;
            var user = SingleOrDefault(" where userid = @0", id);
            copy(user);
        }

        public User(string login)
        {
            var user = SingleOrDefault(" where login = @0", login);
            copy(user);
        }

        public List<Role> Roles()
        {
            List<Role> roles = null;
            using (tagDB t = new tagDB())
            {
                roles = t.Fetch<Role>(_rolesbyuser, UserId);
            }
            return roles;
        }
    }
}