using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;

namespace StudentGrades
{
    public partial class MainWindow : Window
    {
        private List<Student> students;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads CSV data into a list of Student objects.
        /// </summary>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <returns>A list of Student objects.</returns>
        private List<Student> LoadCsvData(string filePath, string FileName)
        {
            var students = new List<Student>();
            double GPACu = 0;
            int countS = 0;
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {

                    var records = csv.GetRecords<dynamic>().ToList();
                    foreach (var record in records)
                    {
                        var student = new Student
                        {
                            ID = record.ZehutNumber,
                            Name = record.Name,
                            LastName = record.LastName,
                            Years = int.Parse(record.Year)
                        };
                        int i = 0;
                        double GradesSt = 0;
                        foreach (var prop in record)
                        {
                            if (i > 3)
                            {
                                string propName = prop.Key;
                                if (double.TryParse(prop.Value.ToString(), out double grade))
                                {
                                    student.Grades[propName] = grade;
                                }
                                string input = prop.Key;
                                string pattern = @"\d+";
                                // Create a Regex object
                                Regex regex = new Regex(pattern);
                                // Find matches
                                Match match = regex.Match(input);
                                double pre;
                                double.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out pre);
                                GradesSt += (pre / 100) * grade;
                            }
                            i++;
                        }
                        student.fullGreads = GradesSt;
                        students.Add(student);
                        countS++;
                        GPACu += GradesSt;
                    }
                    GPACu /= countS;
                    avg.Content = FileName + " = " + GPACu.ToString("F2");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading CSV data: {ex.Message}");
            }
            return students;
        }

        /// <summary>
        /// Handles the button click event to load CSV data.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string csvFilePath = openFileDialog.FileName;
                students = LoadCsvData(csvFilePath, Path.GetFileNameWithoutExtension(csvFilePath));
                StudentDataGrid.ItemsSource = students;
                for (int i = 0; i < StudentDataGrid.Columns.Count; i++)
                {
                    if (i != 1 && i != 2)
                        StudentDataGrid.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Uploads student grades to the grid.
        /// </summary>
        private void UploadGrades(Student student)
        {
            DataTable gradesTable = new DataTable();
            gradesTable.Columns.Add("Subject");
            gradesTable.Columns.Add("Grade");
            double GradesSt = 0;
            foreach (var grade in student.Grades)
            {
                DataRow row = gradesTable.NewRow();
                row["Subject"] = grade.Key;
                row["Grade"] = grade.Value.ToString();
                gradesTable.Rows.Add(row);
            }

            StudentGradesGrid.ItemsSource = gradesTable.DefaultView;
            GradesN.Text = student.fullGreads.ToString();
        }

        /// <summary>
        /// Uploads student details to the grid.
        /// </summary>
        private void UploadDetails(Student student)
        {
            DataTable detailsTable = new DataTable();
            detailsTable.Columns.Add("Property");
            detailsTable.Columns.Add("Value");

            detailsTable.Rows.Add("ID", student.ID);
            detailsTable.Rows.Add("Name", student.Name);
            detailsTable.Rows.Add("LastName", student.LastName);
            detailsTable.Rows.Add("Years", student.Years);

            StudentDetDataGrid.ItemsSource = detailsTable.DefaultView;
        }

        /// <summary>
        /// Handles the selection changed event for the StudentDataGrid.
        /// </summary>
        private void StudentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentDataGrid.SelectedItem is Student selectedStudent)
            {
                UploadDetails(selectedStudent);
                UploadGrades(selectedStudent);
            }
        }

        /// <summary>
        /// Saves student data to a JSON file.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        private void SaveStudentsToJson(string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(students, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, json);
                MessageBox.Show("Students data saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving JSON data: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the button click event to save students data to a JSON file.
        /// </summary>
        private void JsonFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json"
            };
            saveFileDialog.FileName = avg.Content.ToString().Substring(0, avg.Content.ToString().IndexOf('='));

            string jsonFilePath = AppDomain.CurrentDomain.BaseDirectory + "jsonFile\\" + saveFileDialog.FileName + ".json";
            SaveStudentsToJson(jsonFilePath);
            winLoad_Loaded(sender, e);
        }

        /// <summary>
        /// Handles the window load event to populate the ComboBox with JSON files.
        /// </summary>
        private void winLoad_Loaded(object sender, RoutedEventArgs e)
        {
            string directoryPath = AppDomain.CurrentDomain.BaseDirectory + "jsonFile\\";

            // Check if the directory exists
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    // Extract all JSON files in the directory
                    var jsonFiles = Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly).ToList();

                    // Clear the ComboBox before adding new items
                    jsonFile.Items.Clear();

                    if (jsonFiles.Count > 0)
                    {
                        // Add each JSON file name to the ComboBox
                        foreach (var jsonFile1 in jsonFiles)
                        {
                            jsonFile.Items.Add(Path.GetFileName(jsonFile1).Substring(0, Path.GetFileName(jsonFile1).IndexOf('.')));
                        }
                    }
                    else
                    {
                        MessageBox.Show("No JSON files found in the directory.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while accessing the directory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("The directory does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
