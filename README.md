This is Fork of original version for updating to .NET Core 2.0.

# Detached
Detached is a set of tools to make the process of building services or REST APIs faster.
It started with EntityFramework and was inspired by [GraphDiff](https://github.com/refactorthis/GraphDiff).
Each tool has its own nuget and instructions, so please check individual read me as needed.

# Detached.EntityFramework
Allows loading and saving entire entity graphs (the entity with its children/relations) at once and without extra code.

[Read me](./README-ENTITYFRAMEWORK.md)

# Detached.Services
Provides generic repositories based on Detached.EntityFramework.

[Read me](./README-SERVICES.md)

# Detached.Mvc
Removed in current fork. Please, see original source.

Provides generic controllers and validations based on Detached.Services. 
Also provides automatic localization by mapping full names and namespaces of Clr Types to specified keys and resource 
files and a JsonStringLocalizer.

[Read me](./README-MVC.md)

# Demos
Removed. See UnitTests.

# Build
To build the project, you need:
 - Microsoft Visual Studio 2017
 - .NET Core 2.0 or later
https://www.microsoft.com/net/core#windowsvs2015
