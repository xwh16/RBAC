using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBAC.App
{
    public class ListItem
    {
        public int Value { get; set; }
        public string Text { get; set; }

        public ListItem(string _Text, int _Value)
        {
            Text = _Text;
            Value = _Value;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
