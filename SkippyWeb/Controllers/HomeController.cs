using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using ServerSideDatatables.Datatables.Repository;
using ServerSideDatatables.Datatables.Response;

namespace ServerSideDatatables.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Returns JQuery Datatable JSON for server-side processing, using a view in the DB to improve performance 
        /// and simplify logic needed for searching/sorting. This allows generic implementation of a full word search
        /// on the DB against all columns, i.e "adam finance" matching a Person with name "Adam" and linked to 
        /// Department name "Finance".
        /// </summary>
        /// <param name="draw">pass this back unchanged in the response</param>
        /// <param name="start">number of records to skip</param>
        /// <param name="length">number of records to return</param>
        /// <returns>JSON for Datatables response</returns>
        public async System.Threading.Tasks.Task<JsonResult> PeopleListViewData(int draw, int start, int length)
        {
            // get the column index of datatable to sort on
            var orderByColumnNumber = Convert.ToInt32(Request.QueryString["order[0][column]"]);
            var orderColumnName = GetPersonListColumnName(orderByColumnNumber);

            // get direction of sort
            var orderDirection = Request.QueryString["order[0][dir]"] == "asc"
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            //// get the search string
            var searchString = Request.QueryString["search[value]"];

            using (var db = new SkippyEntities())
            {
                var repository = new PersonDepartmentListViewRepository(db);

                var recordsTotal = await repository.GetRecordsTotalAsync();
                var recordsFiltered = await repository.GetRecordsFilteredAsync(searchString);
                var data = await repository.GetPagedSortedFilteredListAsync(start, length, orderColumnName, orderDirection, searchString);

                var response = new DatatablesResponse<PersonDepartmentListView>()
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = data
                };

                // serialize response object to json string
                var jsonResponse = Json(response, JsonRequestBehavior.AllowGet);

                return jsonResponse;
            }
        }

        /// <summary>
        /// Server side method for populating datatables from a normal entity, with limitation on single column search
        /// </summary>
        /// <param name="draw">pass this back unchanged in the response</param>
        /// <param name="start">number of records to skip</param>
        /// <param name="length">number of records to return</param>
        /// <returns>JSON for Datatables response</returns>
        public async System.Threading.Tasks.Task<JsonResult> PeopleData(int draw, int start, int length)
        {
            // get the column index of datatable to sort on
            var orderByColumnNumber = Convert.ToInt32(Request.QueryString["order[0][column]"]);
            var orderColumnName = GetPersonColumnName(orderByColumnNumber);

            // get direction of sort
            var orderDirection = Request.QueryString["order[0][dir]"] == "asc"
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            //// get the search string
            var searchString = Request.QueryString["search[value]"];

            using (var db = new SkippyEntities())
            {
                db.Configuration.LazyLoadingEnabled = false; // needed as otherwise Linq will attempt DB calls to retrieve related entities

                var repository = new PeopleDatatablesRepository(db);

                var recordsTotal = await repository.GetRecordsTotalAsync();
                var recordsFiltered = await repository.GetRecordsFilteredAsync(searchString);
                var data = await repository.GetPagedSortedFilteredListAsync(start, length, orderColumnName, orderDirection, searchString);

                var response = new DatatablesResponse<Person>()
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = data
                };

                // serialize response object to json string
                var jsonResponse = Json(response, JsonRequestBehavior.AllowGet);

                return jsonResponse;
            }
        }

        private string GetPersonColumnName(int columnNumber)
        {
            switch (columnNumber)
            {
                case 0:
                    return "Id";

                case 1:
                    return "Name";

                case 2:
                    return "DateOfBirth";
            }

            return string.Empty;
        }

        private string GetPersonListColumnName(int columnNumber)
        {
            switch (columnNumber)
            {
                case 0:
                    return "Id";

                case 1:
                    return "Name";

                case 2:
                    return "DateOfBirthFormatted";

                case 3:
                    return "DepartmentName";
            }

            return string.Empty;
        }
    }
}