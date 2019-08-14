using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.services
{
    public class ValidationInput: IInputValidator
    {
        HtmlEncoder Html { get; set; }
        JavaScriptEncoder JS { get; set; }

        public ValidationInput()
        {
            Html = HtmlEncoder.Default;
            JS = JavaScriptEncoder.Default;
        }

        public  string ValidateString(string str)
        {

            return this.Html.Encode(str);//(this.JS.Encode(str));
        }

        public void ValidateStringArray(string[] mass)
        {
            for(var i=0;i<mass.Length;++i)
                mass[i] = this.ValidateString(mass[i]);
        }

    }
}
