using System.Collections.Generic;

namespace ServerSideDatatables.Datatables.Response
{
    public class DatatablesResponse<T> : IDatatablesResponse<T>
    {
        public int draw { get; set; }

        public int recordsTotal { get; set; }

        public int recordsFiltered { get; set; }

        public IEnumerable<T> data { get; set; }

        public string error { get; set; }

        public DatatablesResponse() { }

        public DatatablesResponse(int draw, int recordsTotal, int recordsFiltered, IEnumerable<T> data)
        {
            this.draw = draw;
            this.recordsTotal = recordsTotal;
            this.recordsFiltered = recordsFiltered;
            this.data = data;
        }

        public DatatablesResponse(string error)
        {
            this.error = error;
        }
    }
}