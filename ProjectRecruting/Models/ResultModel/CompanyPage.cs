using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain;
using ProjectRecruting.Models.ShortModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.ResultModel
{//
    public class CompanyPage : CompanyShort
    {
        //public int Id { get; set; }

        //public string Name { get; set; }
        //public string Description { get; set; }
        //public string Number { get; set; }
        //public string Email { get; set; }

        public bool CanEdit { get; set; }



        public CompanyPage()
        {
            CanEdit = false;
        }

        public CompanyPage(Company company):base(company)
        {
            CanEdit = false;
        }


        public async static Task<CompanyPage> LoadAllForView(ApplicationDbContext db, Company company, string userId)
        {
            CompanyPage res = new CompanyPage(company);
            res.CanEdit=await Company.CheckAccess(db,userId,company.Id);

            return res;
        }
        }
}
