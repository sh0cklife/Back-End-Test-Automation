using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharpTestProject
{
    public class Issues
    {
        public long id {  get; set; }
        public long number { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }
}
