namespace Fitmeplan.Db.Migration
{
    public class VersionHelper
    {
        public static long CalculateValue(int year, int month, int day, int scriptNumber, int branchScriptNumber)
        {
            return year * 100000000L + month * 1000000L + day * 10000L +  scriptNumber * 100L + branchScriptNumber;
        }
    }
}