using ForariaDomain;


namespace ForariaTest.Unit.Helpers;

public static class TestDataBuilder
{
    public static User CreateUser(
       int id = 1,
       string name = "Test",
       string lastName = "User",
       string email = "test@foraria.com",
       string roleDescription = "Administrador",
       int roleId = 2)
    {
        return new User
        {
            Id = id,
            Name = name,
            LastName = lastName,
            Mail = email,
            Password = "hashed_password",
            Role_id = roleId,
            Role = new Role
            {
                Id = roleId,
                Description = roleDescription
            },
            Residences = new List<Residence?>()
        };
    }

    public static Consortium CreateConsortium(
        int id = 1,
        string name = "Test Consortium",
        string description = "Test Description",
        int? administratorId = null)
    {
        return new Consortium
        {
            Id = id,
            Name = name,
            Description = description,
            AdministratorId = administratorId,
            Administrator = administratorId.HasValue
                ? CreateUser(administratorId.Value)
                : null
        };
    }

    public static Residence CreateResidence(
        int id = 1,
        int consortiumId = 1,
        int floor = 1,
        int number = 101,
        string tower = "A")
    {
        return new Residence
        {
            Id = id,
            ConsortiumId = consortiumId,
            Floor = floor,
            Number = number,
            Tower = tower,
            Consortium = CreateConsortium(consortiumId)
        };
    }

    public static Role CreateRole(int id = 2, string description = "Administrador")
    {
        return new Role
        {
            Id = id,
            Description = description
        };
    }
}
