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

namespace Games_Library
{
    public partial class MainWindow : Window
    {
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public MainWindow()
        {
            InitializeComponent();

            // Spieleliste einlesen und in ListViewGame speichern
            ListViewGame.ItemsSource = ReadCSV(@"C:\Users\Rebin\source\repos\Games-Library\Games-Library/games_list");

            // Die Variable view wird hier als CollectionView deklariert und mit dem Inhalten von ListView gespeichert
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource);
            // Der Filter von view wird für die Suche von Schlüsselwörtern mit der SearchFilter Methode initialisiert
            view.Filter = SearchFilter;
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
                return new Game(data[0], data[1], data[2], data[3], Convert.ToInt32(data[4]), data[5], Convert.ToInt32(data[6]), data[7]);
            });
        }

        // Sortier-Methode beim rauf klicken der Eigenschaft
        private void SortColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();

            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ListViewGame.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;

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
                // Hier wird Cover des Spiels als Bitmap eingelesen und geladen
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(SelectedGame.Cover_Path); ;
                bitmapImage.EndInit();
                img.Source = bitmapImage;

                // Eigenschaften werden den labels übergeben
                labelName.Content = SelectedGame.Name;
                labelGenre.Content = SelectedGame.Genre;
                labelPlatform.Content = SelectedGame.Platform;
                labelRelease_Day.Content = SelectedGame.Release_Day;
                labelRelease_Year.Content = SelectedGame.Release_Year;
                labelRating_Score.Content = SelectedGame.Rating_Score;
                labelDescription.Content = SelectedGame.Description;
            }
        }

        // Suchfilter-Methode, die als Parameter den Inhalt des ListViewGame erhält
        private bool SearchFilter(object item)
        {
            // Wenn die Suchleiste leer ist dann soll die gesamte Liste ausgegeben werden
            if (String.IsNullOrEmpty(searchFilter.Text))
                return true;
            else
                // Wenn die Suchleiste nicht leer ist, soll nach Spielen gesucht werden, welche das Schlüsswort enthält
                return ((item as Game).Name.IndexOf(searchFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        // TextChangedEvent für das Suchfeld
        private void searchFilter_KeywordChanged(object sender, TextChangedEventArgs e)
        {
            // Bei Änderungen im Suchfeld wird die Spieleliste immer wieder neu aktualisiert
            CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource).Refresh();
        }
    }
}
