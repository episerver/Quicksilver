Quicksilver 
===========
[![GitHub version](https://badge.fury.io/gh/episerver%2FQuicksilver.svg)](https://github.com/episerver/Quicksilver)
[![License](http://img.shields.io/:license-apache-blue.svg?style=flat-square)](http://www.apache.org/licenses/LICENSE-2.0.html)

This repository is the starter site for the EPiServer Commerce reference implementation, aka "Quicksilver".

Roadmap
-------------

More features will be added over time.
The following is a list of features to be added in the future:

* In-store pickup.
* Package and bundle support.
* Multi-payment support.

Changes in latest release
-----------------------
* COM-2014 Quicksilver 2.0
* Added usage of the new order system abstractions.
* A number of fixes and improvements.
* Added a simple developer guide to familiarize new users with Quicksilver.

Changes in [1.4](https://github.com/episerver/Quicksilver/releases/tag/v1.4.0)
-----------------------
* COM-76   Support for multi-shipment checkout
* COM-1551 Server error when delete item in shopping cart
* COM-1549 Clarify requirement to change fake credentials in Startup.cs
* COM-1932 Items disappears from cart after confirming updated quantity with Enter
* Updated to Episerver Commerce 9.16.0

Changes in [1.3](https://github.com/episerver/Quicksilver/releases/tag/v1.3.0)
-----------------------
* COM-122  Item in wishlist should show discounted price
* COM-868  Inconsistent price when change currency for current market
* COM-1086 Updated looks for login page
* COM-454  Bad layout of product listing page in IPad landscape orientation
* COM-586  Missing Swedish translation of Registration block
* COM-912  Variations without price for currency are shown with price zero
* COM-1163 Cannot apply coupon code in checkout page
* COM-1291 Update Quicksilver to support read-only mode
* COM-1447 Shipping form breaks when apply/remove coupon code
* COM-1448 Keep facets during scroll
* COM-1489 Coupon codes may be dropped
* Updated to Episerver Commerce 9.11.1

Changes in Quicksilver [1.2](https://github.com/episerver/Quicksilver/releases/tag/v1.2.0)
--------------------------
* COM-111 Support for Commerce 9
* COM-259 GitIgnore file on Quicksilver / GitHub excludes too much. Fixed.
* COM-136 Inconsistent display description in Quickview dialog. Fixed.
* COM-267 Error in console when update quantity in cart/minicart. Fixed.
* COM-261 Should show shipping discount information in order confirmation/email. Fixed.
* COM-138 Many buttons display as text on IPhone devices (640×1136 pixels). Fixed.
* COM-231 Incomplete Owin Support for Projects collaboration. Fixed.
* COM-240 Missing shipping address in order detail when select "Save address" option in checkout page. Fixed.
* COM-137 Navigation Icon to edit mode overlay mini cart and wishlist in mobile view. Fixed.

Changes in Quicksilver  [1.1](https://github.com/episerver/Quicksilver/releases/tag/v1.1.0)
--------------------------
* Improved support for ASP.NET identity in Commerce Manager.
* Bug #126401 - Null reference exception when rendering address in some cases. Fixed.
* Bug #129168 - Quick view in Swedish was badly formatted. Fixed.
* Bug #129209 - After log out, still possible to access edit mode in some cases. Fixed.
* Bug #129197 - "Continue shopping" button not working on mobile browsers. Fixed.
* Bug #129214 - Missing EPiServer standard header/footer rendering. Fixed.

Installation
------------

1.  Configure Visual Studio to add this package source: http://nuget.episerver.com/feed/packages.svc/. This allows missing packages to be downloaded, when the solution is built.
2.  Open solution and build to download nuget package dependencies.
3.  Search the solution for "ChangeThis" and review/update as described.
4.  Run Setup\SetupDatabases.cmd to create the databases. In the unlucky event of errors please check the logs.  
5.  Start the site (Debug-Start from Visual studio) and browse to http://localhost:50244 to finish installation. Login with admin/store.

Note: SQL scripts are executed using Windows authentication so make sure your user has sufficient permissions.

Styling
-------

The styling of the site is done in [less](http://lesscss.org/). In order to be able to recompile the less files to css you will need to
install [nodejs](https://nodejs.org/). If you have nodejs the less files will be recompiled into css on every build. From the command line
you can also execute the following command in folder "Sources\EPiServer.Reference.Commerce.Site\":

```
msbuild -t:BuildLessFiles
```

Compiling the razor views
-------------------------

If you want to build the views to validate their correctness you can set the MvcBuildViews parameter to true.

```
msbuild -p:MvcBuildViews=true
```


SQL Server authentication
-------------------------

If you don't have mixed mode authentication enabled you can edit this line in SetupDatabases.cmd and provide username and password.

```
set sql=sqlcmd -S . -U username -P password
```