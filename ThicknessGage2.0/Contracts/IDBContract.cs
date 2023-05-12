using System;

namespace ThicknessGage2_0.Contracts
{
    public interface IDBContract
    {
        void SaveTo(string value, DateTime dateTime);
        string GetCoilIdZone();
    }
}
