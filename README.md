Quicksilver
===========

This repository is the starter site for EPiServer Commerce based on MVC, aka "Quicksilver".

Release Notes
-------------

**This is a pre-release of Quicksilver** and more features will be added over time.
The following is a list of known limitations in this pre-release:

* Performance testing has not been done.
* In-store pickup is missing.
* Multi-shipment support is missing.
* Package and bundle support is missing.

Installation
------------

1.  Open solution and build to download nuget package dependencies.
2.  Search the solution for "ChangeThis" and review/update as described.
3.  Run Setup\SetupDatabases.cmd to create the databases. In the unlucky event of errors please check the logs.
4.  Start the site (Debug-Start from Visual studio) and browse to http://localhost:50244 to finish installation. Login with admin/store.

Note: SQL scripts are executed using Windows authentication so make sure your user has sufficient permissions

Styling
-------

The styling of the site is done in [less](http://lesscss.org/). In order to be able to recompile the less files to css you will need to
install [nodejs](https://nodejs.org/). If you have nodejs the less files will be recompiled into css on every build. From the command line
you can also execute the following command in folder "Sources\EPiServer.Reference.Commerce.Site\":

```
msbuild -t:BuildLessFiles
```