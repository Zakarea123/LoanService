using LoanService.Models;

namespace LoanService.Data;
using Microsoft.EntityFrameworkCore;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options) { }

    public DbSet<Loan> Loans => Set<Loan>();
}