using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectChecker.Data
{
    public class EGRN
    {
        public string Date {  get; set; }
        public string Address { get; set; }
        public List<string> RightHolder { get; set; }
        public List<string> Right {  get; set; }
    }
}
