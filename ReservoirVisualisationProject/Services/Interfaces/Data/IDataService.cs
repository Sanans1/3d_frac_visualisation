using System.Collections.Generic;
using ReservoirVisualisationProject.Models;

namespace ReservoirVisualisationProject.Services.Interfaces.Data
{
    public interface IDataService
    {
        void SaveWellData(List<WellModel> wells, string filePath);
        List<WellModel> LoadWellData(string filePath);
    }
}