namespace R10.Web.Security
{
    public static class ReleaseAuthorizationPolicy
    {
        public const string CanAccessMainMenu = "CanAccessReleaseMenu";
        public const string CanAccessAuxiliary = "CanAccessReleaseAuxiliary";
        public const string AuxiliaryModify = "AuxiliaryModifyRelease";
        public const string AuxiliaryRemarksOnly = "AuxiliaryRemarksOnlyRelease";
        public const string AuxiliaryLimited = "AuxiliaryLimitedRelease";
        public const string AuxiliaryCanDelete = "AuxiliaryCanDeleteRelease";
    }
}
