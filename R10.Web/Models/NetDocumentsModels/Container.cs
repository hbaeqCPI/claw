using System.Reflection;
using System.Runtime.Serialization;

namespace R10.Web.Models.NetDocumentsModels
{
    public class Container : Document
    {
        /// <summary>
        /// The container's EnvId.
        /// EnvId is saved in tblDocFolder.StorageRootContainerId and tblDocFolder.StorageDefaultFolderId.
        /// Ancestors.Id which is used to locate the parent folder is in EnvId format.
        /// </summary>
        public override string? Id => (EnvId ?? "").Split('|')[0];

        public string? Name => Attributes?.Name;

        public ContainerType? ContainerType => GetContainerTypeByValue(Attributes?.Ext);

        public Folder ToFolder()
        {
            return new Folder
            {
                DocId = DocId,
                EnvId = EnvId,
                DocNum = DocNum,
                Attributes = Attributes,
                Ancestors = Ancestors,
                LimitAceess = LimitAceess,
                Checksum = Checksum,
                ChecksumAlgorithm = ChecksumAlgorithm
            };
        }

        public static ContainerType GetContainerTypeByValue(string? value)
        {
            var type = typeof(ContainerType);
            foreach (var name in Enum.GetNames(type))
            {
                var field = type.GetField(name);
                var attr = field?.GetCustomAttribute<EnumMemberAttribute>();

                if (attr != null && attr.Value == value)
                    return (ContainerType)Enum.Parse(type, name);
            }

            return (ContainerType)0;
        }
    }

    public class ContainerResponse : Container
    {
    }

    public class ContainersResponse
    {
        public List<Container>? Results { get; set; }
    }

    public enum ContainerType
    {
        Undefined,
        [EnumMember(Value = "ndws")]
        Workspace,
        [EnumMember(Value = "ndfld")]
        Folder,
        [EnumMember(Value = "ndflt")]
        Filter
    }
}
