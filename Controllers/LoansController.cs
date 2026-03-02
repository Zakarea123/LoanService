using LoanService.Data;
using LoanService.DTOs;
using LoanService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController :  ControllerBase
{
    private readonly LoanDbContext _db;
    public LoansController(LoanDbContext db) => _db = db;
    
}