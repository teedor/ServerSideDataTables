using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Skippy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var db = new SkippyEntities())
            {
                // create order by using a string
                var source = db.People.AsQueryable();
                var orderBy = "Id";
                var type = typeof(Person);
                var property = type.GetProperty(orderBy);
                var parameter = Expression.Parameter(type, "p");

                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);

                MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));

                var result = source.Provider.CreateQuery<Person>(resultExp)
                    .Skip(10) //start
                    .Take(30); // length;

                Console.WriteLine(string.Format("People: {0}", result.Count()));
                Console.ReadLine();

                var people = db.People
                    .AsQueryable()
                    .OrderBy(x => x.Id)
                    .Skip(10) //start
                    .Take(30); // length

                Console.WriteLine(string.Format("People: {0}", people.Count()));
                Console.ReadLine();
            }
        }
    }
}