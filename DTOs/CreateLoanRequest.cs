namespace LoanService.DTOs;

public class CreateLoanRequest
{
    public int ItemId { get; set; }
    public int BorrowerId { get; set; }
    public DateTime DueDate { get; set; }
}