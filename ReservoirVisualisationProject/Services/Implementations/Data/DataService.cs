using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservoirVisualisationProject.Models;
using ReservoirVisualisationProject.Services.Interfaces.Data;
using Newtonsoft.Json;

namespace ReservoirVisualisationProject.Services.Implementations.Data
{
    public class DataService : IDataService
    {
        public void SaveWellData(List<WellModel> wells, string filePath)
        {
            string json = JsonConvert.SerializeObject(wells);

            File.WriteAllText(filePath, json);
        }

        public List<WellModel> LoadWellData(string filePath)
        {
            string json = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<List<WellModel>>(json);
        }
    }
}
