using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Core.Exceptions
{
    public static class GuardClauseExtensions
    {
        public static void Null(this IGuardClause guardClause, object? input, string parameterName)
        {
            if (input == null)
            {
                throw new ValueNotAllowedException($"The {parameterName} field is required.");
            }
        }

        public static void NullOrEmpty(this IGuardClause guardClause, string? input, string parameterName)
        {
            Guard.Against.Null(input, parameterName);
            if (input == String.Empty)
            {
                throw new ValueNotAllowedException($"The {parameterName} field is required.");
            }
        }

        public static void NullOrZero(this IGuardClause guardClause, int? input, string parameterName)
        {
            Guard.Against.Null(input, parameterName);
            if (input == 0)
            {
                throw new ValueNotAllowedException($"The {parameterName} field is required.");
            }
        }

        public static void NoRecordPermission(this IGuardClause guardClause, bool found)
        {
            if (!found)
            {
                throw new NoRecordPermissionException();
            }
        }

        public static void NoFieldPermission(this IGuardClause guardClause, bool allowed, string parameterName)
        {
            if (!allowed)
            {
                throw new ValueNotAllowedException($"Permission to update the {parameterName} field is denied.");
            }
        }

        public static void ValueNotAllowed(this IGuardClause guardClause, bool allowed, string parameterName)
        {
            if (!allowed)
            {
                throw new ValueNotAllowedException($"Invalid {parameterName}.");
            }
        }

        public static void UnAuthorizedAccess(this IGuardClause guardClause, bool authorized)
        {
            if (!authorized)
            {
                throw new UnauthorizedAccessException();
            }
        }

        public static void RecordExists(this IGuardClause guardClause, bool exists)
        {
            if (exists)
            {
                throw new ValueNotAllowedException($"The record already exists.");
            }
        }

        public static void RecordNotFound(this IGuardClause guardClause, bool found)
        {
            if (!found)
            {
                throw new ValueNotAllowedException($"Record not found or access to the record is denied.");
            }
        }

        public static void ModuleNotEnabled(this IGuardClause guardClause, bool enabled)
        {
            if (!enabled)
            {
                throw new Exception("This feature is not enabled.");
            }
        }
    }
}
