using System.Collections.Generic;
using FracVisualisationSoftware.Models;

namespace FracVisualisationSoftware.Services.Interfaces.Data
{
    public interface IDataService
    {
        void SaveWellData(List<WellModel> wells, string filePath);
        List<WellModel> LoadWellData(string filePath);
    }
}