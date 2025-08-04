using FunctEngine;
using FunctEngine.Enums;
namespace CoreFunctions;

public class UpdateStatus: Funct
{
   public UpdateStatus(VariableCollection variableCollection, object otherparameter) : base(variableCollection, otherparameter)
    {
        Description = "Function to Update Status on Client";
    }
    public override FunctResult Execute()
    {
        FunctResult testResult = this.FunctResult;
        testResult.FunctName = this.Name;
        testResult.FunctDescription = this.Description;
        string stringVariable = this.ReadStringVariable("Argument0", "Update String");
        this.variableCollection.UpdaFunctStusText(stringVariable);
        testResult.Status = FunctStatus.Pass;
        return testResult;
    }
}