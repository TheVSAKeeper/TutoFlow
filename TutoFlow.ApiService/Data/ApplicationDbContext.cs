using Microsoft.EntityFrameworkCore;

namespace TutoFlow.ApiService.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options);
