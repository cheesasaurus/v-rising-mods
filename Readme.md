monorepo for various V Rising mods


### notes
- a `<ProjectReference>` to a "class library" project requires the library dll to be shipped alongside the main dll. not ideal
- but an `<Import>` of a "shared" project does what I want

### resources
- https://learn.microsoft.com/en-us/dotnet/core/tutorials/library-with-visual-studio?pivots=dotnet-6-0