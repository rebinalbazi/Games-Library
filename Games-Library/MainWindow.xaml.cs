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
                // Wenn das Spiel kein Cover enthält, dann wird ein Bild mit "No Cover Available" angezeigt
                if (SelectedGame.Cover_Path.Length != 0 && SelectedGame.Cover_Path != null)
                {
                    bitmapImage.UriSource = new Uri(SelectedGame.Cover_Path);
                } else
                {
                    bitmapImage.UriSource = new Uri("https://upload.wikimedia.org/wikipedia/commons/b/b9/No_Cover.jpg");
                }
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
        private void searchFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Bei Änderungen im Suchfeld wird die Spieleliste immer wieder neu aktualisiert
            CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource).Refresh();
        }
    }
}
