using System.ComponentModel.DataAnnotations;


namespace StudentAPI.Models
{
	public class Registers
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; } = string.Empty;

		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		public string PasswordHash { get; set; } = string.Empty;
	}
}