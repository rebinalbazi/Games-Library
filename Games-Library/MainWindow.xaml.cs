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

                /// Hier wird das Cover des Spiels als Bitmap eingelesen und geladen
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();

                /// Wenn das Spiel kein Cover enthält, dann wird ein Bild mit "No Cover Available" angezeigt
                if (SelectedGame.Cover_Path.Length != 0 && SelectedGame.Cover_Path != null)
                    bitmapImage.UriSource = new Uri(SelectedGame.Cover_Path);
                else
                    bitmapImage.UriSource = new Uri("https:///upload.wikimedia.org/wikipedia/commons/b/b9/No_Cover.jpg");

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
            if (genreFilter.SelectedValue == null || genreFilter.SelectedValue.ToString().Length <= 36)
                genreString = "";
            else
                genreString = genreFilter.SelectedValue.ToString().Substring(38);

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

            /// Sortierung wird zurückgesetzt
            AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
            ListViewGame.Items.SortDescriptions.Clear();

            /// Danach wird die gesamte Spieleliste einfach augegeben
            return true;
        }
        private void resetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource);
            view.Filter = ResetFilter;
        }

        /// createList, create -und cancel List ButtonEventHandleer
        private void createLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            /// InputBox wird angezeigt
            InputBox.Visibility = Visibility.Visible;
        }
        private void saveLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            /// InputBox wird wieder geschlossen und die .csv-Datei wird erzeugt
            InputBox.Visibility = Visibility.Collapsed;
            CreateCSVFile(InputTextBox.Text);
        }
        private void cancelLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            /// InputBox wird wieder geschlossen und das Eingabefeld wird leer gesetzt
            InputBox.Visibility = Visibility.Collapsed;
            InputTextBox.Text = String.Empty;
        }

        /// Lädt einer der gewählten Listen
        private bool LoadLibrary(string libraryFileName)
        {
            ListViewGame.ItemsSource = ReadCSV(@"C:\Users\Rebin\source\repos\Games-Library\Games-Library\database/" + libraryFileName + ".csv");
            return true;
        }

        /// Erstellt eine neue .csv-Datei
        private void CreateCSVFile(string fileName)
        {
            try
            {
                /// .csv-Datei wird mit dem gewünschten Namen erstellt
                string csvpath = @"C:\Users\Rebin\source\repos\Games-Library\Games-Library\database/" + fileName + ".csv";
                File.AppendAllText(csvpath, "");

                /// InputBox wird wieder geleert
                InputTextBox.Text = String.Empty;

                MessageBox.Show("Creating was successful.");

                /// Neu erstellte Liste wird der ComboBox hinzugefügt
                librariesSelection.Items.Add(fileName + ".csv");
            }
            catch
            {
                MessageBox.Show("Error! Please try again.");
            }
        }

        /// Lädt alle .csv-Listen in die ComboBox als ComboBoxItems
        private void CreateComboBoxListItems()
        {
            string[] filePaths = Directory.GetFiles(@"C:\Users\Rebin\source\repos\Games-Library\Games-Library\database\", "*.csv");
            foreach (string file in filePaths)
            {
                librariesSelection.Items.Add(file.Substring(65));
            }
        }

        /// SelectionsHandler für die Auswahl der Listen
        private void librariesSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (librariesSelection != null)
            {
                string libraryFile = librariesSelection.SelectedItem.ToString();
                string libraryFileName = libraryFile.Remove(libraryFile.Length - 4, 4);
                LoadLibrary(libraryFileName);
            }
        }

        /// Methode zum Löschen eines Spiels
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Game SelectedGame = (Game)ListViewGame.SelectedItem;
            string libraryFile = librariesSelection.SelectedItem.ToString();
            string libraryFileName = libraryFile.Remove(libraryFile.Length - 4, 4);

            /// Excel-Instanz wird erstellt:
            Excel.Application excel = new Excel.Application();
            /// Excel-Datei öffnen
            Excel.Workbook sheet = excel.Workbooks.Open(@"C:\Users\Rebin\source\repos\Games-Library\Games-Library\database/" + libraryFile);
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

                    MessageBox.Show("Game has been deleted successfully");
                    break;
                }
            }
            /// Arbeitsblatt wird geschlossen
            sheet.Close();

            /// Spieleliste wird aktualisiert
            LoadLibrary(libraryFileName);

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
            try
            {
                Game SelectedGame = (Game)ListViewGame.SelectedItem;
                string libraryFile = librariesSelection.SelectedItem.ToString();
                string libraryFileName = libraryFile.Remove(libraryFile.Length - 4, 4);

                /// Excel-Instanz wird erstellt:
                Excel.Application excel = new Excel.Application();
                /// Excel-Datei öffnen
                Excel.Workbook sheet = excel.Workbooks.Open(@"C:\Users\Rebin\source\repos\Games-Library\Games-Library\database/" + libraryFile);
                /// Arbeitsblatt wird ausgewählt
                Excel.Worksheet x = excel.ActiveSheet as Excel.Worksheet;
                /// Range wird erstellt
                Excel.Range userRange = x.UsedRange;

                /// Überprüfung für nicht mehr als 100 Eingabe
                int a = Convert.ToInt32(textBoxUser_Score.Text);

                if (a > 100)
                {
                    MessageBox.Show("Enter less 100 number", "Error", MessageBoxButton.OK);
                    textBoxUser_Score.Text = "";
                }
                else
                {
                    if (SelectedGame != null)
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

                                MessageBox.Show("User score has been successfully updated!");
                                break;
                            }
                        }
                        /// Arbeitsblatt wird geschlossen
                        sheet.Close();

                        /// Spieleliste wird aktualisiert
                        LoadLibrary(libraryFileName);

                        textBoxUser_Score.IsReadOnly = true;
                        textBoxUser_Score.BorderThickness = new Thickness(0);
                        saveUserScoreButton.Visibility = Visibility.Hidden;
                        cancelUserScoreButton.Visibility = Visibility.Hidden;
                    } 
                }
            }
            catch
            {
                /// Wenn keine Zahl eingegeben wird, wird eine Fehler ausgeworfen und das Textfeld geleert
                MessageBox.Show("Please only enter numbers", "Error", MessageBoxButton.OK);
                textBoxUser_Score.Text = "";
            }
        }

    }
}
