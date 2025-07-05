namespace FunctEngine
{
    public class SynchronizationAgent
    {
        private List<CriticalSection> criticalSections;
        public SynchronizationAgent() {
            criticalSections= new List<CriticalSection>();
        }
        public void ResetSynchronizationAgent()
        {
            Monitor.Enter(criticalSections);
            criticalSections.Clear();
            Monitor.Exit(criticalSections);
        }
        public bool AddCellToCS(string CriticalSectionName, int CellNumber)
        {
            Monitor.Enter(criticalSections);
            try
            {
                if (HasCS(CriticalSectionName))
                {
                    GetCriticalSection(CriticalSectionName).AddCell(CellNumber);
                    return true;
                }else
                {
                    CriticalSection criticalSection = new CriticalSection(CriticalSectionName);
                    criticalSection.AddCell(CellNumber);
                    criticalSections.Add(criticalSection);
                }
            }
            finally
            {
                Monitor.Exit(criticalSections);
                
            }
            return false;
        }
        public bool RemoveCellFromCS(string CriticalSectionName, int CellNumber)
        {
            Monitor.Enter(criticalSections);
            try
            {
                if (HasCS(CriticalSectionName))
                {
                    GetCriticalSection(CriticalSectionName).RemoveCell(CellNumber);
                    return true;
                }
            }
            finally
            {
                Monitor.Exit(criticalSections);

            }
            return false;
        }
        private bool HasCS(string criticalSectionName)
        {
            foreach (CriticalSection criticalSection in criticalSections)
            {
                if(criticalSection.Name == criticalSectionName) { return true; }
            }
            return false;
        }
        public bool CellIsBlocked(string criticalSectionName, int CellNumber)
        {
            CriticalSection criticalSection = GetCriticalSection(criticalSectionName);
            if (criticalSection != null)
                return criticalSection.CellIsBlocked(CellNumber);
            return false;
        }
        private CriticalSection GetCriticalSection(string name)
        {
            foreach (CriticalSection criticalSection in criticalSections)
            {
                if (criticalSection.Name == name)
                    return criticalSection;
            }
            return null;
        }

        public void RemoveCellFromAllCS(int CellNumber)
        {
            Monitor.Enter(criticalSections);
            foreach (CriticalSection criticalSection in criticalSections)
                criticalSection.RemoveCell(CellNumber);
            Monitor.Exit(criticalSections);
        }
    }
}
