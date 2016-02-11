#JQuery datatables with server-side processing

![datatables](https://raw.githubusercontent.com/stevenalexander/ServerSideDataTables/master/datatables-server-side.png)

## Introduction

JQuery [Datatables](https://datatables.net) is a great tool, attach it to a table of results and it gives you quick and 
easy sorting/searching. Against a small dataset this works fine, but once you start to have >1000 records your page load 
is going to take a long time. To solve this Datatables recommend [server-side processing](https://datatables.net/manual/server-side).

This code is an example of implementing server-side processing for an ASP.MVC web appliction, using a generic approach with
Linq so that you can re-use it for different entities easily with little code repetition. It also shows an implementation of 
full word search across all columns, which is something that the Javascript processing version offers but is very tricky to 
implement on the database side with decent performance. It's a C# .NET implementation but you can take the interfaces and 
calls from the controllers and convert the approach for Java or Ruby (missing the nice Linq stuff tho).

## Details

I'll skip the basic view/js details as that is easily available on the [datatables documentation](https://datatables.net/examples/basic_init/zero_configuration.html).

The request comes into the controller as a GET with all the sort/search details as query parameters (see
[here](https://datatables.net/manual/server-side)), it expects a result matching [this interface](https://github.com/stevenalexander/ServerSideDataTables/blob/master/SkippyWeb/Datatables/Response/IDatatablesResponse.cs):

    public interface IDatatablesResponse<T>
    {
        int draw { get; set; }
        int recordsTotal { get; set; }
        int recordsFiltered { get; set; }
        IEnumerable<T> data { get; set; }
        string error { get; set; }
    }

The [controller](https://github.com/stevenalexander/ServerSideDataTables/blob/master/SkippyWeb/Controllers/HomeController.cs#L41) extracts the parameters, creates the DB context and repository and makes three calls asynchronously:
- get the total records
- get the total filtered records
- get the searched/sorted/paged data

The data is returned and Datatables Javascript uses it to render the table and controls for the correct searched/sorted/paged results.

The magic happens in the DatatablesRepository objects which handle those calls.

### DatatablesRepository classes

## Notes

- I created the initial example with the help of [Stephen Anderson](https://github.com/teedor/ServerSideDataTables)

