Quicksilver
=========

This repository is the starter site for EPiServer Commerce based on MVC, aka "Quicksilver".

Release Notes
-------------

**This is a pre-release of Quicksilver** and it lacks a number of features and may contain serious bugs. 
The following is a list of known issues with this pre-release:

* Performance testing has not been done. There are a number of areas that will most likely be optimized for speed in a later release.
* Multi-lingual support is only partly implemented, and only for the Swedish market.
* The promotions will be extensively re-worked.
* The product detail page allows you to select any combination of color and size, even if the specific combination does not exist as a variation.
* Design is not fully responsive and support for mobile devices is lacking. This means that HTML and CSS structure will change.
* The structure of the project is subject to change. Files and folders will move and be renamed.
* In-store pickup is missing.
* Multi-shipment support is missing.
* Package and bundle support is missing.

Considering the planned changes, **be advised that there will most likely be no simple upgrade path from the pre-release to the release**.

Installation
------------

1.  Open solution and build to download nuget package dependencies.
2.  Search the solution for "ChangeThis" and review/update as described.
3.  Run Setup\SetupDatabases.cmd to create the databases. In the unlucky event of errors please check the logs.
4.  Open nuget package manager console and run Update-EPiDatabase to update to latest database versions.
5.  Start the site (Debug-Start from Visual studio) and browse to http://localhost:50244 to finish installation. Login with admin/store.

Note: SQL scripts are executed using Windows authentication so make sure your user has sufficient permissions
