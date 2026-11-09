using System.ComponentModel.DataAnnotations.Schema;

namespace wt_lab7_7postgres_efcore_denisenko.Models;

/// <summary>Профиль участника киберспортивного сервиса Tournament Hub.</summary>
public sealed class Player
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string Game { get; set; } = string.Empty;
    public int Rating { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Название команды; если игрок не состоит в команде, используется значение «Без команды».</summary>
    public string TeamName { get; set; } = "Без команды";

    /// <summary>Краткое описание профиля игрока (до 500 символов).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Процент побед. Вычисляемое свойство не хранится в PostgreSQL.</summary>
    [NotMapped]
    public double WinRate => Wins + Losses == 0 ? 0 : Math.Round(Wins * 100.0 / (Wins + Losses), 2);
}
