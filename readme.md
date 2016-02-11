#JQuery datatables with server-side processing

![datatables](https://raw.githubusercontent.com/stevenalexander/ServerSideDataTables/master/datatables-server-side.png)

## Introduction

JQuery [Datatables](https://datatables.net) is a great tool, attach it to a table of results and it gives you quick and 
easy sorting/searching. Against a small dataset this works fine, but once you start to have >1000 records your page load 
is going to take a long time. To solve this Datatables recommend [server-side processing](https://datatables.net/manual/server-side).

This code is an example of implementing server-side processing for an ASP.MVC web appliction, using a generic approach with
Linq so that you can re-use it for different entities easily with little code repetition. It also shows an implementation of 
full word search across all columns, which is something that the Javascript processing version offers but is very tricky to 
implement on the database side with decent performance.
