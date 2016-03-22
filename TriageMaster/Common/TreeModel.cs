using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriageMaster.Common
{
    public class TreeModel
    {
        public string Name { get; set; }
        public List<TreeModel> Children { get; set; }
        public string CompletePath { get; set; }
    }    
}
