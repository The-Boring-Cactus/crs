using FunctEngine.Enums;

namespace FunctEngine
{
    [Serializable]
    public class FunctResults
    {
        private List<FunctResult> functResult_list;
        public FunctStatus status;
        private string serialno;
        private string internaserial;
        private DateTime startTime = DateTime.Now;
        private DateTime stopTime = DateTime.Now;
        private string statusText = "";
        private string functScript=string.Empty;
        private string failMessage = "";
        
        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; }
        }
        public FunctResults()
        {
            // TODO: Add constructor logic here
            functResult_list = new List<FunctResult>();

            this.status = FunctStatus.NotRun;
        }
        public void AddResult(FunctResult tr)
        {
            functResult_list.Add(tr);
        }
        public void SetStartTime()
        {
            startTime = DateTime.Now;
        }
        public void SetStopTime()
        {
            stopTime = DateTime.Now;
        }
        public void ClearResults()
        {

            functResult_list.Clear();
        }
        public List<FunctResult> GetResults()
        {
            return functResult_list;


        }
        public DateTime StartTime
        {

            get
            {
                return this.startTime;
            }
        }
        public DateTime StopTime
        {
            get
            {
                return this.stopTime;
            }

        }

        public string FunctScript
        {
            get
            {
                return this.functScript;
            }
            set
            {
                this.functScript = value;
            }
        }
        public long TotalExecutionTime
        {
            get
            {
                return this.StopTime.Ticks - this.StartTime.Ticks;
            }
        }
        public string FailMessage
        {
            get
            {
                return this.failMessage;
            }
            set
            {
                this.failMessage = value;
            }

        }
        private string technician= string.Empty;
        public string SerialNo { get { return serialno; } set { serialno = value; } }   
        public string InternalSerial {  get { return internaserial; } set { internaserial = value; } }
        public string Technician { get { return technician; } set { technician = value; } }
        public FunctResult[] results
        {
            get
            {
                return functResult_list.ToArray();
            }
        }

        
    }
}