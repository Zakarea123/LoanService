namespace LoanService.Models;

public class Loan
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public int BorrowerId { get; set; }

    public DateTime LoanDate { get; set; } = DateTime.UtcNow;

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public bool IsReturned { get; set; } = false;
}