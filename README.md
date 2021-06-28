<h1 align="center">
  <a href="https://github.com/orangebeard-io/SpecFlow-Plugin">
    <img src="https://raw.githubusercontent.com/orangebeard-io/SpecFlow-Plugin/main/.github/logo.svg" alt="Orangebeard.io FitNesse TestSystemListener" height="200">
  </a>
  <br>Orangebeard.io SpecFlow Plugin<br>
</h1>

<h4 align="center">A Plugin to report Specflow tests in Orangebeard.</h4>

<p align="center">
  <a href="https://github.com/orangebeard-io/SpecFlow-Plugin/blob/master/LICENSE.txt">
    <img src="https://img.shields.io/github/license/orangebeard-io/SpecFlow-Plugin?style=flat-square"
      alt="License" />
  </a>
</p>

<div align="center">
  <h4>
    <a href="https://orangebeard.io">Orangebeard</a> |
    <a href="#build">Build</a> |
    <a href="#install">Install</a>
  </h4>
</div>

## Build
 * Clone this repository
 * Open in a .Net IDE
 * Reference the Orangebeard.Client DLL (Find it on NuGet)
 * Build the Plugin DLL

## Install
 * Reference the Plugin in your Solution, make sure it is copied to your output directory (You can find the Plugin on Nuget!)
 * Add hooks file (see HooksExample.cs)
 * create Orangebeard.config.json (and set it to copy to output dir):

```json
    {
  "enabled": true,
  "server": {
    "url": "https://my.orangebeard.app/",
    "project": "MY_PROJECT_NAME",
    "authentication": {
      "accessToken": "MY_AUTH_TOKEN"
    }
  },
  "testSet": {
    "name": "Test run name",
    "description": "test run description",
    "attributes": [ "tag1", "somekey:somevalue" ]
  },
  "rootNamespaces": [ "OptionalRootNameSpace" ]
}

```

Now run your test as you normally do and see the results find their way to Orangebeard!

