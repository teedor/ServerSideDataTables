using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Web;
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
        /// Server side method for populating datatables
        /// </summary>
        /// <param name="draw">pass this back unchanged in the response</param>
        /// <param name="start">number of records to skip</param>
        /// <param name="length">number of records to return</param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<JsonResult> PeopleData(int draw, int start, int length)
        {
            // get the column index of datatable to sort on
            var orderByColumnNumber = Convert.ToInt32(Request.QueryString["order[0][column]"]);
            var orderColumnName = GetColumnName(orderByColumnNumber);

            // get direction of sort
            var orderDirection = Request.QueryString["order[0][dir]"] == "asc"
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            //// get the search string
            var searchString = Request.QueryString["search[value]"];

            using (var db = new SkippyEntities())
            {
                var repository = new PeopleDatatablesRepository(db);

                var recordsTotal = await repository.GetRecordsTotalAsync();
                var recordsFiltered = await repository.GetRecordsFilteredAsync(searchString);
                var data = await repository.GetPagedSortedFilteredListAsync(start, length, orderColumnName, orderDirection, searchString);

                var response = new DatatablesResponse<Person>()
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsTotal,
                    data = data
                };

                // serialize response object to json string
                var jsonResponse = Json(response, JsonRequestBehavior.AllowGet);

                return jsonResponse;
            }
        }

        private string GetColumnName(int columnNumber)
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
    }
}