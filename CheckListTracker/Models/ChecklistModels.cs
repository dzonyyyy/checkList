namespace CheckListTracker.Models;

public class ChecklistTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Technology { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<TemplateSection> Sections { get; set; } = [];
}

public class TemplateSection
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public List<TemplateItem> Items { get; set; } = [];
}

public class TemplateItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = "";
    public string Reference { get; set; } = "";
    public bool IsRequired { get; set; } = true;
    public int Order { get; set; }
}

public class ChecklistExecution
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TemplateId { get; set; } = "";
    public string TemplateName { get; set; } = "";
    public string Technology { get; set; } = "";
    public string MachineName { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Location { get; set; } = "";
    public string Inspector { get; set; } = "";
    public string Customer { get; set; } = "";
    public string ProjectNumber { get; set; } = "";
    public ChecklistType Type { get; set; } = ChecklistType.FAT;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? CompletedDate { get; set; }
    public ExecutionStatus Status { get; set; } = ExecutionStatus.InProgress;
    public string GeneralNotes { get; set; } = "";
    public List<TemplateSection> Sections { get; set; } = [];
    public List<ItemResult> Results { get; set; } = [];
}

public enum ChecklistType { FAT, SAT }
public enum ExecutionStatus { InProgress, Completed, OnHold }
public enum ItemStatus { Pending, Pass, Fail, NA, Observation }

public class ItemResult
{
    public string ItemId { get; set; } = "";
    public ItemStatus Status { get; set; } = ItemStatus.Pending;
    public string Notes { get; set; } = "";
    public List<string> PhotoDataUrls { get; set; } = [];
}
