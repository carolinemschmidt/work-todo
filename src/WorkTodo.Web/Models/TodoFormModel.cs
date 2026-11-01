using System.ComponentModel.DataAnnotations;
using WorkTodo.Core.Constants;
using WorkTodo.Core.Enums;

namespace WorkTodo.Web.Models;

public class TodoFormModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(ValidationConstants.TitleMaxLength, ErrorMessage = "Title can be at most {1} characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(ValidationConstants.DescriptionMaxLength, ErrorMessage = "Description can be at most {1} characters.")]
    public string? Description { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    public TodoStatus Status { get; set; } = TodoStatus.NotStarted;

    public DateOnly? DueDate { get; set; }
}
