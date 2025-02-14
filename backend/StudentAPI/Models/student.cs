using System.ComponentModel.DataAnnotations;

namespace StudentAPI.Models
{
    public class Student
    {
        [Key] 
        public int RollNo { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(1, 100)] 
        public int Age { get; set; }
    }
}