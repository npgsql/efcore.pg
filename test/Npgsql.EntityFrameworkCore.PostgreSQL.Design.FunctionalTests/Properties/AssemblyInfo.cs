using System.Reflection;
using System.Resources;
using Xunit;

// There seem to be some issues running in parallel... See #23
[assembly: CollectionBehavior(DisableTestParallelization = true)]
