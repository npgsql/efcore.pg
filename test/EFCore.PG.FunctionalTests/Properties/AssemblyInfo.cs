

// For the following see https://github.com/aspnet/EntityFrameworkCore/issues/11848
// [assembly: CollectionBehavior(MaxParallelThreads = 2)]
// [assembly: CollectionBehavior(MaxParallelThreads = 2)]

[assembly: CollectionBehavior(DisableTestParallelization = true)]
