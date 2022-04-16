using Microsoft.Data.Sqlite;
using System;
using System.Threading;

namespace Habit_Tracker
{
    class Program
    {
        // Public Release version 1
        static void Main(string[] args)
        {
            #region Connection and Create table
            // Create an instance of the Connection class
            Connection conn = new Connection();

            // Create an instance of the SQLiteConnection class and store the connection
            SqliteConnection connection = conn.CreateConnection();

            // TESTING: this allows me to drop the table if I make changes.
            //TestingDropTable(connection);

            // Create the table schema
            string CreateTableString =
                @"CREATE TABLE IF NOT EXISTS Habits 
                    (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Created_At DEFAULT CURRENT_TIMESTAMP,
                    Last_Updated TEXT,
                    HabitName TEXT,
                    HabitDuration INTEGER)";

            // Create the command to create the table
            SqliteCommand CreateTableCommand = new SqliteCommand(CreateTableString, connection);

            // Execute the Query
            CreateTableCommand.ExecuteNonQuery();
            #endregion

            #region Main Menu
            bool loop = true;

            // moving this part out of the while loop for looks
            Console.WriteLine();
            Console.WriteLine("Habit Tracker v1");
            Console.WriteLine();

            // While loop so I have a way to quit
            while (loop)
            {
                Console.WriteLine();
                Console.WriteLine("add | view | update | delete | quit");
                Console.WriteLine();
                Console.Write("Enter your choice: ");
                string mainMenuChoice = Console.ReadLine();

                switch (mainMenuChoice.ToLower())
                {
                    case "add":
                        // Clear console
                        Console.Clear();
                        // Storing prompt in string so it can be passed optionally to validation
                        string addHabitNamePrompt = "Enter habit to track: ";
                        Console.Write(addHabitNamePrompt);
                        string addHabitNameInput = Console.ReadLine();
                        // Validate the string before calling the add method, including optional prompt
                        string addHabitNameResult = StringValidator(addHabitNameInput, addHabitNamePrompt);
                        // Storing prompt for duration
                        string addHabitDurationPrompt = "Enter duration in minutes: ";
                        Console.Write(addHabitDurationPrompt);
                        string addHabitDurationInput = Console.ReadLine();
                        // Validate that the duration is greater than zero
                        int addHabitDurationResult = IntValidator(addHabitDurationInput, addHabitDurationPrompt);
                        // Add to the Database
                        AddHabit(connection, addHabitNameResult, addHabitDurationResult);
                        // Use the ternary operator to determine the plural of minutes if necessary
                        // and you can use string interpolation within the ternary operator conditions.
                        Console.WriteLine($"{addHabitNameResult} habit with a duration of {(addHabitDurationResult > 1 ? $"{addHabitDurationResult} minutes" : $"{addHabitDurationResult} minute")} has been added.");
                        Console.WriteLine();
                        break;
                    case "view":
                        ReadHabits(connection);
                        break;
                    case "update":
                        Console.Clear();
                        UpdateHabits(connection);
                        break;
                    case "delete":
                        DeleteHabits(connection);
                        break;
                    case "quit":
                        loop = false;
                        break;
                    default:
                        Console.WriteLine("Please choose one of the available options.");
                        break;
                }
            }




            #endregion


            // Closing the connection at the end
            connection.Close();
        }

        #region CRUD
        // Add Habits
        static void AddHabit(SqliteConnection conn, string addHabitName, int addHabitDuration)
        {
            // Create the command
            var addHabitCommand = conn.CreateCommand();
            // Create a timestamp for the update
            DateTime addHabitUpdateDateTime = DateTime.Now;
            // Create the SQL statement
            addHabitCommand.CommandText = "INSERT INTO Habits (HabitName, HabitDuration, Last_Updated) VALUES (@Name, @Duration, @LastUpdated)";
            // Add all of the parameter assignments
            addHabitCommand.Parameters.AddWithValue("@Name", addHabitName);
            addHabitCommand.Parameters.AddWithValue("@Duration", addHabitDuration);
            addHabitCommand.Parameters.AddWithValue("@LastUpdated", addHabitUpdateDateTime);
            // Execute the query
            addHabitCommand.ExecuteNonQuery();
        }

