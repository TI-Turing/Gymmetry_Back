//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataBaseModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class LogErrors
    {
        public System.Guid Id { get; set; }
        public string Error { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
        public Nullable<System.DateTime> DeletedAt { get; set; }
        public string Ip { get; set; }
        public bool IsActive { get; set; }
        public System.Guid UserId { get; set; }
        public System.Guid SubModuleId { get; set; }
    
        public virtual SubModule SubModule { get; set; }
        public virtual User User { get; set; }
    }
}
