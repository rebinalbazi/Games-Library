using Microsoft.VisualStudio.TestTools.UnitTesting;
using Games_Library;
using System.Collections.Generic;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTests
    {
        MainWindow mw = new MainWindow();

        [TestMethod]
        public void LoadLibrary_Error()
        {
            //Arrange
            string library = "Nonexistent_Library";

            //Act
            bool result = mw.LoadLibrary(library);

            //Assert
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void LoadLibrary_Success()
        {
            //Arrange
            string library = "00_All_Games";

            //Act
            bool result = mw.LoadLibrary(library);

            //Assert
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void ReadCSV_Error()
        {
            //Arrange
            string path = @"C:\";
            string file = "Nonexistent_File.csv";

            //Act
            IEnumerable<Game> result = mw.ReadCSV(path + file);

            //Assert
            Assert.AreEqual(result, null);
        }

        [TestMethod]
        public void ReadCSV_Success()
        {
            //Arrange
            string path = mw.GetPath();
            string file = "00_All_Games";

            //Act
            IEnumerable<Game> result = mw.ReadCSV(@"" + path + file + ".csv");

            //Assert
            Assert.AreEqual(result.ToString(), "System.Linq.Enumerable+WhereSelectArrayIterator`2[System.String,Games_Library.Game]");
        }

        [TestMethod]
        public void CreateCSVFile_Error()
        {
            //Arrange
            string file = "00_All_Games";

            //Act
            bool result = mw.CreateCSVFile(file);

            //Assert
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void CreateCSVFile_Success()
        {
            //Arrange
            string path = mw.GetPath();
            string file = "Test";

            //Act
            bool result = mw.CreateCSVFile(file);

            //Assert
            Assert.AreEqual(result, true);
        }
    }
}
