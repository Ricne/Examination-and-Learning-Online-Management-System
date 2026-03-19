using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OMS.Models.ViewModels.Teacher;

public class LessonVm
{
    public int Lessonid { get; set; }

    [Required]
    public int Courseid { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = "";

    public string? Content { get; set; }

    [StringLength(500)]
    public string? Videourl { get; set; }

    public string? Attachmenturl { get; set; }

    public IFormFile? AttachmentFile { get; set; }

    [Range(1, 9999)]
    public int Lessonorder { get; set; }

    public bool Ispublished { get; set; }
}