# The Episerver Commerce MVC Reference Site Guide

This is a short introduction on how to work with the Episerver Commerce MVC reference site.

## Conditional compilation

* The ACTIVATE_DEFAULT_RECOMMENDATION_WIDGETS_FEATURE symbol: Creates and activates the default Recommendations widgets.
* The EXCLUDE_ITEMS_FROM_PROMOTION_ENGINE_FEATURE symbol: Excludes items (catalog entries) from promotion engine.
* The DISABLE_PROMOTION_TYPES_FEATURE symbol: Disables one or all built-in promotion types.
* The IRI_CHARACTERS_IN_URL_FEATURE symbol: Enables the IRI characters in Urls - this should be done both in both the Commerce Manager and the reference site.
* The GOOGLE_ACCOUNT_LOGIN_FEATURE symbol: Enables authenticating users using their Google+ account.
* The FACEBOOK_ACCOUNT_LOGIN_FEATURE symbol: Enables authenticating users using their Facebook account.
* The TWITTER_ACCOUNT_LOGIN_FEATURE symbol: Enables authenticating users using their Twitter account.
* The MICROSOFT_ACCOUNT_LOGIN_FEATURE symbol: Enables authenticating users using their Microsoft account.

--- 

## Styling

This site comes with a single CSS stylesheet, ./styles/style.css, that is a precompilation of all the LESS files in this project. Do not update this file manually, as such changes will be lost whenever the style.less file is recompiled which will replace the css file. Instead you should apply your changes to the appropriate LESS file and then recompile style.less.