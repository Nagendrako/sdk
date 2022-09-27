﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.ApiCompatibility.Abstractions;
using Microsoft.DotNet.ApiCompatibility.Rules;
using Moq;
using Xunit;

namespace Microsoft.DotNet.ApiCompatibility.Tests.Mappers
{
    public class AssemblySetMapperTests
    {
        [Fact]
        public void AssemblySetMapper_Ctor_PropertiesSet()
        {
            IRuleRunner ruleRunner = Mock.Of<IRuleRunner>();
            MapperSettings mapperSettings = new();
            int rightSetSize = 5;

            AssemblySetMapper assemblySetMapper = new(ruleRunner, mapperSettings, rightSetSize);

            Assert.Null(assemblySetMapper.Left);
            Assert.Equal(mapperSettings, assemblySetMapper.Settings);
            Assert.Equal(rightSetSize, assemblySetMapper.Right.Length);
            Assert.Equal(0, assemblySetMapper.AssemblyCount);
        }

        [Fact]
        public void AssemblySetMapper_GetAssembliesWithoutLeftAndRight_EmptyResult()
        {
            AssemblySetMapper assemblySetMapper = new(Mock.Of<IRuleRunner>(), new MapperSettings(), rightSetSize: 1);
            Assert.Empty(assemblySetMapper.GetAssemblies());
            Assert.Equal(0, assemblySetMapper.AssemblyCount);
        }

        [Fact]
        public void AssemblySetMapper_GetAssemblies_ReturnsExpected()
        {
            string[] leftSyntaxes = new[]
            {
                @"
namespace NamespaceInAssemblyA
{
  public class First { }
}
",
                @"
namespace NamespaceInAssemblyB
{
  public class First { }
}
",
                @"
namespace NamespaceInAssemblyC
{
  public class First { }
}
"
            };
            string[] rightSyntaxes1 = new[]
{
                @"
namespace NamespaceInAssemblyA
{
  public class First { }
}
",
                @"
namespace NamespaceInAssemblyB
{
  public class First { }
}
",
                @"
namespace NamespaceInAssemblyC
{
  public class First { }
}
",
                @"
namespace NamespaceInAssemblyD
{
  public class First { }
}
"
            };
            string[] rightSyntaxes2 = new[]
{
                @"
namespace NamespaceInAssemblyA
{
  public class First { }
}
",
                @"
namespace NamespaceInAssemblyB
{
  public class First { }
}
",
                @"
namespace NamespaceInAssemblyC
{
  public class First { }
}
",
                @"
namespace NamespaceInAssemblyD
{
  public class First { }
}
"
            };
            IReadOnlyList<ElementContainer<IAssemblySymbol>> left = SymbolFactory.GetElementContainersFromSyntaxes(leftSyntaxes);
            IReadOnlyList<ElementContainer<IAssemblySymbol>> right1 = SymbolFactory.GetElementContainersFromSyntaxes(rightSyntaxes1);
            IReadOnlyList<ElementContainer<IAssemblySymbol>> right2 = SymbolFactory.GetElementContainersFromSyntaxes(rightSyntaxes2);
            AssemblySetMapper assemblySetMapper = new(Mock.Of<IRuleRunner>(), new MapperSettings(), rightSetSize: 2);
            assemblySetMapper.AddElement(left, ElementSide.Left);
            assemblySetMapper.AddElement(right1, ElementSide.Right);
            assemblySetMapper.AddElement(right2, ElementSide.Right, 1);

            Assert.Equal(0, assemblySetMapper.AssemblyCount);
            IEnumerable<IAssemblyMapper> assemblyMappers = assemblySetMapper.GetAssemblies();
            Assert.Equal(4, assemblySetMapper.AssemblyCount);

            Assert.Equal(4, assemblyMappers.Count());
            Assert.Equal(new string?[] {
                    nameof(AssemblySetMapper_GetAssemblies_ReturnsExpected) + "-0",
                    nameof(AssemblySetMapper_GetAssemblies_ReturnsExpected) + "-1",
                    nameof(AssemblySetMapper_GetAssemblies_ReturnsExpected) + "-2",
                    null
                },
                assemblyMappers.Select(asm => asm.Left?.Element.Name));



            // Verify names
            int counter = 0;
            foreach (IAssemblyMapper assemblyMapper in assemblyMappers)
            {
                string expectedAssemblyName = nameof(AssemblySetMapper_GetAssemblies_ReturnsExpected) + $"-{counter}";

                Assert.Equal(2, assemblyMapper.Right.Length);
                Assert.True(assemblyMapper.Right.All(r => r?.Element.Name == expectedAssemblyName));

                counter++;
            }
        }
    }
}