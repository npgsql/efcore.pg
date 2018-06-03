using System.Reflection;
using System.Resources;
using Xunit;

// For the following see https://github.com/aspnet/EntityFrameworkCore/issues/11848
[assembly: CollectionBehavior(MaxParallelThreads = 4)]
