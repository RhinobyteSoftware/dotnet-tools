

# Rhinobyte.Tools

This repo contains the code to build the Rhinobyte tools libraries for .NET

## Libraries/Projects



## Contributing

Filing issues for problems encountered is greatly appreciated and contribution via PRs is welcomed.

Code changes that modify a library's public API signature will require a new major version number. For any such changes, please create an issue for discussion first and include the label `api-suggestion`. Please do not submit pull requests that include style changes.

Maintaining a very high percentage of test coverage over these libraries will be an important requirement when reviewing pull requests. The `devops/create-code-coverage-report` shell script can be run to execute the tests with code coverage enabled and to generate an html version of the coverage results report. When making a code contribution please ensure you update all existing tests as necessary and add new test cases to cover new code.


## Reference Documentation

* [Tutorial - Write your first dotnet analyzer](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix)
* [How to write a Roslyn Analyzer dev blog post](https://devblogs.microsoft.com/dotnet/how-to-write-a-roslyn-analyzer/)
* [Source Generators Overview](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)


## License

This repository is licensed under the [MIT](LICENSE.txt) license.
