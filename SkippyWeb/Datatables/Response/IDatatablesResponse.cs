using System.Collections.Generic;

namespace ServerSideDatatables.Datatables.Response
{
    public interface IDatatablesResponse<T>
    {
        int draw { get; set; }

        int recordsTotal { get; set; }

        int recordsFiltered { get; set; }

        IEnumerable<T> data { get; set; }

        string error { get; set; }
    }
}