using System.ComponentModel.DataAnnotations;

namespace wt_lab7_7postgres_efcore_denisenko.Models;

/// <summary>Поля, доступные клиенту при создании или изменении профиля.</summary>
public sealed class PlayerRequest
{
    [Required(ErrorMessage = "Введите никнейм игрока.")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Никнейм должен содержать от 2 до 30 символов.")]
    public string Nickname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите название игры.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Название игры должно содержать от 2 до 60 символов.")]
    public string Game { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "Название команды не должно превышать 60 символов.")]
    public string TeamName { get; set; } = "Без команды";

    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов.")]
    public string Description { get; set; } = string.Empty;

    [Range(0, 5000, ErrorMessage = "Рейтинг должен находиться в диапазоне от 0 до 5000.")]
    public int Rating { get; set; }

    [Range(0, 100000, ErrorMessage = "Количество побед должно быть от 0 до 100000.")]
    public int Wins { get; set; }

    [Range(0, 100000, ErrorMessage = "Количество поражений должно быть от 0 до 100000.")]
    public int Losses { get; set; }
}
