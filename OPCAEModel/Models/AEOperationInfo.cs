using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPCAEModel.Models
{
    public class AEOperationInfo
    {
        [Key]
        public int ID { get; set; }
        public string TagName { get; set; }
        public string OperContains { get; set; }
        public string OperUser { get; set; }
        public DateTime? OperTime {  get; set; }
        public byte IsDispose { get; set; } = 0;
        public string Remark { get; set; }
    }
}
