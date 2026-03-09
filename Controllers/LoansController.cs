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
    
    
    // READ: Get active loans for a specified borrower
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
                ItemId = l.ItemId,
                BorrowerId = l.BorrowerId,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                IsReturned = l.IsReturned
            })
            .ToListAsync();

        return Ok(loans);
    }
    
    
    
    // READ: Get loan by ID
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanResponse>> GetLoanById(int id)
    {
        var loan = await _db.Loans.FindAsync(id);
        if (loan is null) return NotFound();

        return Ok(new LoanResponse
        {
            Id = loan.Id,
            ItemId = loan.ItemId,
            BorrowerId = loan.BorrowerId,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            IsReturned = loan.IsReturned
        });
    }
    
    
    
    // READ: Retrieves the history of all previous loans for the user
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
                ItemId = l.ItemId,
                BorrowerId = l.BorrowerId,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                IsReturned = l.IsReturned
            })
            .ToListAsync();

        return Ok(loans);
    }
    
    // READ: Get all active loans (used by frontend for availability check and NotificationService for reminders)
    [HttpGet("active")]
    public async Task<ActionResult<List<LoanResponse>>> GetActiveLoans()
    {
        var loans = await _db.Loans
            .Where(l => !l.IsReturned)
            .OrderBy(l => l.DueDate)
            .Select(l => new LoanResponse
            {
                Id = l.Id,
                ItemId = l.ItemId,
                BorrowerId = l.BorrowerId,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                IsReturned = l.IsReturned
            })
            .ToListAsync();

        return Ok(loans);
    }

    
    
    // CREATE: Borrow a book (Create a new loan)
    [HttpPost]
    public async Task<ActionResult<LoanResponse>> CreateLoan(CreateLoanRequest request)
    {
        if (request.ItemId <= 0) return BadRequest("ItemId måste vara > 0.");
        if (request.BorrowerId <= 0) return BadRequest("BorrowerId måste vara större än 0.");

        // stoppa dubbelutlåning av samma ItemId
        var alreadyLoaned = await _db.Loans.AnyAsync(l => l.ItemId == request.ItemId && !l.IsReturned);
        if (alreadyLoaned) return Conflict("Boken är redan utlånad.");

        var loan = new Loan
        {
            ItemId = request.ItemId,
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
            ItemId = loan.ItemId,
            BorrowerId = loan.BorrowerId,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            IsReturned = loan.IsReturned
        };

        return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, response);
    }
    
    
    
    // UPDATE: Return a book (Change the loan status)
    [HttpPut("{id:int}/return")]
    public async Task<IActionResult> ReturnLoan(int id)
    {
        var loan = await _db.Loans.FindAsync(id);
        if (loan is null) return NotFound();
        if (loan.IsReturned) return BadRequest("Lånet är redan återlämnat.");

        loan.IsReturned = true;
        loan.ReturnDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }
    
    
    
    // DELETE: Delete a Loan (Remove from database table)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var loan = await _db.Loans.FindAsync(id);
        if (loan is null) return NotFound();

        _db.Loans.Remove(loan);
        await _db.SaveChangesAsync();

        return NoContent();
    }

}