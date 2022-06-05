﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entity.Models;

namespace Business.Abstract
{
    public interface IRoleManager
    {
        Role GetRole(int userId);
        void AddRole(Role role);
        void RemoveRole(Role role);
        void UpdateRole(Role role);
        List<Role> GetAllRoles();
    }
}
