using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain.ManyToMany
{
    public class CompanyUser
    {
        public int Id { get; set; }

        public StatusInCompany Status { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public CompanyUser()
        {

        }

        public CompanyUser(string userId,int companyId)
        {
            UserId = userId;
            CompanyId = companyId;
            Status = StatusInCompany.Empty;
        }

        public CompanyUser(string userId, int companyId, StatusInCompany status):this(userId,companyId)
        {
            Status = status;
        }

    }
}
