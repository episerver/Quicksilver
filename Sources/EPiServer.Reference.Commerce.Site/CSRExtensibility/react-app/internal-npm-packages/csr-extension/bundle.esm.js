import n from"axios";
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
***************************************************************************** */var t=function(n,o){return(t=Object.setPrototypeOf||{__proto__:[]}instanceof Array&&function(n,t){n.__proto__=t}||function(n,t){for(var o in t)t.hasOwnProperty(o)&&(n[o]=t[o])})(n,o)};var o=function(n){function o(){return null!==n&&n.apply(this,arguments)||this}return function(n,o){function r(){this.constructor=n}t(n,o),n.prototype=null===o?Object.create(o):(r.prototype=o.prototype,new r)}(o,n),o}((function(){})),r=n.create({baseURL:"./api-extension"}),i={InitConfiguration:function(n){window.CSRExtension&&window.CSRExtension.InitConfiguration(n)}};export{o as AppSettingState,i as CSRExtension,r as HttpRequest};
