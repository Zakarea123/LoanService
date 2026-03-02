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
    
    
    
     // READ: aktiva lån för en specifik borower
    [HttpGet("borrower/{borrowerId:int}")]
    public async Task<ActionResult<List<LoanResponse>>> GetActiveLoansForBorrower(int borrowerId)
    {
        if (borrowerId <= 0) return BadRequest("BorrowerId måste vara större än 0.");

        var loans = await _db.Loans
            .Where(l => l.BorrowerId == borrowerId && !l.IsReturned)
            .OrderByDescending(l => l.LoanDate)
            .Select(l => new LoanResponse
            {
                Id = l.Id,
                BookId = l.BookId,
                BorrowerId = l.BorrowerId,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                IsReturned = l.IsReturned
            })
            .ToListAsync();

        return Ok(loans);
    }
    
    
    
    // READ: hämta lån via id
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanResponse>> GetLoanById(int id)
    {
        var loan = await _db.Loans.FindAsync(id);
        if (loan is null) return NotFound();

        return Ok(new LoanResponse
        {
            Id = loan.Id,
            BookId = loan.BookId,
            BorrowerId = loan.BorrowerId,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            IsReturned = loan.IsReturned
        });
    }
    
    
    
    // READ: Hämtar historiken för alla gamla utlåningar hos inloggade användaren
    [HttpGet("borrower/{borrowerId:int}/history")]
    public async Task<ActionResult<List<LoanResponse>>> GetLoanHistoryForBorrower(int borrowerId)
    {
        if (borrowerId <= 0) return BadRequest("BorrowerId måste vara större än 0.");

        var loans = await _db.Loans
            .Where(l => l.BorrowerId == borrowerId && l.IsReturned)
            .OrderByDescending(l => l.ReturnDate ?? l.LoanDate)
            .Select(l => new LoanResponse
            {
                Id = l.Id,
                BookId = l.BookId,
                BorrowerId = l.BorrowerId,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                IsReturned = l.IsReturned
            })
            .ToListAsync();

        return Ok(loans);
    }
    
    
    
    // CREATE: låna bok (skapa nytt lån)
    [HttpPost]
    public async Task<ActionResult<LoanResponse>> CreateLoan(CreateLoanRequest request)
    {
        if (request.BookId <= 0) return BadRequest("BookId måste vara > 0.");
        if (request.BorrowerId <= 0) return BadRequest("BorrowerId måste vara större än 0.");

        // stoppa dubbelutlåning av samma BookId
        var alreadyLoaned = await _db.Loans.AnyAsync(l => l.BookId == request.BookId && !l.IsReturned);
        if (alreadyLoaned) return Conflict("Boken är redan utlånad.");

        var loan = new Loan
        {
            BookId = request.BookId,
            BorrowerId = request.BorrowerId,
            LoanDate = DateTime.UtcNow,
            DueDate = request.DueDate.ToUniversalTime(),
            IsReturned = false
        };
        
        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();
        
        var response = new LoanResponse
        {
            Id = loan.Id,
            BookId = loan.BookId,
            BorrowerId = loan.BorrowerId,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            IsReturned = loan.IsReturned
        };

        return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, response);
    }
    
}