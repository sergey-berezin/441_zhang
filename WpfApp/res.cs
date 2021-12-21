using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WpfApp
{
    public class Results : IEnumerable<string>
    {
        
        public List<string> img { get; set; }
        public string TYPE { get; set; }
        public Results(string newType, string newImage)
        {
            TYPE = newType;
            img = new List<string>();
            img.Add(newImage);
        }

        public override string ToString()
        {
            return TYPE;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)img).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)img).GetEnumerator();
        }
    }
}