using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;

namespace Games_Library
{
    public partial class MainWindow : Window
    {
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public MainWindow()
        {
            InitializeComponent();

            /// Lädt zum Start des Programmes die 00_All _Games.csv
            LoadLibrary("00_All _Games");

            /// Lädt zum Start alle vorhandenen Listen in die Combobox Items
            CreateComboBoxListItems();

            /// ReleaseYearItems werden der Combobox releaseYearFilter von 1990-2022 hinzugefügt
            CreateReleaseYearList(1990, 2022);
        }
        public IEnumerable<Game> ReadCSV(string fileName)
        {
            /// Für die Auflistung wird die Variable lines als string array deklariert und kann nur .csv dateien einlesen
            string[] lines = File.ReadAllLines(Path.ChangeExtension(fileName, ".csv"));

            /// lines.Select erlaubt, jede Zeile als Spiel zu wiedergeben.
            /// Diese gibt dann ein IEnumerable<Game> zurück.
            return lines.Select(line =>
            {
                /// Nach jedem Semikolin werden die Daten in der .csv-Datei getrennt
                string[] data = line.Split(';');
                /// Das Spiel wird dann mit einzelnen Daten in der richtigen Reihenfolge zurückgegeben.
                return new Game(data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7]);
            });
        }

        /// Holt sich den Pfad mit dem database Ordner, wo sich alle Listen drinnen befinden
        public string GetPath()
        {
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sFile = Path.Combine(sCurrentDirectory, @"..\..\..\Games-Library\database/");
            string sFilePath = Path.GetFullPath(sFile);

            return sFilePath;
        }

        /// Methode die, die Liste lädt
        private bool LoadLibrary(string libraryFileName)
        {
            string path = GetPath();

            ListViewGame.ItemsSource = ReadCSV(@"" + path + libraryFileName + ".csv");
            return true;
        }