        // Read Habits
        static void ReadHabits(SqliteConnection conn)
        {
            SqliteCommand command = conn.CreateCommand();
            command.CommandText = "SELECT * FROM Habits";
            SqliteDataReader dataReader = command.ExecuteReader();

            // Refactored display code to a function for uniformity
            displayListOfHabits(dataReader);
        }

        // Update Habits

        static void UpdateHabits(SqliteConnection conn)
        {
            bool mainUpdateLoop = true;
            while (mainUpdateLoop)
            {
                // Clear the console to load the update interface
                Console.Clear();
                Console.WriteLine("search | main");
                Console.WriteLine();
                Console.Write("Enter your choice: ");
                string outerUpdateMenu = Console.ReadLine();
                switch (outerUpdateMenu.ToLower())
                {
                    case "search":
                        (long resultCount, string updateHabitNameResult) = searchForHabits(conn);

                        if (resultCount >= 1)
                        {
                            // Create a query for getting all possible matches
                            var updateReadCommand = conn.CreateCommand();
                            updateReadCommand.CommandText = "SELECT * FROM Habits WHERE HabitName LIKE @Name";
                            updateReadCommand.Parameters.AddWithValue("@Name", "%" + updateHabitNameResult + "%");
                            SqliteDataReader reader = updateReadCommand.ExecuteReader();

                            // Refactored display code to a function for uniformity
                            displayListOfHabits(reader);

                            // Begin update while loop

                            bool updateLoop = true;
                            while (updateLoop)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Update Options: name | duration | both | main");
                                Console.WriteLine("");
                                Console.Write("Enter your choice: ");
                                string loopMenuChoice = Console.ReadLine();
                                switch (loopMenuChoice.ToLower())
                                {
                                    case "name":
                                        int searchIdValidated = AskForID();
                                        Console.Write("Enter the new name of the Habit: ");
                                        string searchName = Console.ReadLine();
                                        string searchNameValidated = StringValidator(searchName, "Enter the new name of the Habit: ");
                                        var searchCommand = conn.CreateCommand();
                                        searchCommand.CommandText = "UPDATE Habits SET HabitName = @updateName, Last_Updated = @updateDate WHERE Id=@id";
                                        searchCommand.Parameters.AddWithValue("@updateName", searchNameValidated);
                                        searchCommand.Parameters.AddWithValue("@updateDate", DateTime.Now);
                                        searchCommand.Parameters.AddWithValue("@id", searchIdValidated);
                                        searchCommand.ExecuteNonQuery();
                                        ReadHabits(conn);
                                        break;

                                    case "duration":
                                        int durationSearchIdValidated = AskForID();
                                        Console.Write("Enter the new duration of the Habit: ");
                                        string durationSearchName = Console.ReadLine();
                                        int durationValidated = IntValidator(durationSearchName, "Enter the new duration of the Habit: ");
                                        var durationCommand = conn.CreateCommand();
                                        durationCommand.CommandText = "UPDATE Habits SET HabitDuration = @updateDuration, Last_Updated = @durationUpdateDate WHERE Id=@id";
                                        durationCommand.Parameters.AddWithValue("@updateDuration", durationValidated);
                                        durationCommand.Parameters.AddWithValue("@durationUpdateDate", DateTime.Now);
                                        durationCommand.Parameters.AddWithValue("@id", durationSearchIdValidated);
                                        durationCommand.ExecuteNonQuery();
                                        ReadHabits(conn);
                                        break;

                                    case "both":
                                        int bothSearchIdValidated = AskForID();
                                        // Validate name first
                                        Console.Write("Enter the new Habit name: ");
                                        string bothNameUpdateName = Console.ReadLine();
                                        string bothNameUpdateValidated = StringValidator(bothNameUpdateName, "Enter the new Habit name: ");
                                        // validate duration next
                                        Console.Write("Enter the new Habit duration: ");
                                        string bothDurationUpdateDuration = Console.ReadLine();
                                        int bothDurationUpdateValidated = IntValidator(bothDurationUpdateDuration, "Enter the new Habit duration: ");
                                        // Create the update
                                        var bothUpdateCommand = conn.CreateCommand();
                                        bothUpdateCommand.CommandText = "UPDATE Habits SET HabitName = @bothName, HabitDuration = @bothDuration, Last_Updated = @bothUpdateDate WHERE Id=@id";
                                        bothUpdateCommand.Parameters.AddWithValue("@bothName", bothNameUpdateValidated);
                                        bothUpdateCommand.Parameters.AddWithValue("@bothDuration", bothDurationUpdateValidated);
                                        bothUpdateCommand.Parameters.AddWithValue("@bothUpdateDate", DateTime.Now);
                                        bothUpdateCommand.Parameters.AddWithValue("@id", bothSearchIdValidated);
                                        bothUpdateCommand.ExecuteNonQuery();
                                        ReadHabits(conn);
                                        break;

                                    case "main":
                                        updateLoop = false;
                                        Console.Clear();
                                        break;

                                    default:
                                        Console.WriteLine("Please choose one of the available options.");
                                        break;

                                }
                            }

                        }
                        else
                        {
                            Console.WriteLine("No habits match the search term.");
                            Console.WriteLine("Type any key to continue...");
                            Console.ReadKey();
                        }
                        break;
                    case "main":
                        mainUpdateLoop = false;
                        Console.Clear();
                        break;
                    default:
                        Console.WriteLine("Please choose one of the available options.");
                        Console.ReadLine();
                        break;
                }


            }

        }

