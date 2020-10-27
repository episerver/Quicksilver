"use strict";Object.defineProperty(exports,"__esModule",{value:!0});var t,n=(t=require("axios"))&&"object"==typeof t&&"default"in t?t.default:t,e=function(t,n){return(e=Object.setPrototypeOf||{__proto__:[]}instanceof Array&&function(t,n){t.__proto__=n}||function(t,n){for(var e in n)n.hasOwnProperty(e)&&(t[e]=n[e])})(t,n)};
/*! *****************************************************************************
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the Apache License, Version 2.0 (the "License"); you may not use
this file except in compliance with the License. You may obtain a copy of the
License at http://www.apache.org/licenses/LICENSE-2.0

THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABLITY OR NON-INFRINGEMENT.

See the Apache Version 2.0 License for specific language governing permissions
and limitations under the License.
***************************************************************************** */var o=function(t){function n(){return null!==t&&t.apply(this,arguments)||this}return function(t,n){function o(){this.constructor=t}e(t,n),t.prototype=null===n?Object.create(n):(o.prototype=n.prototype,new o)}(n,t),n}((function(){})),r=n.create({baseURL:"./api-extension"}),i={InitConfiguration:function(t){window.CSRExtension&&window.CSRExtension.InitConfiguration(t)}};exports.AppSettingState=o,exports.CSRExtension=i,exports.HttpRequest=r;
