using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace StorageTesting
{
    class MainWindowViewModel: ObservableObject
    {
        public ObservableCollection<string> ListViewItems { get; set; }
        public ObservableCollection<Solution2> WorksheetOutSolutions { get; set; }

        private ExampleWorksheet2 worksheetIn = new ExampleWorksheet2();
        private ExampleWorksheet2 worksheetOut = new ExampleWorksheet2();
        private const string xmlPath = "C:\\Users\\rforzisi\\Documents\\Test Projects\\StorageTesting\\XmlStorage.ICPWorksheet";

        private Stopwatch watch = new Stopwatch();

        public ICommand ClearCommand { get { return new DelegateCommand(Clear);} }
        public ICommand XMLSaveLoadCommand { get { return new DelegateCommand(XMLSaveLoad); } }
        public ICommand XMLSaveLoadEncryptCommand { get { return new DelegateCommand(XMLSaveLoadEncrypt); } }
        public ICommand SQLiteSaveLoadCommand { get { return new DelegateCommand(SQLiteSaveLoad); } }
        public MainWindowViewModel()
        {
            ListViewItems = new ObservableCollection<string>();
            WorksheetOutSolutions = new ObservableCollection<Solution2>();
            //Creating the demo worksheet------------------------------------------------------------------
            worksheetIn.WorksheetName = "Balhhh";
            worksheetIn.Elements.Add(new ElementWavelength2(){ElementName = "Cu", Wavelength = 329.543f, ElementId = 1});
            worksheetIn.Elements.Add(new ElementWavelength2(){ElementName = "Zn", Wavelength = 123.456f, ElementId = 2});

            for (int i = 0; i < 1000; i++) //this looks very wierd and awkward
            {
                worksheetIn.Solutions.Add(new Solution2()
                {
                    SolutionName = "Solution" + i,
                    Measurements = new List<Measurement2>(){ new Measurement2()
                    {
                        Conc = 9001,
                        ElementId = 1,
                        SolutionId = i
                    }, new Measurement2()
                        {
                            Conc = 55,
                            ElementId = 2,
                            SolutionId = i
                        }}
                });
            }
        }

        private void Clear()
        {
            ListViewItems.Clear();
        }

        private void XMLSaveLoad()
        {
            //XML serialization----------------------------------------------------------------------------
            //By far the simplest and easiest method. But is it really the most feasible??
            string xmlPath = "C:\\Users\\rforzisi\\Documents\\Test Projects\\StorageTesting\\XmlStorage.ICPWorksheet";
            watch.Start();
            XmlHelper.ToXmlFile(worksheetIn, xmlPath);
            worksheetOut = XmlHelper.FromXmlFile<ExampleWorksheet2>(xmlPath);

            ListViewItems.Add("XML Create/load time = " + watch.Elapsed);

            watch.Restart();

            for (int i = 0; i < worksheetIn.Solutions.Count; i++)
            {
                //worksheetIn.Solutions.ElementAtOrDefault(i).SolutionName = "We are all the same";
                XmlHelper.ToXmlFile(worksheetIn, xmlPath);
                worksheetOut = XmlHelper.FromXmlFile<ExampleWorksheet2>(xmlPath); //i know this is only going to keep the last one but mehhh
            }

            ListViewItems.Add("XML update time = " + watch.Elapsed);

            string written = XmlHelper.ToXml(worksheetIn);
            //ListViewItems.Add("Total Chars written: " + written.Length);
            int lineCount = written.Length - written.Replace("\r\n", "").Length;
            //ListViewItems.Add("Total Lines written: " + lineCount);

            watch.Reset();
        }

        private void XMLSaveLoadEncrypt()
        {
            //XML serialization with Encryption ----------------------------------------------------------------------------
            //Still pretty simple and easy. plus adds a detail of encryption so you cant sneakily change results
            string xmlPath = "C:\\Users\\rforzisi\\Documents\\Test Projects\\StorageTesting\\XmlStorageEncrypted.ICPWorksheet";
            watch.Start();
            string toEncrypt = XmlHelper.ToXml(worksheetIn);
            //string toEncrypt = "Test";
            string Testout = "";
            string toWrite = EncryptionHelper.EncryptString(toEncrypt, "Password");
            File.WriteAllText(xmlPath,toWrite);

            string read = File.ReadAllText(xmlPath);
            string decrypted = EncryptionHelper.DecryptString(read, "Password");
            worksheetOut = XmlHelper.FromXml<ExampleWorksheet2>(decrypted);
            watch.Stop();
          

            ListViewItems.Add("Write/Read time:" + watch.Elapsed);
            ListViewItems.Add("Total Chars written: " + toEncrypt.Length);
            int lineCount = toEncrypt.Length - toEncrypt.Replace("\r\n", "").Length;
            ListViewItems.Add("Total Lines written: " + lineCount);

            watch.Reset();
        }

        private void SQLiteSaveLoad()
        {
            //Creating the Database------------------------------------------------------------------------
            //In this method, store the entire DB in memory/ram
            string dbPath = "C:\\Users\\rforzisi\\Documents\\Test Projects\\StorageTesting\\DBStorage.ICPWorksheet";
            try
            {
                if(File.Exists(dbPath))
                    File.Delete(dbPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath +";" + "Pooling=False;");
            connection.Open();

            watch.Restart();
            SQLiteHelper.CreateDatabase(dbPath);
            SQLiteHelper.CreateNewSolutionTable(connection);
            SQLiteHelper.CreateNewElementsTable(connection);
            SQLiteHelper.CreateNewMeasurementTable(connection);
            ListViewItems.Add("Creating/Setup of database = " + watch.Elapsed);
            watch.Restart();

            foreach (var solution in worksheetIn.Solutions)
            {
                solution.SolutionType = SolutionType.Default;
            }
            List<Measurement2> allMeasurements = new List<Measurement2>();
            foreach (var sol in worksheetIn.Solutions)
            {
                allMeasurements.AddRange(sol.Measurements);
            }

            watch.Restart();
            SQLiteHelper.AddMultipleSolutions(connection, worksheetIn.Solutions);
            SQLiteHelper.AddMultipleElements(connection, worksheetIn.Elements);
            SQLiteHelper.AddMultipleMeasurements(connection, allMeasurements);

            ListViewItems.Add("Adding Everything as transaction = " + watch.Elapsed);
            watch.Restart();

            foreach (var solution in worksheetIn.Solutions)
            {
                solution.SolutionType = SolutionType.Blank;
            }

            watch.Restart();
            SQLiteHelper.UpdateMultipleSolutions(connection, worksheetIn.Solutions);
            ListViewItems.Add("Update data as transaction = " + watch.Elapsed);
            watch.Restart();

            SQLiteHelper.AddSingleSolution(connection, new Solution2() { SolutionName = "Odd One out", SolutionID = 1234, SolutionType = SolutionType.Standard });
            SQLiteHelper.AddSingleElement(connection, new ElementWavelength2() { ElementName = "Unobtainium", ElementId = 42, Wavelength = 999.999f });
            SQLiteHelper.AddSingleMeasurement(connection, new Measurement2() { SolutionId = 999, ElementId = 42, Conc = 555, Intensity = 123456789 });

            watch.Restart();
            var loadedSolutions= SQLiteHelper.LoadMultipleSolutions(connection);
            var loadedMeasurements = SQLiteHelper.LoadMultipleMeasurements(connection, 999);

            WorksheetOutSolutions.Clear();
            foreach (var solution in loadedSolutions)
            {
                WorksheetOutSolutions.Add(solution);
            }
            ListViewItems.Add("Load Solutions and measurements into memory= " + watch.Elapsed);

            connection.Close();
        }

        /// <summary>
        /// Not used. but keeping for comparison
        /// </summary>
        private void SQLiteSaveLoadSlowMethod()
        {
            //string dbPath = "C:\\Users\\rforzisi\\Documents\\Test Projects\\StorageTesting\\DBStorage.ICPWorksheet";
            //try
            //{
            //    File.Delete(dbPath);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    return;
            //}

            //SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath + ";" + "Pooling=False;");
            //connection.Open();

            //watch.Start();
            ////DB and table creation One item at a time
            //SQLiteHelper.CreateDatabase(dbPath);
            //SQLiteHelper.CreateNewSolutionTable(connection);
            //SQLiteHelper.CreateNewElementsTable(connection);
            //SQLiteHelper.CreateNewMeasurementTable(connection);

            //foreach (var solution in worksheetIn.Solutions)
            //{
            //    SQLiteHelper.AddSingleSolution(connection, solution);
            //}
            //ListViewItems.Add("Create data one at a time = " + watch.Elapsed);
            //watch.Restart();

            //foreach (var element in worksheetIn.ElementWavelengths)
            //{
            //    SQLiteHelper.AddSingleElement(connection, element);
            //}
            //watch.Restart();

            //for (int i = 0; i < worksheetIn.Solutions.Count; i++)
            //{
            //    worksheetIn.Solutions.ElementAtOrDefault(i).SolutionName = "We are all the same";
            //    SQLiteHelper.UpdateSingleSolution(connection, worksheetIn.Solutions.ElementAtOrDefault(i));
            //}
            //ListViewItems.Add("Update data one at a time = " + watch.Elapsed);
            //watch.Restart();

            //connection.Close();

        }

        private void SQLiteExchange() //this may be harder than i think to implement the way mike was saying
        {
            //Creating the database------------------------------------------------------------------------
            //This is the complete re-write method where in ICP will work directly off the database
            string dbPath = "C:\\Users\\rforzisi\\Documents\\Test Projects\\StorageTesting\\DBStorageExchange.ICPWorksheet";

        }

    }
}