        static void DeleteHabits(SqliteConnection conn)
        {
            bool mainDeleteLoop = true;
            while (mainDeleteLoop)
            {
                // Clear the console to load the update interface
                Console.Clear();
                Console.WriteLine("delete | main");
                Console.WriteLine();
                Console.Write("Enter your choice: ");
                string outerDeleteMenu = Console.ReadLine();
                switch (outerDeleteMenu.ToLower())
                {
                    case "delete":
                        (long resultCount, string deleteHabitNameResult) = searchForHabits(conn);
                        var deleteReadCommand = conn.CreateCommand();
                        deleteReadCommand.CommandText = "SELECT * FROM Habits WHERE HabitName LIKE @Name";
                        deleteReadCommand.Parameters.AddWithValue("@Name", "%" + deleteHabitNameResult + "%");
                        SqliteDataReader reader = deleteReadCommand.ExecuteReader();
                        displayListOfHabits(reader);
                        int deleteIdValidated = AskForID();
                        Console.WriteLine($"Are you sure that you want to delete Habit {deleteIdValidated}? yes/no");
                        string deleteOption = Console.ReadLine().ToLower();
                        switch (deleteOption)
                        {
                            case "yes":
                                var deleteCommand = conn.CreateCommand();
                                deleteCommand.CommandText = "DELETE FROM Habits WHERE ID=@id";
                                deleteCommand.Parameters.AddWithValue("@id", deleteIdValidated);
                                deleteCommand.ExecuteNonQuery();
                                ReadHabits(conn);
                                Console.WriteLine($"Habit {deleteIdValidated} has been removed.");
                                Console.Write("Hit any key to continue...");
                                Console.ReadKey();
                                break;
                            case "no":
                                break;
                            default:
                                Console.WriteLine("Please choose one of the available options.");
                                break;
                        }
                        break;

                    case "main":
                        mainDeleteLoop = false;
                        Console.Clear();
                        break;

                    default:
                        Console.WriteLine("Please choose one of the available options.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        #endregion

        #region Validation
        // String Validator
        static string StringValidator(string inputItem, string optionalPrompt = "")
        {
            while (string.IsNullOrEmpty(inputItem.Trim()))
            {
                Console.WriteLine("The entry cannot be empty.");
                Console.WriteLine();
                Console.Write($"{optionalPrompt}");
                inputItem = Console.ReadLine();
            }
            return inputItem;
        }

        // Int Validator
        static int IntValidator(string inputItem, string optionalPrompt = "")
        {
            // Readline is sending in a string, but we want to return an Int
            int amount;
            while (!(int.TryParse(inputItem, out amount) && amount >= 1))
            {
                Console.WriteLine("Duration must be a whole number greater than zero.");
                Console.WriteLine();
                Console.Write($"{optionalPrompt}");
                inputItem = Console.ReadLine();
            }
            return amount;
        }
        #endregion

        static void displayListOfHabits(SqliteDataReader dataReader)
        {
            // a counter variable
            int counter = 0, totalDuration = 0;
            // Header row
            Console.WriteLine();
            Console.WriteLine("ID".PadRight(6) + "Habit".PadRight(31) + "Duration");
            Console.WriteLine("-".PadRight(60, '-'));
            while (dataReader.Read())
            {
                // To preserve column formatting, I will truncate strings over a certain size
                string rawOutput = dataReader.GetString(3);
                if (rawOutput.Length > 25)
                {
                    rawOutput = rawOutput.Substring(0, 25) + "...";
                }
                string output = $"{dataReader[0].ToString().PadRight(5)} { rawOutput.PadRight(30) } {(dataReader.GetInt32(4) > 1 ? $"{dataReader.GetInt32(4).ToString().PadLeft(3)} minutes" : $"{dataReader.GetInt32(4).ToString().PadLeft(3)} minute")}";
                Console.WriteLine(output);
                counter++;
                totalDuration += dataReader.GetInt32(4);
            }
            // Totals row
            Console.WriteLine();
            Console.WriteLine($"Total Habits: {counter.ToString().PadRight(5)} Total Duration: {(totalDuration > 1 ? $"{totalDuration} minutes" : $"{totalDuration} minute")}");
        }

        static int AskForID()
        {
            // TO DO: should add some validation here that makes sure that the Id is within the number of habits.
            Console.Write("Enter the ID of the Habit: ");
            string durationSearchId = Console.ReadLine();
            int cleanedId = IntValidator(durationSearchId, "Enter the ID of the Habit: ");
            return cleanedId;
        }

        static (long, string) searchForHabits(SqliteConnection conn)
        {
            // Present the search interface
            // Storing prompt in string so it can be passed optionally to validation
            Console.Clear();
            Console.WriteLine();
            string HabitNamePrompt = "Enter habit to search: ";
            Console.Write(HabitNamePrompt);
            string HabitNameInput = Console.ReadLine();
            // Validate the string before continuing
            string HabitNameResult = StringValidator(HabitNameInput, HabitNamePrompt);

            // Determine if any entities match the input
            // This whole section is just to determine if at least one entry matches
            SqliteCommand command = conn.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Habits WHERE HabitName LIKE @Name";
            command.Parameters.AddWithValue("@Name", "%" + HabitNameResult + "%");
            var resultCount = (long)command.ExecuteScalar();
            return (resultCount, HabitNameResult);
        }



        // I am adding a drop table function for ease of testing.
        static void TestingDropTable(SqliteConnection conn)
        {
            // Create the SQL string
            string dropHabitTable = "DROP TABLE Habits";
            // Create the command
            var dropHabitTableCommand = conn.CreateCommand();
            // Create the command text
            dropHabitTableCommand.CommandText = dropHabitTable;
            // Execute the query
            dropHabitTableCommand.ExecuteNonQuery();
        }
    }
}
