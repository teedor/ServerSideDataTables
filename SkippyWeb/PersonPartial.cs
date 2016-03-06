using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerSideDatatables
{
    public partial class Person
    {
        public string FormattedDate
        {
            get { return DateOfBirth.ToString("d/M/yyyy"); }
        }
    }
}