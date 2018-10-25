# Detached
Detached is a set of tools to make the process of building services or REST APIs faster.
It started with EntityFramework and was inspired by [GraphDiff](https://github.com/refactorthis/GraphDiff).
Each tool has its own nuget and instructions, so please check individual read me as needed.

If you'd like to participate or need some support, please drop me an email: mail@leonardoporro.com.ar
or [fork me on github](https://github.com/leonardoporro/Detached/fork).
Thanks in advance for your help!

# EntityFrameworkCore Compatibility
2.1.3 was tested. 1.x are incompatible.

# Detached.EntityFramework
Allows loading and saving entire entity graphs (the entity with its children/relations) at once and without extra code.

[Read me](./README-ENTITYFRAMEWORK.md)

# Detached.Services
Provides generic repositories based on Detached.EntityFramework.

[Read me](./README-SERVICES.md)

# Detached.Mvc
Provides generic controllers and validations based on Detached.Services. 
Also provides automatic localization by mapping full names and namespaces of Clr Types to specified keys and resource 
files and a JsonStringLocalizer.

[Read me](./README-MVC.md)

# Demos
Currently building Angular2 and MVC demos.

# Build
To build the project, you need:
 - Microsoft Visual Studio 2017
 - .Net Standard 2.0 compatible environments for the libraries
 - .Net Core 2.1 for the tests and the demos  
 https://www.microsoft.com/net/download/dotnet-core/2.1
