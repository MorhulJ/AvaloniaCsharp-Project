using System;
namespace GymApp.Models;

public class User
{
    public string FirebaseId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Gender { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public int weight { get; set; }
    public int height { get; set; }
}