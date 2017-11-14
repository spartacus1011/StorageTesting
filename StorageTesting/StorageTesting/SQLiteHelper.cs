using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;

namespace StorageTesting
{
    //I still think that surely there must be a better way than to do all this somewhat "Manually". Has no one ever made a library that can handle all this easily?
    public static class SQLiteHelper
    {
        static SQLiteHelper() 
        {
            /* dont use const strings. Building things here would probs be a bit better.
             * It allows for variable names to get changed and still be used the same in the db
             * However, could be potentially bad for worksheet migration
             * gets called just once the first time the SQLiteHelper is used
             */

            //Solution2 solution = new Solution2(); //naming solution
            //string solutionTabledefinition= nameof(solution.SolutionName) + " int, " + nameof(solution.SolutionID) + " int, " + nameof(solution.SolutionTypeId) + " int";
            //string SolutionTabledata = nameof(solution.SolutionName) + ", " + nameof(solution.SolutionID) + ", " + nameof(solution.SolutionTypeId);
        }

        public static bool CreateDatabase(string filePathAndName)
        {
            if (!System.IO.File.Exists(filePathAndName))
            {
                Console.WriteLine("Creating Database at: " + filePathAndName);
                SQLiteConnection.CreateFile(filePathAndName);
                return true;
            }
            else
            {
                Console.WriteLine("Failed to create new database: Database already exists");
                return false;
            }
        }

        //doing it like this is good because it means we can resource it which allows the database file to be read in different languages
        
        private const string SolutionName = "Solution_Name";
        private const string SolutionID = "Solution_Id";
        private const string SolutionType = "Solution_Type";

        private const string ElementsTableName = "Elements";
        private const string ElementsTableData = "Element_name, Element_Id, Element_Wavelength";
        private const string ElementsTableDefinition = "Element_name text, Element_Id int , Element_Wavelength float";

        private const string SolutionTableName = "Solutions";
        private const string SolutionsTableData = "Solution_Name, Solution_Id, Solution_Type_Id";
        private const string SolutionsTableDefinition= "Solution_Name text, Solution_Id int, Solution_Type_Id int";

        private const string MeasurementsTableName = "Measurements";
        private const string MeasurementsTableData = "Solution_Id, Element_Id, Intensity, Conc";
        private const string MeasurementsTableDefinition = "Solution_Id int, Element_Id int, Intensity float, Conc float";

        //Creating tables---------------------------------------------------------------------------------------------
        public static void CreateNewSolutionTable(SQLiteConnection connection) 
        {
           
            CreateTable(connection, SolutionTableName, SolutionsTableDefinition);
        }

        public static void CreateNewElementsTable(SQLiteConnection connection)
        {
            CreateTable(connection, ElementsTableName, ElementsTableDefinition);
        }

        public static void CreateNewMeasurementTable(SQLiteConnection connection)
        {
            CreateTable(connection, MeasurementsTableName, MeasurementsTableDefinition);
        }

        private static void CreateTable(SQLiteConnection connection, string tableName, string tableDefinition)
        {
            try
            {
                string createCommand = string.Format("create table {0} ({1})", tableName, tableDefinition);
                SQLiteCommand create = new SQLiteCommand(createCommand, connection);
                create.ExecuteNonQuery();
                create.Dispose(); //putting the dispose inside the try seems like a bad idea but how else would you do it?
            }
            catch (SQLiteException e)
            {
                //this usually means that the table that is trying to be created already exists
                Console.WriteLine(e);
            }

        }

        //Adding single items---------------------------------------------------------------------------------------------
        public static void AddSingleSolution(SQLiteConnection connection, Solution2 solution)
        {
            List<object> objectsToWrite = new List<object>
            {
                solution.SolutionName,
                solution.SolutionID,
                solution.SolutionTypeId
            };
            AddSingleItem(connection, SolutionTableName, SolutionsTableData, objectsToWrite);
        }

        public static void AddSingleElement(SQLiteConnection connection, ElementWavelength2 element)
        {
            //parameterising could probably be done better and more "automatic" but this gives better control
            List<object> objectsToWrite = new List<object>
            {
                element.ElementName,
                element.ElementId,
                element.Wavelength
            };
            AddSingleItem(connection, ElementsTableName, ElementsTableData, objectsToWrite);
        }

        public static void AddSingleMeasurement(SQLiteConnection connection, Measurement2 measurement)
        {
            //parameterising could probably be done better and more "automatic" but this gives better control
            List<object> objectsToWrite = new List<object>
            {
                measurement.SolutionId,
                measurement.ElementId,
                measurement.Intensity,
                measurement.Conc
            };
            AddSingleItem(connection, MeasurementsTableName, MeasurementsTableData, objectsToWrite);
        }

