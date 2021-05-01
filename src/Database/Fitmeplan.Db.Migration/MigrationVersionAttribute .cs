using FluentMigrator;

namespace Fitmeplan.Db.Migration
{
    public class MigrationVersionAttribute : FluentMigrator.MigrationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationVersionAttribute" /> class.
        /// format YYYYMMDDNNBB
        /// BB - branch script number ##
        /// NN - script number ##
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="scriptNumber">The script number.</param>
        /// <param name="branchScriptNumber">The branch number.</param>
        /// <param name="transaction">The transaction behavior.</param>
        public MigrationVersionAttribute(int year, int month, int day, int scriptNumber, int branchScriptNumber = 0, TransactionBehavior transaction = TransactionBehavior.Default)
            : base(VersionHelper.CalculateValue(year, month, day, branchScriptNumber, scriptNumber), transaction)
        {
        }
    }
}
