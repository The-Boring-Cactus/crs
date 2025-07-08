namespace FunctEngine
{
    public class CriticalSection
    {
        private string name;
        private List<int> cellsWaiting;

        public CriticalSection(string csname)
        {
            cellsWaiting = new List<int>();
            name = csname;
        }
        public string Name { get { return name; } }
        public void AddCell(int cellNumber)
        {
            if (!cellsWaiting.Contains(cellNumber))
            {
                cellsWaiting.Add(cellNumber);
            }
        }
        public void RemoveCell(int cellNumber)
        {
            cellsWaiting.Remove(cellNumber);
        }
        public bool CellIsBlocked(int cellNumber) {  
            if(cellsWaiting.Count == 0)
                return false;

            return cellsWaiting[0]==cellNumber;
        }
    }
}
