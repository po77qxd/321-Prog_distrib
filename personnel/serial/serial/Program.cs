using System.Text.Json;

namespace serial
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Character character = new Character() { FirstName = "prenom", LastName = "nom", Description = "description", PlayedBy = null };

            string json = JsonSerializer.Serialize(character);
            File.WriteAllText("data.json", json);
        }
    }

    public class Character
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public Actor PlayedBy { get; set; }
    }

    public class Actor
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Country { get; set; }
        public bool IsAlive { get; set; }
    }
}
