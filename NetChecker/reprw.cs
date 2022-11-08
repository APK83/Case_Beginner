using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetChecker
{
    public class Reprw
    {
        public List<status_res> UrlList { get; set; } = new List<status_res>();
        public List<status_res> PostgresList { get; set; } = new List<status_res>();

    }
    public class status_res
    {
        public string ResName { get; set; }
        public bool Status { get; set; }
        public DateTime Dat { get; set; }
    }
}
