# Synapse.CustomController Commandline Utility

## Overview

The Synapse.CustomController commandline utility can be used to create a code file and dll for a custom Contoller interface.  The tool provides for a simple "Plan wrapper," but also supports more complex scenarios with custom code.

## Synapse.CustomController.exe

Synapse.CustomController.exe takes a YAML template to create a c# code files and compile them to a dll.  The tool can be executed to create the code files and dll (both), or each task independently.  You may generate a sample .cs and .dll via the 'sample [verbose]' option.  Syntax:

```dos
C:\Synapse\Controller\Assemblies\Custom>Synapse.CustomController.exe
Synapse Server Custom Controller Utility
Syntax: Synapse.CustomController.exe {path to settings file}|sample [verbose]
        sample: Will generate sample settings file.
```

### YAML Template: Top-Level Settings

|Parameter|Type/Value|Required|Default Value|Description
|-|-|-|-|-
|OutputAssembly|`string`|yes|none; will generate a random name if nothing provided|The filename for the dll to be created.
|ApiControllers|`List`|yes*|n/a|If using the utility to create c# files, provide the settings below.

### YAML Template: ApiControllers Settings

|Parameter|Type/Value|Required|Default Value|Description
|-|-|-|-|-
|Name|`string`|yes|none; will generate a random name if nothing provided|Maps to class name in code; must be unique within the dll.
|AuthorizationTopic|`string`|no|none|Provides a designated point in the code to check authorization as specified in Synapse.Server.config.yaml->Authotization section.
|RoutePrefix|`string`|yes|none|The URL prefix for all subsequent ApiMethod routes.
|CreateHelloApiMethod|`bool`|no|true|Creates a "Hello from [Name]Controller, World!" method for simple connectivity testing.
|CreateWhoAmIApiMethod|`bool`|no|true|Creates a method to return the current security context from underlying Execute Controller.
|CreateDalApi|(section)|no|n/a|Optional declaration to generate a DataAccessLayer API for a given class.
| - Class|string|yes|none|The class name for which DAL methods will be created.  This class must exist in the referenced code `File`.
| - File|string|yes|none|The path to the file containing the c# class code as referenced in the `Class` setting.
|CreateClassFileOnly|`bool`|no|false|Prevents the generation of the controller dll as output - only the class files will be created.
|ApiMethods|`List`|yes|n/a|The list of methods to be generated.


#### Details on generating a DataAccesLayer API

The `CreateDalApi->File` setting should reference a file with a complete c# class declaration in it, as shown below.  Be sure the class is in the **same namespace** as the ApiController, which is `Synapse.Custom` by default.  The `CreateDalApi->Class` setting simply reflects the name of the class as specified in the `File`, but id declared in the `CreateDalApi` settings for clarity.

```java
using System;
using System.Collections.Generic;

//use the same namespace as the ApiContoller!
namespace Synapse.Custom
{
    public partial class MyDataClass
    {
        public int Id { get; set; }

        ...
    }
}
```


### YAML Template: ApiMethods Settings

|Parameter|Type/Value|Required|Default Value|Description
|-|-|-|-|-
|HttpMethod|`string`, HttpVerb|yes|Get|Get, Put, Post, Delete, etc.  See <a href="https://msdn.microsoft.com/en-us/library/system.net.http.httpmethod(v=vs.118).aspx" target="_blank">MSDN doc</a> for a complete description.
|AuthorizationTopic|`string`|no|none|Provides a designated point in the code to check authorization as specified in Synapse.Server.config.yaml->Authotization section.
|Route|`string`|yes|none|The specific URL route to the method.  See <a href="https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2" target="_blank">MSDN Attribute Routing doc</a> for details on options.
|ReturnType|`string`|yes|`string`|The Type of the return value of the method.
|Name|`string`|yes|none; will generate a random name if nothing provided|The name for the method in code.  Must be unique within the Controller, or can be a duplicate name with a <a href="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/methods" target="_blank">unique method signature</a>.
|Parms|`string`|no|none|The method parameters that map to the Plan.  Specify parameters for better discoverability of intended method usage through utilities like swagger.  See **note below.
|PlanName|`string`|yes|none|The Synapse Plan to be executed by the method.
|ExecuteAsync|`bool`|no|false|Specifies whether the underlying Execute Controller instance will run the Plan synchronously or asynchronously.  If async, the ReturnType must be `long`.
|CodeBlob|`string`|no|none|A manually coded block to be inserted in the metho between parameter gathering and Plan execution.  Useful for parameter validation.
|Options|(section)|no|n/a|If executing a Plan sychronously, set values here to override default settings.
| - Path|`string`|no|Actions[0]:Result:ExitData|Specify the path in the Plan.ResultPlan to the section or value to return when the Plan completes.
| - SerializationType|`enum`|no|`Json`|Options are: `Yaml = 0, Xml = 1, Json = 2, Html = 3, Unspecified = 4`.  You may provide the string or numeric value.
| - SetContentType|`bool`|no|true|Sets `ContentType` value in HttpResponse Headers as specified in SerializationType.
| - PollingIntervalSeconds|`int`|no|1|The time, in seconds, the Execute Controller will poll for Plan completion.
| - TimeoutSeconds|`int`|no|120|The time, in seconds, before the ExecuteController will stop polling and kill the Plan execution.
| - NodeRootUrl|`string`|no|none|An alternate Node at which to execute the Plan, other than that which is specified in the Execute Controller's Synapse.Server.config.yaml.

