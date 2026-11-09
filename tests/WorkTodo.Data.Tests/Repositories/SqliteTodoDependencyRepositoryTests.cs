using WorkTodo.Data.Tests.Contracts;
using WorkTodo.Data.Tests.Stores;

namespace WorkTodo.Data.Tests.Repositories;

public class SqliteTodoDependencyRepositoryTests() : TodoDependencyRepositoryContractTests(new SqliteRepositoryStore());