        private static void AddSingleItem(SQLiteConnection connection, string tableName, string tableData, IEnumerable<object> items)
        {
            //tableData more or less means columns
            tableData = tableData.Replace(" ", ""); //remove all spaces (Not really needed if you goven over the entered string but this makes things a bit neater)
            string[] splitData = tableData.Split(',');

            if (splitData.Length != items.Count())
            {
                throw new Exception("Table data length and number of items being added must match"); //consider a console out and return?
            }

            string dataWithAt = "";
            foreach (var thing in splitData)
            {
                dataWithAt += "@" + thing + ", ";
            }

            dataWithAt = dataWithAt.Substring(0, dataWithAt.LastIndexOf(",")); //remove the last comma

            string insertSQLstring = string.Format("insert into {0} ({1}) values ({2})", tableName, tableData, dataWithAt);

            using (SQLiteCommand insertSQL = new SQLiteCommand(insertSQLstring, connection))
            {
                for (int i = 0; i < splitData.Length; i++)
                {
                    insertSQL.Parameters.AddWithValue(splitData.ElementAt(i), items.ElementAt(i));
                }

                insertSQL.ExecuteNonQuery();
            }
        }

        //Adding multiple items------------------------------------------------------------------------------------------------------------
        public static void AddMultipleSolutions(SQLiteConnection connection, IEnumerable<Solution2> solutions)
        {
            List<List<object>> allObjectsToWrite = new List<List<object>>(); 
            foreach (var solution in solutions)
            {
                List<object> objectsToWrite = new List<object>
                {
                    solution.SolutionName,
                    solution.SolutionID,
                    solution.SolutionTypeId
                };
                allObjectsToWrite.Add(objectsToWrite);
            }
            AddMultipleItems(connection, SolutionTableName, SolutionsTableData, allObjectsToWrite);
        }

        public static void AddMultipleElements(SQLiteConnection connection, IEnumerable<ElementWavelength2> elements)
        {
            List<List<object>> allObjectsToWrite = new List<List<object>>();
            foreach (var element in elements)
            {
                List<object> objectsToWrite = new List<object>
                {
                    element.ElementName,
                    element.ElementId,
                    element.Wavelength
                };
                allObjectsToWrite.Add(objectsToWrite);
            }
            AddMultipleItems(connection, ElementsTableName, ElementsTableData, allObjectsToWrite);
        }

        public static void AddMultipleMeasurements(SQLiteConnection connection, IEnumerable<Measurement2> measurements)
        {
            List<List<object>> allObjectsToWrite = new List<List<object>>();
            foreach (var measurement in measurements)
            {
                List<object> objectsToWrite = new List<object>
                {
                    measurement.SolutionId,
                    measurement.ElementId,
                    measurement.Intensity,
                    measurement.Conc,
                };
                allObjectsToWrite.Add(objectsToWrite);
            }

            AddMultipleItems(connection, MeasurementsTableName, MeasurementsTableData, allObjectsToWrite);
        }

