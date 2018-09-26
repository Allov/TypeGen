# TypeGen

C# to TypeScript file generator with support for one-class-per-file generation

[![Build status](https://ci.appveyor.com/api/projects/status/pwi1gh8o1byigo2x?svg=true)](https://ci.appveyor.com/project/JacekBurzynski/typegen)

***Please visit project's website: http://jburzynski.net/TypeGen***

## How to get

* [NuGet](https://www.nuget.org/packages/TypeGen)
* [NuGet - DotNetCli version](https://www.nuget.org/packages/TypeGen.DotNetCli)

## Quick start

Get TypeGen via NuGet by either:
* typing `Install-Package TypeGen` into the Package Manager Console
* searching *TypeGen* in the NuGet gallery / NuGet package manager

Mark your C# classes/enums as exportable to TypeScript:

```c#
using TypeGen.Core.TypeAnnotations;

[ExportTsClass]
public class ProductDto
{
    public decimal Price { get; set; }
    public string[] Tags { get; set; }
}
```

After building your project, type into the Package Manager Console:

```
TypeGen ProjectFolder
```

...where *ProjectFolder* is your project folder, relative to your solution directory.

This will generate a single TypeScript file (named *product-dto.ts*) in your project directory. The file will look like this:

```typescript
export class ProductDto {
    price: number;
    tags: string[];
}
```

## Features

Some of TypeGen's features include:

* generating TypeScript classes, interfaces and enums - single class per file
* support for collection (or nested collection) property types
* generic classes/types generation
* support for inheritance
* customizable convertion between C#/TypeScript names (naming conventions)

For complete list of features with examples, please refer to the project's documentation: http://typegen.readthedocs.io