#### **Note on how an auto-generated method will capture HttpRequest Parameters:

All auto-generated methods will capture the entire HttpRequest querystring, request body, and any URL template parameters used in attribute-based routing.  The querystring and URL template parameters will be inserted into the DynamicParameters collection, keyed off the existing parameter name.  The request body will be iserted in with a key of `requestBody`.  All of this data becomes accessible to the Plan via the `Dynamic` section of `Action->Config/Parameters`.

## Example Usage

### Sample Template Input

Assuming the file below is named `myCustom.yaml`, usage to create the class file and dll output is as follows:

```dos
c:\Synapse\Controller\Assemblies\Custom\Synapse.CustomController.exe myCustom.yaml
```

### `myCustom.yaml` template:

```yaml
OutputAssembly: Synapse.CustomAssm.Sample.dll
ApiControllers:
- Name: Custom
  AuthorizationTopic: ApiTopic
  RoutePrefix: my/route
  CreateHelloApiMethod: true   #default true, shown for example only
  CreateWhoAmIApiMethod: true  #default true, shown for example only
  ApiMethods:
  - HttpMethod: Get
    AuthorizationTopic: MethodTopic
    Route: '{interesting}/path'
    ReturnType: object
    Name: MyCustomMethod
    Parms: string interesting, string aaa, string bbb, string ccc = "foo"
    PlanName: sampleHtml
    CodeBlob: string foo = "foo";
    Options:
      Path: Actions[0]:Result:ExitData
      SerializationType: Html
      SetContentType: true
      PollingIntervalSeconds: 2
      TimeoutSeconds: 10
```

The above will auto-generate a c# code file, which is then auto-compiled to a dll, all output into a subfolder called `C:\Synapse\Controller\Assemblies\Custom\Synapse.CustomAssm.Sample`.

