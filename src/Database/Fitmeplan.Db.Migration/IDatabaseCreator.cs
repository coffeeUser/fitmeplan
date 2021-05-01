using System;

namespace Fitmeplan.Db.Migration
{
    internal interface IDatabaseCreator
    {
        void Run();

        void Drop();
    }
}