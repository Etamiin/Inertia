using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

namespace InertiaTests.Web
{
    public class InertiaWebTest
    {
        public InertiaWebTest()
        {
            var parameters = new RequestParameters();

            //parameters.AddHeader("name", "value");
            //parameters.SetContentLength(0);
            //parameters...

            var url = "https://mgapi.skiadian.com/gameapi/get_lb?rank=2";
            Console.WriteLine(WebHelper.GetRequest(new Uri(url), parameters));
        }
    }
}