```java
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Synapse.Core;
using Synapse.Services;

namespace Synapse.Custom
{
    [SynapseCustomAuthorize( "ApiTopic" )]
    [RoutePrefix( "my/route" )]
    public class CustomController : ApiController
    {
        [HttpGet]
        [Route( "hello" )]
        public string Hello()
        {
            return "Hello from CustomController, World!";
        }

        [HttpGet]
        [Route( "hello/whoami" )]
        public string WhoAmI()
        {
            return GetExecuteControllerInstance().WhoAmI();
        }

        [SynapseCustomAuthorize( "MethodTopic" )]
        [HttpGet]
        [Route( "{interesting}/path" )]
        public object MyCustomMethod(string interesting, string aaa, string bbb, string ccc = "foo")
        {
            Dictionary<string, string> parms = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
            parms["interesting"] = string.Format( "{0}", interesting );
            parms["aaa"] = string.Format( "{0}", aaa );
            parms["bbb"] = string.Format( "{0}", bbb );
            parms["ccc"] = string.Format( "{0}", ccc );

            string foo = "foo";

            return (object)StartPlan( planUniqueName: "sampleHtml", parms: parms, serializationType: SerializationType.Html, pollingIntervalSeconds: 2, timeoutSeconds: 10 );
        }

        IExecuteController GetExecuteControllerInstance()
        {
            System.Net.Http.Headers.AuthenticationHeaderValue auth = null;
            if( Request != null )
                if( Request.Headers != null )
                    auth = Request.Headers.Authorization;

            return ExtensibilityUtility.GetExecuteControllerInstance( Url, User, auth );
        }

        object StartPlan(string planUniqueName, Dictionary<string, string> parms = null,
            string path = "Actions[0]:Result:ExitData", SerializationType serializationType = SerializationType.Json,
            bool setContentType = true, int pollingIntervalSeconds = 1, int timeoutSeconds = 120, string nodeRootUrl = null, bool executeAsync = false)
        {
            StartPlanEnvelope pe = new StartPlanEnvelope { DynamicParameters = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase ) };

            IEnumerable<KeyValuePair<string, string>> queryString = this.Request.GetQueryNameValuePairs();
            foreach( KeyValuePair<string, string> kvp in queryString )
                pe.DynamicParameters.Add( kvp.Key, kvp.Value );

            if( parms != null )
                foreach( KeyValuePair<string, string> kvp in parms )
                    pe.DynamicParameters[kvp.Key] = kvp.Value;

            string body = "body";
            if( Url.Request.Properties.ContainsKey( body ) && Url.Request.Properties[body] != null )
                pe.DynamicParameters["requestBody"] = Url.Request.Properties[body].ToString();

            string dryrun = "dryRun";
            bool dryRun = false;
            if( pe.DynamicParameters.ContainsKey( dryrun ) )
                bool.TryParse( pe.DynamicParameters[dryrun], out dryRun );

            string requestnumber = "requestNumber";
            string requestNumber = null;
            if( pe.DynamicParameters.ContainsKey( requestnumber ) )
                requestNumber = pe.DynamicParameters[requestnumber];

            if( executeAsync )
                return GetExecuteControllerInstance().StartPlan( planEnvelope: pe,
                    planUniqueName: planUniqueName, dryRun: dryRun, requestNumber: requestNumber, nodeRootUrl: nodeRootUrl );
            else
                return GetExecuteControllerInstance().StartPlanSync( planEnvelope: pe,
                    planUniqueName: planUniqueName, dryRun: dryRun, requestNumber: requestNumber,
                    path: path, serializationType: serializationType, setContentType: setContentType,
                    pollingIntervalSeconds: pollingIntervalSeconds, timeoutSeconds: timeoutSeconds, nodeRootUrl: nodeRootUrl );
        }
    }
}
```

### Example of Directly Invoking the Compiler

The above will also generate a "make file," which is a alternate input that allows you to directly compile the code file(s) to a dll.  This is useful is you need to manually edit the code files post-template-generation.

Usage:

```dos
c:\Synapse\Controller\Assemblies\Custom\Synapse.CustomController.exe Synapse.CustomAssm.Sample.dll.make.yaml
```

Where `Synapse.CustomAssm.Sample.dll.make.yaml` is:

```yaml
OutputAssembly: Synapse.CustomAssm.Sample.dll
Compiler:
  WarningLevel: 3
  TreatWarningsAsErrors: false
  IncludeDebugInformation: false
  CompilerOptions: /optimize
  ReferencedAssemblies:
  - Newtonsoft.Json.dll
  - Synapse.Core.dll
  - Synapse.Server.Extensibility.dll
  - System.Net.Http.dll
  - System.Net.Http.Formatting.dll
  - System.Web.Http.dll
  - YamlDotNet.dll
Files:
- Synapse.CustomAssm.Sample\Custom.cs
```

## Declare your Custom Controller Interface

After you implement the custom interface and compile it to a dll, you need to specify it in Synapse.Server.config.yaml in the Controller->Assemblies section, as shown below.  The syntax is simply to list the assembly name, as opposed to `assembly:{namepsace:}class`, as in Handler declaration.  Synapse Controller will discover all classes that inherit from `System.Web.Http.ApiController` (<a href="https://msdn.microsoft.com/en-us/library/system.web.http.apicontroller(v=vs.118).aspx" target="_blank">MSDN<a/>).

```yaml
# Configure the 'Assemblies' node of Synapse.Server.config.yaml

Service:
  Name: Synapse.Controller
  DisplayName: Synapse Controller
  Role: Controller
...
Controller:
  ...
  Assemblies: 
  - Name: Synapse.CustomController0
    Config:
      {any required config}
  - Name: Synapse.CustomController1
    Config:
      {any required config}
  - Name: Synapse.CustomController2
    Config:
      {any required config}
  Dal:
    ...
```