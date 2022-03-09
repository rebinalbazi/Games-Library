using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Text;

namespace Games_Library
{
    public partial class MainWindow : Window
    {
        public GridViewColumnHeader listViewSortCol = null;
        public SortAdorner listViewSortAdorner = null;

        public MainWindow()
        {
            InitializeComponent();

            // Lädt zum Start des Programmes die 00_All _Games.csv
            LoadLibrary("00_All _Games");

            // Lädt zum Start alle vorhandenen Listen in die Combobox Items
            CreateComboBoxListItems();

            // Release-Jahr items werden für die Combobox releaseYearFilter von 1990-2022 angelegt
            for (int i = 2022; i > 1989; i--)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = i;
                releaseYearFilter.Items.Add(item);
            }
        }
        public IEnumerable<Game> ReadCSV(string fileName)
        {
            // Für die Auflistung wird die Variable lines als string array deklariert und kann nur .csv dateien einlesen
            string[] lines = File.ReadAllLines(Path.ChangeExtension(fileName, ".csv"));

            // lines.Select erlaubt, jede Zeile als Spiel zu wiedergeben.
            // Diese gibt dann ein IEnumerable<Game> zurück.
            return lines.Select(line =>
            {
                // Nach jedem Semikolin werden die Daten in der .csv-Datei getrennt
                string[] data = line.Split(';');
                // Das Spiel wird dann mit einzelnen Daten in der richtigen Reihenfolge zurückgegeben.
                return new Game(data[0], data[1], data[2], data[3], data[4], Convert.ToInt32(data[5]), data[6]);
            });
        }

        // Sortier-Methode beim rauf klicken der jeweiligen Spiele-Eigenschaft
        private void SortColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();

            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ListViewGame.Items.SortDescriptions.Clear();
            }
            // Aufsteigende Sortierung
            ListSortDirection newDir = ListSortDirection.Ascending;

            // Absteigende Sortierung
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            ListViewGame.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        // Methode für das auswählen eines Spiels
        private void Game_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ausgwähltes Spiel wird in der variable SelectedGame speichern
            Game SelectedGame = (Game)ListViewGame.SelectedItem;

            // Die if-Anweisung kann nur ausgeführt werden wenn ein Spiel ausgewählt ist
            if (SelectedGame != null)
            {
                // Labels anzeigen lassen
                labelName.Visibility = Visibility.Visible;
                labelGenre.Visibility = Visibility.Visible;
                labelPlatform.Visibility = Visibility.Visible;
                labelReleaseDate.Visibility = Visibility.Visible;
                labelRatingScore.Visibility = Visibility.Visible;
                labelDescription.Visibility = Visibility.Visible;

                // Hier wird das Cover des Spiels als Bitmap eingelesen und geladen
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();

                // Wenn das Spiel kein Cover enthält, dann wird ein Bild mit "No Cover Available" angezeigt
                if (SelectedGame.Cover_Path.Length != 0 && SelectedGame.Cover_Path != null)
                    bitmapImage.UriSource = new Uri(SelectedGame.Cover_Path);
                 else
                    bitmapImage.UriSource = new Uri("https://upload.wikimedia.org/wikipedia/commons/b/b9/No_Cover.jpg");
                
                bitmapImage.EndInit();
                img.Source = bitmapImage;

                // Eigenschaften des jeweiligen Spiels werden übergeben
                textBoxName.Text = SelectedGame.Name;
                textBoxGenre.Text = SelectedGame.Genre;
                textBoxPlatform.Text = SelectedGame.Platform;
                textRelease_Date.Text = SelectedGame.Release_Date;
                textBoxRating_Score.Text = SelectedGame.Rating_Score.ToString();
                textBlockDescription.Text =  Encoding.Default.GetString(Encoding.Default.GetBytes(SelectedGame.Description));
            }
        }

        // Filter-Methode für Suche, Genre, Plattform und Release-Jahr
        private bool Filter(object item)
        {
            // Die jeweiligen Filter werden hier in Strings umgewandelt
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

            // Hier wird die Spieleliste mit den angewendeteten Filter zurückgegeben
            return ((item as Game).Name.IndexOf(searchFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (item as Game).Genre.IndexOf(genreString, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (item as Game).Platform.IndexOf(platformString, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (item as Game).Release_Date.IndexOf(releaseYear, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        // Text -und SelectionsHandler für die jeweiligen Filter:
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

        // ResetFilter-Methode um alle Filter wieder zuürckzusetzen und ButtonEventHandler
        private bool ResetFilter(object item)
        {
            // Alle Filter werden auf ihren Standartwert zurückgesetzt
            searchFilter.Text = "";
            genreFilter.SelectedIndex = -1;
            platformFilter.SelectedIndex = -1;
            releaseYearFilter.SelectedIndex = -1;

            // Sortierung wird zurückgesetzt
            AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
            ListViewGame.Items.SortDescriptions.Clear();

            // Danach wird die gesamte Spieleliste einfach augegeben
            return true;
        }
        private void resetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource);
            view.Filter = ResetFilter;
        }
        
        // create -und cancel List ButtonEventHandleer
        private void createListButton_Click(object sender, RoutedEventArgs e)
        {
            // InputBox wird angezeigt
            InputBox.Visibility = Visibility.Visible;
        }
        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            // InputBox wird wieder geschlossen und die .csv-Datei wird erzeugt
            InputBox.Visibility = Visibility.Collapsed;
            CreateCSVFile(InputTextBox.Text);
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            // InputBox wird wieder geschlossen und das Eingabefeld wird leer gesetzt
            InputBox.Visibility = Visibility.Collapsed;
            InputTextBox.Text = String.Empty;
        }

        // .csv-Erstell-Methode
        public void CreateCSVFile(string fileName)
        {
            try
            {
                // .csv-Datei wird mit dem gewünschten Namen erstellt
                string csvpath = @"C:\Users\Rebin\source\repos\Games-Library\Games-Library\database/" + fileName + ".csv";
                File.AppendAllText(csvpath, "");

                // InputBox wird wieder geleert
                InputTextBox.Text = String.Empty;

                MessageBox.Show("Creating was successful.");

                // Neu erstellte Liste wird der ComboBox hinzugefügt
                librariesSelection.Items.Add(fileName);
            }
            catch 
            {
                MessageBox.Show("Error! Please try again.");
            }
        }
        
        // Lädt alle .csv-Listen als ComboBox items
        public void CreateComboBoxListItems()
        {
            string[] filePaths = Directory.GetFiles(@"C:\Users\Rebin\source\repos\Games-Library\Games-Library\database\", "*.csv");
            foreach (string file in filePaths)
            {
                librariesSelection.Items.Add(file.Substring(65));
            }
        }

        //Lädt einer der gwählten Listen
        private bool LoadLibrary(string libraryFileName)
        {
            ListViewGame.ItemsSource = ReadCSV(@"C:\Users\Rebin\source\repos\Games-Library\Games-Library\database/" + libraryFileName + ".csv");
            return true;
        }
        // SelectionsHandler für die Auswahl der Listen
        private void librariesSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (librariesSelection != null)
            {
                string libraryFile = librariesSelection.SelectedItem.ToString();
                string libraryFileName = libraryFile.Remove(libraryFile.Length - 4, 4);
                LoadLibrary(libraryFileName);
            }
        }
    }
}
