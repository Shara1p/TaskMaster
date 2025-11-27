using System.ComponentModel.DataAnnotations;
using TaskMaster.Models.Enums;

namespace TaskMaster.Models.DTO;
public class UpdateTaskStatusDto
{
    [Required]
    public TaskItemStatus? Status { get; set; }
}