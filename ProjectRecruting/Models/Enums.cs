using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models
{
    public class Enums
    {
    }

    //статус людей подписанных на проект
    //Not-не имеет отношения к проекту
    public enum StatusInProject {Empty, InProccessing, Approved, Canceled,CanceledByStudent,Moderator }

    public enum StatusInCompany { Empty, RequestedByUser, RequestedByCompany, Employee, Moderator }

    //статус проекта
    public enum StatusProject {NotStarted, InProccessing, Closed, Complited }

}