        /// Sortier-Methode beim rauf klicken der jeweiligen Spiele-Eigenschaft
        private void SortColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();

            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ListViewGame.Items.SortDescriptions.Clear();
            }
            /// Aufsteigende Sortierung
            ListSortDirection newDir = ListSortDirection.Ascending;

            /// Absteigende Sortierung
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            ListViewGame.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        /// Methode für das auswählen eines Spiels
        private void Game_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /// Ausgwähltes Spiel wird in der variable SelectedGame speichern
            Game SelectedGame = (Game)ListViewGame.SelectedItem;

            /// Die if-Anweisung kann nur ausgeführt werden wenn ein Spiel ausgewählt ist
            if (SelectedGame != null)
            {
                /// Labels, ScrollViewer und Buttons anzeigen lassen
                labelName.Visibility = Visibility.Visible;
                labelGenre.Visibility = Visibility.Visible;
                labelPlatform.Visibility = Visibility.Visible;
                labelReleaseDate.Visibility = Visibility.Visible;
                labelMetaScore.Visibility = Visibility.Visible;
                labelUserScore.Visibility = Visibility.Visible;
                labelDescription.Visibility = Visibility.Visible;
                scrollViewerDescription.Visibility = Visibility.Visible;
                deleteButton.Visibility = Visibility.Visible;
                addEditUserScoreButton.Visibility = Visibility.Visible;
                editGameButton.Visibility = Visibility.Visible;

                /// Hier wird das Cover des Spiels als Bitmap eingelesen und geladen
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();

                Uri uriResult;
                bool result = Uri.TryCreate(SelectedGame.Cover_Path, UriKind.Absolute, out uriResult);
                if (!result)
                    SelectedGame.Cover_Path = "";

                /// Wenn das Spiel kein Cover enthält oder einen ungültigen URL hat, dann wird ein Bild mit "No Cover Available" angezeigt
                if (SelectedGame.Cover_Path.Length != 0 && SelectedGame.Cover_Path != null)
                    bitmapImage.UriSource = new Uri(SelectedGame.Cover_Path);
                else
                    bitmapImage.UriSource = new Uri("https://upload.wikimedia.org/wikipedia/commons/b/b9/No_Cover.jpg");

                bitmapImage.EndInit();
                img.Source = bitmapImage;

                /// Eigenschaften des jeweiligen Spiels werden übergeben
                textBoxName.Text = SelectedGame.Name;
                textBoxGenre.Text = SelectedGame.Genre;
                textBoxPlatform.Text = SelectedGame.Platform;
                textRelease_Date.Text = SelectedGame.Release_Date;
                textBoxMeta_Score.Text = SelectedGame.Meta_Score;
                textBoxUser_Score.Text = SelectedGame.User_Score;
                textBlockDescription.Text = Encoding.Default.GetString(Encoding.Default.GetBytes(SelectedGame.Description));
            }
        }

        /// Filter-Methode für Suche, Genre, Plattform und Release-Jahr
        private bool Filter(object item)
        {
            /// Die jeweiligen Filter werden hier in Strings umgewandelt
            string genreString;
            if (genreFilter.SelectedValue == null || genreFilter.SelectedValue.ToString().Contains("System.Windows.Controls.ComboBoxItem"))
                genreString = "";
            else
                genreString = genreFilter.SelectedValue.ToString();

            string platformString;
            if (platformFilter.SelectedValue == null || platformFilter.SelectedValue.ToString().Length <= 36)
                platformString = "";
            else
                platformString = platformFilter.SelectedValue.ToString().Substring(38);

            string releaseYear;
            if (releaseYearFilter.SelectedValue == null || releaseYearFilter.SelectedValue.ToString().Length <= 36)
                releaseYear = "";
            else
                releaseYear = releaseYearFilter.SelectedValue.ToString().Substring(38);

            /// Hier wird die Spieleliste mit den angewendeteten Filter zurückgegeben
            return ((item as Game).Name.IndexOf(searchFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (item as Game).Genre.IndexOf(genreString, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (item as Game).Platform.IndexOf(platformString, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (item as Game).Release_Date.IndexOf(releaseYear, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// Text -und SelectionsHandler für die jeweiligen Filter:
        private void searchFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource);
            view.Filter = Filter;
        }
        private void genreFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource);
            view.Filter = Filter;
        }
        private void platformFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource);
            view.Filter = Filter;
        }
        private void releaseYearFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource);
            view.Filter = Filter;
        }

        /// Methode zur Erstellung der Release-Jahre für die ComboBox
        private void CreateReleaseYearList(int startYear, int endYear)
        {
            for (int i = endYear; i > startYear - 1; i--)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = i;
                releaseYearFilter.Items.Add(item);
            }
        }

        /// ResetFilter-Methode um alle Filter und Sortierungen wieder zuürckzusetzen und ButtonEventHandler
        private bool ResetFilter(object item)
        {
            /// Alle Filter werden auf ihren Standartwert zurückgesetzt
            searchFilter.Text = "";
            genreFilter.SelectedIndex = -1;
            platformFilter.SelectedIndex = -1;
            releaseYearFilter.SelectedIndex = -1;

            if (listViewSortCol != null)
            {
                /// Sortierung wird zurückgesetzt
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ListViewGame.Items.SortDescriptions.Clear();
            }

            /// Danach wird die gesamte Spieleliste einfach augegeben
            return true;
        }
        private void resetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource);
            view.Filter = ResetFilter;
        }

        /// createList, save -und cancel ButtonEventHandleer
        private void createLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            /// InputBox wird angezeigt
            createLibraryInputBox.Visibility = Visibility.Visible;
        }
        private void saveLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            /// InputBox wird wieder geschlossen und die .csv-Datei wird erzeugt
            createLibraryInputBox.Visibility = Visibility.Collapsed;
            CreateCSVFile(createLibraryInputTextBox.Text);
        }
        private void cancelLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            /// InputBox wird wieder geschlossen und das Eingabefeld wird leer gesetzt
            createLibraryInputBox.Visibility = Visibility.Collapsed;
            createLibraryInputTextBox.Text = String.Empty;
        }

        /// Erstellt eine neue .csv-Datei
        private void CreateCSVFile(string fileName)
        {
            string path = GetPath();
            try
            {
                /// .csv-Datei wird mit dem gewünschten Namen erstellt
                string csvpath = @"" + path + fileName + ".csv";
                File.AppendAllText(csvpath, "");

                /// InputBox wird wieder geleert
                createLibraryInputTextBox.Text = String.Empty;

                MessageBox.Show("Creating was successful.", "Success", MessageBoxButton.OK);

                /// Neu erstellte Liste wird der ComboBox hinzugefügt
                librariesSelection.Items.Add(fileName + ".csv");
            }
            catch
            {
                MessageBox.Show("Error! Please try again.", "Error", MessageBoxButton.OK);
            }
        }

        /// Lädt alle .csv-Listen in die ComboBox als ComboBoxItems
        private void CreateComboBoxListItems()
        {
            string path = GetPath();
            string[] filePaths = Directory.GetFiles(@"" + path, "*.csv");

            foreach (string file in filePaths)
            {
                /// Alle sich befindenen Listen werden ausgelesen und der ComboBox hinzugefügt
                librariesSelection.Items.Add(file.Substring(file.LastIndexOf("database") + 9));
            }
        }

        /// Lädt alle verfügbaren Genres dynamisch aus der derzeit ausgewählten Liste
        private void CreateComBoxGenreItems()
        {
            genreFilter.Items.Clear();
            genreFilter.Items.Add("");

            string path = GetPath();
            string libraryFile = librariesSelection.SelectedItem.ToString();
            string libraryFileName = libraryFile.Remove(libraryFile.Length - 4, 4);
            Game SelectedGame = (Game)ListViewGame.SelectedItem;

            /// Excel-Instanz wird erstellt:
            Excel.Application excel = new Excel.Application();
            /// Excel-Datei öffnen
            Excel.Workbook sheet = excel.Workbooks.Open(@"" + path + libraryFile);
            /// Arbeitsblatt wird ausgewählt
            Excel.Worksheet x = excel.ActiveSheet as Excel.Worksheet;
            /// Range wird erstellt
            Excel.Range userRange = x.UsedRange;

            /// Range iterieren
            for (int i = 1; i < userRange.Rows.Count + 1; i++)
            {
                /// Einzelne Zelle wird eingelesen
                string cel = ((Excel.Range)x.Cells[i, 1]).Value2.ToString();

                /// Genre wird aus dem Spiel rausgesucht
                string genreCel = (cel.Substring(cel.IndexOf(";") + 1).Split(';')[0].Trim());

                /// Genre wird der ComboBox hinzugefügt, mit Überprüfung damit keine doppellten Genre's vorkommen
                if (!genreFilter.Items.Contains(genreCel))
                    genreFilter.Items.Add(genreCel);
            }
            /// Arbeitsblatt wird geschlossen
            sheet.Close();
        }

        /// SelectionsHandler für die Auswahl der Listen
        private void librariesSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (librariesSelection != null)
            {
                string libraryFile = librariesSelection.SelectedItem.ToString();
                string libraryFileName = libraryFile.Remove(libraryFile.Length - 4, 4);
                /// Lädt die ausgewählte Liste
                LoadLibrary(libraryFileName);

                /// Erstellt die Genre's der ausgewählten Liste
                CreateComBoxGenreItems();

                /// Setzt alle Filter zurück
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource);
                view.Filter = ResetFilter;
            }
        }

        /// Methode zum Löschen eines Spiels
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you really want to delete this game?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string path = GetPath();
                string libraryFile = librariesSelection.SelectedItem.ToString();
                string libraryFileName = libraryFile.Remove(libraryFile.Length - 4, 4);
                Game SelectedGame = (Game)ListViewGame.SelectedItem;

                /// Excel-Instanz wird erstellt:
                Excel.Application excel = new Excel.Application();
                /// Excel-Datei öffnen
                Excel.Workbook sheet = excel.Workbooks.Open(@"" + path + libraryFile);
                /// Arbeitsblatt wird ausgewählt
                Excel.Worksheet x = excel.ActiveSheet as Excel.Worksheet;
                /// Range wird erstellt
                Excel.Range userRange = x.UsedRange;

                /// Range iterieren
                for (int i = 1; i < userRange.Rows.Count + 1; i++)
                {
                    /// Einzelne Zelle wird eingelesen
                    string cel = ((Excel.Range)x.Cells[i, 1]).Value2.ToString();

                    /// Überprüft die Übereinstimmung der Daten des Spiels
                    if (cel.Contains(SelectedGame.Name + ";" + SelectedGame.Genre + ";" + SelectedGame.Platform + ";"))
                    {
                        /// Löscht nach Übereinstimmung die Zelle
                        x.Cells[i, 1].Delete(Excel.XlDeleteShiftDirection.xlShiftUp);

                        /// Arbeitsblatt wird gespeichert
                        sheet.Save();

                        MessageBox.Show("Game has been deleted successfully", "Success", MessageBoxButton.OK);
                        break;
                    }
                }
                /// Arbeitsblatt wird geschlossen
                sheet.Close();

                /// Spieleliste wird aktualisiert
                LoadLibrary(libraryFileName);
                CreateComBoxGenreItems();

                /// Alle Daten in der Benutzeroberfläche werden zurückgesetzt
                img.Source = null;
                textBoxName.Text = "";
                textBoxGenre.Text = "";
                textBoxPlatform.Text = "";
                textRelease_Date.Text = "";
                textBoxMeta_Score.Text = "";
                textBoxUser_Score.Text = "";
                textBlockDescription.Text = Encoding.Default.GetString(Encoding.Default.GetBytes(""));

                /// Labels, ScrollViewer und Buttons werden wieder ausgeblendet, da kein Spiel mehr ausgwählt ist
                labelName.Visibility = Visibility.Hidden;
                labelGenre.Visibility = Visibility.Hidden;
                labelPlatform.Visibility = Visibility.Hidden;
                labelReleaseDate.Visibility = Visibility.Hidden;
                labelMetaScore.Visibility = Visibility.Hidden;
                labelUserScore.Visibility = Visibility.Hidden;
                labelDescription.Visibility = Visibility.Hidden;
                scrollViewerDescription.Visibility = Visibility.Hidden;
                deleteButton.Visibility = Visibility.Hidden;
                addEditUserScoreButton.Visibility = Visibility.Hidden;
            }
        }

        /// Aktivier -und Abbruchbutton fürs ändern des User Scores
        private void addEditUserScoreButton_Click(object sender, RoutedEventArgs e)
        {
            textBoxUser_Score.IsReadOnly = false;
            textBoxUser_Score.BorderThickness = new Thickness(1);
            saveUserScoreButton.Visibility = Visibility.Visible;
            cancelUserScoreButton.Visibility = Visibility.Visible;
        }
        private void cancelUserScoreButton_Click(object sender, RoutedEventArgs e)
        {
            /// Die Daten werden wieder zurückgesetzt
            Game SelectedGame = (Game)ListViewGame.SelectedItem;
            textBoxUser_Score.IsReadOnly = true;
            textBoxUser_Score.BorderThickness = new Thickness(0);
            saveUserScoreButton.Visibility = Visibility.Hidden;
            cancelUserScoreButton.Visibility = Visibility.Hidden;
            if (SelectedGame != null)
                textBoxUser_Score.Text = SelectedGame.User_Score;
        }

        /// Methode die den User Score abspeichert
        private void saveUserScoreButton_Click(object sender, RoutedEventArgs e)
        {
            int number;
            bool result = Int32.TryParse(textBoxUser_Score.Text, out number);
            if (result)
            {
                string path = GetPath();
                string libraryFile = librariesSelection.SelectedItem.ToString();
                string libraryFileName = libraryFile.Remove(libraryFile.Length - 4, 4);

                Game SelectedGame = (Game)ListViewGame.SelectedItem;

                /// Überprüfung ob ein Spiel ausgewählt worden ist
                if (ListViewGame.SelectedItem != null)
                {
                    SelectedGame = (Game)ListViewGame.SelectedItem;
                }
                else
                {
                    SelectedGame = new Game(" " + textBoxName.Text,
                        textBoxGenre.Text,
                        textBoxPlatform.Text,
                        textRelease_Date.Text,
                        Encoding.Default.GetString(Encoding.Default.GetBytes(textBlockDescription.Text)),
                        textBoxMeta_Score.Text,
                        textBoxUser_Score.Text,
                        img.Source + " ");
                }

                /// Excel-Instanz wird erstellt:
                Excel.Application excel = new Excel.Application();
                /// Excel-Datei öffnen
                Excel.Workbook sheet = excel.Workbooks.Open(@"" + path + libraryFile);
                /// Arbeitsblatt wird ausgewählt
                Excel.Worksheet x = excel.ActiveSheet as Excel.Worksheet;
                /// Range wird erstellt
                Excel.Range userRange = x.UsedRange;

                /// Überprüfung für nicht mehr als 100 Eingabe
                if (Convert.ToInt32(textBoxUser_Score.Text) < 100)
                {
                    /// UserScore wird mit dem neuen Wert zugewiesen
                    SelectedGame.User_Score = textBoxUser_Score.Text;

                    /// Range iterieren
                    for (int i = 1; i < userRange.Rows.Count + 1; i++)
                    {
                        /// Einzelne Zelle wird eingelesen
                        string cel = ((Excel.Range)x.Cells[i, 1]).Value2.ToString();

                        /// Überprüft die Übereinstimmung der Daten des Spiels
                        if (cel.Contains(SelectedGame.Name + ";" + SelectedGame.Genre + ";" + SelectedGame.Platform + ";"))
                        {
                            /// Aktualisiert das Spiel mit dem neuen User Score
                            x.Cells[i, 1] = SelectedGame.Name + ";" +
                                SelectedGame.Genre + ";" +
                                SelectedGame.Platform + ";" +
                                SelectedGame.Release_Date + ";" +
                                SelectedGame.Description + ";" +
                                SelectedGame.Meta_Score + ";" +
                                SelectedGame.User_Score + ";" +
                                SelectedGame.Cover_Path;

                            /// Arbeitsblatt wird gespeichert
                            sheet.Save();

                            MessageBox.Show("User score has been successfully updated!", "Success", MessageBoxButton.OK);
                            break;
                        }
                    }

                    /// Arbeitsblatt wird geschlossen
                    sheet.Close();

                    /// Spieleliste wird aktualisiert
                    LoadLibrary(libraryFileName);
                    CreateComBoxGenreItems();

                    /// Die Eingabefelder werden ausgeblendet und auch ReadOnly gestellt
                    textBoxUser_Score.IsReadOnly = true;
                    textBoxUser_Score.BorderThickness = new Thickness(0);
                    saveUserScoreButton.Visibility = Visibility.Hidden;
                    cancelUserScoreButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    /// Wenn eine Zahl über 100 eingegeben wird, wird eine Fehler ausgeworfen und das Textfeld geleert
                    MessageBox.Show("Enter less 100 number", "Error", MessageBoxButton.OK);
                    textBoxUser_Score.Text = "";
                }
            }
            else
            {
                /// Wenn keine Zahl eingegeben wird, wird eine Fehler ausgeworfen und das Textfeld geleert
                MessageBox.Show("Please only enter numbers", "Error", MessageBoxButton.OK);
                textBoxUser_Score.Text = "";
            }
        }

        /// Aktivier -und Abbruchbutton fürs hinzufügen eines neuen Spiels
        private void addGameButton_Click(object sender, RoutedEventArgs e)
        {
            /// InputBox für das Erstellen eines Spiels wird eingeblendet
            addGameInputBox.Visibility = Visibility.Visible;
        }
        private void cancelAddGameButton_Click(object sender, RoutedEventArgs e)
        {
            /// Alle Input Felder werden wieder zurückgesetzt und die InputBox ausgeblendet
            addGameInputBox.Visibility = Visibility.Hidden;
            addGameName.Text = "";
            addGameGenre.Text = "";
            addGameplatform.SelectedIndex = -1;
            addGameReleaseDate.Text = "";
            addGameDescription.Text = "";
            addGameMetaScore.Text = "";
            addGameCoverPath.Text = "";
        }

        /// Methode für das hinzufügen eines neuen Spiels in die Liste
        private void saveAddGame_Click(object sender, RoutedEventArgs e)
        {
            /// Überprüfung das keine Semikolons in den Input Feldern sind, da sonst die Datenbankstruktur auseinader fällt
            if (!addGameName.Text.Contains(";") &&
                !addGameGenre.Text.Contains(";") &&
                !addGameReleaseDate.Text.Contains(";") &&
                !addGameDescription.Text.Contains(";") &&
                !addGameMetaScore.Text.Contains(";") &&
                !addGameCoverPath.Text.Contains(";"))
            {
                string platformString;
                if (addGameplatform.SelectedValue == null || addGameplatform.SelectedValue.ToString().Length <= 36)
                    platformString = "";
                else
                    platformString = addGameplatform.SelectedValue.ToString().Substring(38);

                /// Überprüfung wenn das Datumformat falsch ist
                DateTime dt;
                string[] formats = { "yyyy-MM-dd" };
                if (DateTime.TryParseExact(addGameReleaseDate.Text, formats,
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ||
                                addGameReleaseDate.Text == "")
                {
                    /// Überprüfung wenn das Textfeld MetaScore keine Zahlen enthält
                    int number;
                    bool result = Int32.TryParse(addGameMetaScore.Text, out number);
                    if (result || addGameMetaScore.Text == "")
                    {
                        string path = GetPath();
                        string libraryFile = librariesSelection.SelectedItem.ToString();
                        string libraryFileName = libraryFile.Remove(libraryFile.Length - 4, 4);

                        /// Überprüfung wenn nichts im MetaScore Textfeld steht oder für nicht mehr als 100 Eingabe
                        if (addGameMetaScore.Text == "" || Convert.ToInt32(addGameMetaScore.Text) < 100)
                        {
                            /// Excel-Instanz wird erstellt:
                            Excel.Application excel = new Excel.Application();
                            /// Excel-Datei öffnen
                            Excel.Workbook sheet = excel.Workbooks.Open(@"" + path + libraryFile);
                            /// Arbeitsblatt wird ausgewählt
                            Excel.Worksheet x = excel.ActiveSheet as Excel.Worksheet;
                            /// Range wird erstellt
                            Excel.Range userRange = x.UsedRange;

                            int countRecords = userRange.Rows.Count;

                            /// Neues Spiel wird zusammengestellt
                            string newGame = addGameName.Text + ";" +
                                addGameGenre.Text + ";" +
                                platformString + ";" +
                                addGameReleaseDate.Text + ";" +
                                addGameDescription.Text + ";" +
                                addGameMetaScore.Text + ";" + ";" +
                                addGameCoverPath.Text;

                            /// Neues Spiel wird der einer Excel-Zelle hinzugefügt
                            x.Cells[countRecords + 1, 1] = newGame;

                            /// Arbeitsblatt wird gespeichert
                            sheet.Save();

                            MessageBox.Show("The game has been added to the library!", "Success", MessageBoxButton.OK);

                            /// Arbeitsblatt wird geschlossen
                            sheet.Close();

                            /// Spieleliste wird aktualisiert
                            LoadLibrary(libraryFileName);
                            CreateComBoxGenreItems();

                            /// Alle Input Felder werden wieder zurückgesetzt und die InputBox ausgeblendet
                            addGameInputBox.Visibility = Visibility.Hidden;
                            addGameName.Text = "";
                            addGameGenre.Text = "";
                            platformString = "";
                            addGameplatform.SelectedIndex = -1;
                            addGameReleaseDate.Text = "";
                            addGameDescription.Text = "";
                            addGameMetaScore.Text = "";
                            addGameCoverPath.Text = "";
                        }
                        else
                        {
                            /// Wenn eine Zahl über 100 eingegeben wird, wird eine Fehler ausgeworfen und das Textfeld geleert
                            MessageBox.Show("Enter less 100 number", "Error", MessageBoxButton.OK);
                            addGameMetaScore.Text = "";
                        }
                    }
                    else
                    {
                        /// Wenn keine Zahl eingegeben wird, wird eine Fehler ausgeworfen und das Textfeld geleert
                        MessageBox.Show("Please only enter numbers", "Error", MessageBoxButton.OK);
                        addGameMetaScore.Text = "";
                    }
                }
                else
                {
                    /// Wenn ein falsches Datumformat eingegeben word ist, wird ein Fehler ausgeworfen und das Textfeld geleert
                    MessageBox.Show("Please enter the correct date format", "Error", MessageBoxButton.OK);
                    addGameReleaseDate.Text = "";
                }
            }
            else
            {
                /// Wenn in einer der Textfelder sich ein Semikolon befindet, wird ein Fehler ausgeworfen
                MessageBox.Show("The input fields must not contain semicolons!", "Error", MessageBoxButton.OK);
            }
        }

        /// Aktivier -und Abbruchbutton fürs editieren eines Spiels
        private void editGameButton_Click(object sender, RoutedEventArgs e)
        {
            /// InputBox für das Bearbeiten eines Spiels wird eingeblendet
            editGameInputBox.Visibility = Visibility.Visible;

            /// Eigenschaften des jeweiligen Spiels werden in den Input Feldern reingeschrieben
            Game SelectedGame = (Game)ListViewGame.SelectedItem;
            if (ListViewGame.SelectedItem != null)
            {
                editGameName.Text = SelectedGame.Name;
                editGameGenre.Text = SelectedGame.Genre;
                editGameplatform.Text = SelectedGame.Platform;
                editGameReleaseDate.Text = SelectedGame.Release_Date;
                editGameMetaScore.Text = SelectedGame.Meta_Score;
                editGameDescription.Text = Encoding.Default.GetString(Encoding.Default.GetBytes(SelectedGame.Description));
                editGameCoverPath.Text = SelectedGame.Cover_Path;
            }
            else
            {
                editGameName.Text = textBoxName.Text;
                editGameGenre.Text = textBoxGenre.Text;
                editGameplatform.Text = textBoxPlatform.Text;
                editGameReleaseDate.Text = textRelease_Date.Text;
                editGameMetaScore.Text = textBoxMeta_Score.Text;
                editGameDescription.Text = Encoding.Default.GetString(Encoding.Default.GetBytes(textBlockDescription.Text));
                editGameCoverPath.Text = img.Source.ToString();
            }
        }
        private void cancelEditGameButton_Click(object sender, RoutedEventArgs e)
        {
            editGameInputBox.Visibility = Visibility.Hidden;
        }

        /// Methode für das bearbeiten eines  Spiels
        private void saveEditGame_Click(object sender, RoutedEventArgs e)
        {
            Game SelectedGame = (Game)ListViewGame.SelectedItem;
            if (ListViewGame.SelectedItem != null)
            {
                SelectedGame = (Game)ListViewGame.SelectedItem;
            }
            else
            {
                SelectedGame = new Game(" " + textBoxName.Text,
                    textBoxGenre.Text,
                    textBoxPlatform.Text,
                    textRelease_Date.Text,
                    Encoding.Default.GetString(Encoding.Default.GetBytes(textBlockDescription.Text)),
                    textBoxMeta_Score.Text,
                    textBoxUser_Score.Text,
                    img.Source + " ");
            }

            /// Überprüfung das keine Semikolons in den Input Feldern sind, da sonst die Datenbankstruktur auseinader fällt
            if (!addGameName.Text.Contains(";") &&
                !addGameGenre.Text.Contains(";") &&
                !addGameReleaseDate.Text.Contains(";") &&
                !addGameDescription.Text.Contains(";") &&
                !addGameMetaScore.Text.Contains(";") &&
                !addGameCoverPath.Text.Contains(";"))
            {
                string platformString;
                if (addGameplatform.SelectedValue == null || addGameplatform.SelectedValue.ToString().Length <= 36)
                    platformString = "";
                else
                    platformString = addGameplatform.SelectedValue.ToString().Substring(38);

                /// Überprüfung wenn das Datumformat falsch ist
                DateTime dt;
                string[] formats = { "yyyy-MM-dd" };
                if (DateTime.TryParseExact(addGameReleaseDate.Text, formats,
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ||
                                addGameReleaseDate.Text == "")
                {
                    /// Überprüfung wenn das Textfeld MetaScore keine Zahlen enthält
                    int number;
                    bool result = Int32.TryParse(addGameMetaScore.Text, out number);
                    if (result || addGameMetaScore.Text == "")
                    {
                        string path = GetPath();
                        string libraryFile = librariesSelection.SelectedItem.ToString();
                        string libraryFileName = libraryFile.Remove(libraryFile.Length - 4, 4);

                        /// Überprüfung wenn nichts im MetaScore Textfeld steht oder für nicht mehr als 100 Eingabe
                        if (addGameMetaScore.Text == "" || Convert.ToInt32(addGameMetaScore.Text) < 100)
                        {
                            /// Excel-Instanz wird erstellt:
                            Excel.Application excel = new Excel.Application();
                            /// Excel-Datei öffnen
                            Excel.Workbook sheet = excel.Workbooks.Open(@"" + path + libraryFile);
                            /// Arbeitsblatt wird ausgewählt
                            Excel.Worksheet x = excel.ActiveSheet as Excel.Worksheet;
                            /// Range wird erstellt
                            Excel.Range userRange = x.UsedRange;

                            /// Range iterieren
                            for (int i = 1; i < userRange.Rows.Count + 1; i++)
                            {
                                /// Einzelne Zelle wird eingelesen
                                string cel = ((Excel.Range)x.Cells[i, 1]).Value2.ToString();

                                /// Überprüft die Übereinstimmung der Daten des Spiels
                                if (cel.Contains(SelectedGame.Name + ";" + SelectedGame.Genre + ";" + SelectedGame.Platform + ";"))
                                {
                                    /// Aktualisiert das Spiel mit dem neuen User Score
                                    x.Cells[i, 1] = editGameName.Text + ";" +
                                        editGameGenre.Text + ";" +
                                        editGameplatform.Text + ";" +
                                        editGameReleaseDate.Text + ";" +
                                        editGameDescription.Text + ";" +
                                        editGameMetaScore.Text + ";" +
                                        SelectedGame.User_Score + ";" +
                                        editGameCoverPath.Text;

                                    /// Arbeitsblatt wird gespeichert
                                    sheet.Save();

                                    MessageBox.Show("Game has been successfully updated!", "Success", MessageBoxButton.OK);
                                    break;
                                }
                            }

                            /// Arbeitsblatt wird geschlossen
                            sheet.Close();

                            /// Spieleliste wird aktualisiert
                            LoadLibrary(libraryFileName);
                            CreateComBoxGenreItems();

                            /// Alle Input Felder werden wieder zurückgesetzt und die InputBox ausgeblendet
                            editGameInputBox.Visibility = Visibility.Hidden;

                            /// Die Änderung wird direkt nach Speicherung links im Infobereich aktualisiert
                            textBoxName.Text = editGameName.Text;
                            textBoxGenre.Text = editGameGenre.Text;
                            textBoxPlatform.Text = editGameplatform.Text;
                            textRelease_Date.Text = editGameReleaseDate.Text;
                            textBoxMeta_Score.Text = editGameMetaScore.Text;
                            textBlockDescription.Text = Encoding.Default.GetString(Encoding.Default.GetBytes(editGameDescription.Text));

                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();

                            Uri uriResult;
                            bool result_ = Uri.TryCreate(editGameCoverPath.Text, UriKind.Absolute, out uriResult);
                            if (!result_)
                                editGameCoverPath.Text = "";

                            if (editGameCoverPath.Text.Length != 0 && editGameCoverPath.Text != null)
                                bitmapImage.UriSource = new Uri(editGameCoverPath.Text);
                            else
                                bitmapImage.UriSource = new Uri("https://upload.wikimedia.org/wikipedia/commons/b/b9/No_Cover.jpg");

                            bitmapImage.EndInit();
                            img.Source = bitmapImage;
                        }
                        else
                        {
                            /// Wenn eine Zahl über 100 eingegeben wird, wird eine Fehler ausgeworfen und das Textfeld geleert
                            MessageBox.Show("Enter less 100 number", "Error", MessageBoxButton.OK);
                            addGameMetaScore.Text = "";
                        }
                    }
                    else
                    {
                        /// Wenn keine Zahl eingegeben wird, wird eine Fehler ausgeworfen und das Textfeld geleert
                        MessageBox.Show("Please only enter numbers", "Error", MessageBoxButton.OK);
                        addGameMetaScore.Text = "";
                    }
                }
                else
                {
                    /// Wenn ein falsches Datumformat eingegeben word ist, wird ein Fehler ausgeworfen und das Textfeld geleert
                    MessageBox.Show("Please enter the correct date format", "Error", MessageBoxButton.OK);
                    addGameReleaseDate.Text = "";
                }
            }
            else
            {
                /// Wenn in einer der Textfelder sich ein Semikolon befindet, wird ein Fehler ausgeworfen
                MessageBox.Show("The input fields must not contain semicolons!", "Error", MessageBoxButton.OK);
            }
        }
    }
}