        private static void AddMultipleItems(SQLiteConnection connection, string tableName, string tableData, IEnumerable<IEnumerable<object>> allItems) //an enumberable of an enumerable kinda seems like a bad idea...
        {
            //the order of the items in all items is important!!! it must match the order of things in table data
            //tableData more or less means columns
            tableData = tableData.Replace(" ", ""); //remove all spaces (Not really needed if you goven over the entered string but this makes things a bit neater)
            string[] splitData = tableData.Split(',');

            foreach (var items in allItems)
            {
                if (splitData.Length != items.Count())
                {
                    throw new Exception("Table data length and number of items being added must match"); //consider a console out and return?
                }
            }

            string dataWithAt = "";
            foreach (var thing in splitData)
            {
                dataWithAt += "@" + thing + ", ";
            }
            dataWithAt = dataWithAt.Substring(0, dataWithAt.LastIndexOf(",")); //remove the last comma


            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.Transaction = transaction;

                    foreach (var items in allItems)
                    {
                        string commandString = string.Format("insert into {0} ({1}) values ({2})", tableName, tableData, dataWithAt);
                        command.CommandText = commandString;

                        for (int i = 0; i < splitData.Length; i++)
                        {
                            command.Parameters.AddWithValue(splitData.ElementAt(i), items.ElementAt(i));
                        }
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        //Loading multiple items------------------------------------------------------------------------------------------------------
        public static List<Solution2> LoadMultipleSolutions(SQLiteConnection connection)
        {
            //I have no idea how any of this works... but it does. Try and figure this out/ see if there is a better way that makes sense
            SQLiteDataAdapter adapter = new SQLiteDataAdapter();
            adapter.SelectCommand = new SQLiteCommand("select Solution_name, Solution_Id, Solution_Type_Id from Solutions", connection);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            List<Solution2> loadedSolutions = (from rw in dt.AsEnumerable()
                                               select new Solution2()
                                               {
                                                   SolutionName = Convert.ToString(rw["Solution_name"]),
                                                   SolutionID = Convert.ToInt32(rw["Solution_Id"]),
                                                   SolutionTypeId = Convert.ToInt32(rw["Solution_Type_Id"])
                                               }).ToList();

            return loadedSolutions;
        }



        public static List<ElementWavelength2> LoadMultipleElements(SQLiteConnection connection)
        {
            List<ElementWavelength2> loadedElements = new List<ElementWavelength2>();
            return loadedElements;
        }

        public static List<Measurement2> LoadMultipleMeasurementsFromSolution(SQLiteConnection connection, int solutionId)
        {
            DataTable dt = LoadMultipleItems(connection, MeasurementsTableName, MeasurementsTableData, "where Solution_Id = " + solutionId);

            List<Measurement2> loadedMeasurements= (from rw in dt.AsEnumerable()
                select new Measurement2()
                {
                    Conc = Convert.ToSingle(rw["Conc"]),
                    Intensity = Convert.ToSingle(rw["Intensity"]),
                    ElementId = Convert.ToInt32(rw["Element_Id"]),
                    SolutionId = Convert.ToInt32(rw["Solution_Id"])

                }).ToList();

            return loadedMeasurements;
        }
        private static DataTable LoadMultipleItems(SQLiteConnection connection, string tableName, string tableData, string whereConstraints = "") //Giving up on this, for now. (FastForward) This is the same as the rest. not sure why i was having difficulty
        {
            //make sure if you use the where constraints, add the "where " to the string

            SQLiteDataAdapter adapter = new SQLiteDataAdapter();
            adapter.SelectCommand = new SQLiteCommand(connection);
            adapter.SelectCommand.CommandText = string.Format("select {0} from {1} {2}", tableData ,tableName, whereConstraints);

            DataTable dt = new DataTable();
            adapter.Fill(dt);
            
            return dt;

        }

        /// <summary>
        /// Currently only updates solution name
        /// </summary>
        public static void UpdateSingleSolution(SQLiteConnection connection, Solution2 solution)
        {
            //lazyness but it works really well
            List<Solution2> singleItemList = new List<Solution2>(1){solution};
            UpdateMultipleSolutions(connection, singleItemList);
        }

        public static void UpdateMultipleSolutions(SQLiteConnection connection, IEnumerable<Solution2> solutions)
        {
            List<List<object>> allObjectsToWrite = new List<List<object>>();
            foreach (var solution in solutions)
            {
                List<object> objectsToWrite = new List<object>
                {
                    solution.SolutionName,
                    solution.SolutionID,
                    solution.SolutionTypeId
                };
                allObjectsToWrite.Add(objectsToWrite);
            }
            
            string whereConditions = "Solution_Id = @Solution_Id";
            UpdateMultipleItems(connection,SolutionTableName, SolutionsTableData, allObjectsToWrite, whereConditions);
        }

        public static void UpdateMultipleMeasurements(SQLiteConnection connection, IEnumerable<Measurement2> measurements)
        {
            //Should probably add in something to sort by solutions
            //No wait... it should do that automatically if the list is propery controlled outside of the helper due to each measurement being linked to a solutionId. Interesting...
            List<List<object>> allObjectsToWrite = new List<List<object>>();
            foreach (var measuremnt in measurements)
            {
                List<object> objectsToWrite = new List<object>
                {
                    measuremnt.SolutionId,
                    measuremnt.ElementId,
                    measuremnt.Intensity,
                    measuremnt.Conc,
                };
                allObjectsToWrite.Add(objectsToWrite);
            }

            string whereConditions = "Solution_Id = @Solution_Id"; //this wont work 100% properly as it will make all the solutions measurements the same. can be made butter using guids or if implemented properly, replicate Ids (or any other easily identifyable Id)
            UpdateMultipleItems(connection, MeasurementsTableName, MeasurementsTableData, allObjectsToWrite, whereConditions);
        }

        private static void UpdateMultipleItems(SQLiteConnection connection, string tableName, string tableData, IEnumerable<IEnumerable<object>> allItems, string whereconditions = "") //need to figure out the where conditions. may not be pretty
        {
            //tableData more or less means columns
            tableData = tableData.Replace(" ", ""); //remove all spaces (Not really needed if you goven over the entered string but this makes things a bit neater)
            string[] splitData = tableData.Split(',');
            
            foreach (var items in allItems)
            {
                if (splitData.Length != items.Count())
                {
                    throw new Exception("Table data length and number of items being added must match"); //consider a console out and return?
                }
            }

            string dataWithAt = "";
            foreach (var thing in splitData)
            {
                dataWithAt += thing + " = @" + thing + ", ";
            }
            dataWithAt = dataWithAt.Substring(0, dataWithAt.LastIndexOf(",")); //remove the last comma

            if (whereconditions == "") //take care when using this method. Maybe make it good practise to make the ID for all data first just incase. If you are trying to update what you are using in the where conditions, it wont work!!
            {
                whereconditions = splitData.FirstOrDefault() + " = @" + splitData.FirstOrDefault();
            }

            SQLiteTransaction transaction = connection.BeginTransaction();
            SQLiteCommand command = new SQLiteCommand(connection);
            command.Transaction = transaction;

            foreach (var items in allItems)
            {
                command.CommandText = string.Format("Update {0} set {1} where {2}", tableName, dataWithAt, whereconditions);

                for (int i = 0; i < splitData.Length; i++)
                {
                    command.Parameters.AddWithValue(splitData.ElementAt(i), items.ElementAt(i));
                }
                command.ExecuteNonQuery();
            }

            transaction.Commit();
            command.Dispose();
            transaction.Dispose();
        }
    }
}
