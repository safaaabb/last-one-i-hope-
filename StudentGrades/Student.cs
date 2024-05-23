using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGrades
{
    public class Student
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public int Years { get; set; }
        public Dictionary<string, double> Grades { get; set; }
        public double fullGreads {  get; set; }

        public Student()
        {
            Grades = new Dictionary<string, double>();
        }
    }

}
