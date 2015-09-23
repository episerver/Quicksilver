Quicksilver
===========

This repository is the starter site for EPiServer Commerce based on MVC, aka "Quicksilver".

Release Notes
-------------

This is the first release of Quicksilver and more features will be added over time.
The following is a list of features to be added in the future:

* In-store pickup.
* Multi-shipment support.
* Package and bundle support.

Changes in this release
-----------------------

* Numerous bug fixes. For future releases a proper bug list will be included.
* New look with improved responsive design.

Installation
------------

1.  Configure Visual Studio to add this package source: http://nuget.episerver.com/feed/packages.svc/. This allows missing packages to be downloaded, when the solution is built.
2.  Open solution and build to download nuget package dependencies.
3.  Search the solution for "ChangeThis" and review/update as described.
4.  Run Setup\SetupDatabases.cmd to create the databases. In the unlucky event of errors please check the logs.  
5.  Start the site (Debug-Start from Visual studio) and browse to http://localhost:50244 to finish installation. Login with admin/store.

Note: SQL scripts are executed using Windows authentication so make sure your user has sufficient permissions

Styling
-------

The styling of the site is done in [less](http://lesscss.org/). In order to be able to recompile the less files to css you will need to
install [nodejs](https://nodejs.org/). If you have nodejs the less files will be recompiled into css on every build. From the command line
you can also execute the following command in folder "Sources\EPiServer.Reference.Commerce.Site\":

```
msbuild -t:BuildLessFiles
```