namespace LoanService.DTOs;

public class LoanResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int BorrowerId { get; set; } 
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned { get; set; }
}