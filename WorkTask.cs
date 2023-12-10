using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public class WorkTask
{
    // [Key]
    public string JobNumber { get; set; }
    public string TaskNumber { get; set; }
    
    [DataType(DataType.Time)]
    public TimeSpan TimeElapsed { get; set; }
    public string Comments { get; set; }
    
    [DataType(DataType.Date)]
    // [Column(TypeName = "Date")]
    public DateTime Date { get; set; }
}
