using WorkTodo.Core.Enums;

namespace WorkTodo.Services.Models;

public record TodoFilter(int? TagId = null, Priority? Priority = null, TodoStatus? Status = null);
