namespace LoanService.DTOs;

public class CreateLoanRequest
{
    public int BookId { get; set; }
    public int BorrowerId { get; set; }
    public DateTime DueDate { get; set; }
}