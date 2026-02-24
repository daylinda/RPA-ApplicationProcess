using Google.Cloud.Firestore;
using Swashbuckle.Swagger.Annotations;
using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations;

namespace RPA_Api.Model
{

    [FirestoreData]
    public class UserRecord
    {
        [Required]        
        [FirestoreProperty]
        public string Id {  get; set; }

        [Required]
        [FirestoreProperty]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [FirestoreProperty]
        public string Email { get; set; }

        [Required]
        [Phone]
        [RegularExpression(@"^(?:\+61|0)4\d{8}$")]
        [FirestoreProperty]
        public string PhoneNumber { get; set; } = string.Empty;

      


    }
}
