using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Main : Node2D
{
	public static Main Instance;

	public static TrackerContext TrackerDbContext;
	public TextEdit EarliestDateText;
	private static DateTime DateParseResult;
	public static DayOfWeek WeekStartDay = DayOfWeek.Saturday;

	private static bool Reloading = false;
	




	// Called when the node enters the scene tree for the first time.
	public async override void _Ready()
	{
		Instance = this;

		EarliestDateText = GetNode<TextEdit>("CanvasLayer/EarliestDateLabel/TextEdit");

		if (!Reloading)
		{
			int diff = (7 + (DateTime.Now.DayOfWeek - WeekStartDay)) % 7;
			var WeekStart = DateTime.Now.AddDays(-1 * diff).Date;

			EarliestDateText.Text = WeekStart.ToShortDateString();
		}
		else
		{
			EarliestDateText.Text = DateParseResult.ToShortDateString();
		}

		// Create the database if it doesn't exist
		var Context = new TrackerContext();
		
		// Context.Database.EnsureDeletedAsync();
		await Context.Database.EnsureCreatedAsync();
		TrackerDbContext = Context;
		
		foreach (var Item in TrackerDbContext.Tasks)
		{
			// Filter the objects added from the database to only those after the cutoff
			DateTime ParseResult;
			DateTime.TryParse(EarliestDateText.Text, out ParseResult);

			if (Item.Date <= ParseResult)
				continue;

			AddNewTask(Item);
		}

		Reloading = false;
	}


	



	public override async void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			await TrackerDbContext.DisposeAsync();
			GetTree().Quit(); // default behavior
		}
	}




	public static GridContainer GetMainGrid()
	{
		var TheContainer = Instance.GetNode<GridContainer>("CanvasLayer/ScrollContainer/GridContainer");
		return TheContainer;
	}


	public async void ButtonAddNewTask()
	{
		WorkTask NewTask = new WorkTask() { JobNumber = "", TaskNumber = "", TimeElapsed = TimeSpan.FromSeconds(0)
		, Comments = "", Date = DateTime.Today};

		AddNewTask(NewTask);
	}


	public async static void AddNewTask(WorkTask TheTask)
	{
		// var TaskScene = ResourceLoader.Load<PackedScene>("res://TaskElement.tscn").Instantiate() as TaskScript;
		var LoadStatus = ResourceLoader.LoadThreadedRequest("res://TaskElement.tscn");
		GD.Print($"Load Status: {LoadStatus}");
		var TaskSceneResource = ResourceLoader.LoadThreadedGet("res://TaskElement.tscn") as PackedScene;
		GD.Print($"Scene resource: {TaskSceneResource}");
		var TaskScene = TaskSceneResource.Instantiate();
		
		GD.Print($"Scene: {TaskScene}");

		var Grid = Instance.GetNode<GridContainer>("CanvasLayer/ScrollContainer/GridContainer");
		Grid.AddChild(TaskScene);

		// await ToSignal(TaskScene, "ready");
		var JobField = TaskScene.GetNode<TextEdit>("MarginContainer/HBoxContainer/JobHBox/TextEdit");
		var TaskField = TaskScene.GetNode<TextEdit>("MarginContainer/HBoxContainer/TaskHBox/TextEdit");
		var TimeField = TaskScene.GetNode<Label>("MarginContainer/HBoxContainer/TimeHBox/TimeText");
		var CommentField = TaskScene.GetNode<TextEdit>("MarginContainer/HBoxContainer/CommentsHBox/CommentsText");
		var DateField = TaskScene.GetNode<Label>("MarginContainer/HBoxContainer/DateHBox/DateText");

		JobField.Text = TheTask.JobNumber;
		TaskField.Text = TheTask.TaskNumber;
		TimeField.Text = TheTask.TimeElapsed.ToString(@"hh\:mm\:ss");
		CommentField.Text = TheTask.Comments;
		DateField.Text = TheTask.Date.ToShortDateString();

		GD.Print($"GUI Object from method: {TaskScene}");
	}


	public async void RemoveSelectedTasks(bool DeleteDatabaseRecords)
	{
		foreach (var TaskItem in GetNode<GridContainer>("CanvasLayer/ScrollContainer/GridContainer").GetChildren())
		{
			var Checked = TaskItem.GetNode<CheckBox>("MarginContainer/HBoxContainer/CheckBox").ButtonPressed;
			
			if (Checked)
			{
				var jobNumber = TaskItem.GetNode<TextEdit>("MarginContainer/HBoxContainer/JobHBox/TextEdit").Text.Trim();
				var taskNumber = TaskItem.GetNode<TextEdit>("MarginContainer/HBoxContainer/TaskHBox/TextEdit").Text.Trim();

				if (DeleteDatabaseRecords)
					DeleteTaskFromDatabase(jobNumber, taskNumber);

				TaskItem.QueueFree();
			}
		}
	}



	public async void DeleteTaskFromDatabase(string jobNumber, string taskNumber)
	{
		var DbItem = TrackerDbContext.Tasks.Where(x => x.JobNumber == jobNumber && x.TaskNumber == taskNumber).FirstOrDefault();
				
		if (DbItem is not null)
		{
			TrackerDbContext.Remove(DbItem);
			await TrackerDbContext.SaveChangesAsync();
		}
	}



	public void CheckDate()
	{
		DateTime.TryParse(EarliestDateText.Text, out DateParseResult);
		// GD.Print($"Date Parse Result: {DateParseResult}");

		if (DateParseResult == DateTime.MinValue)
		{
			GD.Print("Date Parse FAIL!");
			EarliestDateText.Set("background_color", new Color(1,0f,0f, 1f));
		}

		// EarliestDateText.Text = DateParseResult.ToString();
	}


	public async void ReloadGrid()
	{
		Reloading = true;
		CheckDate();
		GetTree().ChangeSceneToFile("Main.tscn");
	}

}
