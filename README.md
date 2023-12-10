Application to track time spent doing various tasks.
Built using the Godot game engine and C#.

![image](https://github.com/GeneralProtectionFault/Time-Tracker/assets/29645865/0ff993ca-1f5c-4819-9f1b-85ac2858a518)

All data is stored in the TimeTracker.db SQLite database (file).

Earliest Date to Display will default to the previous Saturday.  This automatically keeps the default scope to the current week.
Changing and clicking the reload button to the left will show the corresponding records.
The day of the week can be modified by the WeekStartDay variable at the top of the Main.cs script.

* The date (right-most field) column automatically fills in the current date.  If a task is running, or started the following day, a new "line" will automatically be created.
  This separates daily totals.
