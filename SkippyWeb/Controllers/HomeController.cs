using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SkippyWeb.Controllers
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
        public JsonResult PeopleData(int draw, int start, int length)
        {
            using (var db = new SkippyEntities())
            {
                // get the column index of datatable to sort on
                var orderByColumnNumber = Convert.ToInt32(Request.QueryString["order[0][column]"]);
                var columnNameString = GetColumnName(orderByColumnNumber);

                // get direction of sort
                var orderDirection = Request.QueryString["order[0][dir]"] == "asc"
                    ? "OrderBy"
                    : "OrderByDescending";

                // get the search string
                var searchString = Request.QueryString["search[value]"];

                // get an IQueryable
                var source = db.People.Where(x =>
                    // id column (int)
                    SqlFunctions.StringConvert((double)x.Id).Contains(searchString)
                        // name column (string)
                    || x.Name.Contains(searchString)
                        // date of birth column (datetime, formatted as d/M/yyyy) - limitation of sql prevented us from getting leading zeros in day or month - if you figure out how to do this, please let Steven Alexander and Stephen Anderson know
                    || (SqlFunctions.StringConvert((double)SqlFunctions.DatePart("dd", x.DateOfBirth)) + "/" + SqlFunctions.DatePart("mm", x.DateOfBirth) + "/" + SqlFunctions.DatePart("yyyy", x.DateOfBirth)).Contains(searchString))
                .AsQueryable();

                // create generic orderby clause for any column
                var type = typeof(Person);
                var property = type.GetProperty(columnNameString);
                var parameter = Expression.Parameter(type, "p");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);
                var filteredAndOrderedQuery = Expression.Call(typeof(Queryable), orderDirection, new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));

                // filtered count - same query as search query above
                var recordsFiltered = db.People.Count(x =>
                    SqlFunctions.StringConvert((double)x.Id).Contains(searchString)
                    || x.Name.Contains(searchString)
                    || (SqlFunctions.StringConvert((double)SqlFunctions.DatePart("dd", x.DateOfBirth)) + "/" + SqlFunctions.DatePart("mm", x.DateOfBirth) + "/" + SqlFunctions.DatePart("yyyy", x.DateOfBirth)).Contains(searchString));

                // execute query and include paging (skip + take)
                var people = source.Provider.CreateQuery<Person>(filteredAndOrderedQuery)
                    .Skip(start)
                    .Take(length)
                    .ToList();

                // create response object
                var response = new DatatablesResponse<Person>
                {
                    draw = draw,
                    recordsTotal = db.People.Count(),
                    recordsFiltered = recordsFiltered,
                    data = people
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

    public class DatatablesResponse<T>
    {
        public DatatablesResponse()
        {
        }

        public DatatablesResponse(int draw, int recordsTotal, int recordsFiltered, List<T> data)
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

        public int draw { get; set; }

        public int recordsTotal { get; set; }

        public int recordsFiltered { get; set; }

        public List<T> data { get; set; }

        public string error { get; set; }
    }
}