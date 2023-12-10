using Godot;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


public partial class TaskScript : HBoxContainer
{
	public TextEdit JobNumberField { get; set; }
	public TextEdit TaskNumberField { get; set; }
	public Label TimeText { get; set; }
	public TextEdit CommentsField { get; set; }
	public Label DateField { get; set; }

	

	private Timer SecondTimer;

	private double TimeElapsed = 0;
	private bool Elapsing = false;


	

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		TimeText = GetNode<Label>("TimeHBox/TimeText");
		SecondTimer = GetNode<Timer>("SecondTimer");

		JobNumberField = GetNode<TextEdit>("JobHBox/TextEdit");
		TaskNumberField = GetNode<TextEdit>("TaskHBox/TextEdit");
		CommentsField = GetNode<TextEdit>("CommentsHBox/CommentsText");
		DateField = GetNode<Label>("DateHBox/DateText");
	}



	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Elapsing)
		{
			if (SecondTimer.IsStopped())
				SecondTimer.Start();
			TimeText.Text = TimeSpan.FromSeconds(TimeElapsed).ToString(@"hh\:mm\:ss");
			TimeElapsed += delta;
		}
	}


	public async void StartButton()
	{
		Elapsing = true;

		var jobNumber = GetNode<TextEdit>("JobHBox/TextEdit").Text.Trim();
		var taskNumber = GetNode<TextEdit>("TaskHBox/TextEdit").Text.Trim();
		var timeElapsed = GetNode<Label>("TimeHBox/TimeText").Text.Trim();
		var comments = GetNode<TextEdit>("CommentsHBox/CommentsText").Text.Trim();

		if (jobNumber is null || taskNumber is null)
			return;

		var DbItem = GetTask(jobNumber, taskNumber);

		if (DbItem is null)
		{
			// Add to the database
			AddTask(jobNumber, taskNumber, "0");
			return;
		}

		// Check if we're on a different day.  If so, create an new one.
		if (DbItem.Date.ToShortDateString() != DateTime.Now.ToShortDateString())
		{
			var AnotherTask = await AddTask(jobNumber, taskNumber, "0");
			Main.AddNewTask(AnotherTask);
			StopButton();
			return;
		}
		
	
		TimeElapsed =  TimeSpan.Parse(timeElapsed).TotalSeconds;
		UpdateTask(jobNumber, taskNumber, timeElapsed, comments, DateTime.Today);
		
	}


	public void StopButton()
	{
		Elapsing = false;
	}


	public void UpdateTime()
	{
		var jobNumber = JobNumberField.Text.Trim();
		var taskNumber = TaskNumberField.Text.Trim();
		var timeElapsed = TimeText.Text.Trim();
		var comments = CommentsField.Text.Trim();
		
		// Update the database
		UpdateTask(jobNumber, taskNumber, timeElapsed, comments, DateTime.Today);
		GD.Print($"Current date/time: {DateTime.Now}");
		SecondTimer.Start();	
	}


	private async Task<WorkTask> AddTask(string jobNumber, string taskNumber, string timeElapsed)
	{
		WorkTask NewTask = new WorkTask() { JobNumber = jobNumber, TaskNumber = taskNumber, TimeElapsed = TimeSpan.Parse(timeElapsed)
		, Comments = "", Date = DateTime.Today};
		Main.TrackerDbContext.Tasks.Add(NewTask);
		await Main.TrackerDbContext.SaveChangesAsync();

		return NewTask;
	}


	private WorkTask GetTask(string jobNumber, string taskNumber)
	{
		var Task = Main.TrackerDbContext.Tasks.Where(x => x.JobNumber == jobNumber && x.TaskNumber == taskNumber).FirstOrDefault();
		GD.Print($"Returning Task: {Task}");

		return Task;
	}



	/// <summary>
	/// Update the variables that store the values as well as the SQLite database
	/// </summary>
	/// <param name="jobNumber"></param>
	/// <param name="taskNumber"></param>
	/// <param name="NewTime"></param>
	/// <param name="Comments"></param>
	/// <param name="Date"></param>
	private async void UpdateTask(string jobNumber, string taskNumber, string NewTime, string Comments, DateTime Date)
	{
		var DbItem = GetTask(jobNumber, taskNumber);
		if (DbItem is not null)
		{
			GD.Print($"Updating database...");
			DbItem.TimeElapsed = TimeSpan.Parse(NewTime);
			DbItem.Comments = Comments;

			DbItem.Date = Date;

			Main.TrackerDbContext.Tasks.Update(DbItem);
			await Main.TrackerDbContext.SaveChangesAsync();
		}
	}
}
