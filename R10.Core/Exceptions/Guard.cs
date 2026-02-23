using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core
{
    
    public interface IGuardClause
    {
    }
    public class Guard : IGuardClause
    {
        public static IGuardClause Against { get; } = new Guard();

        private Guard() { }
    }
}
