using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    public class UserSystemRoleDTO
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? SystemId { get; set; }
        public string? System { get; set; }
        public string? RoleId { get; set; }
        public string? Role { get; set; }
        public string? RespOffice { get; set; }
    }
}
