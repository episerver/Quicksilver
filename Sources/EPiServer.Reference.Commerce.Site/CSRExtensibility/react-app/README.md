## Introduction
For the CSR Extensibility feature, we created a sample react project, including:
- @episerver/csr-extension: use this package for declaring extension configuration.
- @episerver/csr-app : use this package for debugging purpose.
- scripts: to build the bundle config file and use it for injecting to CSR

## Basic setup
Below are the basic steps to add a CSR extension module
- Declare a setting object
    - We already defined `demo-setting.tsx` with sample configuration. You can change those configurations.
- Call function `CSRExtension.InitConfiguration` with `setting` object you defined as param.
    - This function was defined by CSR itself. When you want to make a change to setting object, call function `CSRExtension.InitConfiguration` again.
- Create a bundle file from `csr-extension.tsx` as entry point by using script `yarn package`
    - This script will output bundle file at `dist` folder
- Specify js and css files which will be injected into CSR
    - By default, Quicksilver take `customized-csr-bundle.min.js` file and inject into current CSR page.

        You can change the injected js, css file at `CSRExtensibility/CSRCustomUI.cs`

        Sample code

        ```c#
        namespace EPiServer.Reference.Commerce.Site.UIComponents
        {
            [ServiceConfiguration(typeof(CSRUIExtensionConfiguration))]
            public class CSRCustomUI : CSRUIExtensionConfiguration
            {
                public CSRCustomUI()
                {
                    ResourceScripts = new string[] { "/CSRExtensibility/react-app/dist/customized-csr-bundle.min.js" };

                    ResourceStyles = new string[]{};
                }
            }
        }
        ```
        You can add ResourceStyles and ResourceScripts. We will take those files and inject into CSR.


## Available Scripts

### `yarn package` and `yarn package:dev`

This script will bundle `src/configuration/csr-extension.tsx` file and output result to `dist` folder.
If you want to pack in the development environment, you can use `yarn package:dev`. This will output bundle file without minified, for debugging.

## Define new API controller
When there is a need to define new API for specific job, you can do by your own or using `HTTPRequest` included in the @episerver/csr-extension package.
HTTPRequest is an axios instance come with the baseURL config.

- First, create a new Controller where extend CSRAPIController at server side:
```c#
[EpiRoutePrefix("csr-demo")]
public class DemoApiController: CSRAPIController
{
    [HttpGet]
    [EpiRoute("getData")]
    public IHttpActionResult Get()
    {
        return Ok("Sample data");
    }
}
```
- Then make API request at the front-end side:
```javascript
import { HttpRequest } from '@episerver/csr-extension';

HTTPRequest.get('csr-demo/getData').then(resp => {
    console.log(resp.data);
});

```

## Development environment
### How to debug the extension bundle in CSR app
We had exported CSRApp component include in internal `@episerver/csr-app` package
Follow these steps to setup debug environment:
- Declare <CSRApp> component and passing `setting` object via props
- Setup local environment of the Quicksilver site and config proxy at `setupProxy.js` in the `src` folder

Here is the sample code to running CSRApp in the development evironment
```javascript
import * as React from 'react';
import ReactDOM from 'react-dom';
import { CSRApp } from '@episerver/csr-app';
import { setting } from './configuration/demo-setting';

import '@episerver/csr-app/bundle.css';

ReactDOM.render(
  <div>
    <CSRApp setting={setting} />
  </div>,
  document.getElementById('root')
);
```

- run `yarn start` to start debugging

## Important Notes

- To use the react hook, please config react as an external library. By default, we already set externals config at `webpack.extension.config.js` file
```javascript
externals: {
    react: 'React'
},
```
We also injected the react library into CSR as a separate library, so that it guarantees the existing CSR and extension share the same react library.


