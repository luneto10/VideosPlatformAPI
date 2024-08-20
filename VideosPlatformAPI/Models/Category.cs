using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideosPlatformAPI.Models;

public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public int Id { get; set; }
    
    [Required] public string Title { get; set; }
    
    [RegularExpression("^#(?:[0-9a-fA-F]{3}){1,2}$", ErrorMessage = "The Color must be a valid hex code.")]
    [Required] public string Color { get; set; }


    public ICollection<Video> Videos { get; set; } = new List<Video>();

}