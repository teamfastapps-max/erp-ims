using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models
{
    public class VendorDocumentModel
    {
        public int VendorDocumentId { get; set; }
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Document type is required")]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; }

        [Display(Name = "Document Number")]
        public string DocumentNumber { get; set; }

        [Required]
        public string FilePath { get; set; }

        public int? UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; }
        public string FileName => System.IO.Path.GetFileName(FilePath);
    }
}
