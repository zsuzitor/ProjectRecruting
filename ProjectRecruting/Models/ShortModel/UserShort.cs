﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.ShortModel
{
    public class UserShort
    {
        public string Name { get; set; }
        public string IdUser { get; set; }

        //

        public string Password { get; set; }

        public UserShort()
        {

        }

        public UserShort(string name, string idUser) : this()
        {
            Name = name;
            IdUser = idUser;
        }
    }
}
