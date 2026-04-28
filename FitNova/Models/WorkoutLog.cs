public class WorkoutLog
{
    public int Id { get; set; }

    // Identificativo dell'utente che esegue l'allenamento
    public string UserId { get; set; }

    // La data dell'allenamento
    public DateTime Date { get; set; }

    // Note opzionali (es. "Oggi sessione intensa")
    public string? Notes { get; set; }

    // Se la tua tabella admin richiede anche i minuti o il tipo:
    // public int DurationMinutes { get; set; }
}