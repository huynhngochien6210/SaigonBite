
using System.ComponentModel.DataAnnotations;

namespace SaigonBite.Models;

public class Brand
{
    public int BrandId { get; set; }

    [Required(ErrorMessage = "Tên thương hiệu không được để trống.")]
    [StringLength(150)]
    public string BrandName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string? Logo { get; set; }

    // Giá trị giả lập, không liên kết bảng Hubs UC-D.
    public int HubCount { get; set; }

    public bool IsActive { get; set; } = true;

    // Soft delete
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property: một Brand có nhiều Food.
    public ICollection<Food> Foods { get; set; }
        = new List<Food>();
}
