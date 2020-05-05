using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using ReservoirVisualisationProject.Models;
using ReservoirVisualisationProject.Services.Implementations.Data;
using ReservoirVisualisationProject.Services.Interfaces.Data;

namespace ReservoirVisualisationProject.Tests
{
    [TestFixture]
    public class DataServiceTests
    {
        private IDataService _dataService;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _dataService = new DataService();
        }

        [Test]
        public void SaveWellData_SaveWellModel_FileCreated()
        {
            List<WellModel> wellModels = new List<WellModel>();

            string fileName = Guid.NewGuid().ToString();

            string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}/{fileName}.txt";

            _dataService.SaveWellData(wellModels, filePath);

            string actualContents = File.ReadAllText(filePath);

            Assert.AreEqual("[]", actualContents);

            File.Delete(filePath);
        }

        [Test]
        public void LoadWellData_LoadWellModel_FileLoads()
        {
            List<WellModel> expectedWellModels = new List<WellModel>();

            string fileName = Guid.NewGuid().ToString();

            string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}/{fileName}.txt";

            string json = JsonConvert.SerializeObject(expectedWellModels);

            File.WriteAllText(filePath, json);

            string actualJson = File.ReadAllText(filePath);

            List<WellModel> actualWellModels = JsonConvert.DeserializeObject<List<WellModel>>(actualJson);

            Assert.AreEqual(expectedWellModels, actualWellModels);

            File.Delete(filePath);
        }
    }
}
