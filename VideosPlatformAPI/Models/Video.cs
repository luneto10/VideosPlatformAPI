using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideosPlatformAPI.Models;

public class Video
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public int Id { get; set; }
    
    [Required] public string Title { get; set; }
    
    [Required] public string Description { get; set; }
    
    [Required]
    [Url(ErrorMessage = "The URL is not valid.")]
    [StringLength(2083, ErrorMessage = "The URL must be 2083 characters or shorter.")]
    public string Url { get; set; }
    
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
}