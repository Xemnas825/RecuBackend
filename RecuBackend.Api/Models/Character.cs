namespace RecuBackend.Api.Models;

public sealed class Character
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public string CharacterClass { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int ArmorClass { get; set; }
    public int HitPoints { get; set; }
    public int ProficiencyBonus { get; set; } = 2;
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;
    public int Initiative { get; set; }
    public int SpeedFeet { get; set; } = 30;
    public string Size { get; set; } = "Medium";
    public bool IsNpc { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Campaign Campaign { get; set; } = null!;
    public ICollection<RollLog> Rolls { get; set; } = new List<RollLog>();
    public ICollection<FileAttachment> Attachments { get; set; } = new List<FileAttachment>();
}
