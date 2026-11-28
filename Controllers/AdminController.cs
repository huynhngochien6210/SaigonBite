
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using SaigonBite.ViewModels;
using Microsoft.EntityFrameworkCore;
using SaigonBite.Data;
using SaigonBite.Models;
using Microsoft.AspNetCore.Http;

namespace SaigonBite.Controllers;

public class AdminController : Controller
{
    private readonly SaigonBite.Services.IAuthenticationService
    _authenticationService;

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    // Constructor
    public AdminController(
        SaigonBite.Services.IAuthenticationService authenticationService,
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _authenticationService = authenticationService;

        _context = context;
        _environment = environment;

    }

    // ==========================================
    // ADMIN LOGIN
    // ==========================================

    // Display Admin Login
    [HttpGet]
    public IActionResult Login()
    {
        return View(new AdminLoginViewModel());
    }

    // Process Admin Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authenticationService.LoginAdminAsync(model);

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(model);
        }

        var user = result.User;

        // Create claims
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.FullName),

            new Claim(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        // Create authentication cookie
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        HttpContext.Session.Clear();

        // Temporary endpoint for testing
        return RedirectToAction(nameof(LoginSuccess));
    }

    // Temporary endpoint to verify Admin authorization
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult LoginSuccess()
    {
        return Content(
            $"Admin login successful. Welcome, {User.Identity?.Name}!");
    }

    // ==========================================
    // UC-A: DASHBOARD
    // ==========================================

    [HttpGet]
    public IActionResult Dashboard()
    {
        return View();
    }

    // ==========================================
    // UC-A: BRAND MANAGEMENT
    // ==========================================

    [HttpGet]

    public async Task<IActionResult> Brands()
    {
        var brands = await _context.Brands
            .AsNoTracking()
            .Include(b => b.Foods)
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(brands);
    }


    // GET: Admin/CreateBrand
    [HttpGet]
    public IActionResult CreateBrand()
    {
        var brand = new Brand
        {
            IsActive = true,
            HubCount = 3
        };

        return View(brand);
    }

    // POST: Admin/CreateBrand
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBrand(
        Brand brand,
        IFormFile? logoFile)
    {
        // Kiểm tra tên thương hiệu đã tồn tại
        bool isDuplicate = await _context.Brands.AnyAsync(
            b => b.BrandName.ToLower() == brand.BrandName.Trim().ToLower()
                 && !b.IsDeleted
        );

        if (isDuplicate)
        {
            ModelState.AddModelError(
                nameof(brand.BrandName),
                "Tên thương hiệu này đã tồn tại."
            );
        }

        // Kiểm tra số lượng Hub
        if (brand.HubCount < 0 || brand.HubCount > 8)
        {
            ModelState.AddModelError(
                nameof(brand.HubCount),
                "Số Hub phải nằm trong khoảng từ 0 đến 8."
            );
        }

        // Logo là bắt buộc khi tạo thương hiệu
        if (logoFile == null || logoFile.Length == 0)
        {
            ModelState.AddModelError(
                "Logo",
                "Vui lòng tải lên logo thương hiệu."
            );
        }
        else
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
            string extension = Path.GetExtension(logoFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "Logo",
                    "Chỉ chấp nhận ảnh JPG, JPEG hoặc PNG."
                );
            }

            if (logoFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "Logo",
                    "Dung lượng ảnh không được vượt quá 5MB."
                );
            }
        }

        if (!ModelState.IsValid)
        {
            return View(brand);
        }

        // Tạo thư mục lưu ảnh nếu chưa tồn tại
        string uploadFolder = Path.Combine(
            _environment.WebRootPath,
            "image",
            "brands"
        );

        Directory.CreateDirectory(uploadFolder);

        // Đặt tên file mới để tránh trùng tên
        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(logoFile!.FileName)}";

        string filePath = Path.Combine(uploadFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await logoFile.CopyToAsync(stream);
        }

        // Lưu đường dẫn ảnh vào database
        brand.Logo = $"/image/brands/{fileName}";

        brand.BrandName = brand.BrandName.Trim();
        brand.Description = brand.Description?.Trim();
        brand.IsDeleted = false;
        brand.CreatedAt = DateTime.Now;
        brand.UpdatedAt = null;

        _context.Brands.Add(brand);

        try
        {
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Thêm thương hiệu thành công.";

            return RedirectToAction(nameof(Brands));
        }
        catch
        {
            // Nếu lưu database thất bại thì xóa ảnh vừa tải lên
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            ModelState.AddModelError(
                string.Empty,
                "Không thể lưu thương hiệu. Vui lòng thử lại."
            );

            return View(brand);
        }
    }



    [HttpGet]
    public async Task<IActionResult> EditBrand(int id)
    {
        var brand = await _context.Brands
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BrandId == id && !b.IsDeleted);

        if (brand == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy thương hiệu.";
            return RedirectToAction(nameof(Brands));
        }

        return View(brand);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBrand(
        int id,
        Brand brand,
        IFormFile? logoFile)
    {
        if (id != brand.BrandId)
        {
            return BadRequest();
        }

        var existingBrand = await _context.Brands
            .FirstOrDefaultAsync(b => b.BrandId == id && !b.IsDeleted);

        if (existingBrand == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy thương hiệu.";
            return RedirectToAction(nameof(Brands));
        }

        // Kiểm tra tên thương hiệu trùng với thương hiệu khác
        if (!string.IsNullOrWhiteSpace(brand.BrandName))
        {
            string normalizedName = brand.BrandName.Trim().ToLower();

            bool isDuplicate = await _context.Brands.AnyAsync(
                b => b.BrandId != id
                     && !b.IsDeleted
                     && b.BrandName.ToLower() == normalizedName
            );

            if (isDuplicate)
            {
                ModelState.AddModelError(
                    nameof(brand.BrandName),
                    "Tên thương hiệu này đã tồn tại."
                );
            }
        }

        // Kiểm tra số lượng Hub
        if (brand.HubCount < 0 || brand.HubCount > 8)
        {
            ModelState.AddModelError(
                nameof(brand.HubCount),
                "Số Hub phải nằm trong khoảng từ 0 đến 8."
            );
        }

        // Nếu người dùng chọn logo mới thì kiểm tra file
        if (logoFile != null && logoFile.Length > 0)
        {
            string extension = Path.GetExtension(logoFile.FileName)
                .ToLowerInvariant();

            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "Logo",
                    "Chỉ chấp nhận ảnh JPG, JPEG hoặc PNG."
                );
            }

            if (logoFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "Logo",
                    "Dung lượng ảnh không được vượt quá 5MB."
                );
            }
        }

        if (!ModelState.IsValid)
        {
            // Giữ logo hiện tại để hiển thị lại trên form
            brand.Logo = existingBrand.Logo;
            return View(brand);
        }

        string? newLogoPath = null;
        string? newLogoPhysicalPath = null;

        try
        {
            // Chỉ tải ảnh mới khi người dùng có chọn file
            if (logoFile != null && logoFile.Length > 0)
            {
                string uploadFolder = Path.Combine(
                    _environment.WebRootPath,
                    "image",
                    "brands"
                );

                Directory.CreateDirectory(uploadFolder);

                string fileName =
                    $"{Guid.NewGuid()}{Path.GetExtension(logoFile.FileName)}";

                newLogoPhysicalPath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(
                    newLogoPhysicalPath,
                    FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                newLogoPath = $"/image/brands/{fileName}";
            }

            // Cập nhật thông tin
            existingBrand.BrandName = brand.BrandName.Trim();
            existingBrand.Description = brand.Description?.Trim();
            existingBrand.HubCount = brand.HubCount;
            existingBrand.IsActive = brand.IsActive;
            existingBrand.UpdatedAt = DateTime.Now;

            // Thay logo nếu có ảnh mới
            if (newLogoPath != null)
            {
                existingBrand.Logo = newLogoPath;
            }

            await _context.SaveChangesAsync();

            // Xóa ảnh cũ sau khi database lưu thành công
            if (newLogoPath != null &&
                !string.IsNullOrWhiteSpace(brand.Logo))
            {
                // Không xóa ở đây vì brand.Logo từ form có thể là dữ liệu gửi lên.
                // Sử dụng đường dẫn logo cũ đã lưu trong database.
            }

            TempData["SuccessMessage"] =
                "Cập nhật thương hiệu thành công.";

            return RedirectToAction(nameof(Brands));
        }
        catch
        {
            // Xóa ảnh mới nếu quá trình cập nhật thất bại
            if (newLogoPhysicalPath != null &&
                System.IO.File.Exists(newLogoPhysicalPath))
            {
                System.IO.File.Delete(newLogoPhysicalPath);
            }

            ModelState.AddModelError(
                string.Empty,
                "Không thể cập nhật thương hiệu. Vui lòng thử lại."
            );

            brand.Logo = existingBrand.Logo;
            return View(brand);
        }
    }


    [HttpGet]
    public async Task<IActionResult> ViewBrand(int id)
    {
        var brand = await _context.Brands
            .AsNoTracking()
            .Include(b => b.Foods.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(b => b.BrandId == id && !b.IsDeleted);

        if (brand == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy thương hiệu.";
            return RedirectToAction(nameof(Brands));
        }

        return View(brand);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.BrandId == id && !b.IsDeleted);

        if (brand == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy thương hiệu cần xóa.";
            return RedirectToAction(nameof(Brands));
        }

        // Đánh dấu đã xóa mềm
        brand.IsDeleted = true;
        brand.IsActive = false;
        brand.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Đã xóa thương hiệu '{brand.BrandName}' thành công.";

        return RedirectToAction(nameof(Brands));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreBrand(int id)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.BrandId == id && !b.IsDeleted);

        if (brand == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy thương hiệu cần khôi phục.";

            return RedirectToAction(nameof(Brands));
        }

        if (brand.IsActive)
        {
            TempData["ErrorMessage"] =
                "Thương hiệu này đang hoạt động, không cần khôi phục.";

            return RedirectToAction(nameof(Brands));
        }

        brand.IsActive = true;
        brand.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Đã khôi phục thương hiệu '{brand.BrandName}' thành công.";

        return RedirectToAction(nameof(Brands));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PauseBrand(int id)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b =>
                b.BrandId == id &&
                !b.IsDeleted);

        if (brand == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy thương hiệu cần tạm dừng.";

            return RedirectToAction(nameof(Brands));
        }

        if (!brand.IsActive)
        {
            TempData["ErrorMessage"] =
                "Thương hiệu này đã ở trạng thái tạm dừng.";

            return RedirectToAction(nameof(Brands));
        }

        brand.IsActive = false;
        brand.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Đã tạm dừng thương hiệu '{brand.BrandName}' thành công.";

        return RedirectToAction(nameof(Brands));
    }
    // GET: Admin/DeletedBrands
    [HttpGet]
    public async Task<IActionResult> DeletedBrands()
    {
        var deletedBrands = await _context.Brands
            .AsNoTracking()
            .Include(b => b.Foods)
            .Where(b => b.IsDeleted)
            .OrderByDescending(b => b.UpdatedAt)
            .ToListAsync();

        return View(deletedBrands);
    }

    // ==========================================
    // UC-A: FOOD MANAGEMENT
    // ==========================================


    [HttpGet]
    public async Task<IActionResult> Foods()
    {
        // Lấy món ăn chưa bị xóa mềm
        var foods = await _context.Foods
            .Include(f => f.Brand)
            .Where(f => !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        // Lấy thương hiệu đang hoạt động để làm bộ lọc
        var brands = await _context.Brands
            .Where(b => !b.IsDeleted && b.IsActive)
            .OrderBy(b => b.BrandName)
            .ToListAsync();

        ViewBag.Brands = brands;

        return View(foods);
    }
    


    // ==========================================
    // UC-A: FOOD MANAGEMENT
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> CreateFood()
    {
        await LoadActiveBrands();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFood(Food model, IFormFile? imageFile)
    {
        // Lấy lại danh sách thương hiệu nếu cần hiển thị lại form
        await LoadActiveBrands();

        // Giữ lại dữ liệu người dùng đã nhập khi validation lỗi
        ViewData["FoodName"] = model.FoodName;
        ViewData["SelectedBrandId"] = model.BrandId;
        ViewData["SelectedCategory"] = model.Category;
        ViewData["Price"] = model.Price;
        ViewData["Description"] = model.Description;
        ViewData["IsAvailable"] = model.IsAvailable;

        // Kiểm tra thương hiệu có tồn tại và đang hoạt động
        bool validBrand = await _context.Brands.AnyAsync(
            b => b.BrandId == model.BrandId
                 && !b.IsDeleted
                 && b.IsActive);

        if (!validBrand)
        {
            ModelState.AddModelError(
                "BrandId",
                "Thương hiệu không tồn tại hoặc đang ngừng hoạt động.");
        }

        // Kiểm tra và lưu ảnh
        string? imagePath = null;

        if (imageFile != null && imageFile.Length > 0)
        {
            string[] allowedExtensions =
            {
            ".jpg", ".jpeg", ".png", ".webp"
        };

            string extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
            }

            if (imageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "Dung lượng ảnh không được vượt quá 5MB.");
            }

            if (ModelState.IsValid)
            {
                string uploadFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "foods");

                Directory.CreateDirectory(uploadFolder);

                string newFileName = $"{Guid.NewGuid()}{extension}";
                string fullPath = Path.Combine(uploadFolder, newFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                imagePath = $"/uploads/foods/{newFileName}";
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.ImageUrl = imagePath;
        model.IsDeleted = false;
        model.CreatedAt = DateTime.Now;
        model.UpdatedAt = null;

        _context.Foods.Add(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Thêm món ăn thành công!";

        return RedirectToAction(nameof(Foods));
    }

    // Hàm lấy thương hiệu đang hoạt động để đưa vào dropdown
    private async Task LoadActiveBrands()
    {
        ViewBag.Brands = await _context.Brands
            .Where(b => !b.IsDeleted && b.IsActive)
            .OrderBy(b => b.BrandName)
            .ToListAsync();
    }


    // ==========================================
    // UC-A: EDIT FOOD
    // ==========================================

    // GET: Hiển thị thông tin món ăn cần chỉnh sửa
    [HttpGet]
    public async Task<IActionResult> EditFood(int id)
    {
        var food = await _context.Foods
            .Include(f => f.Brand)
            .FirstOrDefaultAsync(f => f.FoodId == id && !f.IsDeleted);

        if (food == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy món ăn cần chỉnh sửa.";
            return RedirectToAction(nameof(Foods));
        }

        await LoadActiveBrands();

        return View(food);
    }


    // POST: Lưu thông tin món ăn đã chỉnh sửa
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFood(
        int id,
        Food model,
        IFormFile? imageFile)
    {
        // Tìm bản ghi hiện tại trong database
        var existingFood = await _context.Foods
            .FirstOrDefaultAsync(f => f.FoodId == id && !f.IsDeleted);

        if (existingFood == null)
        {
            TempData["ErrorMessage"] = "Món ăn không tồn tại hoặc đã bị xóa.";
            return RedirectToAction(nameof(Foods));
        }

        // Giữ nguyên ảnh cũ nếu người dùng không tải ảnh mới
        model.ImageUrl = existingFood.ImageUrl;

        // Kiểm tra thương hiệu
        bool validBrand = await _context.Brands.AnyAsync(
            b => b.BrandId == model.BrandId
                 && !b.IsDeleted
                 && b.IsActive);

        if (!validBrand)
        {
            ModelState.AddModelError(
                "BrandId",
                "Thương hiệu không tồn tại hoặc đang ngừng hoạt động.");
        }

        // Kiểm tra danh mục hợp lệ
        string[] allowedCategories =
        {
        "Món chính",
        "Món nước",
        "Ăn vặt",
        "Đồ uống"
    };

        if (!allowedCategories.Contains(model.Category))
        {
            ModelState.AddModelError(
                "Category",
                "Danh mục món ăn không hợp lệ.");
        }

        // Kiểm tra ảnh mới nếu người dùng chọn
        string? extension = null;

        if (imageFile != null && imageFile.Length > 0)
        {
            string[] allowedExtensions =
            {
            ".jpg", ".jpeg", ".png", ".webp"
        };

            extension = Path.GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
            }

            if (imageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "Dung lượng ảnh không được vượt quá 5MB.");
            }
        }

        // Nếu dữ liệu không hợp lệ, trả lại form
        if (!ModelState.IsValid)
        {
            await LoadActiveBrands();
            return View(model);
        }

        // Nếu có ảnh mới, lưu ảnh vào wwwroot/uploads/foods
        if (imageFile != null && imageFile.Length > 0)
        {
            string uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "foods");

            Directory.CreateDirectory(uploadFolder);

            string newFileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(uploadFolder, newFileName);

            using (var stream = new FileStream(
                fullPath,
                FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            model.ImageUrl = $"/uploads/foods/{newFileName}";
        }

        // Cập nhật các trường được phép thay đổi
        existingFood.FoodName = model.FoodName;
        existingFood.BrandId = model.BrandId;
        existingFood.Category = model.Category;
        existingFood.Price = model.Price;
        existingFood.Description = model.Description;
        existingFood.ImageUrl = model.ImageUrl;
        existingFood.IsAvailable = model.IsAvailable;
        existingFood.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật món ăn thành công!";

        return RedirectToAction(nameof(Foods));
    }


    [HttpGet]
    public async Task<IActionResult> ViewFood(int id)
    {
        var food = await _context.Foods
            .AsNoTracking()
            .Include(f => f.Brand)
            .FirstOrDefaultAsync(f => f.FoodId == id && !f.IsDeleted);

        if (food == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy món ăn.";
            return RedirectToAction(nameof(Foods));
        }

        return View(food);

    public IActionResult Brands()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CreateBrand()
    {
        return View();
    }

    [HttpGet]
    public IActionResult EditBrand(int id)
    {
        ViewBag.BrandId = id;
        return View();
    }

    [HttpGet]
    public IActionResult ViewBrand(int id)
    {
        ViewBag.BrandId = id;
        return View();
    }

    // ==========================================
    // UC-A: FOOD MANAGEMENT
    // ==========================================

    [HttpGet]
    public IActionResult Foods()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CreateFood()
    {
        return View();
    }

    [HttpGet]
    public IActionResult EditFood(int id)
    {
        ViewBag.FoodId = id;
        return View();
    }

    [HttpGet]
    public IActionResult ViewFood(int id)
    {
        ViewBag.FoodId = id;
        return View();

    }
}
