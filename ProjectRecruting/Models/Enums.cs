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
    public enum StatusInProject { InProccessing, Approved, Canceled }

    //статус проекта
    public enum StatusProject {NotStarted, InProccessing, Closed, Complited }

}
