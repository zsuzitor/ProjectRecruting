using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models
{
    public class Interface
    {
    }

    public interface IInputValidator
    {
        string ValidateString(string str);
        void ValidateStringArray(string[] mass);

    }

}
