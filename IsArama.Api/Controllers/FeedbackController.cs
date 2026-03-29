using IsArama.Data.Context;
using IsArama.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IsArama.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public FeedbackController(ApplicationDbContext db) => _db = db;

    public record FeedbackRequest(
        string? Name,
        string? Email,
        string Subject,
        string Message,
        string? Platform);

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] FeedbackRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Subject) || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest("Konu ve mesaj zorunludur.");

        if (req.Message.Length > 2000)
            return BadRequest("Mesaj en fazla 2000 karakter olabilir.");

        var feedback = new Feedback
        {
            Name     = req.Name?.Trim(),
            Email    = req.Email?.Trim(),
            Subject  = req.Subject.Trim(),
            Message  = req.Message.Trim(),
            Platform = req.Platform ?? "Web",
            CreatedAt = DateTime.UtcNow
        };

        _db.Feedbacks.Add(feedback);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Geri bildiriminiz alındı, teşekkürler!" });
    }
}
