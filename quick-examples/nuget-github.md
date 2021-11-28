---
title: Installing Nuget packages
parent: Quick Examples
has_children: false
nav_order: 2
---

# Installing Nuget packages

`pipes` is available via Nuget packages in order to use


## From Github

Setup the github source

```sh
dotnet nuget add source https://nuget.pkg.github.com/dbones-labs/index.json -n gh-dbones-labs -u YOUR_USER_NAME -p GH_TOKEN [--store-password-in-clear-text]
```

Install the required packages

```sh
dotnet add PROJECT package Pipes
```

All the `pipes` package versions are listed here: [![Nuget](https://img.shields.io/badge/nuget-pipes-blue)](https://github.com/orgs/dbones-labs/packages?repo_name=pipes)