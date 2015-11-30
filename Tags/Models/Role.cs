using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tags.Models
{
    public partial class Role
    {
        private void copy(Role r)
        {
            RoleId = r.RoleId;
            _Role = r._Role;
            ADGroup = r.ADGroup;
        }

        public static List<Role> Roles()
        {
            return Fetch("");
        }

        public Role() { }

        public Role(int id)
        {
            if (id == 0) return;
            var role = SingleOrDefault(" where roleid = @0", id);
            copy(role);
        }
    }

    public class RoleView
    {
        private static string _updateroles = @"
            merge userrole with (holdlock) as t
            using [role] as s
            on t.userid = {0} and s.roleid = t.roleid and s.roleid in ({1})
            when not matched by target and s.roleid in ({1}) then
                insert (roleid, userid) values (s.roleid, {0})
            when not matched by source and t.userid = {0} then
                delete;
        ";
        public User usr { get; set; }
        public List<int> roles { get; set; }
        public List<Role> roleList { get; set; }

        public RoleView() { }

        public RoleView(int id)
        {
            usr = new User(id);
            roleList = Role.Roles();
            roles = usr.Roles().Select(r => r.RoleId).ToList();
        }

        public void Save()
        {
            using (tagDB t = new tagDB())
            {
                t.Execute(string.Format(_updateroles, usr.UserId, string.Join(",", roles.ToArray())));
            }
        }
    }
}