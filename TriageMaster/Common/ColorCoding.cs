using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TriageMaster.Common
{
    public class ColorCoding
    {
        public Color Color { get; set; }
        public Brush Brush { get { return new SolidColorBrush(Color); } }
        public string Name { get; set; }
    }
}
