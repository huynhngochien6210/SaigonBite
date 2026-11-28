
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaigonBite.Models;

public class Food
{
    public int FoodId { get; set; }

    [Required(ErrorMessage = "Tên món ăn không được để trống.")]
    [StringLength(150)]
    public string FoodName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn thương hiệu.")]
    public int BrandId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Range(0.01, 999999999)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Soft delete
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public Brand? Brand { get; set; }
}
